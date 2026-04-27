const normalizeRoleToken = (value) => {
  if (value === null || value === undefined) {
    return "";
  }

  return String(value).trim().toLowerCase().replace(/[^a-z0-9]/g, "");
};

export const ADMIN_ROLE_IDS = new Set([
  "00000000-0000-0000-0000-000000000002",
  "00000000-0000-0000-0000-000000000003",
]);

const ADMIN_ROLE_TOKEN_IDS = new Set(
  [...ADMIN_ROLE_IDS].map((roleId) => normalizeRoleToken(roleId))
);

const readRoleId = (user) => {
  return user?.roleId || user?.RoleId || user?.role_id || user?.Role_ID || "";
};

export const hasAdminPanelAccess = (user) => {
  const roleId = normalizeRoleToken(readRoleId(user));
  if (roleId && ADMIN_ROLE_TOKEN_IDS.has(roleId)) {
    return true;
  }

  const roleName = normalizeRoleToken(user?.roleName || user?.RoleName);
  return roleName === "psychologist" || roleName === "headofservice";
};

export const isHeadOfServiceUser = (user) => {
  const roleId = normalizeRoleToken(readRoleId(user));
  if (roleId === normalizeRoleToken("00000000-0000-0000-0000-000000000003")) {
    return true;
  }

  const roleName = normalizeRoleToken(user?.roleName || user?.RoleName);
  return roleName === "headofservice";
};
