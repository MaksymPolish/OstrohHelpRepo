const FALLBACK_API_URL = "http://localhost:5000/api";

const safeTrim = (value) => (typeof value === "string" ? value.trim() : "");

const normalizeApiUrl = (value) => {
  const trimmedValue = safeTrim(value);
  if (!trimmedValue) {
    return FALLBACK_API_URL;
  }

  return trimmedValue.replace(/\/+$/, "");
};

const deriveSignalRHubUrl = (apiUrl) => {
  const normalizedApiUrl = normalizeApiUrl(apiUrl);

  if (normalizedApiUrl.endsWith("/api")) {
    return normalizedApiUrl.replace(/\/api$/, "/hubs/chat");
  }

  return `${normalizedApiUrl}/hubs/chat`;
};

export const APP_ENV = safeTrim(process.env.REACT_APP_ENV) || "development";
export const API_BASE_URL = normalizeApiUrl(process.env.REACT_APP_API_URL);
export const GOOGLE_CLIENT_ID = safeTrim(process.env.REACT_APP_GOOGLE_CLIENT_ID);
export const USE_MOCK_AUTH = safeTrim(process.env.REACT_APP_USE_MOCK_AUTH).toLowerCase() === "true";
export const SIGNALR_HUB_URL = safeTrim(process.env.REACT_APP_SIGNALR_HUB_URL) || deriveSignalRHubUrl(API_BASE_URL);

export const ENABLE_TOKEN_REFRESH = safeTrim(process.env.REACT_APP_ENABLE_TOKEN_REFRESH).toLowerCase() === "true";
export const TOKEN_REFRESH_ENDPOINT = safeTrim(process.env.REACT_APP_TOKEN_REFRESH_ENDPOINT) || "/auth/refresh-token";
