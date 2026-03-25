import {
  API_BASE_URL,
  ENABLE_TOKEN_REFRESH,
  GOOGLE_CLIENT_ID,
  SIGNALR_HUB_URL,
  TOKEN_REFRESH_ENDPOINT,
  USE_MOCK_AUTH,
} from "./env";

const isValidHttpUrl = (value) => {
  try {
    const parsedUrl = new URL(value);
    return parsedUrl.protocol === "http:" || parsedUrl.protocol === "https:";
  } catch {
    return false;
  }
};

const isValidWsUrl = (value) => {
  try {
    const parsedUrl = new URL(value);
    return ["ws:", "wss:", "http:", "https:"].includes(parsedUrl.protocol);
  } catch {
    return false;
  }
};

export const validateEnv = () => {
  const validationErrors = [];

  if (!API_BASE_URL || !isValidHttpUrl(API_BASE_URL)) {
    validationErrors.push("REACT_APP_API_URL must be a valid http(s) URL.");
  }

  if (!SIGNALR_HUB_URL || !isValidWsUrl(SIGNALR_HUB_URL)) {
    validationErrors.push("REACT_APP_SIGNALR_HUB_URL must be a valid ws(s) or http(s) URL.");
  }

  if (!USE_MOCK_AUTH && !GOOGLE_CLIENT_ID) {
    validationErrors.push("REACT_APP_GOOGLE_CLIENT_ID is required when REACT_APP_USE_MOCK_AUTH=false.");
  }

  if (ENABLE_TOKEN_REFRESH && !TOKEN_REFRESH_ENDPOINT) {
    validationErrors.push("REACT_APP_TOKEN_REFRESH_ENDPOINT is required when REACT_APP_ENABLE_TOKEN_REFRESH=true.");
  }

  if (validationErrors.length > 0) {
    const message = ["Environment configuration error:", ...validationErrors].join("\n- ");
    throw new Error(message);
  }
};
