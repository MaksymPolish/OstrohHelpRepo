import api from "./api";

const CANDIDATE_ENDPOINTS = [
  "/Questionnaire/Create-Questionnaire",
  "/Create-Questionnaire",
];

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
