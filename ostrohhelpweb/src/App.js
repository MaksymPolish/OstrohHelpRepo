import React, { useState, useEffect, useCallback, createContext, useContext, useMemo } from "react";
import { BrowserRouter as Router, Routes, Route, useLocation, useNavigate } from "react-router-dom";
import "./App.css";
import { Home, ClipboardList, MessageSquare, User } from "lucide-react";
import api from "./services/api";
import useUserPresence from "./hooks/useUserPresence";

// Translations
import translations from "./i18n/translations";

// Layout Components
import Header from "./components/Layout/Header";
import Footer from "./components/Layout/Footer";

// Pages
import LoginPage from "./pages/LoginPage";
import HomePageClean from "./pages/HomePageClean";
import QuestionnairesPage from "./pages/QuestionnairesPage";
import MyQuestionnairesPage from "./pages/MyQuestionnairesPage";
import ConsultationsPage from "./pages/ConsultationsPage";
import AdminPanelPage from "./pages/AdminPanelPage";
import ProfilePage from "./pages/ProfilePage";
import NotFoundPage from "./pages/NotFoundPage";
import { hasAdminPanelAccess } from "./utils/access";

// Create Language Context
export const LanguageContext = createContext();
export const SecurityContext = createContext();
export const PresenceContext = createContext();

export const useLanguage = () => {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error("useLanguage must be used within LanguageProvider");
  }
  return context;
};

export const useSecurity = () => {
  const context = useContext(SecurityContext);
  if (!context) {
    throw new Error("useSecurity must be used within SecurityContext provider");
  }
  return context;
};

export const usePresence = () => {
  const context = useContext(PresenceContext);
  if (!context) {
    throw new Error("usePresence must be used within PresenceContext provider");
  }
  return context;
};

const readStoredUser = () => {
  try {
    const storedUser = localStorage.getItem("user");
    return storedUser ? JSON.parse(storedUser) : null;
  } catch {
    return null;
  }
};

const normalizeSessionUser = (userData) => {
  if (!userData || typeof userData !== "object") {
    return null;
  }

  const nestedUser = userData.user && typeof userData.user === "object" ? userData.user : {};
  const nestedProfile = userData.profile && typeof userData.profile === "object" ? userData.profile : {};

  const pickFirst = (...values) => {
    for (const value of values) {
      if (value !== null && value !== undefined && value !== "") {
        return value;
      }
    }
    return null;
  };

  const courseValue = pickFirst(
    userData.course,
    userData.Course,
    userData.courseNumber,
    userData.CourseNumber,
    userData.studyCourse,
    userData.StudyCourse,
    userData.userCourse,
    userData.UserCourse,
    nestedUser.course,
    nestedUser.Course,
    nestedProfile.course,
    nestedProfile.Course
  );

  return {
    id: pickFirst(userData.id, userData.Id, nestedUser.id, nestedUser.Id),
    email: pickFirst(userData.email, userData.Email, nestedUser.email, nestedUser.Email),
    fullName: pickFirst(userData.fullName, userData.FullName, userData.name, userData.Name, nestedUser.fullName, nestedUser.FullName),
    photoUrl: pickFirst(userData.photoUrl, userData.PhotoUrl, userData.profile, userData.picture, nestedUser.photoUrl, nestedUser.PhotoUrl, nestedProfile.photoUrl, nestedProfile.PhotoUrl, nestedProfile.picture),
    university: pickFirst(userData.university, userData.University, nestedUser.university, nestedUser.University),
    faculty: pickFirst(userData.faculty, userData.Faculty, nestedUser.faculty, nestedUser.Faculty),
    department: pickFirst(userData.department, userData.Department, nestedUser.department, nestedUser.Department),
    course: courseValue,
    enrollmentYear: pickFirst(userData.enrollmentYear, userData.EnrollmentYear, nestedUser.enrollmentYear, nestedUser.EnrollmentYear),
    roleId: pickFirst(userData.roleId, userData.RoleId, userData.role_id, userData.Role_ID, nestedUser.roleId, nestedUser.RoleId, nestedUser.role_id, nestedUser.Role_ID),
    roleName: pickFirst(userData.roleName, userData.RoleName, userData.role_name, userData.Role_Name, nestedUser.roleName, nestedUser.RoleName, nestedUser.role_name, nestedUser.Role_Name),
    expiresAt: pickFirst(userData.expiresAt, userData.ExpiresAt, nestedUser.expiresAt, nestedUser.ExpiresAt),
  };
};

const isAdminRouteUser = (user) => hasAdminPanelAccess(user);

