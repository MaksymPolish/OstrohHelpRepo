import React, { useState } from "react";
import { CheckCircle } from "lucide-react";
import Button from "../components/Common/Button";
import Card from "../components/Common/Card";
import { useLanguage } from "../App";

export default function QuestionnairesPage() {
  const { t, language } = useLanguage();
  const [step, setStep] = useState(1);
  const totalSteps = 4;
  const progress = (step / totalSteps) * 100;
  const [isCompleted, setIsCompleted] = useState(false);

  if (isCompleted) {
    return (
      <div className="max-w-2xl mx-auto mt-12 animate-in zoom-in-95 duration-500">
        <Card className="p-10 text-center">
          <div className="w-20 h-20 mx-auto bg-emerald-100 dark:bg-emerald-900/30 text-emerald-500 rounded-full flex items-center justify-center mb-6">
            <CheckCircle size={40} />
          </div>
          <h2 className="text-2xl font-bold text-slate-800 dark:text-white mb-4">{t("congratulations")}! {t("assessmentCompleted")}</h2>
          <p className="text-slate-500 mb-8">{t("thankYouForFeedback")}</p>
          <Button onClick={() => window.location.reload()}>{t("retakePreviousAssessments")}</Button>
        </Card>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto animate-in slide-in-from-right-8 duration-300">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-slate-800 dark:text-white mb-2">{t("emotionalState")}</h1>
        <p className="text-slate-500">{t("pleaseAnswerHonestly")}</p>
        
        {/* Progress Bar */}
        <div className="mt-6">
          <div className="flex justify-between text-sm font-medium text-slate-500 mb-2">
            <span>{t("question")} {step} {t("of")} {totalSteps}</span>
            <span>{Math.round(progress)}%</span>
          </div>
          <div className="w-full h-2 bg-slate-100 dark:bg-slate-700 rounded-full overflow-hidden">
            <div 
              className="h-full bg-blue-600 transition-all duration-500 ease-out rounded-full"
              style={{ width: `${progress}%` }}
            ></div>
          </div>
        </div>
      </div>

      <Card className="p-8 shadow-sm">
        <h3 className="text-xl font-medium text-slate-800 dark:text-white mb-6">
          {t("questionAboutDepression")}
        </h3>
        
        <div className="space-y-3">
          {[t("notAtAll"), t("severalDays"), t("moreThanHalf"), t("almostDaily")].map((option, i) => (
            <label key={i} className="flex items-center p-4 border border-slate-200 dark:border-slate-700 rounded-xl cursor-pointer hover:bg-blue-50 dark:hover:bg-slate-700/50 transition-colors group">
              <div className="w-5 h-5 rounded-full border border-slate-300 dark:border-slate-500 group-hover:border-blue-500 flex items-center justify-center mr-4">
                <div className="w-2.5 h-2.5 rounded-full bg-blue-500 opacity-0 group-hover:opacity-100 transition-opacity"></div>
              </div>
              <span className="text-slate-700 dark:text-slate-200 font-medium">{option}</span>
            </label>
          ))}
        </div>

        <div className="mt-8 flex justify-between">
          <Button variant="ghost" onClick={() => setStep(Math.max(1, step - 1))} disabled={step === 1}>
            {t("previous")}
          </Button>
          <Button onClick={() => {
            if (step < totalSteps) setStep(step + 1);
            else setIsCompleted(true);
          }}>
            {step === totalSteps ? t("congratulations") : t("next")}
          </Button>
        </div>
      </Card>
    </div>
  );
}
              