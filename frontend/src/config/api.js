import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5297/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const isAuthRequest = error.config?.url?.includes('/auth/');
      const storedUser = localStorage.getItem('user');
      let storedRole = '';

      try {
        storedRole = JSON.parse(storedUser || '{}').role?.toLowerCase() || '';
      } catch {
        storedRole = '';
      }

      localStorage.removeItem('token');
      localStorage.removeItem('user');

      if (!isAuthRequest) {
        window.location.href = storedRole === 'admin' ? '/admin/login' : '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default api;
