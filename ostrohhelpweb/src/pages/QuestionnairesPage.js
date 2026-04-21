import React, { useMemo, useState } from "react";
import { CheckCircle } from "lucide-react";
import Button from "../components/Common/Button";
import Card from "../components/Common/Card";
import { useLanguage, useSecurity } from "../App";
import { createQuestionnaire } from "../services/questionnaireApi";

const DEPRESSION_OPTIONS = [
  { value: 0, label: "Ніколи" },
  { value: 1, label: "Кілька днів" },
  { value: 2, label: "Більше половини днів" },
  { value: 3, label: "Майже щодня" },
];

const BURNOUT_OPTIONS = [
  { value: 0, label: "Ніколи" },
  { value: 1, label: "Рідко" },
  { value: 2, label: "Іноді" },
  { value: 3, label: "Часто" },
  { value: 4, label: "Щодня" },
];

const QUESTION_BLOCKS = [
  {
    id: "depression",
    title: "Блок 1: Загальний емоційний стан",
    subtitle: "Скринінг депресії",
    options: DEPRESSION_OPTIONS,
    questions: [
      "Чи відчуваєте ви за останні два тижні зниження інтересу або задоволення від справ, які зазвичай вам подобалися?",
      "Чи буває у вас відчуття пригніченості, депресії або безпорадності?",
      "Чи важко вам засинати, чи буває переривчастий сон або, навпаки, занадто тривалий?",
      "Чи відчуваєте ви швидку втому або брак енергії для повсякденних справ?",
      "Чи помічали ви зміни в апетиті (відсутність бажання їсти або, навпаки, надмірне переїдання)?",
      "Чи виникає у вас відчуття провини, невдачі або того, що ви підвели себе чи своїх близьких?",
      "Чи важко вам зосередитися на навчанні, читанні або виконанні завдань?",
      "Чи помічали ви за собою незвичну сповільненість у рухах чи мовленні? Або навпаки - надмірну метушливість і неспокій?",
      "Чи з'являлися у вас думки про те, що було б краще померти або завдати собі шкоди?",
    ],
  },
  {
    id: "burnout",
    title: "Блок 2: Академічне вигорання",
    subtitle: "Навчальне виснаження",
    options: BURNOUT_OPTIONS,
    questions: [
      "Чи відчуваєте ви емоційне виснаження через необхідність щодня відвідувати заняття та готуватися до них?",
      "Чи почуваєтеся ви абсолютно спустошеним (як вичавлений лимон) наприкінці навчального дня?",
      "Чи помітили ви, що стали менш зацікавленим у навчанні та майбутній професії порівняно з початком курсу?",
      "Чи з'являються у вас сумніви в тому, що ваше навчання взагалі має сенс і принесе користь?",
      "Чи виникає у вас відчуття роздратування або цинізму щодо викладачів, одногрупників чи самого процесу навчання?",
      "Чи відчуваєте ви, що ваша продуктивність впала і ви не можете ефективно справлятися з навчальними завданнями?",
    ],
  },
];

const getDepressionStatus = (score) => {
  if (score <= 4) {
    return "Стан у межах норми. Депресія відсутня.";
  }
  if (score <= 9) {
    return "Легкий рівень. Можливо, це реакція на тимчасові труднощі. Рекомендується відпочинок.";
  }
  if (score <= 14) {
    return "Помірний рівень. Варто звернути увагу на свій стан, бажано проконсультуватися з психологом.";
  }
  return "Високий рівень. Потрібна допомога фахівця для детальної діагностики.";
};

const getBurnoutStatus = (score) => {
  if (score <= 8) {
    return "Низький рівень вигорання. Ви в ресурсі.";
  }
  if (score <= 16) {
    return "Середній рівень (стадія резистентності). Організм працює на виснаження. Потрібен перегляд режиму дня та зниження навантаження.";
  }
  return "Високий рівень вигорання. Стан глибокого виснаження та втрати мотивації. Рекомендовано звернутися до психолога або взяти академічну відпустку/тривалу перерву.";
};

const buildAssessmentDescription = ({ questions, answers, depressionScore, burnoutScore }) => {
  const depressionStatus = getDepressionStatus(depressionScore);
  const burnoutStatus = getBurnoutStatus(burnoutScore);

  const lines = [
    `Стан студента: ${depressionStatus}`,
    `Рівень вигорання: ${burnoutStatus}`,
    "",
    `Підсумок балів:`,
    `- Депресія (1-9): ${depressionScore}`,
    `- Вигорання (10-15): ${burnoutScore}`,
    "",
    "Питання та відповіді:",
    ...questions.map((question, index) => {
      const answerValue = answers[question.id];
      const answerLabel = question.options.find((option) => option.value === answerValue)?.label || "Без відповіді";
      return `${index + 1}. ${question.text}\n   Відповідь: ${answerLabel} (бал: ${answerValue ?? 0})`;
    }),
  ];

  return lines.join("\n");
};

