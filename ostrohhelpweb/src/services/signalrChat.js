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
};
