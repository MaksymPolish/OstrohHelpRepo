import api from "./api";

export const googleLogin = async (googleToken, profileData = null) => {
  const normalizedProfile = {
    fullName: profileData?.fullName || profileData?.name || null,
    profile: profileData?.profile || profileData?.picture || null,
    email: profileData?.email || null,
  };

  const response = await api.post("/auth/google-login", {
    googleToken,
    idToken: googleToken,
    IdToken: googleToken,
    fullName: normalizedProfile.fullName,
    profile: normalizedProfile.profile,
    email: normalizedProfile.email,
  });

  return response.data;
};
