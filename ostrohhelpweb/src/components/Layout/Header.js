import React from "react";
import { Menu, Moon, Sun } from "lucide-react"; 

export default function Header({
  onMenuToggle,
  isDarkMode,
  onDarkModeToggle,
  currentPageTitle,
  navItems,
  currentView,
  userInitial = "ІП",
  userName = "Іван П.",
  userPhotoUrl = null,
  onLogout,
}) {
  return (
    <header className="sticky top-0 z-30 bg-white/80 dark:bg-slate-900/80 backdrop-blur-md border-b border-slate-100 dark:border-slate-800 px-4 sm:px-6 py-3 flex items-center justify-between">
      <div className="flex items-center">
        <button
          onClick={onMenuToggle}
          className="p-2 mr-3 -ml-2 rounded-lg text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 lg:hidden focus:outline-none"
        >
          <Menu size={24} />
        </button>
        <h2 className="text-lg font-semibold text-slate-800 dark:text-white hidden sm:block capitalize">
          {currentPageTitle ||
            navItems.find((i) => i.id === currentView)?.label ||
            "OA Mind Care"}
        </h2>
      </div>

      <div className="flex items-center space-x-2 sm:space-x-4">
        <button
          onClick={onDarkModeToggle}
          className="p-2.5 rounded-full text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-800 focus:outline-none focus:ring-2 focus:ring-blue-500"
          title={isDarkMode ? "Увімкнути світлу тему" : "Увімкнути темну тему"}
        >
          {isDarkMode ? <Sun size={20} /> : <Moon size={20} />}
        </button>

        <div className="h-8 w-px bg-slate-200 dark:bg-slate-700 hidden sm:block"></div>

        <button className="flex items-center space-x-2 p-1 rounded-full hover:bg-slate-50 dark:hover:bg-slate-800 pr-3">
          {userPhotoUrl ? (
            <img
              src={userPhotoUrl}
              alt={userName}
              className="w-8 h-8 rounded-full object-cover"
              referrerPolicy="no-referrer"
            />
          ) : (
            <div className="w-8 h-8 rounded-full bg-blue-100 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400 flex items-center justify-center font-bold text-sm">
              {userInitial}
            </div>
          )}
          <span className="text-sm font-medium text-slate-700 dark:text-slate-300 hidden sm:block">
            {userName}
          </span>
        </button>

        <button
          onClick={onLogout}
          className="p-2.5 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg focus:outline-none text-sm font-medium flex items-center space-x-1"
          title="Log Out"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
          </svg>
          <span className="hidden sm:inline">Log Out</span>
        </button>
      </div>
    </header>
  );
}
