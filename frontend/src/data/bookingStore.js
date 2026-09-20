const STORAGE_KEY = 'luxe_glow_bookings';

const initialBookings = [
  {
    id: 'LX-8921',
    serviceId: 1,
    serviceName: '24K Gold Luxury Facial',
    category: 'facial',
    practitionerName: 'Dr. Sophia Miller',
    practitionerRole: 'Lead Aesthetician',
    appointmentDate: new Date(Date.now() + 86400000 * 2).toISOString().split('T')[0],
    timeSlot: '10:30 AM',
    customerName: 'Sarah Jenkins',
    customerEmail: 'sarah.j@example.com',
    customerPhone: '+1 (555) 234-8900',
    serviceType: 'Studio Appointment',
    totalPrice: 120,
    status: 'Confirmed',
    createdAt: new Date().toISOString()
  },
  {
    id: 'LX-8412',
    serviceId: 3,
    serviceName: 'Royal Bridal Glow & Makeup',
    category: 'makeup',
    practitionerName: 'Zara Khan',
    practitionerRole: 'Celebrity Makeup Artist',
    appointmentDate: new Date(Date.now() + 86400000 * 5).toISOString().split('T')[0],
    timeSlot: '01:30 PM',
    customerName: 'Amira Rose',
    customerEmail: 'amira.rose@example.com',
    customerPhone: '+1 (555) 789-1122',
    serviceType: 'VIP Home Service',
    totalPrice: 250,
    status: 'Confirmed',
    createdAt: new Date().toISOString()
  }
];

export const getStoredBookings = () => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(initialBookings));
      return initialBookings;
    }
    return JSON.parse(raw);
  } catch (err) {
    console.error('Error loading bookings:', err);
    return initialBookings;
  }
};

export const saveNewBooking = (bookingData, status = 'Confirmed') => {
  try {
    const existing = getStoredBookings();
    const referenceId = `LX-${Math.floor(1000 + Math.random() * 9000)}`;
    const newRecord = {
      ...bookingData,
      id: bookingData.id !== undefined && bookingData.id !== null ? bookingData.id : referenceId,
      bookingCode: referenceId,
      status,
      createdAt: new Date().toISOString()
    };
    const updated = [newRecord, ...existing];
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    window.dispatchEvent(new Event('luxe_bookings_updated'));
    return newRecord;
  } catch (err) {
    console.error('Error saving booking:', err);
    throw err;
  }
};

export const cancelStoredBooking = (bookingId) => {
  try {
    const existing = getStoredBookings();
    const updated = existing.map(item => 
      item.id === bookingId ? { ...item, status: 'Cancelled' } : item
    );
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    window.dispatchEvent(new Event('luxe_bookings_updated'));
    return updated;
  } catch (err) {
    console.error('Error cancelling booking:', err);
    throw err;
  }
};

export const rescheduleStoredBooking = (bookingId, newDate, newTime) => {
  try {
    const existing = getStoredBookings();
    const updated = existing.map(item =>
      item.id === bookingId
        ? { ...item, appointmentDate: newDate, timeSlot: newTime, isRescheduled: true }
        : item
    );
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    window.dispatchEvent(new Event('luxe_bookings_updated'));
    return updated;
  } catch (err) {
    console.error('Error rescheduling stored booking:', err);
    throw err;
  }
};

export const updateStoredBookingStatus = (bookingId, newStatus) => {
  try {
    const existing = getStoredBookings();
    const updated = existing.map(item =>
      item.id === bookingId ? { ...item, status: newStatus } : item
    );
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    window.dispatchEvent(new Event('luxe_bookings_updated'));
    return updated;
  } catch (err) {
    console.error('Error updating stored booking status:', err);
    throw err;
  }
};

