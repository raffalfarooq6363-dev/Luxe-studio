import React, { useCallback, useEffect, useState } from 'react';
import { Link, Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { getAuthErrorMessage } from '../../services/authService';
import bookingService from '../../services/bookingService';
import notificationService from '../../services/notificationService';
import { cancelStoredBooking, getStoredBookings, rescheduleStoredBooking } from '../../data/bookingStore';
import { timeSlots } from '../../data/store';
import BookingModal from '../../components/BookingModal';
import './CustomerDashboard.css';

// 6 Curated Luxury Avatars
const LUXURY_AVATARS = [
  'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80',
  'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&auto=format&fit=crop&q=80',
  'https://images.unsplash.com/photo-1580489944761-15a19d654956?w=150&auto=format&fit=crop&q=80',
  'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=150&auto=format&fit=crop&q=80',
  'https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?w=150&auto=format&fit=crop&q=80',
  'https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&auto=format&fit=crop&q=80'
];

const CustomerDashboard = () => {
  const { user, updateProfile } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [bookings, setBookings] = useState([]);
  const [isBookingOpen, setIsBookingOpen] = useState(false);
  const [unreadMailsCount, setUnreadMailsCount] = useState(0);

  // Filter state for appointments
  const [appointmentFilter, setAppointmentFilter] = useState('All');

  // Modals state
  const [rescheduleTarget, setRescheduleTarget] = useState(null);
  const [rescheduleDate, setRescheduleDate] = useState('');
  const [rescheduleSlot, setRescheduleSlot] = useState(timeSlots[0]);
  const [rescheduleReason, setRescheduleReason] = useState('');
  const [rescheduleLoading, setRescheduleLoading] = useState(false);

  const [passTarget, setPassTarget] = useState(null);

  const [reviewTarget, setReviewTarget] = useState(null);
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewText, setReviewText] = useState('');
  const [reviewSubmittedMessage, setReviewSubmittedMessage] = useState('');

  const [copiedVoucher, setCopiedVoucher] = useState('');

  const [profileForm, setProfileForm] = useState({
    firstName: user?.firstName || '',
    lastName: user?.lastName || '',
    phoneNumber: user?.phoneNumber || '',
    address: user?.address || '',
    city: user?.city || '',
    state: user?.state || '',
    postalCode: user?.postalCode || '',
    profileImageUrl: user?.profileImageUrl || '',
    skinType: 'Normal / Glowing',
    favoriteRitual: '24K Gold Luxury Facial'
  });
  const [profileMessage, setProfileMessage] = useState('');
  const [profileError, setProfileError] = useState('');

  const loadBookings = useCallback(async () => {
    const storedBookings = getStoredBookings();
    const localBookings = user?.email
      ? storedBookings.filter((booking) => booking.customerEmail?.toLowerCase() === user.email.toLowerCase())
      : [];
    if (!user?.id) {
      setBookings(localBookings);
      return;
    }

    try {
      const apiBookings = await bookingService.getUserBookings(user.id);
      setBookings(apiBookings.map((booking) => ({
        id: booking.id,
        serviceName: booking.serviceName,
        practitionerName: booking.assignedStaffMember || 'Luxe Glow Specialist',
        appointmentDate: booking.appointmentDate?.split('T')[0] || '',
        timeSlot: booking.startTime,
        serviceType: 'Studio Appointment',
        totalPrice: booking.totalAmount,
        status: booking.status,
        customerEmail: booking.userEmail,
      })));
    } catch {
      setBookings(localBookings);
    }
  }, [user]);

  const updateUnreadBadge = useCallback(async () => {
    if (user?.email || user?.id) {
      const data = await notificationService.getMyNotifications(user?.id, user?.email);
      const unread = (data || []).filter((m) => !m.isRead).length;
      setUnreadMailsCount(unread);
    }
  }, [user]);

  useEffect(() => {
    loadBookings();
    updateUnreadBadge();
    window.addEventListener('luxe_bookings_updated', loadBookings);
    window.addEventListener('luxe_notifications_updated', updateUnreadBadge);
    const refreshTimer = window.setInterval(() => {
      loadBookings();
      updateUnreadBadge();
    }, 15000);
    return () => {
      window.removeEventListener('luxe_bookings_updated', loadBookings);
      window.removeEventListener('luxe_notifications_updated', updateUnreadBadge);
      window.clearInterval(refreshTimer);
    };
  }, [loadBookings, updateUnreadBadge]);

  useEffect(() => {
    setProfileForm((prev) => ({
      ...prev,
      firstName: user?.firstName || '',
      lastName: user?.lastName || '',
      phoneNumber: user?.phoneNumber || '',
      address: user?.address || '',
      city: user?.city || '',
      state: user?.state || '',
      postalCode: user?.postalCode || '',
      profileImageUrl: user?.profileImageUrl || '',
    }));
  }, [user]);

  const cancelBooking = async (bookingId) => {
    const reason = window.prompt('Please let us know the reason for cancellation (optional):');
    if (reason === null) return;
    try {
      if (typeof bookingId === 'number') {
        await bookingService.updateStatus(bookingId, 'Cancelled', reason || 'Customer cancellation');
      } else {
        cancelStoredBooking(bookingId);
      }
      await loadBookings();
      alert(`Booking #${bookingId} has been cancelled.`);
    } catch (error) {
      setProfileError(getAuthErrorMessage(error, 'Unable to cancel this booking.'));
    }
  };

  const openReschedule = (booking) => {
    setRescheduleTarget(booking);
    setRescheduleDate(booking.appointmentDate || new Date(Date.now() + 86400000).toISOString().split('T')[0]);
    setRescheduleSlot(booking.timeSlot || timeSlots[0]);
    setRescheduleReason('');
  };

  const handleRescheduleSubmit = async (e) => {
    e.preventDefault();
    if (!rescheduleTarget) return;
    setRescheduleLoading(true);
    try {
      if (typeof rescheduleTarget.id === 'number') {
        const match = rescheduleSlot.match(/^(\d{1,2}):(\d{2})\s?(AM|PM)$/i);
        let hour = Number(match ? match[1] : 9);
        const mins = match ? match[2] : '00';
        const period = match ? match[3].toUpperCase() : 'AM';
        if (period === 'PM' && hour !== 12) hour += 12;
        if (period === 'AM' && hour === 12) hour = 0;
        const timeSpanStr = `${String(hour).padStart(2, '0')}:${mins}:00`;

        await bookingService.rescheduleBooking(rescheduleTarget.id, {
          newAppointmentDate: `${rescheduleDate}T00:00:00Z`,
          newStartTime: timeSpanStr,
          reason: rescheduleReason || 'Customer rescheduled via dashboard'
        });
      } else {
        rescheduleStoredBooking(rescheduleTarget.id, rescheduleDate, rescheduleSlot);
      }
      await loadBookings();
      alert(`✨ Appointment #${rescheduleTarget.id} successfully rescheduled to ${rescheduleDate} at ${rescheduleSlot}!`);
      setRescheduleTarget(null);
    } catch (err) {
      alert('Unable to reschedule. Please choose another date or contact the concierge.');
    } finally {
      setRescheduleLoading(false);
    }
  };

  const saveProfile = async (event) => {
    event.preventDefault();
    setProfileMessage('');
    setProfileError('');
    try {
      await updateProfile(profileForm);
      setProfileMessage('Profile updated successfully.');
      setTimeout(() => setProfileMessage(''), 4000);
    } catch (error) {
      setProfileError(getAuthErrorMessage(error, 'Unable to update your profile right now.'));
    }
  };

  const handleReviewSubmit = (e) => {
    e.preventDefault();
    setReviewSubmittedMessage(`Thank you for your ${reviewRating}-star rating! Your feedback has been shared with the concierge.`);
    setTimeout(() => {
      setReviewSubmittedMessage('');
      setReviewTarget(null);
    }, 2500);
  };

  const handleCopyVoucher = (code) => {
    navigator.clipboard.writeText(code);
    setCopiedVoucher(code);
    setTimeout(() => setCopiedVoucher(''), 3000);
  };

  const upcoming = bookings.filter((booking) => ['Pending', 'Confirmed'].includes(booking.status));
  const displayName = user?.firstName ? `${user.firstName} ${user.lastName || ''}`.trim() : 'Valued Guest';

  const glowPoints = bookings.length * 50;
  const tierName = glowPoints >= 350 ? 'Diamond Haute VIP' : glowPoints >= 150 ? 'Gold VIP Club' : 'Silver Member';
  const tierProgress = Math.min(100, Math.round((glowPoints / 350) * 100));

  const filteredBookings = bookings.filter((b) => {
    if (appointmentFilter === 'All') return true;
    if (appointmentFilter === 'Upcoming') return ['Pending', 'Confirmed'].includes(b.status);
    return b.status?.toLowerCase() === appointmentFilter.toLowerCase();
  });

  return (
    <div className="customer-dashboard-page">
      <div className="container dashboard-container">
        {/* SIDEBAR */}
        <aside className="dashboard-sidebar-luxe">
          <div className="user-profile-header">
            <div className="user-avatar-circle">
              {profileForm.profileImageUrl ? (
                <img src={profileForm.profileImageUrl} alt={displayName} />
              ) : (
                displayName[0].toUpperCase()
              )}
            </div>
            <span className="user-tier-badge">✨ {tierName}</span>
            <h3 className="user-name">{displayName}</h3>
            <p className="user-email-text">{user?.email || ''}</p>
          </div>

          <nav className="sidebar-nav-menu">
            <Link
              className={`nav-tab-item ${location.pathname === '/customer' ? 'active' : ''}`}
              to="/customer"
            >
              📊 Overview &amp; Stats
            </Link>
            <Link
              className={`nav-tab-item ${location.pathname === '/customer/bookings' ? 'active' : ''}`}
              to="/customer/bookings"
            >
              📅 My Appointments
            </Link>
            <Link
              className={`nav-tab-item ${location.pathname === '/customer/rewards' ? 'active' : ''}`}
              to="/customer/rewards"
            >
              🎁 Glow Rewards ({glowPoints} pts)
            </Link>
            <Link
              className={`nav-tab-item ${location.pathname === '/customer/inbox' ? 'active' : ''}`}
              to="/customer/inbox"
            >
              <span>📬 My Mails / Inbox</span>
              {unreadMailsCount > 0 && <span className="nav-unread-pill">{unreadMailsCount}</span>}
            </Link>
            <button
              type="button"
              className="nav-tab-item book-now-tab"
              onClick={() => setIsBookingOpen(true)}
            >
              ✨ Reserve New Ritual
            </button>
            <Link
              className={`nav-tab-item ${location.pathname === '/customer/profile' ? 'active' : ''}`}
              to="/customer/profile"
            >
              ⚙️ Profile Settings
            </Link>
          </nav>
        </aside>

        {/* MAIN CONTENT ROUTING */}
        <main className="dashboard-main-content">
          <Routes>
            {/* 1. OVERVIEW */}
            <Route
              index
              element={
                <div className="overview-view">
                  <div className="dashboard-banner">
                    <div>
                      <h2>Welcome Back, {user?.firstName || 'Beautiful'}!</h2>
                      <p>Your sanctuary of bespoke aesthetics, beauty rituals, and relaxation.</p>
                    </div>
                    <button className="btn btn-primary" onClick={() => setIsBookingOpen(true)}>
                      Reserve New Session
                    </button>
                  </div>

                  <div className="stats-row-luxe">
                    <div className="stat-box">
                      <span className="stat-title">Upcoming Treatments</span>
                      <span className="stat-val">{upcoming.length}</span>
                    </div>
                    <div className="stat-box">
                      <span className="stat-title">Total Rituals</span>
                      <span className="stat-val">{bookings.length}</span>
                    </div>
                    <div className="stat-box">
                      <span className="stat-title">Glow Points</span>
                      <span className="stat-val">{glowPoints} pts</span>
                    </div>
                  </div>

                  <section className="dashboard-card-section">
                    <div className="section-head-bar">
                      <h3>Upcoming Appointments</h3>
                      <Link to="/customer/bookings">View All &amp; Manage</Link>
                    </div>
                    {upcoming.length === 0 ? (
                      <div className="empty-dashboard-state">
                        <h4>No upcoming appointments</h4>
                        <p style={{ color: '#8c7385', marginBottom: '1.25rem' }}>Indulge in a 24K Gold Facial, Hydra-Dew treatment, or bespoke hair spa.</p>
                        <button className="btn btn-primary" onClick={() => setIsBookingOpen(true)}>
                          Book Your First Service
                        </button>
                      </div>
                    ) : (
                      <div className="bookings-cards-grid">
                        {upcoming.map((booking) => (
                          <BookingCard
                            key={booking.id}
                            booking={booking}
                            onCancel={cancelBooking}
                            onReschedule={openReschedule}
                            onPass={setPassTarget}
                            onReview={setReviewTarget}
                          />
                        ))}
                      </div>
                    )}
                  </section>
                </div>
              }
            />

            {/* 2. MY APPOINTMENTS */}
            <Route
              path="bookings"
              element={
                <div className="bookings-view">
                  <div className="section-head-bar">
                    <div>
                      <h2>My Appointments &amp; Rituals</h2>
                      <p className="subtitle-txt">Review past reservations, reschedule, or print your official studio pass.</p>
                    </div>
                    <button className="btn btn-primary" onClick={() => setIsBookingOpen(true)}>
                      New Booking
                    </button>
                  </div>

                  {/* Filter Pills */}
                  <div className="booking-filters-bar">
                    {['All', 'Upcoming', 'Confirmed', 'Pending', 'Completed', 'Cancelled'].map((f) => (
                      <button
                        key={f}
                        type="button"
                        className={`filter-pill ${appointmentFilter === f ? 'active' : ''}`}
                        onClick={() => setAppointmentFilter(f)}
                      >
                        {f}
                      </button>
                    ))}
                  </div>

                  <div className="all-bookings-list">
                    {filteredBookings.length ? (
                      filteredBookings.map((booking) => (
                        <BookingCard
                          key={booking.id}
                          booking={booking}
                          onCancel={cancelBooking}
                          onReschedule={openReschedule}
                          onPass={setPassTarget}
                          onReview={setReviewTarget}
                        />
                      ))
                    ) : (
                      <p style={{ padding: '2rem', textAlign: 'center', color: '#8c7385', background: '#fff', borderRadius: '16px' }}>
                        No appointments found matching "{appointmentFilter}".
                      </p>
                    )}
                  </div>
                </div>
              }
            />

            {/* 3. GLOW REWARDS HUB */}
            <Route
              path="rewards"
              element={
                <div className="rewards-hub-view">
                  <div className="rewards-tier-hero">
                    <div>
                      <div className="tier-title-row">
                        <span className="tier-badge-pill">LUXURY CLUB</span>
                        <h2 style={{ margin: 0, fontSize: '1.8rem' }}>{tierName}</h2>
                      </div>
                      <p style={{ color: '#ffd7e0', margin: '0.25rem 0 1rem' }}>
                        Earn 50 Glow Points for every completed beauty ritual. Redeem points for exclusive complimentary enhancements.
                      </p>
                      <div className="tier-progress-track">
                        <div className="tier-progress-fill" style={{ width: `${tierProgress}%` }} />
                      </div>
                      <small style={{ color: '#ffd700' }}>
                        {glowPoints >= 350 ? 'You have unlocked the highest Diamond Tier!' : `${350 - glowPoints} points until Diamond Haute VIP`}
                      </small>
                    </div>

                    <div className="points-balance-display">
                      <span className="pts-sub">Current Balance</span>
                      <span className="pts-num">{glowPoints}</span>
                      <span className="pts-sub">Glow Loyalty Points</span>
                    </div>
                  </div>

                  <div className="section-head-bar">
                    <div>
                      <h3>Exclusive Redeemable Vouchers</h3>
                      <p className="subtitle-txt">Copy your exclusive codes and mention them upon arrival or booking.</p>
                    </div>
                  </div>

                  <div className="vouchers-grid-cards">
                    <div className="reward-voucher-card">
                      <span className="voucher-discount-val">$20 OFF</span>
                      <h4>24K Gold Luxury Facial</h4>
                      <p>Enjoy $20 off your next gold foil infused micro-needling &amp; lymphatic drainage ritual.</p>
                      <div className="voucher-code-copy-row">
                        <code>GLOW20</code>
                        <button
                          type="button"
                          className="copy-code-btn"
                          onClick={() => handleCopyVoucher('GLOW20')}
                        >
                          {copiedVoucher === 'GLOW20' ? '✓ Copied' : 'Copy Code'}
                        </button>
                      </div>
                    </div>

                    <div className="reward-voucher-card">
                      <span className="voucher-discount-val">COMPLIMENTARY</span>
                      <h4>Rose Quartz Aromatherapy</h4>
                      <p>Free upgrade to warm organic Himalayan rose stone treatment on your next massage.</p>
                      <div className="voucher-code-copy-row">
                        <code>ROSEVIP</code>
                        <button
                          type="button"
                          className="copy-code-btn"
                          onClick={() => handleCopyVoucher('ROSEVIP')}
                        >
                          {copiedVoucher === 'ROSEVIP' ? '✓ Copied' : 'Copy Code'}
                        </button>
                      </div>
                    </div>

                    <div className="reward-voucher-card">
                      <span className="voucher-discount-val">VIP WELCOME</span>
                      <h4>French Moët Champagne</h4>
                      <p>Sip fine French champagne and chilled detox tonics upon arriving in your private suite.</p>
                      <div className="voucher-code-copy-row">
                        <code>CHAMPAGNE</code>
                        <button
                          type="button"
                          className="copy-code-btn"
                          onClick={() => handleCopyVoucher('CHAMPAGNE')}
                        >
                          {copiedVoucher === 'CHAMPAGNE' ? '✓ Copied' : 'Copy Code'}
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              }
            />

            {/* 4. IN-APP INBOX */}
            <Route
              path="inbox"
              element={<CustomerInboxView userId={user?.id} userEmail={user?.email} onUnreadCount={setUnreadMailsCount} />}
            />

            {/* 5. PROFILE & PREFERENCES */}
            <Route
              path="profile"
              element={
                <div className="profile-view">
                  <h2>Profile Settings &amp; Beauty Persona</h2>
                  <p className="subtitle-txt">Customize your avatar, skin care preferences, and contact details.</p>
                  {profileMessage && <p className="dashboard-success">{profileMessage}</p>}
                  {profileError && <p className="dashboard-error">{profileError}</p>}

                  <form className="profile-card-form" onSubmit={saveProfile}>
                    <div className="profile-image-preview">
                      {profileForm.profileImageUrl ? (
                        <img src={profileForm.profileImageUrl} alt="Profile preview" />
                      ) : (
                        displayName[0].toUpperCase()
                      )}
                    </div>

                    <label style={{ marginBottom: '0.4rem', display: 'block', fontWeight: 700, fontSize: '0.85rem' }}>
                      Pick a Luxury Avatar
                    </label>
                    <div className="avatar-preset-picker">
                      {LUXURY_AVATARS.map((url, idx) => (
                        <div
                          key={idx}
                          className={`avatar-preset-chip ${profileForm.profileImageUrl === url ? 'active' : ''}`}
                          onClick={() => setProfileForm({ ...profileForm, profileImageUrl: url })}
                        >
                          <img src={url} alt={`Avatar ${idx + 1}`} />
                        </div>
                      ))}
                    </div>

                    <label>
                      Or Custom Image URL
                      <input
                        type="url"
                        placeholder="https://example.com/your-image.jpg"
                        value={profileForm.profileImageUrl}
                        onChange={(event) => setProfileForm({ ...profileForm, profileImageUrl: event.target.value })}
                      />
                    </label>

                    <div className="profile-form-grid">
                      <label>
                        First Name
                        <input
                          type="text"
                          value={profileForm.firstName}
                          onChange={(event) => setProfileForm({ ...profileForm, firstName: event.target.value })}
                          required
                        />
                      </label>
                      <label>
                        Last Name
                        <input
                          type="text"
                          value={profileForm.lastName}
                          onChange={(event) => setProfileForm({ ...profileForm, lastName: event.target.value })}
                          required
                        />
                      </label>
                    </div>

                    <label>
                      Email Address
                      <input type="email" value={user?.email || ''} disabled />
                    </label>

                    <label>
                      Phone Number
                      <input
                        type="tel"
                        value={profileForm.phoneNumber}
                        onChange={(event) => setProfileForm({ ...profileForm, phoneNumber: event.target.value })}
                      />
                    </label>

                    <div className="profile-form-grid">
                      <label>
                        Skin Type &amp; Barrier Profile
                        <select
                          value={profileForm.skinType}
                          onChange={(e) => setProfileForm({ ...profileForm, skinType: e.target.value })}
                        >
                          <option>Normal / Glowing</option>
                          <option>Dry &amp; Dehydrated</option>
                          <option>Sensitive / Rosacea Prone</option>
                          <option>Oily &amp; Blemish Prone</option>
                          <option>Combination</option>
                        </select>
                      </label>
                      <label>
                        Favorite Treatment Ritual
                        <select
                          value={profileForm.favoriteRitual}
                          onChange={(e) => setProfileForm({ ...profileForm, favoriteRitual: e.target.value })}
                        >
                          <option>24K Gold Luxury Facial</option>
                          <option>Hydra-Dew Glass Skin Facial</option>
                          <option>Royal Bridal Glow &amp; Makeup</option>
                          <option>Silk Infusion Hair Spa</option>
                          <option>Rose Quartz Aromatherapy Spa</option>
                        </select>
                      </label>
                    </div>

                    <label>
                      Address
                      <input
                        type="text"
                        value={profileForm.address}
                        onChange={(event) => setProfileForm({ ...profileForm, address: event.target.value })}
                      />
                    </label>

                    <div className="profile-form-grid">
                      <label>
                        City
                        <input
                          type="text"
                          value={profileForm.city}
                          onChange={(event) => setProfileForm({ ...profileForm, city: event.target.value })}
                        />
                      </label>
                      <label>
                        State
                        <input
                          type="text"
                          value={profileForm.state}
                          onChange={(event) => setProfileForm({ ...profileForm, state: event.target.value })}
                        />
                      </label>
                    </div>

                    <label>
                      Postal Code
                      <input
                        type="text"
                        value={profileForm.postalCode}
                        onChange={(event) => setProfileForm({ ...profileForm, postalCode: event.target.value })}
                      />
                    </label>

                    <button className="btn btn-primary" type="submit">
                      Save Profile &amp; Preferences
                    </button>
                  </form>
                </div>
              }
            />

            <Route path="*" element={<Navigate to="/customer" replace />} />
          </Routes>
        </main>
      </div>

      {/* ========================================================
          RESCHEDULE APPOINTMENT MODAL
          ======================================================== */}
      {rescheduleTarget && (
        <div className="modal-backdrop-luxe">
          <div className="luxe-modal-box">
            <button
              type="button"
              className="modal-close-icon"
              onClick={() => setRescheduleTarget(null)}
            >
              ✕
            </button>
            <div className="modal-header-luxe">
              <h3>Reschedule Appointment</h3>
              <p>Pick a convenient new date and slot for your <strong>{rescheduleTarget.serviceName}</strong>.</p>
            </div>

            <form onSubmit={handleRescheduleSubmit}>
              <div className="luxe-form-group">
                <label>Current Appointment</label>
                <input
                  type="text"
                  disabled
                  value={`${rescheduleTarget.appointmentDate} at ${rescheduleTarget.timeSlot}`}
                />
              </div>

              <div className="luxe-form-group">
                <label>New Preferred Date</label>
                <input
                  type="date"
                  min={new Date().toISOString().split('T')[0]}
                  value={rescheduleDate}
                  onChange={(e) => setRescheduleDate(e.target.value)}
                  required
                />
              </div>

              <div className="luxe-form-group">
                <label>New Time Slot</label>
                <select
                  value={rescheduleSlot}
                  onChange={(e) => setRescheduleSlot(e.target.value)}
                  required
                >
                  {timeSlots.map((slot) => (
                    <option key={slot} value={slot}>
                      {slot}
                    </option>
                  ))}
                </select>
              </div>

              <div className="luxe-form-group">
                <label>Reason for Rescheduling (Optional)</label>
                <textarea
                  rows="2"
                  placeholder="e.g., Change in schedule, conflict..."
                  value={rescheduleReason}
                  onChange={(e) => setRescheduleReason(e.target.value)}
                />
              </div>

              <div style={{ display: 'flex', gap: '1rem', marginTop: '1.5rem' }}>
                <button
                  type="button"
                  className="btn btn-outline-primary"
                  style={{ flex: 1 }}
                  onClick={() => setRescheduleTarget(null)}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                  style={{ flex: 1 }}
                  disabled={rescheduleLoading}
                >
                  {rescheduleLoading ? 'Confirming...' : 'Confirm Reschedule'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ========================================================
          LUXURY PRINTABLE APPOINTMENT PASS MODAL
          ======================================================== */}
      {passTarget && (
        <div className="modal-backdrop-luxe">
          <div className="luxe-modal-box" style={{ maxWidth: '620px' }}>
            <button
              type="button"
              className="modal-close-icon"
              onClick={() => setPassTarget(null)}
            >
              ✕
            </button>

            <div className="pass-ticket-wrap">
              <div className="pass-header-brand">
                <h2>LUXE GLOW STUDIO</h2>
                <span>HAUTE WELLNESS &amp; AESTHETICS SANCTUARY</span>
              </div>

              <div className="pass-grid-info">
                <div className="pass-info-cell">
                  <span>Guest Name</span>
                  <strong>{passTarget.customerName || displayName}</strong>
                </div>
                <div className="pass-info-cell">
                  <span>Pass Reference</span>
                  <strong>#{passTarget.id}</strong>
                </div>
                <div className="pass-info-cell">
                  <span>Treatment Ritual</span>
                  <strong>{passTarget.serviceName}</strong>
                </div>
                <div className="pass-info-cell">
                  <span>Assigned Specialist</span>
                  <strong>{passTarget.practitionerName}</strong>
                </div>
                <div className="pass-info-cell">
                  <span>Reservation Date</span>
                  <strong>{passTarget.appointmentDate}</strong>
                </div>
                <div className="pass-info-cell">
                  <span>Arrival Time Slot</span>
                  <strong>{passTarget.timeSlot}</strong>
                </div>
                <div className="pass-info-cell">
                  <span>Status</span>
                  <strong style={{ color: passTarget.status === 'Confirmed' ? '#2e7d32' : '#e65100' }}>
                    {passTarget.status}
                  </strong>
                </div>
                <div className="pass-info-cell">
                  <span>Amount Due / Paid</span>
                  <strong style={{ color: '#d81b60' }}>${passTarget.totalPrice}</strong>
                </div>
              </div>

              <div className="pass-guidelines">
                <strong>Studio Guidelines &amp; Amenities:</strong>
                <ul>
                  <li>Please arrive 10 minutes before your slot to enjoy our complimentary welcome elixir or champagne.</li>
                  <li>Studio Address: 124 Luxury Boulevard, Suite 400, Beverly Hills.</li>
                  <li>Valet parking is complimentary for all Luxe Glow Studio reservations.</li>
                </ul>
              </div>

              <div className="pass-actions-row">
                <button
                  type="button"
                  className="btn btn-outline-primary"
                  onClick={() => setPassTarget(null)}
                >
                  Close
                </button>
                <button
                  type="button"
                  className="btn btn-primary"
                  onClick={() => window.print()}
                >
                  🖨️ Print Pass / Save PDF
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* ========================================================
          LEAVE REVIEW MODAL
          ======================================================== */}
      {reviewTarget && (
        <div className="modal-backdrop-luxe">
          <div className="luxe-modal-box">
            <button
              type="button"
              className="modal-close-icon"
              onClick={() => setReviewTarget(null)}
            >
              ✕
            </button>
            <div className="modal-header-luxe">
              <h3>Share Your Experience</h3>
              <p>How was your <strong>{reviewTarget.serviceName}</strong> with {reviewTarget.practitionerName}?</p>
            </div>

            {reviewSubmittedMessage ? (
              <div className="dashboard-success" style={{ padding: '1.5rem', textAlign: 'center' }}>
                {reviewSubmittedMessage}
              </div>
            ) : (
              <form onSubmit={handleReviewSubmit}>
                <div className="luxe-form-group">
                  <label>Overall Rating</label>
                  <div className="star-rating-selector">
                    {[1, 2, 3, 4, 5].map((star) => (
                      <span
                        key={star}
                        style={{ color: star <= reviewRating ? '#ffd700' : '#e0e0e0', cursor: 'pointer' }}
                        onClick={() => setReviewRating(star)}
                      >
                        ★
                      </span>
                    ))}
                  </div>
                </div>

                <div className="luxe-form-group">
                  <label>Your Feedback &amp; Radiance Results</label>
                  <textarea
                    rows="4"
                    required
                    placeholder="Tell us about the ambiance, practitioner care, and treatment results..."
                    value={reviewText}
                    onChange={(e) => setReviewText(e.target.value)}
                  />
                </div>

                <div style={{ display: 'flex', gap: '1rem', marginTop: '1.5rem' }}>
                  <button
                    type="button"
                    className="btn btn-outline-primary"
                    style={{ flex: 1 }}
                    onClick={() => setReviewTarget(null)}
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary" style={{ flex: 1 }}>
                    Submit Review
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      )}

      <BookingModal
        isOpen={isBookingOpen}
        onClose={() => setIsBookingOpen(false)}
        onBookingSuccess={() => {
          loadBookings();
          navigate('/customer/bookings');
        }}
      />
    </div>
  );
};

const CustomerInboxView = ({ userId, userEmail, onUnreadCount }) => {
  const [mails, setMails] = useState([]);
  const [selectedMail, setSelectedMail] = useState(null);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all');

  const loadMails = useCallback(async () => {
    setLoading(true);
    try {
      const data = await notificationService.getMyNotifications(userId, userEmail);
      setMails(data || []);
      const unread = (data || []).filter((m) => !m.isRead).length;
      if (onUnreadCount) onUnreadCount(unread);
      if (data && data.length > 0 && !selectedMail) {
        setSelectedMail(data[0]);
      }
    } catch {
      setMails([]);
    } finally {
      setLoading(false);
    }
  }, [userId, userEmail, onUnreadCount, selectedMail]);

  useEffect(() => {
    loadMails();
    window.addEventListener('luxe_notifications_updated', loadMails);
    const interval = setInterval(loadMails, 5000);
    return () => {
      window.removeEventListener('luxe_notifications_updated', loadMails);
      clearInterval(interval);
    };
  }, [loadMails]);

  const handleSelectMail = async (mail) => {
    setSelectedMail(mail);
    if (!mail.isRead) {
      try {
        await notificationService.markAsRead(mail.id);
        setMails((prev) =>
          prev.map((m) => (m.id === mail.id ? { ...m, isRead: true } : m))
        );
        if (onUnreadCount) {
          onUnreadCount((prev) => Math.max(0, prev - 1));
        }
      } catch (err) {
        console.error('Failed to mark read', err);
      }
    }
  };

  const handleMarkAllRead = async () => {
    try {
      await notificationService.markAllAsRead(userId, userEmail);
      setMails((prev) => prev.map((m) => ({ ...m, isRead: true })));
      if (onUnreadCount) onUnreadCount(0);
    } catch (err) {
      console.error('Failed to mark all as read', err);
    }
  };

  const handleDelete = async (id, e) => {
    e.stopPropagation();
    try {
      await notificationService.deleteNotification(id);
      const remaining = mails.filter((m) => m.id !== id);
      setMails(remaining);
      if (selectedMail?.id === id) {
        setSelectedMail(remaining[0] || null);
      }
    } catch (err) {
      console.error('Failed to delete', err);
    }
  };

  const filteredMails = mails.filter((m) => {
    if (filter === 'unread') return !m.isRead;
    if (filter === 'confirmed') return m.type?.toLowerCase().includes('confirmed') || m.title?.toLowerCase().includes('confirmed');
    return true;
  });

  return (
    <div className="inbox-view-container">
      <div className="section-head-bar">
        <div>
          <h2>📬 My Mailbox</h2>
          <p className="subtitle-txt">
            Your booking requests, updates, and official confirmation emails arrive here.
          </p>
        </div>
        <div style={{ display: 'flex', gap: '8px' }}>
          <button className="btn btn-secondary" onClick={loadMails} style={{ padding: '0.4rem 0.8rem', fontSize: '0.85rem' }}>🔄 Refresh</button>
          {mails.some((m) => !m.isRead) && (
            <button className="btn btn-secondary" onClick={handleMarkAllRead} style={{ padding: '0.4rem 0.8rem', fontSize: '0.85rem' }}>✓ Mark All Read</button>
          )}
        </div>
      </div>

      <div className="inbox-filters-bar">
        <button
          className={`inbox-filter-tab ${filter === 'all' ? 'active' : ''}`}
          onClick={() => setFilter('all')}
        >
          All Mails ({mails.length})
        </button>
        <button
          className={`inbox-filter-tab ${filter === 'unread' ? 'active' : ''}`}
          onClick={() => setFilter('unread')}
        >
          Unread ({mails.filter((m) => !m.isRead).length})
        </button>
        <button
          className={`inbox-filter-tab ${filter === 'confirmed' ? 'active' : ''}`}
          onClick={() => setFilter('confirmed')}
        >
          Confirmations ({mails.filter((m) => m.type?.toLowerCase().includes('confirmed') || m.title?.toLowerCase().includes('confirmed')).length})
        </button>
      </div>

      {loading && mails.length === 0 ? (
        <div className="admin-loading">Loading your mailbox...</div>
      ) : mails.length === 0 ? (
        <div className="empty-dashboard-state">
          <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>📫</div>
          <h4>Your Mailbox is Empty</h4>
          <p>When you book a service or when the admin confirms your booking, official emails will arrive right here!</p>
        </div>
      ) : (
        <div className="inbox-layout">
          <div className="inbox-list-pane">
            {filteredMails.length === 0 ? (
              <p style={{ padding: '2rem', textAlign: 'center', color: '#8c7385' }}>No mails match this filter.</p>
            ) : (
              filteredMails.map((mail) => (
                <div
                  key={mail.id}
                  className={`inbox-item-row ${selectedMail?.id === mail.id ? 'selected' : ''} ${!mail.isRead ? 'unread' : ''}`}
                  onClick={() => handleSelectMail(mail)}
                >
                  <div className="inbox-item-header">
                    <span className="inbox-sender-name">Luxe Glow Studio</span>
                    <span className="inbox-item-date">{new Date(mail.createdAt).toLocaleDateString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })}</span>
                  </div>
                  <div className="inbox-item-title">
                    {!mail.isRead && <span className="unread-dot" />}
                    <strong>{mail.title}</strong>
                  </div>
                  <p className="inbox-item-snippet">{mail.message}</p>
                  <div className="inbox-item-footer">
                    <span className={`status-pill ${mail.type?.toLowerCase().includes('confirmed') ? 'confirmed' : 'pending'}`}>
                      {mail.type?.replace('Booking', '') || 'Notification'}
                    </span>
                    <button
                      className="inbox-del-btn"
                      title="Delete email"
                      onClick={(e) => handleDelete(mail.id, e)}
                    >
                      🗑
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>

          <div className="inbox-reader-pane">
            {selectedMail ? (
              <div className="email-card-viewer">
                <div className="email-meta-header">
                  <div className="email-meta-from">
                    <div className="sender-avatar">L</div>
                    <div>
                      <h4>Luxe Glow Studio Concierge</h4>
                      <small>&lt;concierge@luxeglowstudio.com&gt;</small>
                    </div>
                  </div>
                  <div className="email-meta-date">
                    {new Date(selectedMail.createdAt).toLocaleString()}
                  </div>
                </div>
                <div className="email-meta-subject">
                  <h3>{selectedMail.title}</h3>
                </div>

                <div className="email-content-body">
                  {selectedMail.templateData ? (
                    <div
                      className="rendered-email-html"
                      dangerouslySetInnerHTML={{ __html: selectedMail.templateData }}
                    />
                  ) : (
                    <div style={{ padding: '1.5rem', lineHeight: '1.6', color: '#2c1825', background: '#fff', borderRadius: '12px', border: '1px solid #fce4ec' }}>
                      <p>{selectedMail.message}</p>
                    </div>
                  )}
                </div>
              </div>
            ) : (
              <div className="empty-reader-prompt">
                <p>Select an email from the left to read its complete contents.</p>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

const BookingCard = ({ booking, onCancel }) => <article className="customer-booking-card"><div className="booking-card-top"><div><span className="booking-badge-id">REF: {booking.id}</span><h4>{booking.serviceName}</h4><p className="practitioner-txt">With {booking.practitionerName}</p></div><span className={`status-badge ${booking.status.toLowerCase()}`}>{booking.status}</span></div><div className="booking-card-details"><div className="detail-item"><span>Date</span><strong>{booking.appointmentDate}</strong></div><div className="detail-item"><span>Time</span><strong>{booking.timeSlot}</strong></div><div className="detail-item"><span>Mode</span><strong>{booking.serviceType}</strong></div><div className="detail-item"><span>Price</span><strong className="price-tag">${booking.totalPrice}</strong></div></div>{booking.status === 'Confirmed' && <div className="booking-card-actions"><button className="btn-cancel-link" onClick={() => onCancel(booking.id)}>Cancel Booking</button></div>}</article>;

export default CustomerDashboard;
