import React, { useState, useRef, useEffect } from "react";
import { Paperclip, Send, Activity } from "lucide-react";
import Button from "../components/Common/Button";
import Card from "../components/Common/Card";
import { useLanguage } from "../App";

export default function ConsultationsPage() {
  const { t, language } = useLanguage();
  const [msg, setMsg] = useState('');
  const [refreshKey, setRefreshKey] = useState(0);

  // Force re-render when language changes
  useEffect(() => {
    setRefreshKey(prev => prev + 1);
  }, [language]);
  
  const messages = [
    { id: 1, text: "Доброго дня! Як ви себе почуваєте сьогодні?", sender: 'doctor', time: '10:00' },
    { id: 2, text: "Вітаю, Дмитре. Трохи краще, ніж вчора, але все ще відчуваю тривогу.", sender: 'user', time: '10:05' },
    { id: 3, text: "Розумію. Чи практикували ви дихальні вправи, які ми обговорювали?", sender: 'doctor', time: '10:08' },
  ];

  return (
    <div key={`consultations-${refreshKey}`} className="h-[calc(100vh-8rem)] flex bg-white dark:bg-slate-800 rounded-2xl shadow-sm border border-slate-100 dark:border-slate-700 overflow-hidden animate-in fade-in zoom-in-95 duration-300">
      {/* Sidebar - Contacts */}
      <div className="w-80 border-r border-slate-100 dark:border-slate-700 hidden md:flex flex-col bg-slate-50/50 dark:bg-slate-800/50">
        <div className="p-4 border-b border-slate-100 dark:border-slate-700">
          <h2 className="font-bold text-lg text-slate-800 dark:text-white">{t("messages")}</h2>
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
              <p className="text-xs text-emerald-500 font-medium">{t("online")}</p>
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
          <div className="flex items-center space-x-2">
            <button className="p-2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors">
              <Paperclip size={20} />
            </button>
            <input 
              type="text" 
              value={msg}
              onChange={(e) => setMsg(e.target.value)}
              placeholder={t("messagePlaceholder")} 
              className="flex-1 bg-slate-50 dark:bg-slate-900 border border-slate-200 dark:border-slate-700 rounded-full px-5 py-2.5 focus:outline-none focus:ring-2 focus:ring-blue-500 dark:text-white"
            />
            <Button className="rounded-full w-11 h-11 p-0 flex items-center justify-center">
              <Send size={18} className="ml-1" />
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}