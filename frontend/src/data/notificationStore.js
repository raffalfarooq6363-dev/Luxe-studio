const STORAGE_KEY = 'luxe_glow_notifications';

const buildConfirmationTemplate = (booking) => {
  const dateStr = booking.appointmentDate || booking.date || 'Upcoming';
  const timeStr = booking.startTime || booking.timeSlot || 'Scheduled Time';
  const serviceStr = booking.serviceName || 'Luxe Treatment';
  const staffStr = booking.assignedStaffMember || booking.practitionerName || 'Luxe Specialist';
  const nameStr = booking.userName || booking.customerName || 'Valued Guest';

  return `
    <div style="font-family: Georgia, serif; max-width: 600px; margin: 0 auto; padding: 25px; background: #fff5f8; border: 1px solid #ffd1dc; border-radius: 12px; color: #2d1822;">
      <div style="text-align: center; border-bottom: 2px solid #dfb0bd; padding-bottom: 15px; margin-bottom: 20px;">
        <h2 style="margin: 0; color: #782944; letter-spacing: 2px; font-size: 22px;">LUXE GLOW STUDIO</h2>
        <p style="margin: 5px 0 0; color: #9c6877; font-size: 12px; text-transform: uppercase; letter-spacing: 1px;">Official Appointment Confirmation</p>
      </div>

      <div style="background: #ffffff; padding: 20px; border-radius: 10px; border: 1px solid #f2cfd8; margin-bottom: 20px;">
        <span style="display: inline-block; background: #2e7d32; color: #ffffff; padding: 4px 10px; border-radius: 20px; font-size: 11px; font-weight: bold; text-transform: uppercase; margin-bottom: 10px;">✓ Reserved &amp; Confirmed</span>
        <h3 style="margin: 5px 0 15px; color: #2d1822; font-size: 18px;">Hello ${nameStr},</h3>
        <p style="margin: 0 0 15px; color: #5a3a47; font-size: 14px; line-height: 1.6;">
          Your appointment at <strong>Luxe Glow Studio</strong> has been officially confirmed by our concierge. We look forward to providing you with a bespoke aesthetic experience.
        </p>

        <table style="width: 100%; border-collapse: collapse; font-size: 14px; color: #3c1e2d; margin-top: 15px;">
          <tr style="border-bottom: 1px solid #fce4ec;">
            <td style="padding: 8px 0; color: #8a6a75;">Service Treatment:</td>
            <td style="padding: 8px 0; text-align: right; font-weight: bold;">${serviceStr}</td>
          </tr>
          <tr style="border-bottom: 1px solid #fce4ec;">
            <td style="padding: 8px 0; color: #8a6a75;">Assigned Specialist:</td>
            <td style="padding: 8px 0; text-align: right; font-weight: bold; color: #782944;">${staffStr}</td>
          </tr>
          <tr style="border-bottom: 1px solid #fce4ec;">
            <td style="padding: 8px 0; color: #8a6a75;">Date &amp; Time:</td>
            <td style="padding: 8px 0; text-align: right; font-weight: bold;">${dateStr} at ${timeStr}</td>
          </tr>
          <tr>
            <td style="padding: 8px 0; color: #8a6a75;">Reference ID:</td>
            <td style="padding: 8px 0; text-align: right; font-weight: bold; color: #d81b60;">#${booking.id}</td>
          </tr>
        </table>
      </div>

      <div style="background: #fff8fa; padding: 15px; border-radius: 8px; border-left: 4px solid #782944; margin-bottom: 20px;">
        <h4 style="margin: 0 0 5px; color: #782944; font-size: 13px;">📌 Sanctuary Arrival Guidelines</h4>
        <p style="margin: 0; font-size: 12px; color: #6a4855; line-height: 1.5;">
          Please arrive 10 minutes prior to your session. Enjoy complimentary French Moët champagne or organic herbal tea in our relaxation lounge upon arrival.
        </p>
      </div>

      <div style="text-align: center; border-top: 1px solid #f2cfd8; padding-top: 15px; font-size: 12px; color: #8a6a75;">
        <p style="margin: 0;">Warmest regards,<br/><strong>The Luxe Glow Studio Concierge Team</strong><br/>124 Luxury Boulevard, Suite 400 • concierge@luxeglowstudio.com</p>
      </div>
    </div>
  `;
};

export const getStoredNotifications = (userEmail) => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    const list = JSON.parse(raw);
    if (!userEmail) return list;
    return list.filter(
      (n) => n.userEmail?.toLowerCase() === userEmail.toLowerCase()
    );
  } catch (err) {
    console.error('Error loading notifications:', err);
    return [];
  }
};

export const saveStoredNotification = (notifData) => {
  try {
    const existing = getStoredNotifications();
    const id = `NOTIF-${Date.now()}-${Math.floor(Math.random() * 1000)}`;

    let htmlTemplate = notifData.templateData;
    if (!htmlTemplate && notifData.booking) {
      htmlTemplate = buildConfirmationTemplate(notifData.booking);
    }

    const newNotif = {
      id,
      userId: notifData.userId || 0,
      userEmail: notifData.userEmail || notifData.recipientEmail || '',
      title: notifData.title || 'Luxe Glow Studio Notification',
      message: notifData.message || '',
      type: notifData.type || 'BookingNotification',
      priority: notifData.priority || 'Medium',
      isRead: false,
      templateData: htmlTemplate || null,
      createdAt: new Date().toISOString()
    };

    const updated = [newNotif, ...existing];
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    window.dispatchEvent(new Event('luxe_notifications_updated'));
    return newNotif;
  } catch (err) {
    console.error('Error saving notification:', err);
    return null;
  }
};

export const markStoredNotificationAsRead = (notifId) => {
  try {
    const existing = getStoredNotifications();
    const updated = existing.map((item) =>
      item.id === notifId ? { ...item, isRead: true, readAt: new Date().toISOString() } : item
    );
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    window.dispatchEvent(new Event('luxe_notifications_updated'));
  } catch (err) {
    console.error('Error marking notification read:', err);
  }
};

export const markAllStoredNotificationsAsRead = (userEmail) => {
  try {
    const existing = getStoredNotifications();
    const updated = existing.map((item) => {
      if (!userEmail || item.userEmail?.toLowerCase() === userEmail.toLowerCase()) {
        return { ...item, isRead: true, readAt: new Date().toISOString() };
      }
      return item;
    });
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    window.dispatchEvent(new Event('luxe_notifications_updated'));
  } catch (err) {
    console.error('Error marking all notifications read:', err);
  }
};

export const deleteStoredNotification = (notifId) => {
  try {
    const existing = getStoredNotifications();
    const updated = existing.filter((item) => item.id !== notifId);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
    window.dispatchEvent(new Event('luxe_notifications_updated'));
  } catch (err) {
    console.error('Error deleting notification:', err);
  }
};
