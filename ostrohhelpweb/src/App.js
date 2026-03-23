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
import HomePageClean from "./pages/HomePageClean";
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

// Wrapper to update currentView based on route
function AppContent() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [currentView, setCurrentView] = useState("home");
  const [language, setLanguage] = useState(() => {
    return localStorage.getItem("language") || "uk";
  });
  const location = useLocation();
  const navigate = useNavigate();

  // Get current dark mode from DOM, not React state
  const [isDarkMode, setIsDarkMode] = useState(() => {
    return document.documentElement.classList.contains("dark");
  });

  const handleLanguageChange = useCallback((newLanguage) => {
    console.log("[App.handleLanguageChange] Called with:", newLanguage);
    setLanguage(newLanguage);
    console.log("[App.handleLanguageChange] State will update to:", newLanguage);
    localStorage.setItem("language", newLanguage);
    console.log("[App.handleLanguageChange] Saved to localStorage:", newLanguage);
  }, []);

  // Helper function for translation - memoized
  const t = useCallback((key) => {
    return translations[language]?.[key] || key;
  }, [language]);

  // Memoize context value to recalculate only when language changes
  const contextValue = useMemo(() => {
    console.log("[App] Creating context value with language:", language);
    return {
      language,
      setLanguage: handleLanguageChange,
      t,
    };
  }, [language, t, handleLanguageChange]);

  // Navigation items for Sidebar with lucide-react icons - memoized
  const navItems = useMemo(() => [
    { id: "home", label: t("home"), icon: Home },
    { id: "consultations", label: t("consultations"), icon: MessageSquare },
    { id: "questionnaires", label: t("questionnaires"), icon: ClipboardList },
    { id: "profile", label: t("profile"), icon: User },
  ], [language]);

  // Update currentView when route changes
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

  // Check authentication and load settings
  useEffect(() => {
    const checkAuth = async () => {
      try {
        setIsLoading(true);
        const token = localStorage.getItem("authToken");
        setIsAuthenticated(!!token);

        // Load language settings
        const savedLanguage = localStorage.getItem("language") || "uk";
        setLanguage(savedLanguage);

        // Load theme settings and apply immediately
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
        console.error("[App] Error checking authentication:", err);
        setError("Failed to verify login status");
      } finally {
        setIsLoading(false);
      }
    };

    checkAuth();
  }, []);

  // Listen to changes in dark class on HTML element
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

  // Log language change
  useEffect(() => {
    console.log("[App] Language changed to:", language);
    console.log("[App] Context will provide language:", language, "and t function");
    // Check that translations are available in new language
    const availableKeys = Object.keys(translations[language] || {});
    console.log("[App] Available translation keys for", language, ":", availableKeys.length, "keys");
    if (availableKeys.length === 0) {
      console.error("[App] WARNING: No translations found for language:", language);
    }
  }, [language]);

  // Event handlers
  const handleDarkModeToggle = useCallback(() => {
    // Disable transitions during theme switch
    const htmlElement = document.documentElement;
    htmlElement.setAttribute("data-theme-switching", "true");
    
    setIsDarkMode((prevMode) => {
      const newDarkMode = !prevMode;
      
      // Immediately change class on HTML element
      if (newDarkMode) {
        htmlElement.classList.add("dark");
      } else {
        htmlElement.classList.remove("dark");
      }
      
      // Save to localStorage
      localStorage.setItem("darkMode", JSON.stringify(newDarkMode));
      
      // Enable transitions again after change
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

    // Navigate to page
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

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-white dark:bg-slate-900">
        <div className="text-center">
          <div className="w-16 h-16 border-4 border-blue-200 dark:border-blue-900 border-t-blue-600 dark:border-t-blue-400 rounded-full animate-spin mx-auto mb-4"></div>
          <p className="text-slate-600 dark:text-slate-400 font-medium">
            Loading...
          </p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <LanguageContext.Provider value={contextValue}>
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
    <LanguageContext.Provider value={contextValue}>
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
                <p className="font-bold">Error</p>
                <p>{error}</p>
              </div>
            )}

            <div className="px-4 sm:px-6 py-6 max-w-7xl mx-auto w-full">
              {/* Direct component rendering based on location - solves Outlet hydration issue with language context */}
              {(() => {
                console.log("[App Switch] Current pathname:", location.pathname, "language:", language);
                switch (location.pathname) {
                  case "/":
                  case "/homepage":
                    console.log("[App Switch] Rendering HomePage");
                    return <HomePageClean />;
                  case "/questionnaires":
                    console.log("[App Switch] Rendering QuestionnairesPage");
                    return <QuestionnairesPage />;
                  case "/consultations":
                    console.log("[App Switch] Rendering ConsultationsPage");
                    return <ConsultationsPage />;
                  case "/profile":
                    console.log("[App Switch] Rendering ProfilePage");
                    return <ProfilePage />;
                  default:
                    console.log("[App Switch] Default - rendering HomePage");
                    return <HomePageClean />;
                }
              })()}
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
