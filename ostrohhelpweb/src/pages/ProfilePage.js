import React, { useState } from "react";
import Button from "../components/Common/Button";
import Card from "../components/Common/Card";
import Badge from "../components/Common/Badge";
import { useLanguage, useSecurity } from "../App";

const normalizeRoleToken = (value) => {
  if (value === null || value === undefined) {
    return "";
  }

  return String(value).trim().toLowerCase().replace(/[^a-z0-9]/g, "");
};

export default function ProfilePage() {
  const { language, setLanguage, t } = useLanguage();
  const { currentUser, handleLogout } = useSecurity();
  const [emailNotifications, setEmailNotifications] = useState(true);

  const resolvedFullName = currentUser?.fullName || "";
  const fullNameParts = resolvedFullName.trim().split(/\s+/).filter(Boolean);
  // Use first token as first name and last token as surname. Any middle names/patronymics are ignored here.
  const firstNameFromSession = fullNameParts[0] || "Користувач";
  const lastNameFromSession = fullNameParts.length > 1 ? fullNameParts[fullNameParts.length - 1] : "-";
  const roleId = normalizeRoleToken(currentUser?.roleId || currentUser?.RoleId || currentUser?.role_id || currentUser?.Role_ID);
  const roleLabel = currentUser?.roleName || currentUser?.RoleName || currentUser?.role_name || currentUser?.Role_Name || (roleId === "000000000002" ? t("clinicalPsychologist") : roleId === "000000000003" ? (language === "uk" ? "Керівник служби" : "Service head") : t("student"));
  const departmentLabel = currentUser?.department || currentUser?.faculty || (language === "uk" ? "Не вказано" : "Not specified");

  const profileData = {
    firstName: firstNameFromSession,
    lastName: lastNameFromSession,
    email: currentUser?.email || "-",
    university: currentUser?.university || "Острозька академія",
    department: departmentLabel,
    accountType: roleLabel,
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
              {currentUser?.photoUrl ? (
                <img
                  src={currentUser.photoUrl}
                  alt={resolvedFullName || "User"}
                  className="w-24 h-24 rounded-2xl object-cover mb-4 shadow-lg"
                  referrerPolicy="no-referrer"
                />
              ) : (
                <div className="w-24 h-24 bg-gradient-to-br from-blue-400 to-blue-600 rounded-2xl flex items-center justify-center text-white text-4xl font-bold mb-4 shadow-lg">
                  {profileData.firstName.charAt(0)}
                </div>
              )}
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
                    setLanguage(e.target.value);
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
