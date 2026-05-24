import React, { useCallback, useEffect, useMemo, useState } from "react";
import { CheckCircle2, RefreshCw, ShieldAlert, Trash2, UserRound, XCircle, Shield, FileText } from "lucide-react";
import Button from "../components/Common/Button";
import Card from "../components/Common/Card";
import { useLanguage, useSecurity } from "../App";
import { deleteQuestionnaire, getAllQuestionnaires, getQuestionaryStatuses, updateQuestionnaireStatus, acceptQuestionnaire } from "../services/questionnaireApi";
import { getAllUsers, updateUserRole } from "../services/userApi";
import { hasAdminPanelAccess, isHeadOfServiceUser } from "../utils/access";
import Modal from "../components/Common/Modal";

const pickFirst = (...values) => {
  for (const value of values) {
    if (value !== null && value !== undefined && value !== "") {
      return value;
    }
  }

  return null;
};

const normalizeQuestionnaire = (item) => {
  return {
    id: pickFirst(item?.id, item?.Id),
    userId: pickFirst(item?.userId, item?.UserId),
    userFullName: pickFirst(item?.userFullName, item?.UserFullName) || "",
    userEmail: pickFirst(item?.userEmail, item?.UserEmail) || "",
    description: pickFirst(item?.description, item?.Description) || "",
    isAnonymous: Boolean(pickFirst(item?.isAnonymous, item?.IsAnonymous, false)),
    statusName: pickFirst(item?.statusName, item?.StatusName, item?.status, item?.Status) || "Pending",
    statusId: pickFirst(item?.statusId, item?.StatusId),
    submittedAt: pickFirst(item?.submittedAt, item?.SubmittedAt, item?.createdAt, item?.CreatedAt),
  };
};

const normalizeStatus = (item) => ({
  id: pickFirst(item?.id, item?.Id),
  name: pickFirst(item?.name, item?.Name) || "",
});

const formatDateTime = (value, language) => {
  if (!value) {
    return "-";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "-";
  }

  return date.toLocaleString(language === "en" ? "en-US" : "uk-UA", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
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
    const firstError = Object.values(data.errors).flat().find((entry) => typeof entry === "string" && entry.trim());
    if (firstError) {
      return firstError;
    }
  }

  return "";
};

