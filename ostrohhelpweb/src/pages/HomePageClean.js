import React, { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useLanguage, useSecurity } from "../App";
import { getUserConsultations } from "../services/chatApi";

export default function HomePageClean() {
  const navigate = useNavigate();
  const { t } = useLanguage();
  const { currentUser } = useSecurity();

  const [upcoming, setUpcoming] = useState([]);
  const [loadingUpcoming, setLoadingUpcoming] = useState(false);

  useEffect(() => {
    const load = async () => {
      if (!currentUser?.id) return;
      setLoadingUpcoming(true);
      try {
        const resp = await getUserConsultations(currentUser.id);
        const now = Date.now();

        // Normalize createdAt and treat future-dated consultations as "upcoming".
        const items = (resp || [])
          .map((c) => ({
            id: c?.id || c?.Id || c?.consultationId || c?.ConsultationId || null,
            title: c?.psychologistName || c?.psychologist || c?.psychologistName || "",
            time: c?.createdAt || c?.CreatedAt || null,
          }))
          .filter((it) => it.id)
          .map((it) => ({ ...it, timeValue: it.time ? new Date(it.time).getTime() : NaN }))
          .filter((it) => !Number.isNaN(it.timeValue) && it.timeValue >= now)
          .sort((a, b) => a.timeValue - b.timeValue)
          .slice(0, 3);

        setUpcoming(items);
      } catch (e) {
        setUpcoming([]);
      } finally {
        setLoadingUpcoming(false);
      }
    };

    load();
  }, [currentUser?.id]);

  return (
    <section className="bg-gradient-to-br from-white to-slate-50 dark:from-slate-900 dark:to-slate-800">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 items-center">
          <div>
            <h1 className="text-3xl sm:text-4xl font-extrabold text-slate-900 dark:text-white">{t("homePageBannerTitle")}</h1>
            <p className="mt-4 text-lg text-slate-700 dark:text-slate-300 max-w-xl">{t("homePageBannerDescription")}</p>

            <div className="mt-8 flex flex-col sm:flex-row gap-3">
              <button
                onClick={() => navigate('/questionnaires')}
                className="inline-flex items-center justify-center rounded-md bg-blue-600 hover:bg-blue-700 text-white px-5 py-3 text-sm font-medium shadow"
              >
                {t("homeHeroPrimaryCTA")}
              </button>

              <button
                onClick={() => navigate('/consultations')}
                className="inline-flex items-center justify-center rounded-md border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-200 px-5 py-3 text-sm font-medium shadow-sm"
              >
                {t("homeHeroSecondaryCTA")}
              </button>
            </div>
          </div>

          <div className="order-first lg:order-last">
            <div className="rounded-xl bg-gradient-to-br from-blue-50 to-white dark:from-slate-800 dark:to-slate-700 p-6 shadow-lg">
              <div className="h-56 sm:h-64 bg-[linear-gradient(135deg,#e6eefc_0%,#ffffff_60%)] dark:bg-[linear-gradient(135deg,#1f2937_0%,#374151_60%)] rounded-md flex items-center justify-center">
                <div className="text-center">
                  <p className="text-slate-700 dark:text-slate-200 font-medium">{t("startYourJourney")}</p>
                  <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{t("upcomingSessions")}</p>
                  <div className="mt-3">
                    {loadingUpcoming ? (
                      <p className="text-xs text-slate-400">Loading...</p>
                    ) : upcoming.length === 0 ? (
                      <p className="text-sm text-slate-500 dark:text-slate-400">{t("noUpcomingSessions")}</p>
                    ) : (
                      <ul className="text-left space-y-2">
                        {upcoming.map((it) => (
                          <li key={it.id} className="text-sm text-slate-700 dark:text-slate-200">
                            <div className="font-medium">{it.title || t("weeklyCheckup")}</div>
                            <div className="text-xs text-slate-500">{new Date(it.timeValue).toLocaleString()}</div>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
