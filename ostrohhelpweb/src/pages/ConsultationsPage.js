import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Paperclip, Send, Activity } from "lucide-react";
import Button from "../components/Common/Button";
import { useLanguage, useSecurity } from "../App";
import {
  getConsultationMessages,
  getUserConsultations,
  sendConsultationMessage,
  uploadMessageFile,
} from "../services/chatApi";
import {
  joinConsultationRoom,
  leaveConsultationRoom,
  subscribeToConsultationKeys,
  subscribeToIncomingMessages,
} from "../services/signalrChat";
import { decryptMessage, encryptMessage } from "../services/encryptionService";

const readFirstDefined = (...values) => {
  for (const value of values) {
    if (value !== null && value !== undefined) {
      return value;
    }
  }
  return null;
};

const normalizeId = (value) => {
  if (value === null || value === undefined) {
    return null;
  }

  if (typeof value === "string" || typeof value === "number") {
    const normalized = String(value).trim();
    return normalized || null;
  }

  if (typeof value === "object") {
    const nested = readFirstDefined(
      value.value,
      value.id,
      value.Id,
      value.guid,
      value.Guid,
      value.userId,
      value.UserId
    );

    if (nested !== null && nested !== undefined && nested !== value) {
      return normalizeId(nested);
    }
  }

  return null;
};

const idsEqual = (left, right) => {
  const leftId = normalizeId(left);
  const rightId = normalizeId(right);

  if (!leftId || !rightId) {
    return false;
  }

  return leftId.toLowerCase() === rightId.toLowerCase();
};

const extractServerErrorMessage = (error) => {
  const data = error?.response?.data;
  if (!data || typeof data !== "object") {
    return "";
  }

  if (typeof data.message === "string" && data.message.trim()) {
    return data.message;
  }

  if (typeof data.error === "string" && data.error.trim()) {
    return data.error;
  }

  if (typeof data.title === "string" && data.title.trim()) {
    return data.title;
  }

  if (data.errors && typeof data.errors === "object") {
    const flatErrors = Object.values(data.errors).flatMap((entry) =>
      Array.isArray(entry) ? entry : [entry]
    );

    const firstText = flatErrors.find((entry) => typeof entry === "string" && entry.trim());
    if (firstText) {
      return firstText;
    }
  }

  return "";
};

const normalizeConsultation = (item) => {
  return {
    id: normalizeId(readFirstDefined(item?.id, item?.Id, item?.consultationId, item?.ConsultationId)),
    studentId: normalizeId(readFirstDefined(item?.studentId, item?.StudentId)),
    studentName: readFirstDefined(item?.studentName, item?.StudentName) || "",
    studentPhotoUrl: readFirstDefined(item?.studentPhotoUrl, item?.StudentPhotoUrl) || null,
    psychologistId: normalizeId(readFirstDefined(item?.psychologistId, item?.PsychologistId)),
    psychologistName: readFirstDefined(item?.psychologistName, item?.PsychologistName) || "",
    psychologistPhotoUrl: readFirstDefined(item?.psychologistPhotoUrl, item?.PsychologistPhotoUrl) || null,
    statusName: readFirstDefined(item?.statusName, item?.StatusName) || "",
    createdAt: readFirstDefined(item?.createdAt, item?.CreatedAt) || null,
  };
};

const isImageUrl = (url) => {
  if (!url || typeof url !== "string") {
    return false;
  }

  return /(\.png|\.jpg|\.jpeg|\.gif|\.webp|\.bmp|\.svg)(\?|$)/i.test(url);
};

