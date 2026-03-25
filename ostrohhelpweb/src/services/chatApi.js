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
  content,
  mediaPaths = [],
}) => {
  const messageText = typeof content === "string" ? content : "";

  const payload = {
    // Required business fields
    senderId,
    consultationId,
    content: messageText,

    // Compatibility aliases for backend DTO naming differences
    SenderId: senderId,
    ConsultationId: consultationId,
    text: messageText,
    Text: messageText,
    message: messageText,
    Message: messageText,

    mediaPaths,
  };

  if (receiverId) {
    payload.receiverId = receiverId;
    payload.ReceiverId = receiverId;
  }

  const response = await api.post("/Message/Send", payload);

  return response.data || null;
};
