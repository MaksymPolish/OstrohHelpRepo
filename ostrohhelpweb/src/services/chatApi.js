import api from "./api";

const MAX_BATCH_UPLOAD_FILES = 6;

const unwrapCollection = (payload) => {
  if (Array.isArray(payload)) {
    return payload;
  }

  if (Array.isArray(payload?.$values)) {
    return payload.$values;
  }

  if (Array.isArray(payload?.items)) {
    return payload.items;
  }

  if (Array.isArray(payload?.data)) {
    return payload.data;
  }

  if (Array.isArray(payload?.messages)) {
    return payload.messages;
  }

  if (Array.isArray(payload?.consultations)) {
    return payload.consultations;
  }

  return [];
};

export const getUserConsultations = async (userId) => {
  const response = await api.get(`/Consultations/Get-All-Consultations-By-UserId/${userId}`);
  return unwrapCollection(response.data);
};

export const getConsultationMessages = async (consultationId) => {
  const response = await api.get("/Message/Recive", {
    params: { idConsultation: consultationId },
  });
  return unwrapCollection(response.data);
};

const normalizeUploadedItems = (payload) => {
  const results = unwrapCollection(payload?.results || payload?.Results || payload);

  return results
    .map((item) => {
      const url = item?.fileUrl || item?.FileUrl || item?.url || item?.Url || null;

      return {
        attachmentId: item?.attachmentId || item?.AttachmentId || null,
        fileName: item?.fileName || item?.FileName || "",
        isSuccess: typeof item?.isSuccess === "boolean" ? item.isSuccess : true,
        errorMessage: item?.errorMessage || item?.ErrorMessage || "",
        url,
      };
    })
    .filter((item) => item.url || item.errorMessage || item.attachmentId);
};

const getFileExtension = (fileName) => {
  if (!fileName || typeof fileName !== "string") {
    return "";
  }

  const cleanName = fileName.trim();
  const dotIndex = cleanName.lastIndexOf(".");
  if (dotIndex <= 0 || dotIndex === cleanName.length - 1) {
    return "";
  }

  return cleanName.slice(dotIndex + 1).toLowerCase();
};

const createGuidLikeId = () => {
  if (typeof crypto !== "undefined" && typeof crypto.randomUUID === "function") {
    return crypto.randomUUID();
  }

  const randomPart = Math.random().toString(16).slice(2, 10);
  return `${Date.now().toString(16)}-${randomPart}`;
};

const createSafeUploadFileName = (file) => {
  const extension = getFileExtension(file?.name);
  const guid = createGuidLikeId();
  return extension ? `${guid}.${extension}` : guid;
};

const buildBatchUploadFormData = (fileList, strategy) => {
  const formData = new FormData();

  fileList.forEach((file, index) => {
    const safeFileName = createSafeUploadFileName(file);

    if (strategy === "files") {
      formData.append("files", file, safeFileName);
      return;
    }

    if (strategy === "file") {
      formData.append("file", file, safeFileName);
      return;
    }

    formData.append(`file[${index}]`, file, safeFileName);
  });

  return formData;
};

const shouldRetryWithAnotherFieldStrategy = (error) => {
  const status = error?.response?.status;
  if (status !== 400) {
    return false;
  }

  const payload = error?.response?.data;
  const message = [
    typeof payload?.message === "string" ? payload.message : "",
    typeof payload?.error === "string" ? payload.error : "",
    typeof payload?.title === "string" ? payload.title : "",
  ]
    .join(" ")
    .toLowerCase();

  return message.includes("no files") || message.includes("file");
};

export const uploadMessageFiles = async (files, options = {}) => {
  const fileList = Array.isArray(files) ? files.filter(Boolean) : [];
  if (fileList.length === 0) {
    return [];
  }

  if (fileList.length > MAX_BATCH_UPLOAD_FILES) {
    throw new Error(`Cannot upload more than ${MAX_BATCH_UPLOAD_FILES} files at once.`);
  }

  const { messageId } = options;
  const strategies = ["files", "file", "indexed"];
  let lastError = null;

  for (const strategy of strategies) {
    try {
      const formData = buildBatchUploadFormData(fileList, strategy);
      const response = await api.post("/Message/BatchUpload", formData, {
        params: messageId ? { messageId } : undefined,
        headers: {
          "Content-Type": "multipart/form-data",
        },
      });

      return normalizeUploadedItems(response.data);
    } catch (error) {
      lastError = error;
      if (!shouldRetryWithAnotherFieldStrategy(error)) {
        throw error;
      }
    }
  }

  throw lastError || new Error("Failed to upload files");
};

export const uploadMessageFile = async (_userId, file, options = {}) => {
  const items = await uploadMessageFiles([file], options);
  return items[0] || null;
};

export const deleteConsultationMessage = async (messageId) => {
  const payload = {
    messageId,
    MessageId: messageId,
  };

  await api.delete("/Message/Delete", {
    data: payload,
  });
};

export const sendConsultationMessage = async ({
  senderId,
  receiverId,
  consultationId,
  encryptedContent,
  iv,
  authTag,
  mediaPaths = [],
}) => {
  const payload = {
    // Required encrypted message fields
    senderId,
    consultationId,
    receiverId,
    encryptedContent,
    iv,
    authTag,

    // Compatibility aliases for backend DTO naming differences
    SenderId: senderId,
    ConsultationId: consultationId,
    ReceiverId: receiverId,
    EncryptedContent: encryptedContent,
    Iv: iv,
    AuthTag: authTag,

    mediaPaths,
  };

  const response = await api.post("/Message/Send", payload);

  return response.data || null;
};
