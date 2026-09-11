import React from 'react';
import './About.css';

const About = () => {
  return (
    <div className="about-page">
      <div className="about-hero">
        <h1>About Luxe Glow Studio</h1>
        <p>Your trusted partner in beauty and wellness</p>
      </div>

      <div className="container">
        <section className="about-section">
          <h2>Our Story</h2>
          <p>
            Founded in 2020, Luxe Glow Studio has been dedicated to providing premium beauty
            and wellness services to our valued clients. We believe that everyone deserves to
            look and feel their best, and our team of expert professionals is committed to
            making that a reality.
          </p>
        </section>

        <section className="about-section">
          <h2>Our Mission</h2>
          <p>
            Our mission is to empower individuals through exceptional beauty services that
            enhance natural beauty and boost confidence. We strive to create a welcoming
            environment where every client feels valued and pampered.
          </p>
        </section>

        <section className="about-section">
          <h2>Why Choose Us</h2>
          <div className="values-grid">
            <div className="value-card">
              <h3>🎓 Expert Team</h3>
              <p>Our professionals are certified and continuously trained in the latest techniques</p>
            </div>
            <div className="value-card">
              <h3>✨ Premium Products</h3>
              <p>We use only the highest quality, trusted beauty products</p>
            </div>
            <div className="value-card">
              <h3>🏆 Award-Winning</h3>
              <p>Recognized for excellence in beauty services</p>
            </div>
            <div className="value-card">
              <h3>💝 Client-Focused</h3>
              <p>Your satisfaction and comfort are our top priorities</p>
            </div>
          </div>
        </section>

        <section className="about-section">
          <h2>Our Services</h2>
          <p>
            We offer a comprehensive range of beauty and wellness services including:
          </p>
          <ul className="services-list-about">
            <li>Facial Treatments & Skincare</li>
            <li>Hair Styling & Coloring</li>
            <li>Professional Makeup</li>
            <li>Nail Care & Art</li>
            <li>Spa & Wellness Treatments</li>
            <li>Bridal Packages</li>
            <li>Home Service Available</li>
          </ul>
        </section>

        <section className="about-section cta-about">
          <h2>Ready to Experience Luxury?</h2>
          <p>Book your appointment today and discover the Luxe Glow difference</p>
          <a href="/register" className="btn btn-primary btn-large">
            Book Appointment
          </a>
        </section>
      </div>
    </div>
  );
};

export default About;
