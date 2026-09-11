import React from 'react';
import { Link } from 'react-router-dom';
import './Home.css';

const Home = () => {
  return (
    <div className="home">
      {/* Hero Section */}
      <section className="hero">
        <div className="hero-content">
          <h1 className="hero-title">Welcome to Luxe Glow Studio</h1>
          <p className="hero-subtitle">
            Experience luxury beauty treatments and wellness services
          </p>
          <div className="hero-buttons">
            <Link to="/services" className="btn btn-primary">
              Explore Services
            </Link>
            <Link to="/register" className="btn btn-secondary">
              Book Now
            </Link>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="features">
        <div className="container">
          <h2 className="section-title">Why Choose Us</h2>
          <div className="features-grid">
            <div className="feature-card">
              <div className="feature-icon">✨</div>
              <h3>Expert Professionals</h3>
              <p>Certified beauty experts with years of experience</p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">🏆</div>
              <h3>Premium Quality</h3>
              <p>Top-quality products and treatments</p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">📅</div>
              <h3>Easy Booking</h3>
              <p>Book appointments online anytime</p>
            </div>
            <div className="feature-card">
              <div className="feature-icon">🏠</div>
              <h3>Home Service</h3>
              <p>Enjoy services at your doorstep</p>
            </div>
          </div>
        </div>
      </section>

      {/* Popular Services Section */}
      <section className="popular-services">
        <div className="container">
          <h2 className="section-title">Popular Services</h2>
          <div className="services-grid">
            <div className="service-card">
              <div className="service-image"></div>
              <h3>Facial Treatments</h3>
              <p>Rejuvenate your skin with our premium facial treatments</p>
              <Link to="/services" className="service-link">
                Learn More →
              </Link>
            </div>
            <div className="service-card">
              <div className="service-image"></div>
              <h3>Hair Styling</h3>
              <p>Professional hair care and styling services</p>
              <Link to="/services" className="service-link">
                Learn More →
              </Link>
            </div>
            <div className="service-card">
              <div className="service-image"></div>
              <h3>Makeup</h3>
              <p>Stunning makeup for all occasions</p>
              <Link to="/services" className="service-link">
                Learn More →
              </Link>
            </div>
            <div className="service-card">
              <div className="service-image"></div>
              <h3>Spa Treatments</h3>
              <p>Relax and unwind with our spa packages</p>
              <Link to="/services" className="service-link">
                Learn More →
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="cta-section">
        <div className="container">
          <h2>Ready to Transform Your Look?</h2>
          <p>Book your appointment today and experience luxury beauty services</p>
          <Link to="/register" className="btn btn-primary btn-large">
            Get Started
          </Link>
        </div>
      </section>
    </div>
  );
};

export default Home;
