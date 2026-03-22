import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Activity, ClipboardList } from 'lucide-react';
import Button from '../components/Common/Button';
import Card from '../components/Common/Card';

export default function HomePage() {
  const [userName, setUserName] = useState('Іван');

  useEffect(() => {
    const name = localStorage.getItem('userName');
    if (name) setUserName(name);
  }, []);

  return (
    <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
      <div className="relative bg-gradient-to-r from-blue-600 to-teal-500 rounded-3xl p-8 sm:p-12 text-white overflow-hidden shadow-lg shadow-blue-500/20">
        <div className="relative z-10 max-w-2xl">
          <h1 className="text-3xl sm:text-4xl font-bold mb-4">Ваше здоров'я - наш пріоритет</h1>
          <p className="text-blue-50 text-lg mb-8">
            Залучайтесь до конфіденційних консультацій з фахівцями психіатрії. Ми тут, щоб підтримати вас на кожному кроці.
          </p>
          <div className="flex flex-wrap gap-4">
            <Link to="/consultations">
              <Button className="bg-white text-blue-600 hover:bg-blue-50">
                Почати консультацію
              </Button>
            </Link>
            <Link to="/questionnaires">
              <Button variant="outline" className="border-white/30 text-white hover:bg-white/10">
                Заповнити анкету
              </Button>
            </Link>
          </div>
        </div>
        <div className="absolute right-0 top-0 w-64 h-full bg-white/10 rounded-l-full blur-3xl transform translate-x-1/2"></div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="p-6 md:col-span-2">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-bold text-slate-800 dark:text-white">Найближчі сесії</h2>
            <Link to="/consultations" className="text-sm text-blue-600 hover:underline">Всі сесії</Link>
          </div>
          <div className="space-y-4">
            <div className="flex items-center p-4 bg-slate-50 dark:bg-slate-900/50 rounded-xl border border-slate-100 dark:border-slate-700">
              <div className="w-12 h-12 rounded-full bg-blue-100 dark:bg-blue-900/50 flex items-center justify-center text-blue-600 dark:text-blue-400 mr-4">
                <span className="font-bold">ДГ</span>
              </div>
              <div className="flex-1">
                <h4 className="font-semibold text-slate-800 dark:text-white">Дмитро Гавриленко</h4>
                <p className="text-sm text-slate-500">Психотерапевт • 14:00, Сьогодні</p>
              </div>
              <Link to="/chat">
                <Button variant="ghost" className="text-blue-600">В чат</Button>
              </Link>
            </div>
            <div className="flex items-center p-4 rounded-xl border border-dashed border-slate-200 dark:border-slate-700">
              <div className="w-12 h-12 rounded-full bg-slate-100 dark:bg-slate-800 flex items-center justify-center text-slate-400 mr-4">
                <Activity size={20} />
              </div>
              <div className="flex-1">
                <p className="text-slate-500 dark:text-slate-400">Більше немає запланованих сесій</p>
              </div>
            </div>
          </div>
        </Card>

        <Card className="p-6 bg-gradient-to-br from-purple-50 to-white dark:from-slate-800 dark:to-slate-800 border-purple-100 dark:border-slate-700">
          <div className="w-12 h-12 rounded-xl bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400 flex items-center justify-center mb-4">
            <ClipboardList size={24} />
          </div>
          <h3 className="text-lg font-bold text-slate-800 dark:text-white mb-2">Щотижневий чекап</h3>
          <p className="text-sm text-slate-600 dark:text-slate-400 mb-6">
            Пройдіть короткий тест для оцінки вашого емоційного стану цього тижня.
          </p>
          <Link to="/questionnaires">
            <Button className="w-full bg-purple-600 hover:bg-purple-700 focus:ring-purple-500 text-white">
              Почати тест
            </Button>
          </Link>
        </Card>
      </div>
    </div>
  );
}
