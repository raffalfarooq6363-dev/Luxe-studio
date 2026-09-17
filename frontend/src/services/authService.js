import api from '../config/api';

export const getAuthErrorMessage = (error, fallback) => {
  const responseData = error.response?.data;
  if (responseData?.message) {
    return responseData.message;
  }

  if (responseData?.errors) {
    const validationMessages = Object.values(responseData.errors).flat();
    if (validationMessages.length > 0) {
      return validationMessages.join(' ');
    }
  }

  if (!error.response) {
    return 'Unable to connect to the server. Please make sure the backend is running.';
  }

  return fallback;
};

const authService = {
  register: async (userData) => {
    const response = await api.post('/auth/register', userData);
    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data.user));
    }
    return response.data;
  },

  login: async (email, password) => {
    const response = await api.post('/auth/login', { email, password });
    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data.user));
    }
    return response.data;
  },

  adminLogin: async (email, password) => {
    const response = await api.post('/auth/admin/login', { email: email.trim(), password });
    if (response.data.token && response.data.user) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data.user));
    }
    return response.data;
  },

  adminRegister: async (userData) => {
    const response = await api.post('/auth/admin/setup', {
      ...userData,
      email: userData.email.trim(),
    });
    return response.data;
  },

  updateProfile: async (profileData) => {
    const response = await api.put('/auth/profile', profileData);
    localStorage.setItem('user', JSON.stringify(response.data));
    return response.data;
  },

  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  getCurrentUser: () => {
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      return null;
    }

    try {
      return JSON.parse(userStr);
    } catch {
      localStorage.removeItem('user');
      localStorage.removeItem('token');
      return null;
    }
  },

  getToken: () => {
    return localStorage.getItem('token');
  },

  isAuthenticated: () => {
    return !!localStorage.getItem('token');
  },
};

export default authService;
