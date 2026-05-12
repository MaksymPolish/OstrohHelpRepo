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

  if (Array.isArray(payload?.questionnaires)) {
    return payload.questionnaires;
  }

  return [];
};

export const createQuestionnaire = async (payload) => {
  const response = await api.post("/Questionnaire/Create-Questionnaire", payload);
  return response.data || null;
};

export const getUserQuestionnaires = async (userId) => {
  const response = await api.get(`/Questionnaire/get-by-user-id/${userId}`);
  return unwrapCollection(response.data);
};

export const getAllQuestionnaires = async () => {
  const response = await api.get("/Questionnaire/all");
  return unwrapCollection(response.data);
};

export const getQuestionaryStatuses = async () => {
  const response = await api.get("/QuestiStatController/Get-All-Statuses");
  return unwrapCollection(response.data);
};

export const updateQuestionnaireStatus = async ({ id, statusId }) => {
  const response = await api.put("/questionnaire/UpdateStatus", {
    id,
    statusId,
  });
  return response.data || null;
};

export const deleteQuestionnaire = async (questionnaireId) => {
  // Pass the ID as the data, and explicitly set the Content-Type header
  const response = await api.delete("/questionnaire/Delete-Questionnaire", {
    data: JSON.stringify(questionnaireId), // Ensure it's a JSON string
    headers: {
      'Content-Type': 'application/json'
    }
  });
  return response.data || null;
};

export const acceptQuestionnaire = async ({ questionaryId, psychologistId, scheduledTime }) => {
  const response = await api.post("/Consultations/Accept-Questionnaire", {
    questionaryId,
    psychologistId,
    scheduledTime,
  });
  return response.data || null;
};

