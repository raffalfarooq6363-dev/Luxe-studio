import React, { useCallback, useEffect, useState } from 'react';
import { Link, Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { getAuthErrorMessage } from '../../services/authService';
import bookingService from '../../services/bookingService';
import { cancelStoredBooking, getStoredBookings } from '../../data/bookingStore';
import BookingModal from '../../components/BookingModal';
import './CustomerDashboard.css';

const CustomerDashboard = () => {
  const { user, updateProfile } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [bookings, setBookings] = useState([]);
  const [isBookingOpen, setIsBookingOpen] = useState(false);
  const [profileForm, setProfileForm] = useState({
    firstName: user?.firstName || '',
    lastName: user?.lastName || '',
    phoneNumber: user?.phoneNumber || '',
    address: user?.address || '',
    city: user?.city || '',
    state: user?.state || '',
    postalCode: user?.postalCode || '',
    profileImageUrl: user?.profileImageUrl || '',
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

  useEffect(() => {
    loadBookings();
    window.addEventListener('luxe_bookings_updated', loadBookings);
    const refreshTimer = window.setInterval(loadBookings, 15000);
    return () => {
      window.removeEventListener('luxe_bookings_updated', loadBookings);
      window.clearInterval(refreshTimer);
    };
  }, [loadBookings]);

  useEffect(() => {
    setProfileForm({
      firstName: user?.firstName || '',
      lastName: user?.lastName || '',
      phoneNumber: user?.phoneNumber || '',
      address: user?.address || '',
      city: user?.city || '',
      state: user?.state || '',
      postalCode: user?.postalCode || '',
      profileImageUrl: user?.profileImageUrl || '',
    });
  }, [user]);

  const cancelBooking = async (bookingId) => {
    if (window.confirm(`Cancel booking ${bookingId}?`)) {
      try {
        if (typeof bookingId === 'number') {
          await bookingService.cancelBooking(bookingId);
        } else {
          cancelStoredBooking(bookingId);
        }
        await loadBookings();
      } catch (error) {
        setProfileError(getAuthErrorMessage(error, 'Unable to cancel this booking.'));
      }
    }
  };

  const saveProfile = async (event) => {
    event.preventDefault();
    setProfileMessage('');
    setProfileError('');
    try {
      await updateProfile(profileForm);
      setProfileMessage('Profile updated successfully.');
    } catch (error) {
      setProfileError(getAuthErrorMessage(error, 'Unable to update your profile right now.'));
    }
  };

  const upcoming = bookings.filter((booking) => ['Pending', 'Confirmed'].includes(booking.status));
  const displayName = user?.firstName ? `${user.firstName} ${user.lastName || ''}`.trim() : 'Valued Guest';

  return (
    <div className="customer-dashboard-page"><div className="container dashboard-container">
      <aside className="dashboard-sidebar-luxe"><div className="user-profile-header"><div className="user-avatar-circle">{user?.profileImageUrl ? <img src={user.profileImageUrl} alt={displayName} /> : displayName[0].toUpperCase()}</div><h3 className="user-name">{displayName}</h3><p className="user-email-text">{user?.email || ''}</p></div><nav className="sidebar-nav-menu"><Link className={`nav-tab-item ${location.pathname === '/customer' ? 'active' : ''}`} to="/customer">Overview &amp; Stats</Link><Link className={`nav-tab-item ${location.pathname === '/customer/bookings' ? 'active' : ''}`} to="/customer/bookings">My Appointments</Link><button type="button" className="nav-tab-item book-now-tab" onClick={() => setIsBookingOpen(true)}>Book New Treatment</button><Link className={`nav-tab-item ${location.pathname === '/customer/profile' ? 'active' : ''}`} to="/customer/profile">Profile Settings</Link></nav></aside>
      <main className="dashboard-main-content"><Routes>
        <Route index element={<div className="overview-view"><div className="dashboard-banner"><div><h2>Hello, {user?.firstName || 'Beautiful'}!</h2><p>Manage your upcoming treatments and reservations.</p></div><button className="btn btn-primary" onClick={() => setIsBookingOpen(true)}>Book New Session</button></div><div className="stats-row-luxe"><div className="stat-box"><span className="stat-title">Upcoming</span><span className="stat-val">{upcoming.length}</span></div><div className="stat-box"><span className="stat-title">Total Bookings</span><span className="stat-val">{bookings.length}</span></div><div className="stat-box"><span className="stat-title">Glow Points</span><span className="stat-val">{bookings.length * 50} pts</span></div></div><section className="dashboard-card-section"><div className="section-head-bar"><h3>Upcoming Appointments</h3><Link to="/customer/bookings">View All</Link></div>{upcoming.length === 0 ? <div className="empty-dashboard-state"><h4>No upcoming appointments</h4><button className="btn btn-primary" onClick={() => setIsBookingOpen(true)}>Book Your First Service</button></div> : <div className="bookings-cards-grid">{upcoming.map((booking) => <BookingCard key={booking.id} booking={booking} onCancel={cancelBooking} />)}</div>}</section></div>} />
        <Route path="bookings" element={<div className="bookings-view"><div className="section-head-bar"><div><h2>My Appointments</h2><p className="subtitle-txt">Review past and upcoming reservations.</p></div><button className="btn btn-primary" onClick={() => setIsBookingOpen(true)}>New Booking</button></div><div className="all-bookings-list">{bookings.length ? bookings.map((booking) => <BookingCard key={booking.id} booking={booking} onCancel={cancelBooking} />) : <p>No bookings found.</p>}</div></div>} />
        <Route path="profile" element={<div className="profile-view"><h2>Profile Settings</h2><p className="subtitle-txt">Manage your contact information and preferences.</p>{profileMessage && <p className="dashboard-success">{profileMessage}</p>}{profileError && <p className="dashboard-error">{profileError}</p>}<form className="profile-card-form" onSubmit={saveProfile}><div className="profile-image-preview">{profileForm.profileImageUrl ? <img src={profileForm.profileImageUrl} alt="Profile preview" /> : displayName[0].toUpperCase()}</div><label>Profile Image URL<input type="url" placeholder="https://example.com/your-image.jpg" value={profileForm.profileImageUrl} onChange={(event) => setProfileForm({ ...profileForm, profileImageUrl: event.target.value })} /></label><div className="profile-form-grid"><label>First Name<input type="text" value={profileForm.firstName} onChange={(event) => setProfileForm({ ...profileForm, firstName: event.target.value })} required /></label><label>Last Name<input type="text" value={profileForm.lastName} onChange={(event) => setProfileForm({ ...profileForm, lastName: event.target.value })} required /></label></div><label>Email Address<input type="email" value={user?.email || ''} disabled /></label><label>Phone Number<input type="tel" value={profileForm.phoneNumber} onChange={(event) => setProfileForm({ ...profileForm, phoneNumber: event.target.value })} /></label><label>Address<input type="text" value={profileForm.address} onChange={(event) => setProfileForm({ ...profileForm, address: event.target.value })} /></label><div className="profile-form-grid"><label>City<input type="text" value={profileForm.city} onChange={(event) => setProfileForm({ ...profileForm, city: event.target.value })} /></label><label>State<input type="text" value={profileForm.state} onChange={(event) => setProfileForm({ ...profileForm, state: event.target.value })} /></label></div><label>Postal Code<input type="text" value={profileForm.postalCode} onChange={(event) => setProfileForm({ ...profileForm, postalCode: event.target.value })} /></label><button className="btn btn-primary" type="submit">Save Profile</button></form></div>} />
        <Route path="*" element={<Navigate to="/customer" replace />} />
      </Routes></main>
    </div><BookingModal isOpen={isBookingOpen} onClose={() => setIsBookingOpen(false)} onBookingSuccess={() => { loadBookings(); navigate('/customer/bookings'); }} /></div>
  );
};

const BookingCard = ({ booking, onCancel }) => <article className="customer-booking-card"><div className="booking-card-top"><div><span className="booking-badge-id">REF: {booking.id}</span><h4>{booking.serviceName}</h4><p className="practitioner-txt">With {booking.practitionerName}</p></div><span className={`status-badge ${booking.status.toLowerCase()}`}>{booking.status}</span></div><div className="booking-card-details"><div className="detail-item"><span>Date</span><strong>{booking.appointmentDate}</strong></div><div className="detail-item"><span>Time</span><strong>{booking.timeSlot}</strong></div><div className="detail-item"><span>Mode</span><strong>{booking.serviceType}</strong></div><div className="detail-item"><span>Price</span><strong className="price-tag">${booking.totalPrice}</strong></div></div>{booking.status === 'Confirmed' && <div className="booking-card-actions"><button className="btn-cancel-link" onClick={() => onCancel(booking.id)}>Cancel Booking</button></div>}</article>;

export default CustomerDashboard;
