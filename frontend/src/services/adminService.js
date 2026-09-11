import api from '../config/api';

const adminService = {
  getDashboard: async () => {
    const response = await api.get('/admin/dashboard');
    return response.data;
  },

  getAnalytics: async (startDate, endDate) => {
    const response = await api.get('/admin/analytics', {
      params: { startDate, endDate },
    });
    return response.data;
  },

  getUsers: async (searchParams) => {
    const response = await api.get('/admin/users', { params: searchParams });
    return response.data;
  },

  getUser: async (userId) => {
    const response = await api.get(`/admin/users/${userId}`);
    return response.data;
  },

  updateUserStatus: async (userId, isActive) => {
    const response = await api.put(`/admin/users/${userId}/status`, { isActive });
    return response.data;
  },

  createService: async (serviceData) => {
    const response = await api.post('/admin/services', serviceData);
    return response.data;
  },
};

export default adminService;
