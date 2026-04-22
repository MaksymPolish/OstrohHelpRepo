import api from "./api";

const CANDIDATE_ENDPOINTS = [
  "/Questionnaire/Create-Questionnaire",
  "/Create-Questionnaire",
];

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
  let lastError = null;

  for (const endpoint of CANDIDATE_ENDPOINTS) {
    try {
      const response = await api.post(endpoint, payload);
      return response.data || null;
    } catch (error) {
      lastError = error;
    }
  }

  throw lastError || new Error("Failed to create questionnaire");
};

export const getUserQuestionnaires = async (userId) => {
  const candidateEndpoints = [
    `/Questionnaire/get-by-user-id/${userId}`,
    `/Questionnaire/Get-By-User-Id/${userId}`,
    `/Questionnaire/Get-By-UserId/${userId}`,
  ];

  let lastError = null;

  for (const endpoint of candidateEndpoints) {
    try {
      const response = await api.get(endpoint);
      return unwrapCollection(response.data);
    } catch (error) {
      lastError = error;
    }
  }

  throw lastError || new Error("Failed to load user questionnaires");
};
