import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { servicesData, categories, timeSlots, practitioners } from '../data/store';
import BookingModal from '../components/BookingModal';
import './Home.css';

const Home = () => {
  const [isBookingOpen, setIsBookingOpen] = useState(false);
  const [selectedServiceId, setSelectedServiceId] = useState(null);
  const [selectedSpecialist, setSelectedSpecialist] = useState('');
  const [quickServiceId, setQuickServiceId] = useState(String(servicesData[0].id));
  const [quickDate, setQuickDate] = useState(new Date(Date.now() + 86400000).toISOString().split('T')[0]);
  const [quickSlot, setQuickSlot] = useState(timeSlots[1]);

  // Concern tab filter
  const [activeConcern, setActiveConcern] = useState('all');

  // FAQ open/close state
  const [openFaqIndex, setOpenFaqIndex] = useState(0);

  const openBooking = (serviceId = null, specialist = '') => {
    setSelectedServiceId(serviceId);
    setSelectedSpecialist(specialist);
    setIsBookingOpen(true);
  };

  const handleQuickBook = (event) => {
    event.preventDefault();
    openBooking(quickServiceId);
  };

  // Testimonials data
  const testimonials = [
    {
      id: 1,
      name: 'Victoria Stirling',
      role: 'VIP Member · Fashion Director',
      rating: 5,
      quote: 'The 24K Gold Ritual at Luxe Glow is simply peerless. The bespoke attention, heated treatment suites, and radiant results make every appointment an essential monthly indulgence.',
      initials: 'VS'
    },
    {
      id: 2,
      name: 'Genevieve Dupond',
      role: 'Bridal Guest',
      rating: 5,
      quote: 'Zara Khan and the team made me feel like royalty on my wedding day. My hair and skin stayed impeccably fresh for 14 hours. The champagne reception was the sweetest touch.',
      initials: 'GD'
    },
    {
      id: 3,
      name: 'Camilla Vance',
      role: 'Dermatology Advocate',
      rating: 5,
      quote: 'From pore vacuum extraction to LED collagen infusion, the Hydra-Dew facial gave me glassy, baby-soft skin without a second of downtime. Truly the pinnacle of modern aesthetics.',
      initials: 'CV'
    }
  ];

  // FAQs data
  const faqs = [
    {
      q: 'How do I prepare for my luxury facial or skincare treatment?',
      a: 'We recommend arriving 10 minutes prior to your reservation to enjoy our complimentary welcome elixir or French champagne. Please avoid heavy chemical exfoliants or sunbeds for 48 hours prior to your ritual.'
    },
    {
      q: 'Can I request a specific aesthetician or stylist?',
      a: 'Yes, absolutely! You can choose your preferred specialist directly when booking online or by contacting our concierge team.'
    },
    {
      q: 'What is your appointment confirmation & cancellation policy?',
      a: 'Once submitted, your booking is reviewed by our master team and an official luxury confirmation ticket is dispatched to your portal inbox and email. We graciously ask for 24-hour notice for any rescheduling.'
    },
    {
      q: 'Are your formulations cruelty-free and organic?',
      a: 'Every serum, botanical oil, and pigment in our studio is 100% cruelty-free, dermatologist-approved, and crafted with ethically sourced, high-potency organic ingredients.'
    },
    {
      q: 'Do you offer VIP packages and bridal party bookings?',
      a: 'Yes! We offer bespoke half-day and full-day private suite takeovers for bridal parties, red carpet prep, and executive wellness retreats with custom catering.'
    }
  ];

  // Gallery items
  const galleryItems = [
    {
      src: 'https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=800&auto=format&fit=crop&q=80',
      alt: 'Luxury Suite',
      caption: 'Private Aroma Therapy Suite',
      className: 'tall',
      tag: 'Sanctuary'
    },
    {
      src: 'https://images.unsplash.com/photo-1570172619644-dfd03ed5d881?w=800&auto=format&fit=crop&q=80',
      alt: '24K Gold Facial',
      caption: '24K Gold Flake Therapy',
      className: 'wide',
      tag: 'Signature'
    },
    {
      src: 'https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=800&auto=format&fit=crop&q=80',
      alt: 'Hair Styling',
      caption: 'Haute Balayage Studio',
      className: '',
      tag: 'Couture'
    },
    {
      src: 'https://images.unsplash.com/photo-1512290900672-1f5be6cb75b2?w=800&auto=format&fit=crop&q=80',
      alt: 'Hydra-Dew Extraction',
      caption: 'Deep Dew Extraction & LED',
      className: '',
      tag: 'Skin Science'
    },
    {
      src: 'https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?w=800&auto=format&fit=crop&q=80',
      alt: 'Bridal Glamour',
      caption: 'Couture Bridal Glam',
      className: 'wide',
      tag: 'Bridal'
    }
  ];

  // Filter services by active concern
  const filteredServices = activeConcern === 'all'
    ? servicesData
    : servicesData.filter(s => s.category === activeConcern);

  return (
    <div className="luxe-home">
      {/* 1. HERO SECTION */}
      <section className="luxe-hero">
        <div className="container hero-scene">
          <div className="hero-copy">
            <div className="hero-live-pill">
              <span className="live-dot"></span>
              <span>Sanctuary Open · Bespoke Reservations Available</span>
            </div>
            <span className="hero-eyebrow">THE HAUTE AESTHETICS SANCTUARY</span>
            <h1>Where beauty becomes your signature.</h1>
            <p className="hero-description">
              Bespoke skin, hair, bridal couture, and holistic spa rituals designed to elevate your natural radiance, leaving you polished, peaceful, and completely radiant.
            </p>
            <div className="hero-highlights" aria-label="Studio highlights">
              <span>Private VIP Suites</span>
              <span>24K Swiss Gold Serums</span>
              <span>Paris-Trained Artisans</span>
              <span>100% Organic</span>
            </div>
            <div className="hero-actions">
              <button type="button" className="btn btn-primary btn-large hero-main-cta" onClick={() => openBooking()}>
                Reserve Your Ritual
              </button>
              <Link to="/services" className="hero-text-link">Explore Haute Menu <span aria-hidden="true">-&gt;</span></Link>
            </div>
          </div>
          <div className="feature-photo right-photo">
            <img
              src="https://images.unsplash.com/photo-1522335789203-aabd1fc54bc9?auto=format&fit=crop&w=1200&q=80"
              alt="Pink luxury beauty treatment"
            />
            <div className="floating-service-chip">
              <span className="chip-sparkle">✨</span>
              <div>
                <strong>24K Gold Ritual</strong>
                <span className="chip-sub">Paris Luxury Aesthetic Award 2025</span>
              </div>
            </div>
            <div className="floating-rating-chip">
              <div className="avatar-group">
                <span className="mini-av">🌸</span>
                <span className="mini-av">✨</span>
                <span className="mini-av">💎</span>
              </div>
              <div className="rating-info">
                <strong>4.98 / 5.0 ★</strong>
                <span>Over 15,000+ Verified Guests</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* 2. PRESS & LUXURY TICKER MARQUEE */}
      <div className="luxury-ticker-wrap">
        <div className="luxury-ticker-content">
          <span>✦ VOGUE BEAUTY AWARDS 2025</span>
          <span>✦ HARPER'S BAZAAR TOP LUXURY SPA</span>
          <span>✦ 100% DERMATOLOGIST-FORMULATED ORGANICS</span>
          <span>✦ PARIS CERTIFIED MASTER AESTHETICIANS</span>
          <span>✦ 24K SWISS GOLD ACTIVE BOTANICALS</span>
          <span>✦ SOUNDPROOF PRIVATE HEATED SUITES</span>
          <span>✦ COMPLIMENTARY FRENCH CHAMPAGNE &amp; ELIXIRS</span>
          <span>✦ VOGUE BEAUTY AWARDS 2025</span>
          <span>✦ HARPER'S BAZAAR TOP LUXURY SPA</span>
          <span>✦ 100% DERMATOLOGIST-FORMULATED ORGANICS</span>
        </div>
      </div>

      {/* 3. UNIFIED LUXURY RESERVATION & METRICS SUITE */}
      <section className="luxe-reservation-suite-section">
        <div className="container">
          <div className="luxe-reservation-card">
            <div className="luxe-metrics-top-strip">
              <div className="metric-cell">
                <strong className="metric-num">15,000+</strong>
                <span className="metric-txt">Satisfied Guests</span>
              </div>
              <div className="metric-cell">
                <strong className="metric-num">4.98 ★</strong>
                <span className="metric-txt">Guest Rating</span>
              </div>
              <div className="metric-cell">
                <strong className="metric-num">25+</strong>
                <span className="metric-txt">Master Specialists</span>
              </div>
              <div className="metric-cell">
                <strong className="metric-num">100%</strong>
                <span className="metric-txt">Organic Formulations</span>
              </div>
            </div>

            <form className="quick-book-bar-clean" onSubmit={handleQuickBook}>
              <div className="quick-field-clean">
                <label htmlFor="quick-service">Select Treatment Ritual</label>
                <select id="quick-service" value={quickServiceId} onChange={(event) => setQuickServiceId(event.target.value)}>
                  {servicesData.map((service) => (
                    <option key={service.id} value={service.id}>{service.name} (${service.price})</option>
                  ))}
                </select>
              </div>
              <div className="quick-field-clean">
                <label htmlFor="quick-date">Preferred Date</label>
                <input
                  id="quick-date"
                  type="date"
                  value={quickDate}
                  min={new Date().toISOString().split('T')[0]}
                  onChange={(event) => setQuickDate(event.target.value)}
                />
              </div>
              <div className="quick-field-clean">
                <label htmlFor="quick-slot">Preferred Time Slot</label>
                <select id="quick-slot" value={quickSlot} onChange={(event) => setQuickSlot(event.target.value)}>
                  {timeSlots.map((slot) => (
                    <option key={slot} value={slot}>{slot}</option>
                  ))}
                </select>
              </div>
              <button type="submit" className="btn btn-primary quick-reserve-btn-luxe">
                Reserve Session
              </button>
            </form>
          </div>
        </div>
      </section>

      {/* 4. THE 4-STEP LUXE SANCTUARY JOURNEY */}
      <section className="journey-steps-section">
        <div className="container">
          <div className="section-header text-center">
            <span className="section-sub-badge">THE ELEVATED STANDARD</span>
            <h2 className="section-heading">The Luxe Sanctuary Experience</h2>
            <p className="section-lead">Every visit is thoughtfully curated from arrival to departure to ensure five-star serenity and unmatched results.</p>
          </div>

          <div className="journey-steps-grid">
            <div className="journey-step-card">
              <div className="step-number">01</div>
              <div className="step-icon">🔬</div>
              <h3>3D Diagnostic &amp; Elixir</h3>
              <p>Arrive to ceremonial matcha or chilled champagne while our specialist conducts a deep 3D skin &amp; hair diagnostic.</p>
            </div>

            <div className="journey-step-card">
              <div className="step-number">02</div>
              <div className="step-icon">🕯️</div>
              <h3>Private Heated Suite</h3>
              <p>Step into soundproof sanctuary suites with memory foam heated loungers, custom aromatherapy, and ambient soundscapes.</p>
            </div>

            <div className="journey-step-card">
              <div className="step-number">03</div>
              <div className="step-icon">✨</div>
              <h3>Master Artisan Ritual</h3>
              <p>Indulge in meticulous treatment application utilizing 24K active gold, organic Swiss serums, and gentle micro-technology.</p>
            </div>

            <div className="journey-step-card">
              <div className="step-number">04</div>
              <div className="step-icon">🎁</div>
              <h3>Signature Glow &amp; Gift</h3>
              <p>Admire your immediate radiant glow in our vanity lounge. Receive bespoke homecare recommendations and aftercare concierge support.</p>
            </div>
          </div>
        </div>
      </section>

      {/* 5. THE HAUTE AMBIANCE ("WHY LUXE GLOW") */}
      <section className="why-us-section">
        <div className="container">
          <div className="why-us-layout-clean">
            <div className="why-us-text">
              <span className="section-sub-badge">THE HAUTE AMBIANCE</span>
              <h2 className="section-heading">Why Discerning Clients Choose Luxe Glow Studio</h2>
              <p className="lead-p">
                We bridge medical-grade dermatological science with five-star hospitality to curate an unforgettable wellness sanctuary.
              </p>
              <div className="perks-list">
                <div className="perk-box">
                  <div className="perk-icon">🕯️</div>
                  <div>
                    <h4>Private VIP Treatment Sanctuaries</h4>
                    <p>Indulge in sound-isolated suites with heated memory foam beds, ambient soundscapes, and aromatherapy.</p>
                  </div>
                </div>
                <div className="perk-box">
                  <div className="perk-icon">🧬</div>
                  <div>
                    <h4>Bespoke Skin Barrier Diagnostics</h4>
                    <p>Every session begins with advanced 3D skin analysis to craft tailored antioxidant elixirs and serums.</p>
                  </div>
                </div>
                <div className="perk-box">
                  <div className="perk-icon">🥂</div>
                  <div>
                    <h4>Complimentary Champagne &amp; Elixir Bar</h4>
                    <p>Sip cold-pressed organic detox tonics, ceremonial matcha, or chilled champagne upon arrival.</p>
                  </div>
                </div>
              </div>
            </div>

            <div className="why-us-photo-frame">
              <img
                src="https://images.unsplash.com/photo-1540555700478-4be289fbecef?auto=format&fit=crop&w=1000&q=80"
                alt="Luxe Glow Sanctuary Interior"
                className="why-us-photo-img"
              />
              <div className="why-us-gold-badge">
                <span className="gold-star-ic">★</span>
                <span>10+ Years of Aesthetic Excellence</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* 6. TREATMENT CATEGORIES */}
      <section className="categories-section">
        <div className="container">
          <div className="section-header text-center">
            <span className="section-sub-badge">CURATED EXPERIENCES</span>
            <h2 className="section-heading">Indulge In Tailored Beauty Collections</h2>
            <p className="section-lead">Explore our curated collections of aesthetic &amp; pampering treatments.</p>
          </div>
          <div className="categories-grid">
            {categories.filter((category) => category.id !== 'all').map((category) => (
              <Link to="/services" key={category.id} className="category-card">
                <div className="category-icon-bubble">{category.icon}</div>
                <h3>{category.name}</h3>
                <p>Tailored treatments designed for radiant, transformative results</p>
                <span className="cat-explore-link">Explore Menu</span>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* 7. INTERACTIVE RITUALS BY CONCERN */}
      <section className="rituals-concern-section">
        <div className="container">
          <div className="section-header text-center">
            <span className="section-sub-badge">BESPOKE SELECTION</span>
            <h2 className="section-heading">Signature Rituals By Category</h2>
            <p className="section-lead">Select your desired ritual to experience immediate and lasting radiance.</p>
          </div>

          <div className="concern-tabs-bar">
            <button
              type="button"
              className={`concern-tab-btn ${activeConcern === 'all' ? 'active' : ''}`}
              onClick={() => setActiveConcern('all')}
            >
              All Rituals
            </button>
            <button
              type="button"
              className={`concern-tab-btn ${activeConcern === 'facial' ? 'active' : ''}`}
              onClick={() => setActiveConcern('facial')}
            >
              Skin Glow &amp; Facials
            </button>
            <button
              type="button"
              className={`concern-tab-btn ${activeConcern === 'hair' ? 'active' : ''}`}
              onClick={() => setActiveConcern('hair')}
            >
              Hair Couture &amp; Spa
            </button>
            <button
              type="button"
              className={`concern-tab-btn ${activeConcern === 'makeup' ? 'active' : ''}`}
              onClick={() => setActiveConcern('makeup')}
            >
              Bridal &amp; Glamour
            </button>
            <button
              type="button"
              className={`concern-tab-btn ${activeConcern === 'spa' ? 'active' : ''}`}
              onClick={() => setActiveConcern('spa')}
            >
              Body Spa &amp; Aromatherapy
            </button>
          </div>

          <div className="featured-services-grid">
            {filteredServices.slice(0, 6).map((service) => (
              <div key={service.id} className="luxe-service-card">
                <div className="card-image-wrap">
                  <img src={service.image} alt={service.name} />
                  {service.isPopular && <span className="service-badge-popular">Signature</span>}
                  <span className="service-badge-duration">{service.durationMinutes} min</span>
                </div>
                <div className="card-content-wrap">
                  <div className="card-rating-row">
                    <span className="stars">★ {service.rating}</span>
                    <span className="reviews">({service.reviewsCount} reviews)</span>
                  </div>
                  <h3 className="service-title">{service.name}</h3>
                  <p className="service-desc">{service.description}</p>
                  <ul className="service-feature-list">
                    {service.features.slice(0, 3).map((feature) => (
                      <li key={feature}>✦ {feature}</li>
                    ))}
                  </ul>
                  <div className="card-action-row">
                    <div className="price-block">
                      <span className="price-label">Starting at</span>
                      <span className="price-amount">${service.price}</span>
                    </div>
                    <button className="btn btn-primary btn-book-service" onClick={() => openBooking(service.id)}>
                      Reserve Now
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 8. MEET OUR MASTER SPECIALISTS */}
      <section className="specialists-section">
        <div className="container">
          <div className="section-header text-center">
            <span className="section-sub-badge">WORLD-CLASS ARTISANS</span>
            <h2 className="section-heading">Meet Our Master Specialists</h2>
            <p className="section-lead">Board-certified aestheticians, Paris-trained stylists, and celebrity makeup artists.</p>
          </div>
          <div className="specialists-grid">
            {practitioners.map((practitioner) => (
              <div key={practitioner.id} className="specialist-card">
                <div className="specialist-photo-wrap">
                  <img src={practitioner.image} alt={practitioner.name} />
                  <span className="specialist-exp-badge">⭐ {practitioner.rating}</span>
                </div>
                <div className="specialist-body">
                  <h3 className="specialist-name">{practitioner.name}</h3>
                  <p className="specialist-role">{practitioner.role}</p>
                  <p className="specialist-rating">
                    <strong>{practitioner.reviews} verified reviews</strong>
                  </p>
                  <button
                    type="button"
                    className="btn btn-outline-primary specialist-book-btn"
                    onClick={() => openBooking(null, practitioner.name)}
                  >
                    Book with {practitioner.name.split(' ')[1] || practitioner.name}
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 9. GLOWING CLIENT TESTIMONIALS */}
      <section className="testimonials-section">
        <div className="container">
          <div className="section-header text-center">
            <span className="section-sub-badge">VERIFIED PRAISE</span>
            <h2 className="section-heading">What Our Guests Say</h2>
            <p className="section-lead">Discover why over 15,000 clients trust Luxe Glow Studio with their defining moments.</p>
          </div>
          <div className="testimonials-grid">
            {testimonials.map((t, idx) => (
              <div key={t.id} className={`testimonial-card ${idx === 0 ? 'featured-testimonial' : ''}`}>
                <div className="test-stars">★★★★★</div>
                <p className="test-quote">"{t.quote}"</p>
                <div className="test-author-info">
                  <div className="test-avatar">{t.initials}</div>
                  <div>
                    <h4>{t.name}</h4>
                    <span>{t.role}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 10. STUDIO AESTHETIC GALLERY */}
      <section className="studio-gallery-section">
        <div className="container">
          <div className="section-header text-center">
            <span className="section-sub-badge">THE SANCTUARY</span>
            <h2 className="section-heading">Inside Luxe Glow Studio</h2>
            <p className="section-lead">An atmosphere designed to restore serenity, beauty, and confidence.</p>
          </div>
          <div className="gallery-grid">
            {galleryItems.map((item, index) => (
              <div key={index} className={`gallery-item ${item.className}`}>
                <img src={item.src} alt={item.alt} />
                <div className="gallery-overlay">
                  <span className="gallery-tag-pill">{item.tag}</span>
                  <span className="gallery-cap">{item.caption}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 11. FAQ ACCORDION */}
      <section className="faq-section">
        <div className="container">
          <div className="section-header text-center">
            <span className="section-sub-badge">QUESTIONS &amp; ANSWERS</span>
            <h2 className="section-heading">Frequently Asked Questions</h2>
            <p className="section-lead">Everything you need to know before stepping into our sanctuary.</p>
          </div>
          <div className="faq-list">
            {faqs.map((faq, index) => {
              const isOpen = openFaqIndex === index;
              return (
                <div key={index} className={`faq-item ${isOpen ? 'open' : ''}`}>
                  <button
                    type="button"
                    className="faq-question"
                    onClick={() => setOpenFaqIndex(isOpen ? -1 : index)}
                  >
                    <span>{faq.q}</span>
                    <span className="faq-icon">{isOpen ? '✕' : '+'}</span>
                  </button>
                  {isOpen && (
                    <div className="faq-answer">
                      <p>{faq.a}</p>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* 12. LUXURY CTA BANNER */}
      <section className="luxe-cta-banner">
        <div className="container">
          <div className="cta-banner-content">
            <span className="cta-badge">BESPOKE APPOINTMENTS</span>
            <h2>Ready To Experience The Luxe Glow?</h2>
            <p>
              Book your bespoke ritual today and enjoy complimentary hydration therapy, chilled French champagne, and a private consultation with our master aestheticians.
            </p>
            <div className="cta-buttons">
              <button className="btn btn-white btn-large" onClick={() => openBooking()}>
                Reserve Your Session Now
              </button>
              <Link to="/contact" className="btn btn-outline-white btn-large">Contact Studio Concierge</Link>
            </div>
          </div>
        </div>
      </section>

      {/* 13. FLOATING VIP CONCIERGE BOOKING BUTTON */}
      <button
        type="button"
        className="floating-vip-book-btn"
        onClick={() => openBooking()}
        title="Reserve an appointment instantly"
      >
        <span>✨ Reserve Appointment</span>
      </button>

      <BookingModal
        isOpen={isBookingOpen}
        onClose={() => setIsBookingOpen(false)}
        initialServiceId={selectedServiceId}
        initialPractitionerName={selectedSpecialist}
      />
    </div>
  );
};

export default Home;


