import React, { useState } from 'react';
import BookingModal from '../components/BookingModal';
import './About.css';

const About = () => {
  const [isBookingOpen, setIsBookingOpen] = useState(false);

  return (
    <div className="luxe-about-page">
      <div className="about-hero-section">
        <div className="container text-center">
          <span className="section-sub-badge">OUR ESSENCE & STORY</span>
          <h1 className="about-hero-title">Elevating Modern Beauty Rituals</h1>
          <p className="about-hero-subtitle">
            Founded with a passion for holistic wellness, transformative aesthetics, and uncompromising luxury.
          </p>
        </div>
      </div>

      <div className="container about-main-body">
        <section className="about-split-section">
          <div className="about-text-col">
            <span className="section-sub-badge">WHO WE ARE</span>
            <h2>A Sanctuary Created For Your Radiant Transformation</h2>
            <p>
              At Luxe Glow Studio, we believe that self-care is a vital ritual, not a luxury. Our team of dermatologically trained aestheticians, master hair colorists, and makeup artists bring together the latest science and pure botanical extracts to craft an experience that rejuvenates your body and spirit.
            </p>
            <p>
              From our signature 24K gold facial treatments to couture bridal glam, every detail of our studio is designed to make you feel pampered, valued, and empowered.
            </p>
          </div>
          <div className="about-img-col">
            <img
              src="https://images.unsplash.com/photo-1570172619644-dfd03ed5d881?w=800&auto=format&fit=crop&q=80"
              alt="Luxe Glow Salon Ambience"
              className="about-feature-img"
            />
          </div>
        </section>

        <section className="about-pillars-section">
          <div className="text-center mb-4">
            <span className="section-sub-badge">OUR PROMISE</span>
            <h2 className="section-heading">The Luxe Glow Pillars</h2>
          </div>

          <div className="pillars-grid">
            <div className="pillar-card">
              <div className="pillar-icon">??</div>
              <h3>Certified Master Artists</h3>
              <p>Continuous education with top international beauty academies.</p>
            </div>
            <div className="pillar-card">
              <div className="pillar-icon">??</div>
              <h3>Cruelty-Free & Organic</h3>
              <p>Strictly non-toxic, hypoallergenic, and ethically sourced formulas.</p>
            </div>
            <div className="pillar-card">
              <div className="pillar-icon">?</div>
              <h3>Tailored Consultations</h3>
              <p>Personalized skin & hair diagnostic before every single ritual.</p>
            </div>
            <div className="pillar-card">
              <div className="pillar-icon">??</div>
              <h3>Bespoke Home Concierge</h3>
              <p>Five-star salon and spa services delivered directly to your doorstep.</p>
            </div>
          </div>
        </section>

        <section className="about-cta-card">
          <h2>Begin Your Glow Journey Today</h2>
          <p>Book your initial consultation and discover personalized beauty treatments.</p>
          <button 
            className="btn btn-primary btn-large"
            onClick={() => setIsBookingOpen(true)}
          >
            <span>? Reserve Your Appointment</span>
          </button>
        </section>
      </div>

      <BookingModal
        isOpen={isBookingOpen}
        onClose={() => setIsBookingOpen(false)}
      />
    </div>
  );
};

export default About;