const normalizeMessage = (item) => {
  const attachments = [];

  if (Array.isArray(item?.mediaPaths)) {
    for (const mediaItem of item.mediaPaths) {
      if (typeof mediaItem === "string") {
        attachments.push(mediaItem);
      } else if (mediaItem?.url || mediaItem?.Url || mediaItem?.fileUrl || mediaItem?.FileUrl) {
        attachments.push(mediaItem.url || mediaItem.Url || mediaItem.fileUrl || mediaItem.FileUrl);
      }
    }
  }

  if (Array.isArray(item?.MediaPaths)) {
    for (const mediaItem of item.MediaPaths) {
      if (typeof mediaItem === "string") {
        attachments.push(mediaItem);
      } else if (mediaItem?.url || mediaItem?.Url || mediaItem?.fileUrl || mediaItem?.FileUrl) {
        attachments.push(mediaItem.url || mediaItem.Url || mediaItem.fileUrl || mediaItem.FileUrl);
      }
    }
  }

  if (Array.isArray(item?.attachments)) {
    for (const attachment of item.attachments) {
      if (typeof attachment === "string") {
        attachments.push(attachment);
      } else if (attachment?.fileUrl) {
        attachments.push(attachment.fileUrl);
      }
    }
  }

  if (item?.fileUrl) {
    attachments.push(item.fileUrl);
  }

  if (item?.FileUrl) {
    attachments.push(item.FileUrl);
  }

  const fallbackMessageId = [
    readFirstDefined(item?.createdAt, item?.CreatedAt, Date.now()),
    readFirstDefined(item?.senderId, item?.SenderId, "sender"),
    readFirstDefined(item?.content, item?.Content, "message"),
  ].join("-");

  return {
    id: normalizeId(readFirstDefined(item?.id, item?.Id)) || fallbackMessageId,
    senderId: normalizeId(readFirstDefined(item?.senderId, item?.SenderId)),
    consultationId: normalizeId(readFirstDefined(item?.consultationId, item?.ConsultationId)),
    content: readFirstDefined(item?.content, item?.Content, item?.text, item?.Text, item?.message, item?.Message) || "",
    encryptedContent: readFirstDefined(item?.encryptedContent, item?.EncryptedContent) || null,
    iv: readFirstDefined(item?.iv, item?.Iv) || null,
    authTag: readFirstDefined(item?.authTag, item?.AuthTag) || null,
    createdAt: readFirstDefined(item?.sentAt, item?.SentAt, item?.createdAt, item?.CreatedAt) || null,
    attachments: [...new Set(attachments.filter((entry) => typeof entry === "string" && entry.trim()))],
  };
};

const sortMessagesOldToNew = (items) => {
  return [...items].sort((left, right) => {
    const leftTime = left?.createdAt ? new Date(left.createdAt).getTime() : Number.NaN;
    const rightTime = right?.createdAt ? new Date(right.createdAt).getTime() : Number.NaN;

    const safeLeftTime = Number.isNaN(leftTime) ? 0 : leftTime;
    const safeRightTime = Number.isNaN(rightTime) ? 0 : rightTime;

    if (safeLeftTime !== safeRightTime) {
      return safeLeftTime - safeRightTime;
    }

    return String(left?.id || "").localeCompare(String(right?.id || ""));
  });
};

