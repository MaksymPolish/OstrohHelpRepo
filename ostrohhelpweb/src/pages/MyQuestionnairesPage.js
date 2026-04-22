import React, { useEffect, useMemo, useState } from "react";
import Card from "../components/Common/Card";
import { useLanguage, useSecurity } from "../App";
import { getUserQuestionnaires } from "../services/questionnaireApi";

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
    description: pickFirst(item?.description, item?.Description) || "",
    statusName: pickFirst(item?.statusName, item?.StatusName, item?.status, item?.Status) || "Pending",
    submittedAt: pickFirst(item?.submittedAt, item?.SubmittedAt, item?.createdAt, item?.CreatedAt),
    isAnonymous: Boolean(pickFirst(item?.isAnonymous, item?.IsAnonymous, false)),
  };
};

const formatDateTime = (value, locale) => {
  if (!value) {
    return "-";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "-";
  }

  return date.toLocaleString(locale === "en" ? "en-US" : "uk-UA");
};

export default function MyQuestionnairesPage() {
  const { t, language } = useLanguage();
  const { currentUser } = useSecurity();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [items, setItems] = useState([]);

  const userId = useMemo(() => currentUser?.id || currentUser?.Id || null, [currentUser]);

  useEffect(() => {
    const load = async () => {
      if (!userId) {
        setItems([]);
        return;
      }

      setIsLoading(true);
      setError("");
      try {
        const response = await getUserQuestionnaires(userId);
        const normalized = (response || []).map(normalizeQuestionnaire);
        normalized.sort((a, b) => {
          const left = a.submittedAt ? new Date(a.submittedAt).getTime() : 0;
          const right = b.submittedAt ? new Date(b.submittedAt).getTime() : 0;
          return right - left;
        });
        setItems(normalized);
      } catch {
        setError(t("myQuestionnairesLoadError"));
      } finally {
        setIsLoading(false);
      }
    };

    load();
  }, [userId, t]);

  return (
    <div className="max-w-4xl mx-auto animate-in slide-in-from-right-8 duration-300">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-slate-800 dark:text-white mb-2">{t("myQuestionnairesTitle")}</h1>
        <p className="text-slate-500">{t("myQuestionnairesDescription")}</p>
      </div>

      {error && (
        <div className="mb-4 rounded-xl border border-red-200 bg-red-50 text-red-700 px-4 py-3 text-sm">
          {error}
        </div>
      )}

      {isLoading && <p className="text-slate-500">{t("myQuestionnairesLoading")}</p>}

      {!isLoading && items.length === 0 && (
        <Card className="p-6">
          <p className="text-slate-600 dark:text-slate-300">{t("myQuestionnairesEmpty")}</p>
        </Card>
      )}

      <div className="space-y-4">
        {items.map((item, index) => (
          <Card key={item.id || `questionnaire-${index}`} className="p-6">
            <div className="flex items-start justify-between gap-4 mb-3">
              <div>
                <h3 className="text-lg font-semibold text-slate-800 dark:text-white">{t("myQuestionnairesCardTitle")} #{index + 1}</h3>
                <p className="text-xs text-slate-500">{t("myQuestionnairesSubmittedAt")}: {formatDateTime(item.submittedAt, language)}</p>
              </div>
              <span className="inline-flex items-center rounded-full bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300 px-3 py-1 text-xs font-semibold">
                {item.statusName}
              </span>
            </div>

            <p className="text-slate-700 dark:text-slate-200 whitespace-pre-wrap">{item.description || t("myQuestionnairesDescriptionMissing")}</p>

            {item.isAnonymous && (
              <p className="mt-3 text-xs text-slate-500">{t("myQuestionnairesSubmittedAnonymously")}</p>
            )}
          </Card>
        ))}
      </div>
    </div>
  );
}
