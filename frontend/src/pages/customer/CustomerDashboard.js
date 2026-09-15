import React, { useEffect, useState } from 'react';
import { Link, Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { cancelStoredBooking, getStoredBookings } from '../../data/bookingStore';
import BookingModal from '../../components/BookingModal';
import './CustomerDashboard.css';

const CustomerDashboard = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [bookings, setBookings] = useState([]);
  const [isBookingOpen, setIsBookingOpen] = useState(false);

  const loadBookings = () => setBookings(getStoredBookings());

  useEffect(() => {
    loadBookings();
    window.addEventListener('luxe_bookings_updated', loadBookings);
    return () => window.removeEventListener('luxe_bookings_updated', loadBookings);
  }, []);

  const cancelBooking = (bookingId) => {
    if (window.confirm(`Cancel booking ${bookingId}?`)) {
      cancelStoredBooking(bookingId);
      loadBookings();
    }
  };

  const upcoming = bookings.filter((booking) => booking.status === 'Confirmed');
  const displayName = user?.firstName ? `${user.firstName} ${user.lastName || ''}`.trim() : 'Valued Guest';

  return (
    <div className="customer-dashboard-page"><div className="container dashboard-container">
      <aside className="dashboard-sidebar-luxe"><div className="user-profile-header"><div className="user-avatar-circle">{displayName[0].toUpperCase()}</div><h3 className="user-name">{displayName}</h3><p className="user-email-text">{user?.email || ''}</p></div><nav className="sidebar-nav-menu"><Link to="/customer">Overview &amp; Stats</Link><Link to="/customer/bookings">My Appointments</Link><button type="button" className="nav-tab-item book-now-tab" onClick={() => setIsBookingOpen(true)}>Book New Treatment</button><Link to="/customer/profile">Profile Settings</Link></nav></aside>
      <main className="dashboard-main-content"><Routes>
        <Route index element={<div className="overview-view"><div className="dashboard-banner"><div><h2>Hello, {user?.firstName || 'Beautiful'}!</h2><p>Manage your upcoming treatments and reservations.</p></div><button className="btn btn-primary" onClick={() => setIsBookingOpen(true)}>Book New Session</button></div><div className="stats-row-luxe"><div className="stat-box"><span className="stat-title">Upcoming</span><span className="stat-val">{upcoming.length}</span></div><div className="stat-box"><span className="stat-title">Total Bookings</span><span className="stat-val">{bookings.length}</span></div><div className="stat-box"><span className="stat-title">Glow Points</span><span className="stat-val">{bookings.length * 50} pts</span></div></div><section className="dashboard-card-section"><div className="section-head-bar"><h3>Upcoming Appointments</h3><Link to="/customer/bookings">View All</Link></div>{upcoming.length === 0 ? <div className="empty-dashboard-state"><h4>No upcoming appointments</h4><button className="btn btn-primary" onClick={() => setIsBookingOpen(true)}>Book Your First Service</button></div> : <div className="bookings-cards-grid">{upcoming.map((booking) => <BookingCard key={booking.id} booking={booking} onCancel={cancelBooking} />)}</div>}</section></div>} />
        <Route path="bookings" element={<div className="bookings-view"><div className="section-head-bar"><div><h2>My Appointments</h2><p className="subtitle-txt">Review past and upcoming reservations.</p></div><button className="btn btn-primary" onClick={() => setIsBookingOpen(true)}>New Booking</button></div><div className="all-bookings-list">{bookings.length ? bookings.map((booking) => <BookingCard key={booking.id} booking={booking} onCancel={cancelBooking} />) : <p>No bookings found.</p>}</div></div>} />
        <Route path="profile" element={<div className="profile-view"><h2>Profile Settings</h2><p className="subtitle-txt">Manage your contact information and preferences.</p><form className="profile-card-form" onSubmit={(event) => { event.preventDefault(); window.alert('Profile updated successfully.'); }}><label>Full Name<input type="text" defaultValue={displayName} required /></label><label>Email Address<input type="email" defaultValue={user?.email || ''} disabled /></label><label>Phone Number<input type="tel" defaultValue={user?.phoneNumber || ''} /></label><label>Beauty Preferences<textarea rows="3" defaultValue="Sensitive skin, prefers organic products." /></label><button className="btn btn-primary" type="submit">Save Preferences</button></form></div>} />
        <Route path="*" element={<Navigate to="/customer" replace />} />
      </Routes></main>
    </div><BookingModal isOpen={isBookingOpen} onClose={() => setIsBookingOpen(false)} onBookingSuccess={() => { loadBookings(); navigate('/customer/bookings'); }} /></div>
  );
};

const BookingCard = ({ booking, onCancel }) => <article className="customer-booking-card"><div className="booking-card-top"><div><span className="booking-badge-id">REF: {booking.id}</span><h4>{booking.serviceName}</h4><p className="practitioner-txt">With {booking.practitionerName}</p></div><span className={`status-badge ${booking.status.toLowerCase()}`}>{booking.status}</span></div><div className="booking-card-details"><div className="detail-item"><span>Date</span><strong>{booking.appointmentDate}</strong></div><div className="detail-item"><span>Time</span><strong>{booking.timeSlot}</strong></div><div className="detail-item"><span>Mode</span><strong>{booking.serviceType}</strong></div><div className="detail-item"><span>Price</span><strong className="price-tag">${booking.totalPrice}</strong></div></div>{booking.status === 'Confirmed' && <div className="booking-card-actions"><button className="btn-cancel-link" onClick={() => onCancel(booking.id)}>Cancel Booking</button></div>}</article>;

export default CustomerDashboard;