// Wrapper to update currentView based on route
function AppContent() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentView, setCurrentView] = useState("home");
  const [language, setLanguage] = useState(() => {
    return localStorage.getItem("language") || "uk";
  });
  const presence = useUserPresence(isAuthenticated);
  const location = useLocation();
  const navigate = useNavigate();

  // Get current dark mode from DOM, not React state
  const [isDarkMode, setIsDarkMode] = useState(() => {
    return document.documentElement.classList.contains("dark");
  });

  const handleLanguageChange = useCallback((newLanguage) => {
    setLanguage(newLanguage);
    localStorage.setItem("language", newLanguage);
  }, []);

  // Helper function for translation - memoized
  const t = useCallback((key) => {
    return translations[language]?.[key] || key;
  }, [language]);

  // Memoize context value to recalculate only when language changes
  const contextValue = useMemo(() => {
    return {
      language,
      setLanguage: handleLanguageChange,
      t,
    };
  }, [language, t, handleLanguageChange]);

  // Navigation items for Header with lucide-react icons - memoized
  const navItems = useMemo(() => [
    { id: "home", label: t("home"), icon: Home },
    { id: "consultations", label: t("consultations"), icon: MessageSquare },
    { id: "questionnaires", label: t("questionnaires"), icon: ClipboardList },
    { id: "profile", label: t("profile"), icon: User },
  ], [t]);

  const showAdminPanel = useMemo(() => isAdminRouteUser(currentUser), [currentUser]);

  // Update currentView when route changes
  useEffect(() => {
    const pathMap = {
      "/": "home",
      "/homepage": "home",
      "/consultations": "consultations",
      "/questionnaires": "questionnaires",
      "/my-questionnaires": "questionnaires",
      "/profile": "profile",
      "/admin": "admin",
    };
    
    const matchedView = pathMap[location.pathname] || "";
    setCurrentView(matchedView);
  }, [location.pathname]);

  // Check authentication and load settings
  useEffect(() => {
    const checkAuth = async () => {
      try {
        setIsLoading(true);
        const token = localStorage.getItem("authToken");
        setIsAuthenticated(!!token);

        const storedUser = token ? normalizeSessionUser(readStoredUser()) : null;
        setCurrentUser(storedUser);

        if (token && storedUser && (storedUser.id || storedUser.email)) {
          try {
            let refreshedUserData = null;

            if (storedUser.id) {
              const byIdResponse = await api.get(`/auth/${storedUser.id}`);
              refreshedUserData = byIdResponse?.data || null;
            } else if (storedUser.email) {
              const byEmailResponse = await api.get("/auth/get-by-email", {
                params: { email: storedUser.email },
              });
              refreshedUserData = byEmailResponse?.data || null;
            }

            if (refreshedUserData) {
              const mergedUser = normalizeSessionUser({
                ...storedUser,
                ...refreshedUserData,
              });

              if (mergedUser) {
                setCurrentUser(mergedUser);
                localStorage.setItem("user", JSON.stringify(mergedUser));
              }
            }
          } catch {
            // Keep existing local session user if profile refresh is unavailable.
          }
        }

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

  const handleLoginSuccess = useCallback((authData = null) => {
    setIsAuthenticated(true);

    const normalizedAuthData = normalizeSessionUser(authData);
    const sessionUser = normalizedAuthData || normalizeSessionUser(readStoredUser());

    setCurrentUser(sessionUser);
    navigate("/homepage");
  }, [navigate]);

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

  const handleNavigation = (viewId) => {
    // Navigate to page
    const routeMap = {
      home: "/",
      consultations: "/consultations",
      questionnaires: "/questionnaires",
      myQuestionnaires: "/my-questionnaires",
      profile: "/profile",
      admin: "/admin",
    };
    navigate(routeMap[viewId] || "/");
  };

  const handleLogout = useCallback(() => {
    if (window.confirm(t("logoutConfirm"))) {
      localStorage.removeItem("authToken");
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("user");
      setIsAuthenticated(false);
      setCurrentUser(null);
      navigate("/");
    }
  }, [navigate, t]);

  const userDisplayName = currentUser?.fullName || currentUser?.email || "User";
  const userInitial = (userDisplayName || "U").trim().charAt(0).toUpperCase() || "U";

  const securityContextValue = useMemo(() => {
    return {
      isAuthenticated,
      currentUser,
      setCurrentUser,
      handleLogout,
    };
  }, [isAuthenticated, currentUser, handleLogout]);

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
      <SecurityContext.Provider value={securityContextValue}>
        <LanguageContext.Provider value={contextValue}>
          <Routes>
            <Route
              path="*"
              element={<LoginPage onLoginSuccess={handleLoginSuccess} />}
            />
          </Routes>
        </LanguageContext.Provider>
      </SecurityContext.Provider>
    );
  }

  return (
    <SecurityContext.Provider value={securityContextValue}>
      <PresenceContext.Provider value={presence}>
        <LanguageContext.Provider value={contextValue}>
          <div
            className={`flex flex-col min-h-screen bg-white dark:bg-slate-900 pb-20`}
          >
            <Header
              isDarkMode={isDarkMode}
              onDarkModeToggle={handleDarkModeToggle}
              navItems={navItems}
              currentView={currentView}
              onNavigate={handleNavigation}
              onLogout={handleLogout}
              userInitial={userInitial}
              userName={userDisplayName}
              userPhotoUrl={currentUser?.photoUrl || null}
              showAdminPanel={showAdminPanel}
            />

            <div className="flex flex-1 overflow-hidden relative">
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
                    switch (location.pathname) {
                      case "/":
                      case "/homepage":
                        return <HomePageClean />;
                      case "/questionnaires":
                        return <QuestionnairesPage />;
                      case "/my-questionnaires":
                        return <MyQuestionnairesPage />;
                      case "/consultations":
                        return <ConsultationsPage />;
                      case "/profile":
                        return <ProfilePage />;
                      case "/admin":
                        return <AdminPanelPage />;
                      default:
                        return <NotFoundPage />;
                    }
                  })()}
                </div>
              </main>
            </div>

            <Footer />
          </div>
        </LanguageContext.Provider>
      </PresenceContext.Provider>
    </SecurityContext.Provider>
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
