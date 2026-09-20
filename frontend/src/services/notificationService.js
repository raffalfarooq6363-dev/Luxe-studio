import api from '../config/api';
import {
  getStoredNotifications,
  markStoredNotificationAsRead,
  markAllStoredNotificationsAsRead,
  deleteStoredNotification
} from '../data/notificationStore';

const notificationService = {
  getMyNotifications: async (userId, userEmail) => {
    let apiNotifs = [];
    if (userId || userEmail) {
      try {
        const response = await api.get('/notifications/my', {
          params: { userId: userId || undefined, email: userEmail || undefined },
        });
        apiNotifs = response.data || [];
      } catch (err) {
        console.warn('Backend notification fetch failed, using local store:', err);
      }
    }

    const storedNotifs = getStoredNotifications(userEmail);

    const map = new Map();
    storedNotifs.forEach((n) => map.set(String(n.id), n));
    apiNotifs.forEach((n) => map.set(String(n.id), n));

    const combined = Array.from(map.values());
    combined.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
    return combined;
  },

  markAsRead: async (id) => {
    if (typeof id === 'number' || (!isNaN(id) && !String(id).startsWith('NOTIF-'))) {
      try {
        const response = await api.put(`/notifications/${id}/read`);
        markStoredNotificationAsRead(id);
        return response.data;
      } catch (err) {
        markStoredNotificationAsRead(id);
      }
    } else {
      markStoredNotificationAsRead(id);
    }
  },

  markAllAsRead: async (userId, userEmail) => {
    if (userId) {
      try {
        await api.put('/notifications/mark-all-read', null, {
          params: { userId },
        });
      } catch (err) {
        console.warn('API mark all read failed:', err);
      }
    }
    markAllStoredNotificationsAsRead(userEmail);
  },

  deleteNotification: async (id) => {
    if (typeof id === 'number' || (!isNaN(id) && !String(id).startsWith('NOTIF-'))) {
      try {
        await api.delete(`/notifications/${id}`);
      } catch (err) {
        console.warn('API delete notification failed:', err);
      }
    }
    deleteStoredNotification(id);
  },
};

export default notificationService;