const buildRequestDescription = ({ topic, urgency, details }) => {
  return [
    "Форма заявки",
    `Тема: ${topic || "Не вказано"}`,
    `Терміновість: ${urgency || "Не вказано"}`,
    "",
    "Опис:",
    details || "Користувач не залишив детального опису.",
  ].join("\n");
};

export default function QuestionnairesPage() {
  const { t } = useLanguage();
  const { currentUser } = useSecurity();

  const [mode, setMode] = useState("assessment");
  const [step, setStep] = useState(0);
  const [answers, setAnswers] = useState({});
  const [requestTopic, setRequestTopic] = useState("");
  const [requestUrgency, setRequestUrgency] = useState("Середня");
  const [requestDetails, setRequestDetails] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState("");
  const [isCompleted, setIsCompleted] = useState(false);

  const questions = useMemo(() => {
    const flattened = [];
    let ordinal = 1;

    QUESTION_BLOCKS.forEach((block) => {
      block.questions.forEach((questionText) => {
        flattened.push({
          id: `${block.id}-${ordinal}`,
          ordinal,
          blockId: block.id,
          blockTitle: block.title,
          blockSubtitle: block.subtitle,
          text: questionText,
          options: block.options,
        });
        ordinal += 1;
      });
    });

    return flattened;
  }, []);

  const totalSteps = questions.length;
  const progress = totalSteps > 0 ? ((step + 1) / totalSteps) * 100 : 0;
  const currentQuestion = questions[step];

  const depressionScore = useMemo(
    () => questions
      .filter((question) => question.blockId === "depression")
      .reduce((sum, question) => sum + (answers[question.id] ?? 0), 0),
    [answers, questions]
  );

  const burnoutScore = useMemo(
    () => questions
      .filter((question) => question.blockId === "burnout")
      .reduce((sum, question) => sum + (answers[question.id] ?? 0), 0),
    [answers, questions]
  );

  const currentAnswer = currentQuestion ? answers[currentQuestion.id] : undefined;

  const getUserId = () => {
    return currentUser?.id || currentUser?.Id || null;
  };

  const submitPayload = async (description) => {
    const userId = getUserId();
    const payload = {
      description,
      isAnonymous: false,
      userId,

      // Compatibility aliases
      Description: description,
      IsAnonymous: false,
      UserId: userId,
    };

    await createQuestionnaire(payload);
  };

  const handleAnswerSelect = (value) => {
    if (!currentQuestion) {
      return;
    }

    setAnswers((prev) => ({
      ...prev,
      [currentQuestion.id]: value,
    }));
  };

  const handleNext = () => {
    if (!currentQuestion) {
      return;
    }

    if (step < totalSteps - 1) {
      setStep((prev) => prev + 1);
      return;
    }

    handleSubmitAssessment();
  };

  const handleSubmitAssessment = async () => {
    setIsSubmitting(true);
    setSubmitError("");

    try {
      const description = buildAssessmentDescription({
        questions,
        answers,
        depressionScore,
        burnoutScore,
      });

      await submitPayload(description);
      setIsCompleted(true);
    } catch {
      setSubmitError("Не вдалося відправити анкету. Спробуйте ще раз.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSubmitRequest = async () => {
    setIsSubmitting(true);
    setSubmitError("");

    try {
      const description = buildRequestDescription({
        topic: requestTopic,
        urgency: requestUrgency,
        details: requestDetails,
      });

      await submitPayload(description);
      setIsCompleted(true);
    } catch {
      setSubmitError("Не вдалося відправити заявку. Спробуйте ще раз.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const resetAll = () => {
    setIsCompleted(false);
    setStep(0);
    setAnswers({});
    setRequestTopic("");
    setRequestUrgency("Середня");
    setRequestDetails("");
    setSubmitError("");
    setMode("assessment");
  };

  if (isCompleted) {
    return (
      <div className="max-w-2xl mx-auto mt-12 animate-in zoom-in-95 duration-500">
        <Card className="p-10 text-center">
          <div className="w-20 h-20 mx-auto bg-emerald-100 dark:bg-emerald-900/30 text-emerald-500 rounded-full flex items-center justify-center mb-6">
            <CheckCircle size={40} />
          </div>
          <h2 className="text-2xl font-bold text-slate-800 dark:text-white mb-4">{t("congratulations")}! {t("assessmentCompleted")}</h2>
          <p className="text-slate-500 mb-8">{t("thankYouForFeedback")}</p>
          <Button onClick={resetAll}>{t("retakePreviousAssessments")}</Button>
        </Card>
      </div>
    );
  }

  if (mode === "request") {
    const canSubmitRequest = requestTopic.trim().length > 0 && requestDetails.trim().length > 0;

    return (
      <div className="max-w-3xl mx-auto animate-in slide-in-from-right-8 duration-300">
        <div className="mb-8 flex items-center justify-between gap-3">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 dark:text-white mb-2">Форма заявки</h1>
            <p className="text-slate-500">Опишіть ваш запит, і ми передамо його спеціалісту.</p>
          </div>
          <Button variant="ghost" onClick={() => setMode("assessment")}>Пройти анкету</Button>
        </div>

        <Card className="p-8 shadow-sm space-y-5">
          {submitError && (
            <div className="rounded-xl border border-red-200 bg-red-50 text-red-700 px-4 py-3 text-sm">
              {submitError}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-200 mb-2">Тема звернення</label>
            <input
              type="text"
              value={requestTopic}
              onChange={(event) => setRequestTopic(event.target.value)}
              className="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 px-4 py-3 text-slate-800 dark:text-slate-200"
              placeholder="Наприклад: Тривожність, вигорання, конфлікти у групі"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-200 mb-2">Терміновість</label>
            <select
              value={requestUrgency}
              onChange={(event) => setRequestUrgency(event.target.value)}
              className="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 px-4 py-3 text-slate-800 dark:text-slate-200"
            >
              <option value="Низька">Низька</option>
              <option value="Середня">Середня</option>
              <option value="Висока">Висока</option>
              <option value="Критична">Критична</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-200 mb-2">Детальний опис</label>
            <textarea
              value={requestDetails}
              onChange={(event) => setRequestDetails(event.target.value)}
              rows={6}
              className="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 px-4 py-3 text-slate-800 dark:text-slate-200"
              placeholder="Опишіть ситуацію детальніше"
            />
          </div>

          <div className="flex justify-end">
            <Button onClick={handleSubmitRequest} disabled={!canSubmitRequest || isSubmitting}>
              {isSubmitting ? "Відправляємо..." : "Відправити заявку"}
            </Button>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto animate-in slide-in-from-right-8 duration-300">
      <div className="mb-8 flex items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-slate-800 dark:text-white mb-2">{t("emotionalState")}</h1>
          <p className="text-slate-500">{t("pleaseAnswerHonestly")}</p>
        </div>
        <Button variant="ghost" onClick={() => setMode("request")}>Заповнити форму заявки</Button>
      </div>

      <div className="mb-8">
        {submitError && (
          <div className="mb-4 rounded-xl border border-red-200 bg-red-50 text-red-700 px-4 py-3 text-sm">
            {submitError}
          </div>
        )}
        
        {/* Progress Bar */}
        <div className="mt-6">
          <div className="flex justify-between text-sm font-medium text-slate-500 mb-2">
            <span>{t("question")} {step + 1} {t("of")} {totalSteps}</span>
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
        {currentQuestion && (
          <>
            <div className="mb-3">
              <p className="text-sm font-semibold text-blue-600 dark:text-blue-400">{currentQuestion.blockTitle}</p>
              <p className="text-xs text-slate-500">{currentQuestion.blockSubtitle}</p>
            </div>
            <h3 className="text-xl font-medium text-slate-800 dark:text-white mb-6">
              {currentQuestion.ordinal}. {currentQuestion.text}
            </h3>
          </>
        )}
        
        <div className="space-y-3">
          {currentQuestion?.options.map((option) => {
            const isSelected = currentAnswer === option.value;

            return (
              <button
                key={option.value}
                type="button"
                onClick={() => handleAnswerSelect(option.value)}
                className={`w-full text-left flex items-center p-4 border rounded-xl transition-colors ${
                  isSelected
                    ? "border-blue-500 bg-blue-50 dark:bg-blue-900/20"
                    : "border-slate-200 dark:border-slate-700 hover:bg-blue-50 dark:hover:bg-slate-700/50"
                }`}
              >
                <div className={`w-5 h-5 rounded-full border flex items-center justify-center mr-4 ${
                  isSelected ? "border-blue-500" : "border-slate-300 dark:border-slate-500"
                }`}>
                  <div className={`w-2.5 h-2.5 rounded-full bg-blue-500 ${isSelected ? "opacity-100" : "opacity-0"}`}></div>
                </div>
                <span className="text-slate-700 dark:text-slate-200 font-medium">{option.label}</span>
                <span className="ml-auto text-sm text-slate-500">{option.value}</span>
              </button>
            );
          })}
        </div>

        <div className="mt-8 flex justify-between">
          <Button variant="ghost" onClick={() => setStep((prev) => Math.max(0, prev - 1))} disabled={step === 0 || isSubmitting}>
            {t("previous")}
          </Button>
          <Button onClick={handleNext} disabled={currentAnswer === undefined || isSubmitting}>
            {step === totalSteps - 1 ? t("congratulations") : t("next")}
          </Button>
        </div>
      </Card>
    </div>
  );
}
              