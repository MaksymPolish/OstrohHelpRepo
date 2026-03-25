import React from "react";
import { Link } from "react-router-dom";
import { Activity } from "lucide-react";
import Button from "../components/Common/Button";
import Card from "../components/Common/Card";
import { useLanguage } from "../App";

export default function HomePageClean() {
  const { t } = useLanguage();

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="p-6 md:col-span-2">
          <h2 className="text-xl font-bold text-slate-800 dark:text-white mb-6">{t("upcomingSessions")}</h2>
          <div className="space-y-4">
            <div className="flex items-center p-4 bg-slate-50 dark:bg-slate-900/50 rounded-xl">
              <div className="w-12 h-12 rounded-full bg-blue-100 dark:bg-blue-900/50 flex items-center justify-center text-blue-600 mr-4">ДГ</div>
              <div className="flex-1">
                <h4 className="font-semibold text-slate-800 dark:text-white">Дмитро Гавриленко</h4>
                <p className="text-sm text-slate-500">{t("psychotherapist")} • 14:00</p>
              </div>
              <Link to="/consultations"><Button variant="ghost">{t("toChat")}</Button></Link>
            </div>
            <div className="flex items-center p-4 rounded-xl border border-dashed">
              <Activity size={20} className="mr-4" />
              <p className="text-slate-500">{t("noUpcomingSessions")}</p>
            </div>
          </div>
        </Card>

        <Card className="p-6">
          <h3 className="text-lg font-bold mb-2">{t("weeklyCheckup")}</h3>
          <p className="text-sm text-slate-600 dark:text-slate-400 mb-6">{t("emotionalAssessment")}</p>
          <Link to="/questionnaires">
            <Button className="w-full bg-purple-600">{t("emotionalAssessment")}</Button>
          </Link>
        </Card>
      </div>
    </div>
  );
}
