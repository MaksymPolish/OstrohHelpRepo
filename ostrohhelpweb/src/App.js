import React, { useState, useEffect, useCallback, createContext, useContext, useMemo } from "react";
import { BrowserRouter as Router, Routes, Route, useLocation, useNavigate } from "react-router-dom";
import "./App.css";
import { Home, Users, ClipboardList, MessageSquare, User } from "lucide-react";

// Translations
import translations from "./i18n/translations";

// Layout Components
import Header from "./components/Layout/Header";
import Footer from "./components/Layout/Footer";
import Sidebar from "./components/Layout/Sidebar";

// Pages
import LoginPage from "./pages/LoginPage";
import HomePage from "./pages/HomePage";
import QuestionnairesPage from "./pages/QuestionnairesPage";
import ConsultationsPage from "./pages/ConsultationsPage";
import ProfilePage from "./pages/ProfilePage";

// Create Language Context
export const LanguageContext = createContext();

export const useLanguage = () => {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error("useLanguage must be used within LanguageProvider");
  }
  return context;
};

// Wrapper для оновлення currentView відповідно до маршруту
function AppContent() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [currentView, setCurrentView] = useState("home");
  const [language, setLanguage] = useState("uk");
  const location = useLocation();
  const navigate = useNavigate();

  // Отримуємо поточний темний режим з DOM, а не з React стану
  const [isDarkMode, setIsDarkMode] = useState(() => {
    return document.documentElement.classList.contains("dark");
  });

  // Helper функція для перекладу - не мемоизована
  const t = (key) => {
    return translations[language]?.[key] || key;
  };

  // Навігаційні елементи для Sidebar з іконками lucide-react - мемоизовано
  const navItems = useMemo(() => [
    { id: "home", label: t("home"), icon: Home },
    { id: "consultations", label: t("consultations"), icon: MessageSquare },
    { id: "questionnaires", label: t("questionnaires"), icon: ClipboardList },
    { id: "profile", label: t("profile"), icon: User },
  ], [language]);

  // Оновлювати currentView при зміні маршруту
  useEffect(() => {
    const pathMap = {
      "/": "home",
      "/consultations": "consultations",
      "/questionnaires": "questionnaires",
      "/profile": "profile",
    };
    
    const matchedView = pathMap[location.pathname] || "home";
    setCurrentView(matchedView);
  }, [location.pathname]);

  // Перевірка автентифікації та завантаження налаштувань
  useEffect(() => {
    const checkAuth = async () => {
      try {
        setIsLoading(true);
        const token = localStorage.getItem("authToken");
        setIsAuthenticated(!!token);

        // Завантажити налаштування мови
        const savedLanguage = localStorage.getItem("language") || "uk";
        setLanguage(savedLanguage);

        // Завантажити налаштування теми та миттєво застосувати
        const savedDarkMode = localStorage.getItem("darkMode");
        if (savedDarkMode !== null) {
          const isDark = JSON.parse(savedDarkMode);
          const htmlElement = document.documentElement;
          if (isDark) {
            htmlElement.classList.add("dark");
          } else {
            htmlElement.classList.remove("dark");
          }
          setIsDarkMode(isDark);
        }
      } catch (err) {
        console.error("Помилка при перевірці автентифікації:", err);
        setError("Не вдалося перевірити статус входу");
      } finally {
        setIsLoading(false);
      }
    };

    checkAuth();
  }, []);

  // Слухаємо зміни класу dark на HTML елементі
  useEffect(() => {
    const observer = new MutationObserver(() => {
      const isDark = document.documentElement.classList.contains("dark");
      setIsDarkMode(isDark);
    });

    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ["class"],
    });

    return () => observer.disconnect();
  }, []);

  // Обробники подій
  const handleDarkModeToggle = useCallback(() => {
    // Вимкнути transitions під час переходу
    const htmlElement = document.documentElement;
    htmlElement.setAttribute("data-theme-switching", "true");
    
    setIsDarkMode((prevMode) => {
      const newDarkMode = !prevMode;
      
      // Миттєво змінюємо класс на HTML елементі
      if (newDarkMode) {
        htmlElement.classList.add("dark");
      } else {
        htmlElement.classList.remove("dark");
      }
      
      // Зберігаємо в localStorage
      localStorage.setItem("darkMode", JSON.stringify(newDarkMode));
      
      // Включити transitions знову після зміни
      setTimeout(() => {
        htmlElement.removeAttribute("data-theme-switching");
      }, 0);
      
      return newDarkMode;
    });
  }, []);

  const handleSidebarToggle = () => {
    setSidebarOpen(!sidebarOpen);
  };

  const handleNavigation = (viewId) => {
    setSidebarOpen(false);

    // Навігація до сторінки
    const routeMap = {
      home: "/",
      consultations: "/consultations",
      questionnaires: "/questionnaires",
      profile: "/profile",
    };
    navigate(routeMap[viewId] || "/");
  };

  const handleLogout = () => {
    if (window.confirm(t("logoutConfirm"))) {
      localStorage.removeItem("authToken");
      setIsAuthenticated(false);
      navigate("/");
    }
  };

  const handleLanguageChange = useCallback((newLanguage) => {
    console.log("[App] handleLanguageChange called with:", newLanguage);
    setLanguage(newLanguage);
    localStorage.setItem("language", newLanguage);
    // Перезагрузить страницу чтобы применить новый язык ко всем компонентам
    setTimeout(() => {
      window.location.reload();
    }, 300);
  }, []);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-white dark:bg-slate-900">
        <div className="text-center">
          <div className="w-16 h-16 border-4 border-blue-200 dark:border-blue-900 border-t-blue-600 dark:border-t-blue-400 rounded-full animate-spin mx-auto mb-4"></div>
          <p className="text-slate-600 dark:text-slate-400 font-medium">
            Завантаження...
          </p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <LanguageContext.Provider value={{ language, setLanguage: handleLanguageChange, t }}>
        <Routes>
          <Route
            path="*"
            element={
              <LoginPage onLoginSuccess={() => {
                setIsAuthenticated(true);
                navigate('/homepage');
              }} />
            }
          />
        </Routes>
      </LanguageContext.Provider>
    );
  }

  return (
    <LanguageContext.Provider value={{ language, setLanguage: handleLanguageChange, t }}>
      <div
        className={`flex flex-col min-h-screen bg-white dark:bg-slate-900`}
      >
        <Header
          onMenuToggle={handleSidebarToggle}
          isDarkMode={isDarkMode}
          onDarkModeToggle={handleDarkModeToggle}
          navItems={navItems}
          currentView={currentView}
        />

        <div className="flex flex-1 overflow-hidden">
          <Sidebar
            isOpen={sidebarOpen}
            onClose={() => setSidebarOpen(false)}
            navItems={navItems}
            currentView={currentView}
            onNavigate={handleNavigation}
            onLogout={handleLogout}
          />

          <main className="flex-1 overflow-y-auto">
            {error && (
              <div className="bg-red-100 dark:bg-red-900/30 border-l-4 border-red-500 text-red-700 dark:text-red-400 p-4 m-4 rounded">
                <p className="font-bold">Помилка</p>
                <p>{error}</p>
              </div>
            )}

            <div className="px-4 sm:px-6 py-6 max-w-7xl mx-auto w-full" key={`routes-${language}`}>
              <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/homepage" element={<HomePage />} />
                <Route path="/questionnaires" element={<QuestionnairesPage />} />
                <Route path="/consultations" element={<ConsultationsPage />} />
                <Route path="/profile" element={<ProfilePage />} />
                <Route path="*" element={<HomePage />} />
              </Routes>
            </div>
          </main>
        </div>

        <Footer />
      </div>
    </LanguageContext.Provider>
  );
}

function App() {
  return (
    <Router>
      <AppContent />
    </Router>
  );
}

export default App;
