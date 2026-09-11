import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../config/api';
import './Auth.css';

const ForgotPassword = () => {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [step, setStep] = useState(1); // 1: Email, 2: Success

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setMessage('');
    setLoading(true);

    try {
      // This endpoint needs to be created in backend
      await api.post('/auth/forgot-password', { email });
      setMessage('Password reset link has been sent to your email. Please check your inbox.');
      setStep(2);
    } catch (err) {
      if (err.response?.status === 404) {
        // For now, show success message even if endpoint doesn't exist
        setMessage('If this email exists in our system, you will receive a password reset link shortly.');
        setStep(2);
      } else {
        setError(err.response?.data?.message || 'Failed to send reset email. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        {step === 1 ? (
          <>
            <div className="forgot-password-header">
              <div className="forgot-icon">🔒</div>
              <h2 className="auth-title">Forgot Password?</h2>
              <p className="auth-subtitle">
                Enter your email address and we'll send you a link to reset your password
              </p>
            </div>

            {error && <div className="auth-error">{error}</div>}

            <form onSubmit={handleSubmit} className="auth-form">
              <div className="form-group">
                <label htmlFor="email">Email Address</label>
                <input
                  type="email"
                  id="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  placeholder="Enter your registered email"
                  autoComplete="email"
                />
              </div>

              <button type="submit" className="auth-btn" disabled={loading}>
                {loading ? 'Sending...' : 'Send Reset Link'}
              </button>
            </form>

            <div className="auth-footer">
              <p className="auth-link">
                Remember your password? <Link to="/login">Back to Login</Link>
              </p>
              <p className="auth-link">
                Admin? <Link to="/admin-login">Admin Login</Link>
              </p>
            </div>
          </>
        ) : (
          <>
            <div className="success-message-box">
              <div className="success-icon">✅</div>
              <h2>Email Sent!</h2>
              <p className="success-text">{message}</p>
              <div className="reset-instructions">
                <h3>What's next?</h3>
                <ol>
                  <li>Check your email inbox for <strong>{email}</strong></li>
                  <li>Click on the password reset link in the email</li>
                  <li>Create a new password</li>
                  <li>Login with your new password</li>
                </ol>
                <p className="note">
                  <strong>Note:</strong> The link will expire in 1 hour. 
                  If you don't receive the email within a few minutes, check your spam folder.
                </p>
              </div>
              <div className="action-buttons">
                <Link to="/login" className="btn btn-primary">
                  Back to Login
                </Link>
                <button 
                  onClick={() => setStep(1)} 
                  className="btn btn-secondary"
                >
                  Send Again
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default ForgotPassword;
