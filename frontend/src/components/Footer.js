import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import './Footer.css';

const Footer = () => {
  const [email, setEmail] = useState('');
  const [subscribed, setSubscribed] = useState(false);

  const handleSubscribe = (e) => {
    e.preventDefault();
    if (email) {
      setSubscribed(true);
      setEmail('');
    }
  };

  return (
    <footer className="luxe-footer">
      <div className="container footer-content">
        <div className="footer-grid">
          {/* Brand Col */}
          <div className="footer-brand-col">
            <div className="footer-logo">
              <span className="logo-spark">🌸</span>
              <span className="brand-title">Luxe Glow Studio</span>
            </div>
            <p className="footer-about-text">
              Where luxury meets tranquility. Bespoke beauty, skincare, and wellness rituals created for your radiant transformation.
            </p>
            <div className="footer-social-links">
              <a href="#instagram" aria-label="Instagram">📸</a>
              <a href="#facebook" aria-label="Facebook">📘</a>
              <a href="#tiktok" aria-label="TikTok">🎵</a>
              <a href="#pinterest" aria-label="Pinterest">📌</a>
            </div>
          </div>

          {/* Quick Links */}
          <div className="footer-col">
            <h4>Quick Links</h4>
            <ul>
              <li><Link to="/">Home</Link></li>
              <li><Link to="/services">All Treatments</Link></li>
              <li><Link to="/about">Our Story</Link></li>
              <li><Link to="/gallery">Lookbook Gallery</Link></li>
              <li><Link to="/contact">Contact Studio</Link></li>
            </ul>
          </div>

          {/* Opening Hours & Contact */}
          <div className="footer-col">
            <h4>Studio Hours</h4>
            <div className="hours-block">
              <p><span>Mon - Fri:</span> 9:00 AM - 8:00 PM</p>
              <p><span>Saturday:</span> 9:00 AM - 7:00 PM</p>
              <p><span>Sunday:</span> 10:00 AM - 5:00 PM</p>
            </div>
            <div className="address-block">
              <p>📍 742 Evergreen Avenue, Luxury District</p>
              <p>📞 +1 (800) 589-3456</p>
              <p>✉️ concierge@luxeglowstudio.com</p>
            </div>
          </div>

          {/* Newsletter */}
          <div className="footer-col">
            <h4>Exclusive Glow Club</h4>
            <p className="newsletter-desc">
              Subscribe for VIP invites, seasonal offers, and skincare tips.
            </p>
            {subscribed ? (
              <div className="newsletter-success">
                ✨ Thank you for subscribing to Luxe Glow!
              </div>
            ) : (
              <form onSubmit={handleSubscribe} className="footer-subscribe-form">
                <input
                  type="email"
                  placeholder="Enter your email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
                <button type="submit" className="btn btn-primary">
                  Join
                </button>
              </form>
            )}
          </div>
        </div>

        {/* Bottom Bar */}
        <div className="footer-bottom-bar">
          <p>© {new Date().getFullYear()} Luxe Glow Studio. All rights reserved.</p>
          <div className="legal-links">
            <a href="#privacy">Privacy Policy</a>
            <span>•</span>
            <a href="#terms">Terms of Service</a>
            <span>•</span>
            <a href="#hygiene">Hygiene & Safety</a>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
