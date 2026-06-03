import React, { useState, useRef, useEffect } from "react";
import { Paperclip, Send, Activity, Mic } from "lucide-react";
import Button from "../components/Common/Button";

export default function ChatPage() {
  const [msg, setMsg] = useState('');
  const [isListening, setIsListening] = useState(false);
  const [isUserSpeaking, setIsUserSpeaking] = useState(false);
  const [speechSupported, setSpeechSupported] = useState(true);
  const [silenceDelayMs, setSilenceDelayMs] = useState(2500);
  const [speechError, setSpeechError] = useState("");
  const [language, setLanguage] = useState("uk-UA");
  const recognitionRef = useRef(null);
  const silenceTimeoutRef = useRef(null);
  const baseMessageRef = useRef('');
  const silenceDelayRef = useRef(2500);
  const languageRef = useRef("uk-UA");

  useEffect(() => {
    silenceDelayRef.current = silenceDelayMs;
  }, [silenceDelayMs]);

  useEffect(() => {
    languageRef.current = language;
    // if recognition exists and is active, apply change by restarting
    if (recognitionRef.current) {
      try {
        const wasListening = isListening;
        recognitionRef.current.stop();
        recognitionRef.current.lang = languageRef.current;
        if (wasListening) {
          setTimeout(() => {
            try { recognitionRef.current.start(); } catch (e) {}
          }, 200);
        }
      } catch (e) {
        // ignore
      }
    }
  }, [language]);

  // Persist language selection
  useEffect(() => {
    try {
      const saved = localStorage.getItem('chat_speech_language');
      if (saved) setLanguage(saved);
    } catch (e) {
      // ignore
    }
  }, []);

  useEffect(() => {
    try {
      localStorage.setItem('chat_speech_language', language);
    } catch (e) {
      // ignore
    }
  }, [language]);

  useEffect(() => {
    if (!speechError) {
      return undefined;
    }

    const toastTimer = setTimeout(() => {
      setSpeechError("");
    }, 3500);

    return () => clearTimeout(toastTimer);
  }, [speechError]);

  const clearSilenceTimer = () => {
    if (silenceTimeoutRef.current) {
      clearTimeout(silenceTimeoutRef.current);
      silenceTimeoutRef.current = null;
    }
  };

  const startSilenceTimer = () => {
    clearSilenceTimer();
    silenceTimeoutRef.current = setTimeout(() => {
      recognitionRef.current?.stop();
    }, silenceDelayRef.current);
  };

  useEffect(() => {
    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

    if (!SpeechRecognition) {
      setSpeechSupported(false);
      setSpeechError("Браузер не підтримує голосове введення.");
      return undefined;
    }

    const recognition = new SpeechRecognition();
    recognition.lang = languageRef.current || "uk-UA";
    recognition.continuous = true;
    recognition.interimResults = true;

    recognition.onstart = () => {
      setIsListening(true);
      setIsUserSpeaking(false);
      setSpeechError("");
      clearSilenceTimer();
    };

    recognition.onresult = (event) => {
      setIsUserSpeaking(true);
      const transcript = Array.from(event.results)
        .map((result) => result[0]?.transcript || "")
        .join(" ")
        .replace(/\s+/g, " ")
        .trim();

      const base = baseMessageRef.current.trim();
      const merged = [base, transcript].filter(Boolean).join(" ").replace(/\s+/g, " ").trim();
      setMsg(merged);
      startSilenceTimer();
    };

    recognition.onspeechstart = () => {
      setIsUserSpeaking(true);
      clearSilenceTimer();
    };

    recognition.onspeechend = () => {
      setIsUserSpeaking(false);
      startSilenceTimer();
    };

    recognition.onerror = (event) => {
      setIsListening(false);
      setIsUserSpeaking(false);
      clearSilenceTimer();

      if (event.error === "not-allowed" || event.error === "service-not-allowed") {
        setSpeechError("Немає доступу до мікрофона. Дозвольте його в браузері.");
        return;
      }

      if (event.error === "no-speech") {
        setSpeechError("Не вдалося розпізнати мовлення. Спробуйте ще раз.");
        return;
      }

      setSpeechError("Сталася помилка голосового введення.");
    };

    recognition.onend = () => {
      setIsListening(false);
      setIsUserSpeaking(false);
      clearSilenceTimer();
    };

    recognitionRef.current = recognition;

    return () => {
      clearSilenceTimer();
      recognition.stop();
      recognitionRef.current = null;
    };
  }, []);

  const toggleSpeechToText = () => {
    if (!recognitionRef.current) {
      return;
    }

    if (isListening) {
      recognitionRef.current.stop();
      clearSilenceTimer();
      return;
    }

    baseMessageRef.current = msg;
    try {
      recognitionRef.current.start();
    } catch (error) {
      setIsListening(false);
      setIsUserSpeaking(false);
      setSpeechError("Не вдалося запустити мікрофон. Спробуйте ще раз.");
    }
  };

  const silenceDelayOptions = [
    { label: "2с", value: 2000 },
    { label: "3с", value: 3000 },
    { label: "5с", value: 5000 },
  ];
  
  const messages = [
    { id: 1, text: "Доброго дня! Як ви себе почуваєте сьогодні?", sender: 'doctor', time: '10:00' },
    { id: 2, text: "Вітаю, Дмитре. Трохи краще, ніж вчора, але все ще відчуваю тривогу.", sender: 'user', time: '10:05' },
    { id: 3, text: "Розумію. Чи практикували ви дихальні вправи, які ми обговорювали?", sender: 'doctor', time: '10:08' },
  ];

  return (
    <div className="h-[calc(100vh-8rem)] flex bg-white dark:bg-slate-800 rounded-2xl shadow-sm border border-slate-100 dark:border-slate-700 overflow-hidden animate-in fade-in zoom-in-95 duration-300">
      {/* Sidebar - Contacts */}
      <div className="w-80 border-r border-slate-100 dark:border-slate-700 hidden md:flex flex-col bg-slate-50/50 dark:bg-slate-800/50">
        <div className="p-4 border-b border-slate-100 dark:border-slate-700">
          <h2 className="font-bold text-lg text-slate-800 dark:text-white">Повідомлення</h2>
        </div>
        <div className="overflow-y-auto flex-1 p-2 space-y-1">
          <div className="flex items-center p-3 bg-blue-50 dark:bg-blue-900/20 rounded-xl cursor-pointer">
            <div className="relative mr-3">
              <div className="w-10 h-10 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center font-bold">ДГ</div>
              <div className="absolute bottom-0 right-0 w-3 h-3 rounded-full bg-emerald-500 border-2 border-white dark:border-slate-800"></div>
            </div>
            <div className="flex-1 overflow-hidden">
              <div className="flex justify-between items-center mb-0.5">
                <h4 className="font-medium text-slate-900 dark:text-white truncate">Дмитро Гавриленко</h4>
                <span className="text-xs text-slate-500">10:08</span>
              </div>
              <p className="text-sm text-slate-500 truncate">Розумію. Чи практикували ви...</p>
            </div>
          </div>
          {/* Inactive contact */}
          <div className="flex items-center p-3 hover:bg-slate-100 dark:hover:bg-slate-700/50 rounded-xl cursor-pointer transition-colors">
            <div className="relative mr-3">
              <div className="w-10 h-10 rounded-full bg-slate-200 dark:bg-slate-700 text-slate-600 dark:text-slate-300 flex items-center justify-center font-bold">ОК</div>
            </div>
            <div className="flex-1 overflow-hidden">
              <div className="flex justify-between items-center mb-0.5">
                <h4 className="font-medium text-slate-700 dark:text-slate-200 truncate">Олена Коваленко</h4>
                <span className="text-xs text-slate-400">Вчора</span>
              </div>
              <p className="text-sm text-slate-400 truncate">Дякую за сесію.</p>
            </div>
          </div>
        </div>
      </div>

      {/* Main Chat Area */}
      <div className="flex-1 flex flex-col">
        {/* Chat Header */}
        <div className="p-4 border-b border-slate-100 dark:border-slate-700 flex justify-between items-center bg-white dark:bg-slate-800 z-10">
          <div className="flex items-center">
            <div className="w-10 h-10 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center font-bold mr-3 md:hidden">ДГ</div>
            <div>
              <h3 className="font-bold text-slate-800 dark:text-white">Дмитро Гавриленко</h3>
              <p className="text-xs text-emerald-500 font-medium">Онлайн</p>
            </div>
          </div>
          <Button variant="ghost" className="p-2 rounded-full">
            <Activity size={20} />
          </Button>
        </div>

        {/* Messages */}
        <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-6 bg-slate-50 dark:bg-slate-900/50">
          {messages.map((m) => (
            <div key={m.id} className={`flex ${m.sender === 'user' ? 'justify-end' : 'justify-start'}`}>
              <div className={`max-w-[75%] rounded-2xl px-5 py-3 ${
                m.sender === 'user' 
                  ? 'bg-blue-600 text-white rounded-br-sm shadow-sm' 
                  : 'bg-white dark:bg-slate-800 text-slate-800 dark:text-slate-200 border border-slate-100 dark:border-slate-700 rounded-bl-sm shadow-sm'
              }`}>
                <p>{m.text}</p>
                <span className={`text-[10px] mt-1 block ${m.sender === 'user' ? 'text-blue-100 text-right' : 'text-slate-400'}`}>
                  {m.time}
                </span>
              </div>
            </div>
          ))}
        </div>

        {/* Input Area */}
        <div className="p-4 bg-white dark:bg-slate-800 border-t border-slate-100 dark:border-slate-700">
          {speechError && (
            <div className="mb-2 rounded-xl border border-rose-200 bg-rose-50 px-3 py-2 text-xs text-rose-700 dark:border-rose-900/50 dark:bg-rose-900/20 dark:text-rose-200">
              {speechError}
            </div>
          )}
          <div className="mb-2 flex items-center gap-2 text-xs text-slate-500 dark:text-slate-400">
            <span>Автостоп після тиші:</span>
            <div className="flex items-center gap-1">
              {silenceDelayOptions.map((option) => (
                <button
                  key={option.value}
                  type="button"
                  onClick={() => setSilenceDelayMs(option.value)}
                  className={`rounded-full px-2.5 py-1 transition-colors ${
                    silenceDelayMs === option.value
                      ? "bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-200"
                      : "bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-700 dark:text-slate-300 dark:hover:bg-slate-600"
                  }`}
                >
                  {option.label}
                </button>
              ))}
            </div>
            <div className="ml-4 flex items-center gap-2">
              <span>Мова:</span>
              <button
                type="button"
                onClick={() => setLanguage('uk-UA')}
                className={`rounded-full px-2.5 py-1 transition-colors ${
                  language === 'uk-UA'
                    ? "bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-200"
                    : "bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-700 dark:text-slate-300 dark:hover:bg-slate-600"
                }`}
              >
                UA
              </button>
              <button
                type="button"
                onClick={() => setLanguage('en-US')}
                className={`rounded-full px-2.5 py-1 transition-colors ${
                  language === 'en-US'
                    ? "bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-200"
                    : "bg-slate-100 text-slate-600 hover:bg-slate-200 dark:bg-slate-700 dark:text-slate-300 dark:hover:bg-slate-600"
                }`}
              >
                EN
              </button>
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <button className="p-2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors">
              <Paperclip size={20} />
            </button>
            <div className="relative flex-1">
              <input 
                type="text" 
                value={msg}
                onChange={(e) => setMsg(e.target.value)}
                placeholder="Введіть повідомлення..." 
                className="w-full bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-700 rounded-full px-5 py-2.5 focus:outline-none focus:ring-2 focus:ring-blue-500 dark:text-white"
              />
            </div>
            <button
              type="button"
              onClick={toggleSpeechToText}
              disabled={!speechSupported}
              aria-label="Speech to text"
              title={isListening ? 'Зупинити запис' : `Розпізнати мову (${language === 'uk-UA' ? 'UA' : 'EN'})`}
              className={`p-2 rounded-full transition-colors relative ${
                speechSupported
                  ? isListening
                    ? 'text-rose-500 bg-rose-50 dark:bg-rose-900/30'
                    : 'text-slate-400 hover:text-blue-600 dark:hover:text-blue-400'
                  : 'text-slate-300 dark:text-slate-600 cursor-not-allowed'
              }`}
            >
              {isListening && (
                <>
                  <span className="absolute inset-0 rounded-full border border-rose-300/70 dark:border-rose-400/40 animate-ping" />
                  <span className="absolute inset-[-5px] rounded-full border border-rose-300/50 dark:border-rose-400/30 animate-pulse" />
                </>
              )}
              <Mic size={18} className={isUserSpeaking ? "animate-pulse" : ""} />
            </button>
            <div className="text-xs text-slate-500 dark:text-slate-400 ml-2">
              {isListening ? 'Запис...' : 'Клікніть мікрофон для голосу'}
            </div>
            <Button className="rounded-full w-11 h-11 p-0 flex items-center justify-center">
              <Send size={18} className="ml-1" />
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}