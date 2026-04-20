import api from "./api";

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

export const uploadMessageFile = async (userId, file) => {
  const formData = new FormData();
  formData.append("file", file);

  const response = await api.post(`/Message/UploadToCloud/${userId}`, formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return response.data || null;
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