export default function AdminPanelPage() {
  const { t, language } = useLanguage();
  const { currentUser } = useSecurity();
  const [items, setItems] = useState([]);
  const [statuses, setStatuses] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [processingId, setProcessingId] = useState(null);
  const [error, setError] = useState("");
  const [infoMessage, setInfoMessage] = useState("");
  const [isScheduleModalOpen, setIsScheduleModalOpen] = useState(false);
  const [selectedQuestionnaireId, setSelectedQuestionnaireId] = useState(null);
  const [scheduledDate, setScheduledDate] = useState("");
  const [scheduledTime, setScheduledTime] = useState("");
  const [viewMode, setViewMode] = useState("questionnaires"); // "questionnaires" або "users"
  const [users, setUsers] = useState([]);
  const [isRoleModalOpen, setIsRoleModalOpen] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState(null);
  const [selectedUserName, setSelectedUserName] = useState("");
  const [selectedRole, setSelectedRole] = useState("");

  const canAccess = useMemo(() => hasAdminPanelAccess(currentUser), [currentUser]);
  const canDelete = useMemo(() => isHeadOfServiceUser(currentUser), [currentUser]);

  const rejectedStatusId = useMemo(() => {
    const status = statuses.find((entry) => String(entry.name || "").toLowerCase() === "rejected");
    return status?.id || "00000000-0000-0000-0000-000000000012";
  }, [statuses]);

  const isFinishedStatus = (statusId) => {
    // ID 00000000-0000-0000-0000-000000000011 (Accepted) і 00000000-0000-0000-0000-000000000012 (Rejected)
    return statusId === "00000000-0000-0000-0000-000000000011" || 
           statusId === "00000000-0000-0000-0000-000000000012";
  };

  const loadData = useCallback(async ({ silent = false } = {}) => {
    if (!silent) {
      setIsLoading(true);
    } else {
      setIsRefreshing(true);
    }

    setError("");
    setInfoMessage("");

    try {
      if (viewMode === "questionnaires") {
        const [statusResult, questionnaireResult] = await Promise.all([
          getQuestionaryStatuses(),
          getAllQuestionnaires(),
        ]);

        const normalizedStatuses = (statusResult || []).map(normalizeStatus);
        const normalizedItems = (questionnaireResult || []).map(normalizeQuestionnaire);

        normalizedItems.sort((left, right) => {
          const leftTime = left.submittedAt ? new Date(left.submittedAt).getTime() : 0;
          const rightTime = right.submittedAt ? new Date(right.submittedAt).getTime() : 0;
          return rightTime - leftTime;
        });

        setStatuses(normalizedStatuses);
        setItems(normalizedItems);

        if (normalizedItems.length === 0) {
          setInfoMessage(t("adminPanelEmpty"));
        }
      } else if (viewMode === "users") {
        const usersResult = await getAllUsers();
        const normalizedUsers = (usersResult || []).map((user) => ({
          id: user?.id || user?.Id,
          fullName: user?.fullName || user?.FullName || "",
          email: user?.email || user?.Email || "",
          role: user?.role || user?.Role || "Student",
        }));
        setUsers(normalizedUsers);

        if (normalizedUsers.length === 0) {
          setInfoMessage(t("adminPanelUsersEmpty"));
        }
      }
    } catch (loadError) {
      setError(extractServerErrorMessage(loadError) || t("adminPanelLoadError"));
    } finally {
      setIsLoading(false);
      setIsRefreshing(false);
    }
  }, [t, viewMode]);

  useEffect(() => {
    if (!canAccess) {
      setIsLoading(false);
      return;
    }

    loadData();
  }, [canAccess, loadData]);

  const handleAccept = async (questionnaireId) => {
    // Відкриваємо модаль для вибору часу
    setSelectedQuestionnaireId(questionnaireId);
    setIsScheduleModalOpen(true);
    setError("");
    setInfoMessage("");
    
    // Встановлюємо значення за замовчуванням - сьогодні + 1 година
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    setScheduledDate(tomorrow.toISOString().split("T")[0]);
    setScheduledTime("10:00");
  };

  const handleScheduleConfirm = async () => {
    if (!scheduledDate || !scheduledTime) {
      setError(t("adminPanelSelectDateTime") || "Виберіть дату та час");
      return;
    }

    const psychologistId = currentUser?.id || currentUser?.userId;
    if (!psychologistId) {
      setError(t("adminPanelPsychologistIdMissing") || "Не знайдено ID психолога");
      return;
    }

    setProcessingId(selectedQuestionnaireId);
    setError("");

    try {
      const dateTime = new Date(`${scheduledDate}T${scheduledTime}:00.000Z`);
      await acceptQuestionnaire({
        questionaryId: selectedQuestionnaireId,
        psychologistId,
        scheduledTime: dateTime.toISOString(),
      });
      setItems((currentItems) => currentItems.filter((item) => item.id !== selectedQuestionnaireId));
      setInfoMessage(t("adminPanelAcceptedSuccess"));
      setIsScheduleModalOpen(false);
    } catch (acceptError) {
      setError(extractServerErrorMessage(acceptError) || t("adminPanelAcceptError"));
    } finally {
      setProcessingId(null);
    }
  };

  const handleReject = async (questionnaireId) => {
    if (!rejectedStatusId) {
      setError(t("adminPanelStatusMissing"));
      return;
    }

    setProcessingId(questionnaireId);
    setError("");
    setInfoMessage("");

    try {
      await updateQuestionnaireStatus({ id: questionnaireId, statusId: rejectedStatusId });
      setItems((currentItems) => currentItems.filter((item) => item.id !== questionnaireId));
      setInfoMessage(t("adminPanelRejectedSuccess"));
    } catch (rejectError) {
      setError(extractServerErrorMessage(rejectError) || t("adminPanelRejectError"));
    } finally {
      setProcessingId(null);
    }
  };

  const handleDelete = async (questionnaireId) => {
    if (!canDelete) {
      setError(t("adminPanelDeleteRestricted"));
      return;
    }

    if (!window.confirm(t("adminPanelDeleteConfirm"))) {
      return;
    }

    setProcessingId(questionnaireId);
    setError("");
    setInfoMessage("");

    try {
      await deleteQuestionnaire(questionnaireId);
      setItems((currentItems) => currentItems.filter((item) => item.id !== questionnaireId));
      setInfoMessage(t("adminPanelDeletedSuccess"));
    } catch (deleteError) {
      setError(extractServerErrorMessage(deleteError) || t("adminPanelDeleteError"));
    } finally {
      setProcessingId(null);
    }
  };

  const handleOpenRoleModal = (userId, userName, currentRole) => {
    setSelectedUserId(userId);
    setSelectedUserName(userName);
    setSelectedRole(currentRole || "");
    setIsRoleModalOpen(true);
  };

  const handleUpdateRole = async () => {
    if (!selectedRole) {
      setError(t("adminPanelSelectRole"));
      return;
    }

    setProcessingId(selectedUserId);
    setError("");

    try {
      await updateUserRole({
        userId: selectedUserId,
        role: selectedRole,
      });
      setUsers((currentUsers) =>
        currentUsers.map((user) =>
          user.id === selectedUserId ? { ...user, role: selectedRole } : user
        )
      );
      setInfoMessage(t("adminPanelRoleUpdatedSuccess"));
      setIsRoleModalOpen(false);
    } catch (updateError) {
      setError(extractServerErrorMessage(updateError) || t("adminPanelRoleUpdateError"));
    } finally {
      setProcessingId(null);
    }
  };

  if (!canAccess) {
    return (
      <div className="max-w-3xl mx-auto">
        <Card className="p-8 border border-amber-200 bg-amber-50 dark:bg-amber-950/20 dark:border-amber-900/40">
          <div className="flex items-start gap-4">
            <div className="w-12 h-12 rounded-2xl bg-amber-100 dark:bg-amber-900/40 text-amber-700 dark:text-amber-300 flex items-center justify-center shrink-0">
              <ShieldAlert size={24} />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-slate-800 dark:text-white mb-2">{t("adminPanelTitle")}</h1>
              <p className="text-slate-600 dark:text-slate-300">{t("adminPanelAccessDenied")}</p>
            </div>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto animate-in slide-in-from-right-8 duration-300">
      <Modal
        isOpen={isScheduleModalOpen}
        onClose={() => setIsScheduleModalOpen(false)}
        title={t("adminPanelScheduleConsultation") || "Планування консультації"}
        actions={[
          <Button
            key="cancel"
            variant="outline"
            onClick={() => setIsScheduleModalOpen(false)}
          >
            {t("adminPanelCancel") || "Скасувати"}
          </Button>,
          <Button
            key="confirm"
            onClick={handleScheduleConfirm}
            disabled={processingId === selectedQuestionnaireId}
          >
            {processingId === selectedQuestionnaireId ? t("adminPanelProcessing") : t("adminPanelConfirm") || "Підтвердити"}
          </Button>,
        ]}
      >
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
              {t("adminPanelDate") || "Дата консультації"}
            </label>
            <input
              type="date"
              value={scheduledDate}
              onChange={(e) => setScheduledDate(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
              {t("adminPanelTime") || "Час консультації"}
            </label>
            <input
              type="time"
              value={scheduledTime}
              onChange={(e) => setScheduledTime(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white"
            />
          </div>
        </div>
      </Modal>

      <Modal
        isOpen={isRoleModalOpen}
        onClose={() => {
          setIsRoleModalOpen(false);
          setSelectedRole("");
        }}
        title={t("adminPanelChangeRole") || "Зміна ролі"}
        actions={[
          <Button
            key="cancel"
            variant="outline"
            onClick={() => {
              setIsRoleModalOpen(false);
              setSelectedRole("");
            }}
          >
            {t("adminPanelCancel") || "Скасувати"}
          </Button>,
          <Button
            key="confirm"
            onClick={handleUpdateRole}
            disabled={!selectedRole || processingId === selectedUserId}
          >
            {processingId === selectedUserId ? t("adminPanelProcessing") : t("adminPanelUpdate") || "Обновити"}
          </Button>,
        ]}
      >
        <div className="space-y-4">
          <div>
            <p className="text-sm text-slate-600 dark:text-slate-400 mb-3">
              {t("adminPanelUserInfo")}: <strong>{selectedUserName}</strong>
            </p>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
              {t("adminPanelNewRole") || "Нова роль"}
            </label>
            <select
              value={selectedRole}
              onChange={(e) => setSelectedRole(e.target.value)}
              className="w-full px-3 py-2 border border-slate-300 dark:border-slate-600 rounded-lg bg-white dark:bg-slate-700 text-slate-900 dark:text-white"
            >
              <option value="">-- {t("adminPanelSelectRole")} --</option>
              <option value="Student">{t("adminPanelRoleStudent")}</option>
              <option value="Psychologist">{t("adminPanelRolePsychologist")}</option>
              <option value="HeadOfService">{t("adminPanelRoleHeadOfService")}</option>
            </select>
          </div>
        </div>
      </Modal>
      <div className="flex flex-col gap-4 md:flex-row md:items-end md:justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-slate-800 dark:text-white mb-2">
            {viewMode === "questionnaires" ? t("adminPanelTitle") : t("adminPanelUsersTitle")}
          </h1>
          <p className="text-slate-500 dark:text-slate-400 max-w-2xl">
            {viewMode === "questionnaires" ? t("adminPanelDescription") : t("adminPanelUsersDescription")}
          </p>
        </div>

        <div className="flex flex-col gap-3 md:flex-row">
          {canDelete && (
            <Button
              variant={viewMode === "users" ? "secondary" : "outline"}
              onClick={() => {
                setViewMode("users");
                setError("");
                setInfoMessage("");
              }}
              className="md:self-start"
            >
              <UserRound size={16} />
              <span className="ml-2">{t("adminPanelViewUsers") || "Користувачі"}</span>
            </Button>
          )}

          <Button
            variant={viewMode === "questionnaires" ? "secondary" : "outline"}
            onClick={() => {
              setViewMode("questionnaires");
              setError("");
              setInfoMessage("");
            }}
            className="md:self-start"
          >
            <FileText size={16} />
            <span className="ml-2">{t("adminPanelViewQuestionnaires") || "Анкети"}</span>
          </Button>

          <Button
            variant="outline"
            onClick={() => loadData({ silent: true })}
            disabled={isRefreshing || isLoading}
            className="md:self-start"
          >
            <RefreshCw size={16} className={isRefreshing ? "animate-spin" : ""} />
            <span className="ml-2">{t("adminPanelRefresh")}</span>
          </Button>
        </div>
      </div>

      {error && (
        <div className="mb-4 rounded-xl border border-red-200 bg-red-50 text-red-700 px-4 py-3 text-sm">
          {error}
        </div>
      )}

      {infoMessage && (
        <div className="mb-4 rounded-xl border border-green-200 bg-green-50 text-green-700 px-4 py-3 text-sm">
          {infoMessage}
        </div>
      )}

      {isLoading ? (
        <p className="text-slate-500 dark:text-slate-400">{t("adminPanelLoading")}</p>
      ) : viewMode === "questionnaires" && items.length === 0 ? (
        <Card className="p-6">
          <p className="text-slate-600 dark:text-slate-300">{t("adminPanelEmpty")}</p>
        </Card>
      ) : viewMode === "users" && users.length === 0 ? (
        <Card className="p-6">
          <p className="text-slate-600 dark:text-slate-300">{t("adminPanelUsersEmpty")}</p>
        </Card>
      ) : viewMode === "questionnaires" ? (
        <div className="space-y-4">
          {items.map((item, index) => (
            <Card key={item.id || `admin-questionnaire-${index}`} className="p-5 md:p-6">
              <div className="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-4">
                <div className="space-y-3 flex-1">
                  <div className="flex flex-wrap items-center gap-3">
                    <div className="inline-flex items-center gap-2 rounded-full bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300 px-3 py-1 text-xs font-semibold">
                      <UserRound size={14} />
                      <span>{t("adminPanelUser")}:</span>
                      <span>{item.isAnonymous ? t("adminPanelAnonymous") : item.userFullName || t("adminPanelUnknownUser")}</span>
                    </div>
                    <span className="inline-flex items-center rounded-full bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300 px-3 py-1 text-xs font-semibold">
                      {item.statusName}
                    </span>
                  </div>

                  <div>
                    <p className="text-sm text-slate-500 dark:text-slate-400">
                      {t("adminPanelSubmittedAt")}: {formatDateTime(item.submittedAt, language)}
                    </p>
                    {!item.isAnonymous && item.userEmail && (
                      <p className="text-sm text-slate-500 dark:text-slate-400">{item.userEmail}</p>
                    )}
                  </div>

                  <div>
                    <h2 className="text-lg font-semibold text-slate-800 dark:text-white mb-2">
                      {t("adminPanelRequestDescription")}
                    </h2>
                    <p className="whitespace-pre-wrap text-slate-700 dark:text-slate-200 leading-6">
                      {item.description || t("adminPanelNoDescription")}
                    </p>
                  </div>
                </div>

                <div className="flex flex-col gap-2 shrink-0 lg:w-44">
                  {!isFinishedStatus(item.statusId) && (
                    <Button
                      variant="secondary"
                      onClick={() => handleAccept(item.id)}
                      disabled={processingId === item.id}
                      fullWidth
                    >
                      <CheckCircle2 size={16} />
                      <span className="ml-2">
                        {processingId === item.id ? t("adminPanelProcessing") : t("adminPanelAccept")}
                      </span>
                    </Button>
                  )}

                  {!isFinishedStatus(item.statusId) && (
                    <Button
                      variant="outline"
                      onClick={() => handleReject(item.id)}
                      disabled={processingId === item.id}
                      fullWidth
                    >
                      <XCircle size={16} />
                      <span className="ml-2">
                        {processingId === item.id ? t("adminPanelProcessing") : t("adminPanelReject")}
                      </span>
                    </Button>
                  )}

                  {canDelete && (
                    <Button
                      variant="danger"
                      onClick={() => handleDelete(item.id)}
                      disabled={processingId === item.id}
                      fullWidth
                    >
                      <Trash2 size={16} />
                      <span className="ml-2">{t("adminPanelDelete")}</span>
                    </Button>
                  )}
                </div>
              </div>
            </Card>
          ))}
        </div>
      ) : (
        <div className="space-y-4">
          {users.map((user, index) => (
            <Card key={user.id || `admin-user-${index}`} className="p-5 md:p-6">
              <div className="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-4">
                <div className="space-y-3 flex-1">
                  <div className="flex flex-wrap items-center gap-3">
                    <div className="inline-flex items-center gap-2 rounded-full bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300 px-3 py-1 text-xs font-semibold">
                      <Shield size={14} />
                      <span>{user.role}</span>
                    </div>
                  </div>

                  <div>
                    <h3 className="text-lg font-semibold text-slate-800 dark:text-white">
                      {user.fullName || t("adminPanelUnknownUser")}
                    </h3>
                    <p className="text-sm text-slate-500 dark:text-slate-400">{user.email}</p>
                  </div>
                </div>

                <div className="flex flex-col gap-2 shrink-0 lg:w-44">
                  <Button
                    variant="outline"
                    onClick={() => handleOpenRoleModal(user.id, user.fullName, user.role)}
                    disabled={processingId === user.id}
                    fullWidth
                  >
                    <Shield size={16} />
                    <span className="ml-2">{t("adminPanelChangeRole") || "Змінити роль"}</span>
                  </Button>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