export default function ConsultationsPage() {
  const { t } = useLanguage();
  const { currentUser } = useSecurity();
  const [msg, setMsg] = useState("");
  const [consultations, setConsultations] = useState([]);
  const [selectedConsultationId, setSelectedConsultationId] = useState(null);
  const [messages, setMessages] = useState([]);
  const [decryptedMessages, setDecryptedMessages] = useState([]);
  const [pendingFiles, setPendingFiles] = useState([]);
  const [isLoadingConsultations, setIsLoadingConsultations] = useState(false);
  const [isLoadingMessages, setIsLoadingMessages] = useState(false);
  const [isSending, setIsSending] = useState(false);
  const [isDecrypting, setIsDecrypting] = useState(false);
  const [keyVersion, setKeyVersion] = useState(0);
  const [error, setError] = useState("");
  const fileInputRef = useRef(null);
  const messageInputRef = useRef(null);
  const consultationKeysRef = useRef({});

  const normalizedCurrentUserId = useMemo(() => normalizeId(currentUser?.id), [currentUser?.id]);

  const selectedConsultation = useMemo(
    () => consultations.find((item) => item.id === selectedConsultationId) || null,
    [consultations, selectedConsultationId]
  );

  const resolvePeer = (consultation) => {
    if (!consultation || !normalizedCurrentUserId) {
      return { id: null, name: "", photoUrl: null };
    }

    const isCurrentUserPsychologist = idsEqual(consultation.psychologistId, normalizedCurrentUserId);

    if (isCurrentUserPsychologist) {
      return {
        id: consultation.studentId || null,
        name: consultation.studentName || "User",
        photoUrl: consultation.studentPhotoUrl || null,
      };
    }

    return {
      id: consultation.psychologistId || null,
      name: consultation.psychologistName || "User",
      photoUrl: consultation.psychologistPhotoUrl || null,
    };
  };

  const formatMessageTime = (value) => {
    if (!value) {
      return "";
    }

    const parsedDate = new Date(value);
    if (Number.isNaN(parsedDate.getTime())) {
      return "";
    }

    return parsedDate.toLocaleTimeString([], {
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const formatMessageDate = (value) => {
    if (!value) {
      return "";
    }

    const parsedDate = new Date(value);
    if (Number.isNaN(parsedDate.getTime())) {
      return "";
    }

    return parsedDate.toLocaleDateString([], {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });
  };

  const getConsultationKey = useCallback((consultationId) => {
    const normalizedId = normalizeId(consultationId);
    if (!normalizedId) {
      return null;
    }

    return consultationKeysRef.current[normalizedId] || null;
  }, []);

  const decryptMessages = useCallback(async (sourceMessages) => {
    const nextMessages = await Promise.all(
      sourceMessages.map(async (messageItem) => {
        const hasPlainContent = typeof messageItem.content === "string" && messageItem.content.trim().length > 0;
        const hasEncryptedPayload =
          Boolean(messageItem.encryptedContent) &&
          Boolean(messageItem.iv) &&
          Boolean(messageItem.authTag);

        if (hasPlainContent || !hasEncryptedPayload) {
          return messageItem;
        }

        const secretKey = getConsultationKey(messageItem.consultationId);
        if (!secretKey) {
          return messageItem;
        }

        try {
          const plaintext = await decryptMessage({
            encryptedContent: messageItem.encryptedContent,
            iv: messageItem.iv,
            authTag: messageItem.authTag,
            secretKey,
          });

          return {
            ...messageItem,
            content: plaintext,
          };
        } catch {
          return {
            ...messageItem,
            content: "[Encrypted message could not be decrypted]",
          };
        }
      })
    );

    return sortMessagesOldToNew(nextMessages);
  }, [getConsultationKey]);


  const appendUniqueMessage = (nextMessage) => {
    setMessages((prevMessages) => {
      if (!nextMessage?.id) {
        return sortMessagesOldToNew([...prevMessages, nextMessage]);
      }

      const exists = prevMessages.some((messageItem) => messageItem.id === nextMessage.id);
      if (exists) {
        return prevMessages;
      }

      return sortMessagesOldToNew([...prevMessages, nextMessage]);
    });
  };

  useEffect(() => {
    const loadConsultations = async () => {
      if (!currentUser?.id) {
        return;
      }

      setIsLoadingConsultations(true);
      setError("");
      try {
        const response = await getUserConsultations(currentUser.id);
        const normalizedConsultations = (response || [])
          .map(normalizeConsultation)
          .filter((item) => item.id);

        setConsultations(normalizedConsultations);
        if (normalizedConsultations.length > 0) {
          setSelectedConsultationId((prevId) => prevId || normalizedConsultations[0].id);
        }
      } catch {
        setError("Не вдалося завантажити список чатів.");
      } finally {
        setIsLoadingConsultations(false);
      }
    };

    loadConsultations();
  }, [currentUser?.id]);

  useEffect(() => {
    const loadMessages = async () => {
      if (!selectedConsultationId) {
        setMessages([]);
        setDecryptedMessages([]);
        return;
      }

      setIsLoadingMessages(true);
      setError("");
      try {
        const response = await getConsultationMessages(selectedConsultationId);
        setMessages(sortMessagesOldToNew((response || []).map(normalizeMessage)));
      } catch {
        setError("Не вдалося завантажити повідомлення.");
      } finally {
        setIsLoadingMessages(false);
      }
    };

    loadMessages();
  }, [selectedConsultationId]);

  useEffect(() => {
    const unsubscribe = subscribeToConsultationKeys((payload) => {
      const consultationId = normalizeId(
        readFirstDefined(payload?.consultationId, payload?.ConsultationId)
      );
      const key = readFirstDefined(payload?.key, payload?.Key);

      if (!consultationId || !key) {
        return;
      }

      consultationKeysRef.current = {
        ...consultationKeysRef.current,
        [consultationId]: key,
      };
      setKeyVersion((prev) => prev + 1);
    });

    return () => {
      unsubscribe();
    };
  }, []);

  useEffect(() => {
    let isMounted = true;

    const syncDecryptedMessages = async () => {
      if (messages.length === 0) {
        setDecryptedMessages([]);
        return;
      }

      setIsDecrypting(true);
      try {
        const resolvedMessages = await decryptMessages(messages);
        if (isMounted) {
          setDecryptedMessages(resolvedMessages);
        }
      } finally {
        if (isMounted) {
          setIsDecrypting(false);
        }
      }
    };

    syncDecryptedMessages();

    return () => {
      isMounted = false;
    };
  }, [messages, keyVersion, decryptMessages]);

  useEffect(() => {
    if (!selectedConsultationId) {
      return undefined;
    }

    const handleIncomingMessage = (incomingPayload) => {
      if (!incomingPayload || incomingPayload.__signalrError) {
        return;
      }

      const normalizedIncoming = normalizeMessage(incomingPayload);
      if (!normalizedIncoming) {
        return;
      }

      const incomingConsultationId = normalizeId(
        incomingPayload.consultationId ||
        incomingPayload.ConsultationId ||
        normalizedIncoming.consultationId ||
        null
      );

      if (String(incomingConsultationId || "") !== String(selectedConsultationId)) {
        return;
      }

      appendUniqueMessage(normalizedIncoming);
    };

    const unsubscribe = subscribeToIncomingMessages(handleIncomingMessage);

    joinConsultationRoom(selectedConsultationId).catch(() => {
      // Realtime is optional; API polling remains available.
    });

    return () => {
      unsubscribe();
      leaveConsultationRoom(selectedConsultationId).catch(() => {
        // Ignore leave errors during navigation between chats.
      });
    };
  }, [selectedConsultationId]);

  const handlePickFiles = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (event) => {
    const selectedFiles = Array.from(event.target.files || []);
    if (selectedFiles.length === 0) {
      return;
    }
    setPendingFiles((prev) => [...prev, ...selectedFiles]);
    event.target.value = "";
  };

  const removePendingFile = (fileIndex) => {
    setPendingFiles((prev) => prev.filter((_, index) => index !== fileIndex));
  };

  useEffect(() => {
    if (!messageInputRef.current) {
      return;
    }

    messageInputRef.current.style.height = "auto";
    messageInputRef.current.style.height = `${Math.min(messageInputRef.current.scrollHeight, 160)}px`;
  }, [msg]);

  const handleMessageKeyDown = (event) => {
    if (event.key !== "Enter" || event.shiftKey) {
      return;
    }

    event.preventDefault();

    if (!canSend || !selectedConsultation) {
      return;
    }

    handleSend();
  };

  const handleSend = async () => {
    const text = msg.trim();
    if ((!text && pendingFiles.length === 0) || !selectedConsultation || !normalizedCurrentUserId) {
      return;
    }

    const consultationKey = getConsultationKey(selectedConsultation.id);
    if (!consultationKey) {
      setError("Ключ шифрування ще не отримано. Спробуйте через кілька секунд.");
      return;
    }

    const peer = resolvePeer(selectedConsultation);
    const participantIds = [selectedConsultation.studentId, selectedConsultation.psychologistId]
      .map((entry) => normalizeId(entry))
      .filter(Boolean);

    const alternateReceivers = participantIds.filter((entry) => !idsEqual(entry, normalizedCurrentUserId));

    const receiverCandidates = [normalizeId(peer.id), ...alternateReceivers].filter(
      (entry, index, source) => entry && source.indexOf(entry) === index
    );

    if (receiverCandidates.length === 0) {
      setError("Не вдалося визначити отримувача.");
      return;
    }

    setIsSending(true);
    setError("");
    try {
      const uploadedPaths = await Promise.all(
        pendingFiles.map(async (file) => {
          const uploadedFile = await uploadMessageFile(currentUser.id, file);
          return uploadedFile?.url || null;
        })
      );

      const mediaPaths = uploadedPaths.filter(Boolean);
      const encryptedPayload = await encryptMessage(text || "Файл", consultationKey);

      let sent = false;
      let lastSendError = null;

      for (const receiverId of receiverCandidates) {
        try {
          await sendConsultationMessage({
            senderId: normalizedCurrentUserId,
            receiverId,
            consultationId: normalizeId(selectedConsultation.id),
            encryptedContent: encryptedPayload.encryptedContent,
            iv: encryptedPayload.iv,
            authTag: encryptedPayload.authTag,
            mediaPaths,
          });
          sent = true;
          break;
        } catch (sendError) {
          lastSendError = sendError;
        }
      }

      if (!sent) {
        throw lastSendError || new Error("Failed to send message");
      }

      const refreshedMessages = await getConsultationMessages(selectedConsultation.id);
      setMessages(sortMessagesOldToNew((refreshedMessages || []).map(normalizeMessage)));
      setMsg("");
      setPendingFiles([]);
    } catch (sendError) {
      const serverMessage = extractServerErrorMessage(sendError);
      setError(serverMessage || "Не вдалося відправити повідомлення.");
    } finally {
      setIsSending(false);
    }
  };

  const selectedPeer = resolvePeer(selectedConsultation);
  const canSend = !isSending && (msg.trim().length > 0 || pendingFiles.length > 0);

  return (
    <div className="h-[calc(100vh-8rem)] flex bg-white dark:bg-slate-800 rounded-2xl shadow-sm border border-slate-100 dark:border-slate-700 overflow-hidden animate-in fade-in zoom-in-95 duration-300">
      {/* Sidebar - Contacts */}
      <div className="w-80 border-r border-slate-100 dark:border-slate-700 hidden md:flex flex-col bg-slate-50/50 dark:bg-slate-800/50">
        <div className="p-4 border-b border-slate-100 dark:border-slate-700">
          <h2 className="font-bold text-lg text-slate-800 dark:text-white">{t("messages")}</h2>
        </div>
        <div className="overflow-y-auto flex-1 p-2 space-y-1">
          {isLoadingConsultations && (
            <p className="px-3 py-2 text-sm text-slate-500">Завантаження чатів...</p>
          )}

          {!isLoadingConsultations && consultations.length === 0 && (
            <p className="px-3 py-2 text-sm text-slate-500">У вас поки немає активних чатів.</p>
          )}

          {consultations.map((consultation) => {
            const peer = resolvePeer(consultation);
            const isActive = consultation.id === selectedConsultationId;
            const initials = (peer.name || "U")
              .split(" ")
              .filter(Boolean)
              .slice(0, 2)
              .map((part) => part.charAt(0).toUpperCase())
              .join("");

            return (
              <button
                key={consultation.id}
                type="button"
                onClick={() => setSelectedConsultationId(consultation.id)}
                className={`w-full text-left flex items-center p-3 rounded-xl transition-colors ${
                  isActive
                    ? "bg-blue-50 dark:bg-blue-900/20"
                    : "hover:bg-slate-100 dark:hover:bg-slate-700/50"
                }`}
              >
                <div className="relative mr-3">
                  {peer.photoUrl ? (
                    <img
                      src={peer.photoUrl}
                      alt={peer.name}
                      className="w-10 h-10 rounded-full object-cover"
                      referrerPolicy="no-referrer"
                    />
                  ) : (
                    <div className="w-10 h-10 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center font-bold">
                      {initials || "U"}
                    </div>
                  )}
                </div>
                <div className="flex-1 overflow-hidden">
                  <div className="flex justify-between items-center mb-0.5">
                    <h4 className="font-medium text-slate-900 dark:text-white truncate">{peer.name}</h4>
                    <span className="text-xs text-slate-500">{formatMessageTime(consultation.createdAt)}</span>
                  </div>
                  <p className="text-sm text-slate-500 truncate">{consultation.statusName || "Активний чат"}</p>
                </div>
              </button>
            );
          })}
        </div>
      </div>

      {/* Main Chat Area */}
      <div className="flex-1 flex flex-col">
        {/* Chat Header */}
        <div className="p-4 border-b border-slate-100 dark:border-slate-700 flex justify-between items-center bg-white dark:bg-slate-800 z-10">
          <div className="flex items-center">
            {selectedPeer.photoUrl ? (
              <img
                src={selectedPeer.photoUrl}
                alt={selectedPeer.name}
                className="w-10 h-10 rounded-full object-cover mr-3"
                referrerPolicy="no-referrer"
              />
            ) : (
              <div className="w-10 h-10 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center font-bold mr-3 md:hidden">
                {(selectedPeer.name || "U").charAt(0).toUpperCase()}
              </div>
            )}
            <div>
              <h3 className="font-bold text-slate-800 dark:text-white">{selectedPeer.name || "Оберіть чат"}</h3>
              <p className="text-xs text-emerald-500 font-medium">{selectedConsultation ? t("online") : ""}</p>
            </div>
          </div>
          <Button variant="ghost" className="p-2 rounded-full">
            <Activity size={20} />
          </Button>
        </div>

        {/* Messages */}
        <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-6 bg-slate-50 dark:bg-slate-900/50">
          {error && (
            <div className="bg-red-100 dark:bg-red-900/30 border border-red-200 dark:border-red-700 text-red-700 dark:text-red-300 px-4 py-3 rounded-xl">
              {error}
            </div>
          )}

          {isLoadingMessages && <p className="text-sm text-slate-500">Завантаження повідомлень...</p>}

          {!isLoadingMessages && selectedConsultation && decryptedMessages.length === 0 && (
            <p className="text-sm text-slate-500">Поки що немає повідомлень у цьому чаті.</p>
          )}

          {isDecrypting && <p className="text-sm text-slate-500">Розшифровка повідомлень...</p>}

          {!selectedConsultation && (
            <p className="text-sm text-slate-500">Оберіть чат із лівої панелі, щоб почати спілкування.</p>
          )}

          {decryptedMessages.map((m, index) => {
            const isMine = idsEqual(m.senderId, normalizedCurrentUserId);

            return (
              <div key={`${m.id}-${index}`} className={`flex ${isMine ? "justify-end" : "justify-start"}`}>
                <div
                  className={`max-w-[75%] rounded-2xl px-5 py-3 ${
                    isMine
                      ? "bg-blue-600 text-white rounded-br-sm shadow-sm"
                      : "bg-white dark:bg-slate-800 text-slate-800 dark:text-slate-200 border border-slate-100 dark:border-slate-700 rounded-bl-sm shadow-sm"
                  }`}
                >
                  {m.content && <p>{m.content}</p>}

                  {Array.isArray(m.attachments) && m.attachments.length > 0 && (
                    <div className="mt-2 space-y-2">
                      {m.attachments.map((attachmentUrl, attachmentIndex) => (
                        <div key={`${attachmentUrl}-${attachmentIndex}`}>
                          {isImageUrl(attachmentUrl) ? (
                            <a href={attachmentUrl} target="_blank" rel="noreferrer">
                              <img
                                src={attachmentUrl}
                                alt="attachment"
                                className="max-h-48 rounded-lg object-cover"
                                referrerPolicy="no-referrer"
                              />
                            </a>
                          ) : (
                            <a
                              href={attachmentUrl}
                              target="_blank"
                              rel="noreferrer"
                              className={`underline break-all ${isMine ? "text-blue-100" : "text-blue-600 dark:text-blue-400"}`}
                            >
                              {attachmentUrl}
                            </a>
                          )}
                        </div>
                      ))}
                    </div>
                  )}

                  <div className={`text-[10px] mt-1 ${isMine ? "text-blue-100 text-right" : "text-slate-400"}`}>
                    <span className="block">{formatMessageTime(m.createdAt)}</span>
                    <span className="block">{formatMessageDate(m.createdAt)}</span>
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        {/* Input Area */}
        <div className="p-4 bg-white dark:bg-slate-800 border-t border-slate-100 dark:border-slate-700">
          {pendingFiles.length > 0 && (
            <div className="mb-3 flex flex-wrap gap-2">
              {pendingFiles.map((file, index) => (
                <button
                  key={`${file.name}-${index}`}
                  type="button"
                  onClick={() => removePendingFile(index)}
                  className="text-xs px-3 py-1 rounded-full bg-slate-100 dark:bg-slate-700 text-slate-700 dark:text-slate-200"
                >
                  {file.name} x
                </button>
              ))}
            </div>
          )}

          <div className="flex items-center space-x-2">
            <input
              ref={fileInputRef}
              type="file"
              multiple
              className="hidden"
              onChange={handleFileChange}
            />

            <button
              type="button"
              onClick={handlePickFiles}
              className="p-2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors"
            >
              <Paperclip size={20} />
            </button>

            <textarea
              ref={messageInputRef}
              value={msg}
              onChange={(e) => setMsg(e.target.value)}
              onKeyDown={handleMessageKeyDown}
              placeholder={t("messagePlaceholder")}
              disabled={!selectedConsultation || isSending}
              rows={1}
              className="flex-1 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-700 rounded-2xl px-5 py-3 focus:outline-none focus:ring-2 focus:ring-blue-500 dark:text-white resize-none overflow-y-auto min-h-12 max-h-40 leading-6"
            />

            <Button
              className="rounded-full w-11 h-11 p-0 flex items-center justify-center"
              onClick={handleSend}
              disabled={!selectedConsultation || !canSend}
            >
              <Send size={18} className="ml-1" />
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}