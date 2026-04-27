import api from "./api";

const CANDIDATE_ENDPOINTS = [
  "/Questionnaire/Create-Questionnaire",
  "/Create-Questionnaire",
];

const ADMIN_CANDIDATE_ENDPOINTS = {
  list: ["/Questionnaire/all", "/Questionnaire/Get-All", "/all"],
  statuses: [
    "/QuestiStatController/Get-All-Statuses",
    "/QuestionaryStatus/Get-All-Statuses",
    "/Get-All-Statuses",
  ],
  updateStatus: [
    "/Questionnaire/Update-Status",
    "/Questionnaire/UpdateStatus",
    "/Update-Status",
  ],
  delete: ["/Questionnaire/Delete", "/Delete"],
};

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

const requestWithCandidates = async ({ endpoints, method, data, params }) => {
  let lastError = null;

  for (const endpoint of endpoints) {
    try {
      const response = await api.request({
        url: endpoint,
        method,
        data,
        params,
      });

      return response.data ?? null;
    } catch (error) {
      lastError = error;
    }
  }

  throw lastError || new Error("Request failed");
};

export const createQuestionnaire = async (payload) => {
  return requestWithCandidates({
    endpoints: CANDIDATE_ENDPOINTS,
    method: "post",
    data: payload,
  });
};

export const getUserQuestionnaires = async (userId) => {
  const candidateEndpoints = [
    `/Questionnaire/get-by-user-id/${userId}`,
    `/Questionnaire/Get-By-User-Id/${userId}`,
    `/Questionnaire/Get-By-UserId/${userId}`,
  ];

  const responseData = await requestWithCandidates({
    endpoints: candidateEndpoints,
    method: "get",
  });

  return unwrapCollection(responseData);
};

export const getAllQuestionnaires = async () => {
  const responseData = await requestWithCandidates({
    endpoints: ADMIN_CANDIDATE_ENDPOINTS.list,
    method: "get",
  });

  return unwrapCollection(responseData);
};

export const getQuestionaryStatuses = async () => {
  const responseData = await requestWithCandidates({
    endpoints: ADMIN_CANDIDATE_ENDPOINTS.statuses,
    method: "get",
  });

  return unwrapCollection(responseData);
};

export const updateQuestionnaireStatus = async ({ questionnaireId, statusId }) => {
  return requestWithCandidates({
    endpoints: ADMIN_CANDIDATE_ENDPOINTS.updateStatus,
    method: "put",
    data: { questionnaireId, statusId },
  });
};

export const deleteQuestionnaire = async (questionnaireId) => {
  return requestWithCandidates({
    endpoints: ADMIN_CANDIDATE_ENDPOINTS.delete,
    method: "delete",
    data: { questionnaireId },
  });
};

