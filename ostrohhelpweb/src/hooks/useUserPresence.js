import { useEffect, useMemo, useState } from "react";
import {
  getOnlineUserIdsSnapshot,
  startChatConnection,
  stopChatConnection,
  subscribeToUserStatusChanges,
} from "../services/signalrChat";

export default function useUserPresence(enabled = true) {
  const [onlineUserIds, setOnlineUserIds] = useState(() => getOnlineUserIdsSnapshot());

  useEffect(() => {
    if (!enabled) {
      setOnlineUserIds(new Set());
      return undefined;
    }

    let isActive = true;

    const unsubscribe = subscribeToUserStatusChanges(({ onlineUserIds: snapshot }) => {
      if (!isActive) {
        return;
      }

      setOnlineUserIds(new Set(snapshot));
    });

    startChatConnection().catch(() => {
      // The shared SignalR client already retries automatically; keep the UI state stable.
    });

    return () => {
      isActive = false;
      unsubscribe();
      stopChatConnection().catch(() => {
        // Ignore shutdown errors during navigation/logout.
      });
    };
  }, [enabled]);

  const isUserOnline = useMemo(() => {
    return (userId) => {
      if (!userId) {
        return false;
      }

      return onlineUserIds.has(String(userId).trim());
    };
  }, [onlineUserIds]);

  return {
    onlineUserIds,
    isUserOnline,
  };
}