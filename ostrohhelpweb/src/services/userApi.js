import api from "./api";

const unwrapCollection = (payload) => {
  if (Array.isArray(payload)) {
    return payload;
  }

  if (Array.isArray(payload?.$values)) {
    return payload.$values;
  }

  if (Array.isArray(payload?.items)) {
    return payload.items;
  }

  if (Array.isArray(payload?.data)) {
    return payload.data;
  }

  if (Array.isArray(payload?.users)) {
    return payload.users;
  }

  return [];
};

export const getAllUsers = async () => {
  const response = await api.get("/auth/all");
  const users = unwrapCollection(response.data);
  
  // Нормалізуємо дані: конвертуємо roleName → role для консистентності
  return users.map((user) => ({
    ...user,
    role: user.roleName || user.role || "Student",
  }));
};

export const updateUserRole = async ({ userId, role }) => {
  const response = await api.put("/auth/update-role", {
    userId,
    role,
  });
  return response.data || null;
};
