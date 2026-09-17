import React, { useState } from 'react';
import { servicesData, categories } from '../data/store';
import BookingModal from '../components/BookingModal';
import './Services.css';

const Services = () => {
  const [selectedCategory, setSelectedCategory] = useState('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [isBookingOpen, setIsBookingOpen] = useState(false);
  const [selectedServiceId, setSelectedServiceId] = useState(null);

  const filteredServices = servicesData.filter(service => {
    const matchesCategory = selectedCategory === 'all' || service.category === selectedCategory;
    const matchesSearch = service.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          service.description.toLowerCase().includes(searchQuery.toLowerCase());
    return matchesCategory && matchesSearch;
  });

  const handleBookService = (serviceId) => {
    setSelectedServiceId(serviceId);
    setIsBookingOpen(true);
  };

  return (
    <div className="luxe-services-page">
      {/* Hero Header */}
      <section className="services-hero-header">
        <div className="container text-center">
          <span className="section-sub-badge">BESPOKE BEAUTY MENU</span>
          <h1 className="services-page-title">Luxury Treatments & Rituals</h1>
          <p className="services-page-subtitle">
            Crafted with organic essences and modern aesthetic innovations for an unparalleled experience.
          </p>

          {/* Search Box */}
          <div className="search-bar-wrap">
            <span className="search-icon">??</span>
            <input
              type="text"               placeholder="Search treatments (e.g., Gold Facial, Bridal Glam, Balayage, Spa)..." value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="services-search-input"             />
          </div>
        </div>
      </section>

      {/* Category Tabs */}
      <section className="services-tabs-section">
        <div className="container">
          <div className="categories-filter-bar">
            {categories.map(cat => (
              <button
                key={cat.id}
                className={`filter-btn ${selectedCategory === cat.id ? 'active' : ''}`}
                onClick={() => setSelectedCategory(cat.id)}
              >
                <span className="cat-emoji">{cat.icon}</span>
                <span>{cat.name}</span>
              </button>
            ))}
          </div>

          {/* Services Grid */}
          <div className="services-main-grid">
            {filteredServices.length === 0 ? (
              <div className="no-services-found">
                <span className="empty-icon">??</span>
                <h3>No treatments found matching {searchQuery}</h3>
                <p>Try searching for a different keyword or browse our categories.</p>
                <button 
                  className="btn btn-secondary"
                  onClick={() => { setSearchQuery(''); setSelectedCategory('all'); }}
                >
                  Reset Filters
                </button>
              </div>
            ) : (
              filteredServices.map(service => (
                <div key={service.id} className="service-card-luxury">
                  <div className="service-img-wrapper">
                    <img src={service.image} alt={service.name} />
                    {service.isPopular && <span className="badge-popular">Top Rated</span>}
                    <span className="badge-time">?? {service.durationMinutes} min</span>
                  </div>

                  <div className="service-body">
                    <div className="service-meta-top">
                      <span className="rating-pill">? {service.rating} ({service.reviewsCount})</span>
                      <span className="category-pill">{service.category.toUpperCase()}</span>
                    </div>

                    <h3 className="service-name">{service.name}</h3>
                    <p className="service-explanation">{service.description}</p>

                    <div className="service-includes-box">
                      <span className="includes-label">Includes:</span>
                      <ul className="includes-list">
                        {service.features.map((feat, idx) => (
                          <li key={idx}>? {feat}</li>
                        ))}
                      </ul>
                    </div>

                    <div className="service-footer-action">
                      <div className="service-price-block">
                        <span className="cur-label">Total price</span>
                        <span className="cur-amount"></span>
                      </div>

                      <button
                        className="btn btn-primary btn-book"
                        onClick={() => handleBookService(service.id)}
                      >
                        Book Appointment
                      </button>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </section>

      {/* Booking Modal */}
      <BookingModal
        isOpen={isBookingOpen}
        onClose={() => setIsBookingOpen(false)}
        initialServiceId={selectedServiceId}
      />
    </div>
  );
};

export default Services;

