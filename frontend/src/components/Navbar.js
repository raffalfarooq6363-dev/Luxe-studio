import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Navbar.css';

const Navbar = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/');
    setIsMenuOpen(false);
  };

  const getDashboardLink = () => {
    if (!user) return null;
    switch (user.role?.toLowerCase()) {
      case 'admin':
        return '/admin';
      case 'customer':
        return '/customer';
      case 'practitioner':
        return '/practitioner';
      default:
        return null;
    }
  };

  return (
    <nav className="navbar">
      <div className="navbar-container">
        <Link to="/" className="navbar-logo">
          Luxe Glow Studio
        </Link>

        <button
          className="navbar-toggle"
          onClick={() => setIsMenuOpen(!isMenuOpen)}
        >
          <span></span>
          <span></span>
          <span></span>
        </button>

        <ul className={`navbar-menu ${isMenuOpen ? 'active' : ''}`}>
          <li className="navbar-item">
            <Link to="/" className="navbar-link" onClick={() => setIsMenuOpen(false)}>
              Home
            </Link>
          </li>
          <li className="navbar-item">
            <Link to="/about" className="navbar-link" onClick={() => setIsMenuOpen(false)}>
              About
            </Link>
          </li>
          <li className="navbar-item">
            <Link to="/services" className="navbar-link" onClick={() => setIsMenuOpen(false)}>
              Services
            </Link>
          </li>
          <li className="navbar-item">
            <Link to="/gallery" className="navbar-link" onClick={() => setIsMenuOpen(false)}>
              Gallery
            </Link>
          </li>
          <li className="navbar-item">
            <Link to="/contact" className="navbar-link" onClick={() => setIsMenuOpen(false)}>
              Contact
            </Link>
          </li>

          {user ? (
            <>
              <li className="navbar-item">
                <Link
                  to={getDashboardLink()}
                  className="navbar-link navbar-link-dashboard"
                  onClick={() => setIsMenuOpen(false)}
                >
                  Dashboard
                </Link>
              </li>
              <li className="navbar-item">
                <span className="navbar-user">
                  Welcome, {user.firstName || user.email}
                </span>
              </li>
              <li className="navbar-item">
                <button className="navbar-btn navbar-btn-logout" onClick={handleLogout}>
                  Logout
                </button>
              </li>
            </>
          ) : (
            <>
              <li className="navbar-item">
                <Link
                  to="/login"
                  className="navbar-btn navbar-btn-login"
                  onClick={() => setIsMenuOpen(false)}
                >
                  Login
                </Link>
              </li>
              <li className="navbar-item">
                <Link
                  to="/register"
                  className="navbar-btn navbar-btn-register"
                  onClick={() => setIsMenuOpen(false)}
                >
                  Register
                </Link>
              </li>
            </>
          )}
        </ul>
      </div>
    </nav>
  );
};

export default Navbar;
