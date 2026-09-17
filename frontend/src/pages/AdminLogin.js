import React, { useState } from 'react';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { getAuthErrorMessage } from '../services/authService';
import './Auth.css';

const AdminLogin = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { adminLogin, logout } = useAuth();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const data = await adminLogin(email, password);
      
      // Check if user is admin
      if (data.user?.role?.toLowerCase() !== 'admin') {
        logout();
        setError('Access Denied: Admin credentials required. Only administrators can login here.');
        setLoading(false);
        return;
      }

      navigate('/admin');
    } catch (err) {
      setError(getAuthErrorMessage(err, 'Login failed. Please check your admin credentials.'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-container admin-login-bg">
      <div className="auth-card admin-login-card">
        <div className="admin-login-header">
          <div className="admin-icon">🔐</div>
          <h2 className="auth-title">Admin Panel Access</h2>
          <p className="auth-subtitle">Restricted Area - Administrators Only</p>
        </div>

        {error && <div className="auth-error">{error}</div>}
        {location.state?.message && !error && <div className="auth-success">{location.state.message}</div>}

        <form onSubmit={handleSubmit} className="auth-form">
          <div className="form-group">
            <label htmlFor="email">Admin Email</label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              placeholder="admin@luxeglowstudio.com"
              autoComplete="email"
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Admin Password</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="Enter admin password"
              autoComplete="current-password"
            />
          </div>

          <div className="form-actions">
            <Link to="/forgot-password" className="forgot-link">
              Forgot Password?
            </Link>
          </div>

          <button type="submit" className="auth-btn admin-btn" disabled={loading}>
            {loading ? 'Verifying...' : 'Login to Admin Panel'}
          </button>
        </form>

        <p className="auth-link">
          Need admin account? <Link to="/admin/register">Register here</Link>
        </p>
      </div>
    </div>
  );
};

export default AdminLogin;
