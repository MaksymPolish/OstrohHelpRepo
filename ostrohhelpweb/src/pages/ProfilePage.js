import React, { useState, useEffect } from "react";
import Button from "../components/Common/Button";
import Card from "../components/Common/Card";
import Badge from "../components/Common/Badge";
import { useLanguage } from "../App";

export default function ProfilePage() {
  const { language, setLanguage, t } = useLanguage();
  const [emailNotifications, setEmailNotifications] = useState(true);

  // Add logging on component load and language change
  useEffect(() => {
    console.log("[ProfilePage] Component rendered with language:", language);
  }, [language]);

  const profileData = {
    firstName: "Іван",
    lastName: "Петренко",
    email: "ivan.petrenko@student.oa.edu.ua",
    university: "Острозька академія",
    department: "Факультет інформатики",
    enrollmentYear: "2024",
    accountType: t("student"),
  };

  const handleLogout = () => {
    if (window.confirm(t("logoutConfirm"))) {
      localStorage.removeItem("authToken");
      window.location.href = "/";
    }
  };

  return (
    <div className="bg-white dark:bg-slate-900">
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Main Content */}
        <div className="lg:col-span-2">
          {/* Profile Information */}
          <Card className="mb-8">
            <h2 className="text-2xl font-bold text-slate-900 dark:text-white mb-6">
              {t("personalData")}
            </h2>

            {/* Profile Picture */}
            <div className="mb-8">
              <div className="w-24 h-24 bg-gradient-to-br from-blue-400 to-blue-600 rounded-2xl flex items-center justify-center text-white text-4xl font-bold mb-4 shadow-lg">
                {profileData.firstName.charAt(0)}
              </div>
              <p className="text-sm text-slate-600 dark:text-slate-400">
                {t("syncedWithUniversity")}
              </p>
            </div>

            {/* Profile Fields */}
            <div className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                    {t("firstName")}
                  </label>
                  <div className="px-4 py-3 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-slate-900 dark:text-white font-medium">
                    {profileData.firstName}
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                    {t("lastName")}
                  </label>
                  <div className="px-4 py-3 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-slate-900 dark:text-white font-medium">
                    {profileData.lastName}
                  </div>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                  {t("email")}
                </label>
                <div className="px-4 py-3 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-slate-900 dark:text-white font-medium break-all">
                  {profileData.email}
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                  {t("university")}
                </label>
                <div className="px-4 py-3 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-slate-900 dark:text-white font-medium">
                  {profileData.university}
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                  {t("department")}
                </label>
                <div className="px-4 py-3 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-slate-900 dark:text-white font-medium">
                  {profileData.department}
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                    {t("enrollmentYear")}
                  </label>
                  <div className="px-4 py-3 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-slate-900 dark:text-white font-medium">
                    {profileData.enrollmentYear}
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                    {t("accountType")}
                  </label>
                  <div className="px-4 py-3 bg-slate-50 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl text-slate-900 dark:text-white font-medium">
                    <Badge status="online">{profileData.accountType}</Badge>
                  </div>
                </div>
              </div>
            </div>
          </Card>

          {/* Settings Card */}
          <Card>
            <h2 className="text-2xl font-bold text-slate-900 dark:text-white mb-6">
              {t("accountSettings")}
            </h2>

            <div className="space-y-6">
              {/* Language */}
              <div>
                <label className="block text-sm font-medium text-slate-700 dark:text-slate-300 mb-2">
                  {t("interfaceLanguage")}
                </label>
                <select
                  value={language}
                  onChange={(e) => {
                    const newLang = e.target.value;
                    console.log("[ProfilePage] Language select changed to:", newLang);
                    console.log("[ProfilePage] Calling setLanguage function:", typeof setLanguage);
                    setLanguage(newLang);
                    console.log("[ProfilePage] Current localStorage after setLanguage:", localStorage.getItem("language"));
                  }}
                  className="w-full px-4 py-3 border border-slate-200 dark:border-slate-700 rounded-xl bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="uk">Українська</option>
                  <option value="en">English</option>
                </select>
              </div>

              {/* Email Notifications */}
              <div className="border-t border-slate-200 dark:border-slate-700 pt-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium text-slate-900 dark:text-white">
                      {t("emailNotifications")}
                    </p>
                    <p className="text-sm text-slate-600 dark:text-slate-400">
                      {t("receiveUpdates")}
                    </p>
                  </div>
                  <input
                    type="checkbox"
                    checked={emailNotifications}
                    onChange={(e) => setEmailNotifications(e.target.checked)}
                    className="w-5 h-5 text-blue-600 border-slate-300 rounded focus:ring-blue-500 cursor-pointer"
                  />
                </div>
              </div>
            </div>
          </Card>
        </div>

        {/* Sidebar */}
        <div>
          {/* Logout */}
          <Button
            variant="danger"
            className="w-full"
            onClick={handleLogout}
          >
            {t("logout")}
          </Button>
        </div>
      </div>
    </div>
  );
}
