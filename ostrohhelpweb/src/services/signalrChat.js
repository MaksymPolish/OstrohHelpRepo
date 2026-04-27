import {
  HubConnectionBuilder,
  HubConnectionState,
  HttpTransportType,
  LogLevel,
} from "@microsoft/signalr";
import { SIGNALR_HUB_URL } from "../config/env";

let connection = null;
let connectionPromise = null;
const messageSubscribers = new Set();
const messageUpdatedSubscribers = new Set();
const keySubscribers = new Set();
const presenceSubscribers = new Set();
const onlineUserIds = new Set();

const normalizeHubUrlForNegotiation = (value) => {
  if (!value || typeof value !== "string") {
    return value;
  }

  if (value.startsWith("ws://")) {
    return value.replace("ws://", "http://");
  }

  if (value.startsWith("wss://")) {
    return value.replace("wss://", "https://");
  }

  return value;
};

const dispatchIncomingMessage = (payload) => {
  for (const subscriber of messageSubscribers) {
    try {
      subscriber(payload);
    } catch {
      // Ignore subscriber errors to keep SignalR stream alive.
    }
  }
};

const dispatchMessageUpdated = (payload) => {
  for (const subscriber of messageUpdatedSubscribers) {
    try {
      subscriber(payload);
    } catch {
      // Ignore subscriber errors to keep SignalR stream alive.
    }
  }
};

const dispatchConsultationKey = (payload) => {
  for (const subscriber of keySubscribers) {
    try {
      subscriber(payload);
    } catch {
      // Ignore subscriber errors to keep SignalR stream alive.
    }
  }
};

const dispatchUserStatusChanged = (userId, isOnline) => {
  const normalizedUserId = typeof userId === "string" || typeof userId === "number"
    ? String(userId).trim()
    : "";

  if (!normalizedUserId) {
    return;
  }

  if (isOnline) {
    onlineUserIds.add(normalizedUserId);
  } else {
    onlineUserIds.delete(normalizedUserId);
  }

  const snapshot = new Set(onlineUserIds);

  for (const subscriber of presenceSubscribers) {
    try {
      subscriber({ userId: normalizedUserId, isOnline: Boolean(isOnline), onlineUserIds: snapshot });
    } catch {
      // Ignore subscriber errors to keep SignalR stream alive.
    }
  }
};

const buildConnection = () => {
  const hubConnection = new HubConnectionBuilder()
    .withUrl(normalizeHubUrlForNegotiation(SIGNALR_HUB_URL), {
      accessTokenFactory: () => localStorage.getItem("authToken") || "",
      transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
      withCredentials: true,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.None)
    .build();

  hubConnection.on("ReceiveMessage", (message) => {
    dispatchIncomingMessage(message);
  });

  hubConnection.on("MessageUpdated", (message) => {
    dispatchMessageUpdated(message);
  });

  hubConnection.on("ReceiveConsultationKey", (payload) => {
    dispatchConsultationKey(payload);
  });

  hubConnection.on("UserStatusChanged", (userId, isOnline) => {
    dispatchUserStatusChanged(userId, isOnline);
  });

  hubConnection.on("Error", (errorPayload) => {
    dispatchIncomingMessage({
      __signalrError: true,
      payload: errorPayload,
    });
  });

  return hubConnection;
};

const ensureConnection = async () => {
  if (!connection) {
    connection = buildConnection();
  }

  if (connection.state === HubConnectionState.Connected) {
    return connection;
  }

  if (!connectionPromise) {
    connectionPromise = connection
      .start()
      .catch((error) => {
        throw error;
      })
      .finally(() => {
        connectionPromise = null;
      });
  }

  await connectionPromise;
  return connection;
};

export const subscribeToIncomingMessages = (handler) => {
  messageSubscribers.add(handler);
  return () => {
    messageSubscribers.delete(handler);
  };
};

export const subscribeToMessageUpdates = (handler) => {
  messageUpdatedSubscribers.add(handler);
  return () => {
    messageUpdatedSubscribers.delete(handler);
  };
};

export const subscribeToConsultationKeys = (handler) => {
  keySubscribers.add(handler);
  return () => {
    keySubscribers.delete(handler);
  };
};

export const subscribeToUserStatusChanges = (handler) => {
  presenceSubscribers.add(handler);
  return () => {
    presenceSubscribers.delete(handler);
  };
};

export const getOnlineUserIdsSnapshot = () => new Set(onlineUserIds);

export const getUserOnlineStatus = (userId) => {
  if (!userId) {
    return false;
  }

  return onlineUserIds.has(String(userId).trim());
};

export const startChatConnection = ensureConnection;

export const joinConsultationRoom = async (consultationId) => {
  if (!consultationId) {
    return;
  }

  const hub = await ensureConnection();
  await hub.invoke("JoinConsultation", String(consultationId));
};

export const leaveConsultationRoom = async (consultationId) => {
  if (!consultationId) {
    return;
  }

  const hub = await ensureConnection();
  await hub.invoke("LeaveConsultation", String(consultationId));
};

export const stopChatConnection = async () => {
  if (!connection) {
    return;
  }

  if (connection.state !== HubConnectionState.Disconnected) {
    await connection.stop();
  }

  connection = null;
  connectionPromise = null;
  onlineUserIds.clear();
};
