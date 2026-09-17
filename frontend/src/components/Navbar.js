import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import BookingModal from './BookingModal';
import './Navbar.css';

const Navbar = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isBookingOpen, setIsBookingOpen] = useState(false);

  const dashboardPath = user?.role?.toLowerCase() === 'admin' ? '/admin' : '/customer';
  const closeMenu = () => setIsMenuOpen(false);
  const handleLogout = () => { logout(); closeMenu(); navigate('/'); };

  return <>
    <nav className="luxe-navbar"><div className="navbar-container">
      <Link to="/" className="navbar-logo" onClick={closeMenu}><span className="logo-icon">L</span><div className="logo-text"><span className="brand-main">Luxe Glow</span></div></Link>
      <button type="button" className={`navbar-toggle ${isMenuOpen ? 'open' : ''}`} onClick={() => setIsMenuOpen((open) => !open)} aria-label="Toggle navigation menu"><span /><span /><span /></button>
      <div className={`navbar-menu-wrap ${isMenuOpen ? 'active' : ''}`}><ul className="navbar-nav-links">{[['/', 'Home'], ['/services', 'Services'], ['/about', 'About Us'], ['/gallery', 'Lookbook'], ['/contact', 'Contact']].map(([path, label]) => <li key={path}><Link to={path} className="nav-link" onClick={closeMenu}>{label}</Link></li>)}</ul><div className="navbar-actions"><button type="button" className="nav-book-btn" onClick={() => { closeMenu(); setIsBookingOpen(true); }}>Book Appointment</button>{user ? <div className="user-dropdown-area"><Link to={dashboardPath} className="nav-btn-dashboard" onClick={closeMenu}>Dashboard</Link><button type="button" className="nav-btn-logout" onClick={handleLogout}>Logout</button></div> : <div className="auth-buttons"><Link to="/login" className="nav-btn-login" onClick={closeMenu}>Login</Link><Link to="/register" className="nav-btn-register" onClick={closeMenu}>Register</Link></div>}</div></div>
    </div></nav>
    <BookingModal isOpen={isBookingOpen} onClose={() => setIsBookingOpen(false)} />
  </>;
};

export default Navbar;
