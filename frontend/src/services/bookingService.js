import api from '../config/api';

const bookingService = {
  searchBookings: async (searchParams) => {
    const response = await api.post('/booking/search', searchParams);
    return response.data;
  },

  getAvailability: async (serviceId, date) => {
    const response = await api.get('/booking/availability', {
      params: { serviceId, date },
    });
    return response.data;
  },

  createBooking: async (bookingData) => {
    const response = await api.post('/booking', bookingData);
    return response.data;
  },

  getBooking: async (bookingId) => {
    const response = await api.get(`/booking/${bookingId}`);
    return response.data;
  },

  getUserBookings: async (userId) => {
    const response = await api.get('/booking', { params: { userId, page: 1, pageSize: 100 } });
    return response.data.bookings || [];
  },

  updateBooking: async (bookingId, bookingData) => {
    const response = await api.put(`/booking/${bookingId}`, bookingData);
    return response.data;
  },

  cancelBooking: async (bookingId) => {
    const response = await api.delete(`/booking/${bookingId}`);
    return response.data;
  },
};

export default bookingService;
