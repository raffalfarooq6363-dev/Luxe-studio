import React, { useState } from 'react';
import { Routes, Route, Link, Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import './PractitionerDashboard.css';

const PractitionerDashboard = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('overview');

  return (
    <div className="dashboard practitioner-dashboard">
      <div className="dashboard-sidebar">
        <div className="dashboard-user">
          <div className="user-avatar practitioner-avatar">
            {user?.firstName?.[0]}{user?.lastName?.[0]}
          </div>
          <h3>{user?.firstName} {user?.lastName}</h3>
          <p className="role-badge practitioner-badge">Practitioner</p>
        </div>

        <nav className="dashboard-nav">
          <Link
            to="/practitioner"
            className={activeTab === 'overview' ? 'active' : ''}
            onClick={() => setActiveTab('overview')}
          >
            📊 Dashboard
          </Link>
          <Link
            to="/practitioner/appointments"
            className={activeTab === 'appointments' ? 'active' : ''}
            onClick={() => setActiveTab('appointments')}
          >
            📅 My Appointments
          </Link>
          <Link
            to="/practitioner/schedule"
            className={activeTab === 'schedule' ? 'active' : ''}
            onClick={() => setActiveTab('schedule')}
          >
            🗓️ Schedule
          </Link>
          <Link
            to="/practitioner/clients"
            className={activeTab === 'clients' ? 'active' : ''}
            onClick={() => setActiveTab('clients')}
          >
            👥 Clients
          </Link>
          <Link
            to="/practitioner/profile"
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
              <h1>Practitioner Dashboard</h1>
              <div className="dashboard-stats">
                <div className="stat-card stat-info">
                  <h3>Today's Appointments</h3>
                  <p className="stat-number">5</p>
                </div>
                <div className="stat-card stat-success">
                  <h3>This Week</h3>
                  <p className="stat-number">23</p>
                </div>
                <div className="stat-card stat-primary">
                  <h3>Total Clients</h3>
                  <p className="stat-number">87</p>
                </div>
                <div className="stat-card stat-warning">
                  <h3>Rating</h3>
                  <p className="stat-number">4.8⭐</p>
                </div>
              </div>

              <div className="practitioner-schedule">
                <h2>Today's Schedule</h2>
                <p>Your appointments for today will appear here</p>
              </div>
            </div>
          } />
          <Route path="/appointments" element={
            <div>
              <h1>My Appointments</h1>
              <p>All your appointments will be displayed here</p>
            </div>
          } />
          <Route path="/schedule" element={
            <div>
              <h1>Manage Schedule</h1>
              <p>Set your availability and working hours</p>
            </div>
          } />
          <Route path="/clients" element={
            <div>
              <h1>My Clients</h1>
              <p>View and manage your client list</p>
            </div>
          } />
          <Route path="/profile" element={
            <div>
              <h1>My Profile</h1>
              <p>Update your profile and portfolio</p>
            </div>
          } />
          <Route path="*" element={<Navigate to="/practitioner" replace />} />
        </Routes>
      </div>
    </div>
  );
};

export default PractitionerDashboard;
