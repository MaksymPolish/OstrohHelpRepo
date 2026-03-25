import axios from 'axios';
import { API_BASE_URL, ENABLE_TOKEN_REFRESH, TOKEN_REFRESH_ENDPOINT } from '../config/env';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

const refreshClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

let refreshRequestPromise = null;

const clearSessionAndRedirect = () => {
  localStorage.removeItem('authToken');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('user');
  window.location.href = '/login';
};

// Add authorization token to all requests
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Handle errors
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config || {};
    const requestUrl = originalRequest.url || '';
    const isRefreshCall = requestUrl.includes(TOKEN_REFRESH_ENDPOINT);

    if (
      error.response?.status === 401 &&
      ENABLE_TOKEN_REFRESH &&
      !originalRequest._retry &&
      !isRefreshCall
    ) {
      originalRequest._retry = true;

      try {
        if (!refreshRequestPromise) {
          const refreshToken = localStorage.getItem('refreshToken');

          if (!refreshToken) {
            throw new Error('Refresh token not found in localStorage.');
          }

          refreshRequestPromise = refreshClient
            .post(TOKEN_REFRESH_ENDPOINT, { refreshToken })
            .then((response) => {
              const responseData = response.data || {};
              const nextAccessToken = responseData.jwtToken || responseData.accessToken || responseData.token;
              const nextRefreshToken = responseData.refreshToken;

              if (!nextAccessToken) {
                throw new Error('Refresh endpoint did not return an access token.');
              }

              localStorage.setItem('authToken', nextAccessToken);
              if (nextRefreshToken) {
                localStorage.setItem('refreshToken', nextRefreshToken);
              }

              return nextAccessToken;
            })
            .finally(() => {
              refreshRequestPromise = null;
            });
        }

        const newAccessToken = await refreshRequestPromise;
        originalRequest.headers = {
          ...(originalRequest.headers || {}),
          Authorization: `Bearer ${newAccessToken}`,
        };

        return api(originalRequest);
      } catch (refreshError) {
        clearSessionAndRedirect();
        return Promise.reject(refreshError);
      }
    }

    if (error.response?.status === 401) {
      clearSessionAndRedirect();
    }

    return Promise.reject(error);
  }
);

export default api;
