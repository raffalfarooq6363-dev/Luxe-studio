import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { servicesData, categories, timeSlots } from '../data/store';
import BookingModal from '../components/BookingModal';
import './Home.css';

const Home = () => {
  const [isBookingOpen, setIsBookingOpen] = useState(false);
  const [selectedServiceId, setSelectedServiceId] = useState(null);
  const [quickServiceId, setQuickServiceId] = useState(String(servicesData[0].id));
  const [quickDate, setQuickDate] = useState(new Date(Date.now() + 86400000).toISOString().split('T')[0]);
  const [quickSlot, setQuickSlot] = useState(timeSlots[1]);

  const openBooking = (serviceId = null) => {
    setSelectedServiceId(serviceId);
    setIsBookingOpen(true);
  };

  const handleQuickBook = (event) => {
    event.preventDefault();
    openBooking(quickServiceId);
  };

  return (
    <div className="luxe-home">
      <section className="luxe-hero">
        <div className="container hero-scene">
          <div className="feature-photo left-photo">
            <img
              src="https://images.unsplash.com/photo-1521590832167-7e5c8e2a3121?auto=format&fit=crop&w=1200&q=80"
              alt="Salon interior"
            />
          </div>
          <div className="feature-photo right-photo">
            <img
              src="https://images.unsplash.com/photo-1522335789203-aabd1fc54bc9?auto=format&fit=crop&w=1200&q=80"
              alt="Pink luxury beauty treatment"
            />
            <div className="floating-service-chip">
              <span>24K Gold Facial Treatment</span>
            </div>
          </div>
        </div>
      </section>
      <section className="quick-book-section"><div className="container"><form className="quick-book-bar" onSubmit={handleQuickBook}><div className="quick-field"><label htmlFor="quick-service">Select Treatment</label><select id="quick-service" value={quickServiceId} onChange={(event) => setQuickServiceId(event.target.value)}>{servicesData.map((service) => <option key={service.id} value={service.id}>{service.name} (${service.price})</option>)}</select></div><div className="quick-field"><label htmlFor="quick-date">Preferred Date</label><input id="quick-date" type="date" value={quickDate} min={new Date().toISOString().split('T')[0]} onChange={(event) => setQuickDate(event.target.value)} /></div><div className="quick-field"><label htmlFor="quick-slot">Time Slot</label><select id="quick-slot" value={quickSlot} onChange={(event) => setQuickSlot(event.target.value)}>{timeSlots.map((slot) => <option key={slot} value={slot}>{slot}</option>)}</select></div><button type="submit" className="btn btn-primary quick-submit-btn">Reserve Now</button></form></div></section>
      <section className="categories-section"><div className="container"><div className="section-header text-center"><span className="section-sub-badge">TREATMENT CATEGORIES</span><h2 className="section-heading">Indulge In Luxury Self-Care</h2><p className="section-lead">Explore our curated collections of aesthetic &amp; pampering treatments.</p></div><div className="categories-grid">{categories.filter((category) => category.id !== 'all').map((category) => <Link to="/services" key={category.id} className="category-card"><div className="category-icon-bubble">{category.icon}</div><h3>{category.name}</h3><p>Tailored treatments designed for radiant results</p><span className="cat-explore-link">Explore Menu</span></Link>)}</div></div></section>
      <section className="featured-services-section"><div className="container"><div className="section-header-flex"><div><span className="section-sub-badge">SIGNATURE RITUALS</span><h2 className="section-heading">Most Requested Treatments</h2></div><Link to="/services" className="btn btn-secondary">View All Services</Link></div><div className="featured-services-grid">{servicesData.slice(0, 4).map((service) => <div key={service.id} className="luxe-service-card"><div className="card-image-wrap"><img src={service.image} alt={service.name} />{service.isPopular && <span className="service-badge-popular">Popular</span>}<span className="service-badge-duration">{service.durationMinutes} min</span></div><div className="card-content-wrap"><div className="card-rating-row"><span className="stars">* {service.rating}</span><span className="reviews">({service.reviewsCount} reviews)</span></div><h3 className="service-title">{service.name}</h3><p className="service-desc">{service.description}</p><ul className="service-feature-list">{service.features.slice(0, 2).map((feature) => <li key={feature}>{feature}</li>)}</ul><div className="card-action-row"><div className="price-block"><span className="price-label">Starting at</span><span className="price-amount">${service.price}</span></div><button className="btn btn-primary btn-book-service" onClick={() => openBooking(service.id)}>Book Now</button></div></div></div>)}</div></div></section>
      <section className="luxe-cta-banner"><div className="container"><div className="cta-banner-content"><span className="cta-badge">EXCLUSIVE OFFER</span><h2>Ready To Experience The Luxe Glow?</h2><p>Book your bespoke treatment today and receive a complimentary hydration therapy on your first visit.</p><div className="cta-buttons"><button className="btn btn-white btn-large" onClick={() => openBooking()}>Book Your Session Now</button><Link to="/contact" className="btn btn-outline-white btn-large">Contact Studio</Link></div></div></div></section>
      <BookingModal isOpen={isBookingOpen} onClose={() => setIsBookingOpen(false)} initialServiceId={selectedServiceId} />
    </div>
  );
};

export default Home;
