import React, { useState, useEffect } from 'react';
import { Routes, Route, Link, Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import adminService from '../../services/adminService';
import './AdminDashboard.css';

const AdminDashboard = () => {
  const { user } = useAuth();
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('overview');

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      const data = await adminService.getDashboard();
      setDashboardData(data);
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
      // Sample data for demonstration
      setDashboardData({
        totalUsers: 150,
        totalAppointments: 320,
        totalRevenue: 45000,
        pendingAppointments: 12,
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="dashboard admin-dashboard">
      <div className="dashboard-sidebar">
        <div className="dashboard-user">
          <div className="user-avatar admin-avatar">
            {user?.firstName?.[0]}{user?.lastName?.[0]}
          </div>
          <h3>{user?.firstName} {user?.lastName}</h3>
          <p className="role-badge">Admin</p>
        </div>

        <nav className="dashboard-nav">
          <Link
            to="/admin"
            className={activeTab === 'overview' ? 'active' : ''}
            onClick={() => setActiveTab('overview')}
          >
            📊 Dashboard
          </Link>
          <Link
            to="/admin/bookings"
            className={activeTab === 'bookings' ? 'active' : ''}
            onClick={() => setActiveTab('bookings')}
          >
            📅 Bookings
          </Link>
          <Link
            to="/admin/users"
            className={activeTab === 'users' ? 'active' : ''}
            onClick={() => setActiveTab('users')}
          >
            👥 Users
          </Link>
          <Link
            to="/admin/services"
            className={activeTab === 'services' ? 'active' : ''}
            onClick={() => setActiveTab('services')}
          >
            💼 Services
          </Link>
          <Link
            to="/admin/practitioners"
            className={activeTab === 'practitioners' ? 'active' : ''}
            onClick={() => setActiveTab('practitioners')}
          >
            👨‍⚕️ Practitioners
          </Link>
          <Link
            to="/admin/payments"
            className={activeTab === 'payments' ? 'active' : ''}
            onClick={() => setActiveTab('payments')}
          >
            💰 Payments
          </Link>
          <Link
            to="/admin/reports"
            className={activeTab === 'reports' ? 'active' : ''}
            onClick={() => setActiveTab('reports')}
          >
            📈 Reports
          </Link>
        </nav>
      </div>

      <div className="dashboard-content">
        <Routes>
          <Route path="/" element={
            <div>
              <h1>Admin Dashboard</h1>
              {loading ? (
                <p>Loading...</p>
              ) : (
                <>
                  <div className="dashboard-stats">
                    <div className="stat-card stat-primary">
                      <h3>Total Users</h3>
                      <p className="stat-number">{dashboardData?.totalUsers || 0}</p>
                    </div>
                    <div className="stat-card stat-success">
                      <h3>Total Appointments</h3>
                      <p className="stat-number">{dashboardData?.totalAppointments || 0}</p>
                    </div>
                    <div className="stat-card stat-warning">
                      <h3>Revenue</h3>
                      <p className="stat-number">${dashboardData?.totalRevenue || 0}</p>
                    </div>
                    <div className="stat-card stat-info">
                      <h3>Pending</h3>
                      <p className="stat-number">{dashboardData?.pendingAppointments || 0}</p>
                    </div>
                  </div>

                  <div className="admin-quick-actions">
                    <h2>Quick Actions</h2>
                    <div className="action-buttons">
                      <Link to="/admin/bookings" className="action-btn">
                        📅 Manage Bookings
                      </Link>
                      <Link to="/admin/services" className="action-btn">
                        ➕ Add Service
                      </Link>
                      <Link to="/admin/users" className="action-btn">
                        👥 View Users
                      </Link>
                      <Link to="/admin/reports" className="action-btn">
                        📊 View Reports
                      </Link>
                    </div>
                  </div>
                </>
              )}
            </div>
          } />
          <Route path="/bookings" element={
            <div>
              <h1>Manage Bookings</h1>
              <p>Booking management interface will be here</p>
            </div>
          } />
          <Route path="/users" element={
            <div>
              <h1>User Management</h1>
              <p>User management interface will be here</p>
            </div>
          } />
          <Route path="/services" element={
            <div>
              <h1>Service Management</h1>
              <p>Service management interface will be here</p>
            </div>
          } />
          <Route path="/practitioners" element={
            <div>
              <h1>Practitioner Management</h1>
              <p>Practitioner management interface will be here</p>
            </div>
          } />
          <Route path="/payments" element={
            <div>
              <h1>Payment Management</h1>
              <p>Payment management interface will be here</p>
            </div>
          } />
          <Route path="/reports" element={
            <div>
              <h1>Reports & Analytics</h1>
              <p>Reports interface will be here</p>
            </div>
          } />
          <Route path="*" element={<Navigate to="/admin" replace />} />
        </Routes>
      </div>
    </div>
  );
};

export default AdminDashboard;
