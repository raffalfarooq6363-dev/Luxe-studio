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

  getBookings: async (searchParams = {}) => {
    const response = await api.get('/booking', { params: searchParams });
    return response.data;
  },

  updateBookingStatus: async (bookingId, status, reason = '') => {
    const response = await api.put(`/booking/${bookingId}/status`, { status, reason });
    return response.data;
  },

  getBookingStats: async (startDate, endDate) => {
    const response = await api.get('/booking/stats', { params: { startDate, endDate } });
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

  sendMessageToUser: async ({ userId, userEmail, title, message }) => {
    const response = await api.post('/notifications/send', { userId, userEmail, title, message });
    return response.data;
  },

  getEmailSettings: async () => {
    const response = await api.get('/booking/email-settings');
    return response.data;
  },

  updateEmailSettings: async (settings) => {
    const response = await api.post('/booking/email-settings', settings);
    return response.data;
  },

  sendTestEmail: async (toEmail) => {
    const response = await api.post('/booking/test-email', null, {
      params: { to: toEmail }
    });
    return response.data;
  },

  getSmtpStatus: async () => {
    const response = await api.get('/booking/smtp-status');
    return response.data;
  },

  resendEmailToCustomer: async (bookingId, type, details = {}) => {
    const response = await api.post(`/booking/${bookingId}/resend-email`, null, {
      params: { 
        type,
        email: details.userEmail || details.clientEmail || details.customerEmail,
        name: details.userName || details.customerName,
        service: details.serviceName,
        date: details.appointmentDate,
        time: details.startTime || details.timeSlot
      }
    });
    return response.data;
  },
};

export default adminService;
