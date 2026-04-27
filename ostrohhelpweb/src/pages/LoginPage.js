import React, { useState, useEffect } from "react";
import { GoogleLogin } from "@react-oauth/google";
import Button from "../components/Common/Button";
import Card from "../components/Common/Card";
import translations from "../i18n/translations";
import { googleLogin } from "../services/authApi";
import { USE_MOCK_AUTH } from "../config/env";

export default function LoginPage({ onLoginSuccess }) {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const [language, setLanguage] = useState("uk");

  // Load language from localStorage on mount
  useEffect(() => {
    const savedLanguage = localStorage.getItem("language") || "uk";
    setLanguage(savedLanguage);
  }, []);

  // Helper function for translation
  const t = (key) => {
    return translations[language]?.[key] || key;
  };

  const decodeJwtPayload = (token) => {
    try {
      if (!token || typeof token !== "string") {
        return null;
      }

      const parts = token.split(".");
      if (parts.length < 2) {
        return null;
      }

      const payload = parts[1].replace(/-/g, "+").replace(/_/g, "/");
      const padded = payload.padEnd(payload.length + ((4 - (payload.length % 4)) % 4), "=");
      const decoded = atob(padded);
      return JSON.parse(decoded);
    } catch {
      return null;
    }
  };

  const extractServerErrorMessage = (error) => {
    const data = error?.response?.data;
    if (!data || typeof data !== "object") {
      return "";
    }

    if (typeof data.message === "string" && data.message.trim()) {
      return data.message;
    }

    if (typeof data.error === "string" && data.error.trim()) {
      return data.error;
    }

    if (typeof data.title === "string" && data.title.trim()) {
      return data.title;
    }

    if (data.errors && typeof data.errors === "object") {
      const firstEntry = Object.values(data.errors)[0];
      if (Array.isArray(firstEntry) && firstEntry.length > 0) {
        return firstEntry[0];
      }
      if (typeof firstEntry === "string") {
        return firstEntry;
      }
    }

    return "";
  };

  const persistUserSession = (authData) => {
    localStorage.setItem("authToken", authData.jwtToken);

    if (authData.refreshToken) {
      localStorage.setItem("refreshToken", authData.refreshToken);
    }

    localStorage.setItem(
      "user",
      JSON.stringify({
        id: authData.id,
        email: authData.email,
        fullName: authData.fullName,
        photoUrl: authData.photoUrl || authData.profile || authData.picture || null,
        university: authData.university || authData.University || null,
        faculty: authData.faculty || authData.Faculty || null,
        department: authData.department || authData.Department || null,
        course: authData.course ?? authData.Course ?? null,
        enrollmentYear: authData.enrollmentYear || authData.EnrollmentYear || null,
          roleId: authData.roleId || authData.RoleId || authData.role_id || authData.Role_ID || null,
          roleName: authData.roleName || authData.RoleName || authData.role_name || authData.Role_Name || null,
        expiresAt: authData.expiresAt,
      })
    );
  };

  const handleMockGoogleLogin = async () => {
    setIsLoading(true);
    setError("");

    try {
      // Simulate successful Google login
      const mockToken = "google_token_" + Date.now();
      const mockUser = {
        id: "mock-user-id",
        email: "john.student@example.com",
        fullName: "John Student",
        photoUrl: "https://via.placeholder.com/150",
        university: "Острозька академія",
        faculty: "Факультет інформатики",
        department: "Факультет інформатики",
        course: 2,
        enrollmentYear: "2024",
        roleId: "mock-role-id",
        roleName: "Student",
      };

      localStorage.setItem("authToken", mockToken);
      localStorage.setItem(
        "user",
        JSON.stringify(mockUser)
      );

      // Handle successful login
      if (onLoginSuccess) {
        onLoginSuccess({
          jwtToken: mockToken,
          ...mockUser,
        });
      }
    } catch (err) {
      setError(t("googleAuthFailed"));
      console.error("Login error:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleGoogleCredentialSuccess = async (credentialResponse) => {
    setIsLoading(true);
    setError("");

    try {
      const credentialToken = credentialResponse?.credential;

      if (!credentialToken) {
        throw new Error("Google credential is missing.");
      }

      const decodedCredential = decodeJwtPayload(credentialToken);
      const profileData = {
        fullName: decodedCredential?.name || decodedCredential?.given_name || null,
        profile: decodedCredential?.picture || null,
        email: decodedCredential?.email || null,
      };

      const authData = await googleLogin(credentialToken, profileData);
      const normalizedAuthData = {
        ...authData,
        photoUrl: authData?.photoUrl || authData?.profile || profileData?.profile || null,
      };

      persistUserSession(normalizedAuthData);

      if (onLoginSuccess) {
        onLoginSuccess(normalizedAuthData);
      }
    } catch (err) {
      if (err?.response?.status === 400) {
        const serverMessage = extractServerErrorMessage(err);
        setError(serverMessage || t("googleAuthFailed"));
      } else {
        setError(t("googleAuthFailed"));
      }
      console.error("Google auth exchange failed:", err);
      if (err?.response?.data) {
        console.error("[Login] Backend error response:", err.response.data);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleGoogleCredentialError = () => {
    setError(t("googleLoginUnavailable"));
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50 dark:from-slate-950 dark:to-slate-900 px-3 py-12">
      {/* Language Selector */}
      <div className="absolute top-4 right-4">
        <select
          value={language}
          onChange={(e) => {
            setLanguage(e.target.value);
            localStorage.setItem("language", e.target.value);
          }}
          className="px-3 py-2 border border-slate-200 dark:border-slate-700 rounded-lg bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
        >
          <option value="uk">Українська</option>
          <option value="en">English</option>
        </select>
      </div>

      <div className="w-full max-w-lg mx-auto">
        {/* Main Card */}
        <Card className="shadow-lg dark:shadow-xl dark:border-slate-700 p-8">
          {/* Logo */}
          <div className="text-center mb-8">
            <div className="w-16 h-16 bg-gradient-to-r from-blue-600 to-purple-600 rounded-xl flex items-center justify-center mx-auto mb-4">
              <span className="text-white font-bold text-3xl">OA</span>
            </div>
            <h1 className="text-3xl font-bold text-gray-900 dark:text-white">{t("ostroghelpTitle")}</h1>
            <p className="text-gray-600 dark:text-slate-400 mt-2">{t("mentalHealthPlatform")}</p>
          </div>

          <div className="border-t border-slate-200 dark:border-slate-600 pt-8">
            {/* Login Form */}
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2 text-center">
              {t("welcomeBack")}
            </h2>
            <p className="text-center text-gray-600 dark:text-slate-400 mb-6">
              {t("signInWithUniversity")}
            </p>

            {error && (
              <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-400 px-4 py-3 rounded-lg mb-6">
                {error}
              </div>
            )}

            {/* Google Login */}
            {USE_MOCK_AUTH ? (
              <>
                <Button
                  variant="outline"
                  fullWidth
                  size="lg"
                  onClick={handleMockGoogleLogin}
                  disabled={isLoading}
                  className="flex items-center justify-center gap-3 border-2 dark:border-slate-600 dark:text-white dark:hover:bg-slate-800"
                >
                  <svg
                    className="w-6 h-6"
                    fill="currentColor"
                    viewBox="0 0 24 24"
                    xmlns="http://www.w3.org/2000/svg"
                  >
                    <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4" />
                    <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853" />
                    <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05" />
                    <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335" />
                  </svg>
                  {isLoading ? t("signingIn") : t("continueWithGoogle")}
                </Button>
                <p className="mt-3 text-xs text-amber-700 dark:text-amber-400 text-center">
                  {t("mockAuthModeNotice")}
                </p>
              </>
            ) : (
              <div className="flex justify-center">
                <GoogleLogin
                  onSuccess={handleGoogleCredentialSuccess}
                  onError={handleGoogleCredentialError}
                  text="continue_with"
                  width="320"
                />
              </div>
            )}

            {/* Help Text */}
            <p className="text-center text-sm text-gray-600 dark:text-slate-400 mt-6">
              {t("useUniversityEmail")}
            </p>
          </div>
        </Card>

        {/* Footer Note */}
        <p className="text-center text-gray-600 dark:text-slate-400 mt-6 text-sm">
          {t("firstTimeHere")}{" "}
          <button className="text-blue-600 dark:text-blue-400 font-semibold hover:text-blue-700 dark:hover:text-blue-300 bg-none border-none cursor-pointer p-0">
            {t("contactInstitution")}
          </button>
        </p>
      </div>
    </div>
  );
}
