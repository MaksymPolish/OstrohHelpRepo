import React, { useEffect, useRef, useState } from "react";
import { Brain, ChevronDown, Moon, ShieldCheck, Sun } from "lucide-react";
import { useLanguage } from "../../App";

export default function Header({
  isDarkMode,
  onDarkModeToggle,
  navItems,
  currentView,
  onNavigate,
  userInitial = "ІП",
  userName = "Іван П.",
  userPhotoUrl = null,
  onLogout,
  showAdminPanel = false,
}) {
  const { t } = useLanguage();
  const [isProfileMenuOpen, setIsProfileMenuOpen] = useState(false);
  const [isQuestionnairesMenuOpen, setIsQuestionnairesMenuOpen] = useState(false);
  const profileMenuRef = useRef(null);
  const questionnairesMenuRef = useRef(null);

  useEffect(() => {
    const handleOutsideClick = (event) => {
      if (!profileMenuRef.current?.contains(event.target)) {
        setIsProfileMenuOpen(false);
      }

      if (!questionnairesMenuRef.current?.contains(event.target)) {
        setIsQuestionnairesMenuOpen(false);
      }
    };

    const handleEscape = (event) => {
      if (event.key === "Escape") {
        setIsProfileMenuOpen(false);
        setIsQuestionnairesMenuOpen(false);
      }
    };

    document.addEventListener("mousedown", handleOutsideClick);
    document.addEventListener("keydown", handleEscape);

    return () => {
      document.removeEventListener("mousedown", handleOutsideClick);
      document.removeEventListener("keydown", handleEscape);
    };
  }, []);

  const handleOpenProfile = () => {
    setIsProfileMenuOpen(false);
    onNavigate("profile");
  };

  const handleOpenQuestionnaires = () => {
    setIsQuestionnairesMenuOpen(false);
    onNavigate("questionnaires");
  };

  const handleOpenMyQuestionnaires = () => {
    setIsQuestionnairesMenuOpen(false);
    onNavigate("myQuestionnaires");
  };

  const handleOpenAdminPanel = () => {
    setIsProfileMenuOpen(false);
    onNavigate("admin");
  };

  return (
    <header className="sticky top-0 z-30 bg-white/90 dark:bg-slate-900/90 backdrop-blur-md border-b border-slate-100 dark:border-slate-800 px-4 sm:px-6 py-3">
      <div className="relative flex items-center justify-between gap-3 sm:gap-4">
        <div className="flex items-center space-x-2 min-w-0 shrink-0">
          <div className="w-8 h-8 rounded-lg bg-blue-600 text-white flex items-center justify-center shadow-sm shrink-0">
            <Brain size={18} />
          </div>
          <h2 className="text-base sm:text-lg font-semibold text-slate-800 dark:text-white truncate">
            OA Mind Care
          </h2>
        </div>

        <nav className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 max-w-[52vw] sm:max-w-[58vw] md:max-w-[62vw]">
          <div className="flex items-center justify-center gap-2">
            {navItems
              .filter((item) => item.id !== "profile")
              .map((item) => {
                const Icon = item.icon;
                const isActive = currentView === item.id;

                if (item.id === "questionnaires") {
                  return (
                    <div key={item.id} className="relative" ref={questionnairesMenuRef}>
                      <button
                        type="button"
                        onClick={() => setIsQuestionnairesMenuOpen((prev) => !prev)}
                        className={`inline-flex items-center gap-2 px-4 py-2 rounded-lg whitespace-nowrap transition-colors ${
                          isActive
                            ? "bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300 font-semibold"
                            : "bg-slate-100/80 text-slate-600 hover:text-slate-900 dark:bg-slate-800 dark:text-slate-300 dark:hover:text-white"
                        }`}
                      >
                        {Icon ? <Icon size={16} /> : null}
                        <span className="text-sm">{item.label}</span>
                        <ChevronDown size={14} />
                      </button>

                      {isQuestionnairesMenuOpen && (
                        <div className="absolute left-0 mt-2 w-52 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-lg p-1 z-50">
                          <button
                            type="button"
                            onClick={handleOpenQuestionnaires}
                            className="w-full text-left px-3 py-2 rounded-lg text-sm text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700"
                          >
                            {t("questionnairesTakeAssessment")}
                          </button>
                          <button
                            type="button"
                            onClick={handleOpenMyQuestionnaires}
                            className="w-full text-left px-3 py-2 rounded-lg text-sm text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700"
                          >
                            {t("questionnairesMyList")}
                          </button>
                        </div>
                      )}
                    </div>
                  );
                }

                return (
                  <button
                    key={item.id}
                    type="button"
                    onClick={() => onNavigate(item.id)}
                    className={`inline-flex items-center gap-2 px-4 py-2 rounded-lg whitespace-nowrap transition-colors ${
                      isActive
                        ? "bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300 font-semibold"
                        : "bg-slate-100/80 text-slate-600 hover:text-slate-900 dark:bg-slate-800 dark:text-slate-300 dark:hover:text-white"
                    }`}
                  >
                    {Icon ? <Icon size={16} /> : null}
                    <span className="text-sm">{item.label}</span>
                  </button>
                );
              })}
          </div>
        </nav>

        <div className="flex items-center space-x-2 sm:space-x-4 shrink-0">
          <button
            onClick={onDarkModeToggle}
            className="p-2.5 rounded-full text-slate-500 hover:bg-slate-100 dark:hover:bg-slate-800 focus:outline-none focus:ring-2 focus:ring-blue-500"
            title={isDarkMode ? "Увімкнути світлу тему" : "Увімкнути темну тему"}
          >
            {isDarkMode ? <Sun size={20} /> : <Moon size={20} />}
          </button>

          <div className="h-8 w-px bg-slate-200 dark:bg-slate-700 hidden sm:block"></div>

          <div className="relative" ref={profileMenuRef}>
            <button
              type="button"
              onClick={() => setIsProfileMenuOpen((prev) => !prev)}
              className="flex items-center space-x-2 p-1 rounded-full hover:bg-slate-50 dark:hover:bg-slate-800 pr-3"
            >
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
              <ChevronDown size={16} className="text-slate-500 hidden sm:block" />
            </button>

            {isProfileMenuOpen && (
              <div className="absolute right-0 mt-2 w-44 rounded-xl bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 shadow-lg p-1 z-50">
                <button
                  type="button"
                  onClick={handleOpenProfile}
                  className="w-full text-left px-3 py-2 rounded-lg text-sm text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700"
                >
                  {t("profile")}
                </button>
                {showAdminPanel && (
                  <button
                    type="button"
                    onClick={handleOpenAdminPanel}
                    className="w-full text-left px-3 py-2 rounded-lg text-sm text-slate-700 dark:text-slate-200 hover:bg-slate-100 dark:hover:bg-slate-700 inline-flex items-center gap-2"
                  >
                    <ShieldCheck size={14} />
                    <span>{t("adminPanel")}</span>
                  </button>
                )}
                <button
                  type="button"
                  onClick={() => {
                    setIsProfileMenuOpen(false);
                    onLogout();
                  }}
                  className="w-full text-left px-3 py-2 rounded-lg text-sm text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20"
                >
                  {t("logout")}
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}
