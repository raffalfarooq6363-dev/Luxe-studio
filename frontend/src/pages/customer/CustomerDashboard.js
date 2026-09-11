import React, { useState, useEffect } from 'react';
import { Routes, Route, Link, Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import bookingService from '../../services/bookingService';
import './CustomerDashboard.css';

const CustomerDashboard = () => {
  const { user } = useAuth();
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('overview');

  useEffect(() => {
    fetchBookings();
  }, []);

  const fetchBookings = async () => {
    try {
      const data = await bookingService.searchBookings({
        page: 1,
        pageSize: 10,
      });
      setBookings(data.bookings || []);
    } catch (error) {
      console.error('Error fetching bookings:', error);
      // Sample data for demonstration
      setBookings([]);
    } finally {
      setLoading(false);
    }
  };

  const handleCancelBooking = async (bookingId) => {
    if (window.confirm('Are you sure you want to cancel this booking?')) {
      try {
        await bookingService.cancelBooking(bookingId);
        fetchBookings();
        alert('Booking cancelled successfully');
      } catch (error) {
        alert('Failed to cancel booking');
      }
    }
  };

  return (
    <div className="dashboard">
      <div className="dashboard-sidebar">
        <div className="dashboard-user">
          <div className="user-avatar">
            {user?.firstName?.[0]}{user?.lastName?.[0]}
          </div>
          <h3>{user?.firstName} {user?.lastName}</h3>
          <p>{user?.email}</p>
        </div>

        <nav className="dashboard-nav">
          <Link
            to="/customer"
            className={activeTab === 'overview' ? 'active' : ''}
            onClick={() => setActiveTab('overview')}
          >
            📊 Overview
          </Link>
          <Link
            to="/customer/bookings"
            className={activeTab === 'bookings' ? 'active' : ''}
            onClick={() => setActiveTab('bookings')}
          >
            📅 My Bookings
          </Link>
          <Link
            to="/customer/book"
            className={activeTab === 'book' ? 'active' : ''}
            onClick={() => setActiveTab('book')}
          >
            ➕ New Booking
          </Link>
          <Link
            to="/customer/profile"
            className={activeTab === 'profile' ? 'active' : ''}
            onClick={() => setActiveTab('profile')}
          >
            👤 Profile
          </Link>
        </nav>
      </div>

      <div className="dashboard-content">
        <Routes>
          <Route path="/" element={
            <div>
              <h1>Welcome, {user?.firstName}!</h1>
              <div className="dashboard-stats">
                <div className="stat-card">
                  <h3>Total Bookings</h3>
                  <p className="stat-number">{bookings.length}</p>
                </div>
                <div className="stat-card">
                  <h3>Upcoming</h3>
                  <p className="stat-number">
                    {bookings.filter(b => b.status === 'Confirmed').length}
                  </p>
                </div>
                <div className="stat-card">
                  <h3>Completed</h3>
                  <p className="stat-number">
                    {bookings.filter(b => b.status === 'Completed').length}
                  </p>
                </div>
              </div>

              <div className="recent-bookings">
                <h2>Recent Bookings</h2>
                {loading ? (
                  <p>Loading...</p>
                ) : bookings.length === 0 ? (
                  <div className="empty-state">
                    <p>No bookings yet</p>
                    <Link to="/customer/book" className="btn btn-primary">
                      Book Now
                    </Link>
                  </div>
                ) : (
                  <div className="bookings-list">
                    {bookings.slice(0, 3).map((booking) => (
                      <div key={booking.id} className="booking-card">
                        <div className="booking-info">
                          <h3>{booking.serviceName}</h3>
                          <p>{new Date(booking.appointmentDate).toLocaleDateString()}</p>
                          <span className={`status-badge status-${booking.status.toLowerCase()}`}>
                            {booking.status}
                          </span>
                        </div>
                        <div className="booking-actions">
                          <button
                            className="btn-cancel"
                            onClick={() => handleCancelBooking(booking.id)}
                          >
                            Cancel
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          } />
          <Route path="/bookings" element={
            <div>
              <h1>My Bookings</h1>
              {/* Bookings list component */}
              <p>All bookings will be displayed here</p>
            </div>
          } />
          <Route path="/book" element={
            <div>
              <h1>Book a Service</h1>
              {/* Booking form component */}
              <p>Booking form will be here</p>
            </div>
          } />
          <Route path="/profile" element={
            <div>
              <h1>My Profile</h1>
              {/* Profile component */}
              <p>Profile settings will be here</p>
            </div>
          } />
          <Route path="*" element={<Navigate to="/customer" replace />} />
        </Routes>
      </div>
    </div>
  );
};

export default CustomerDashboard;
