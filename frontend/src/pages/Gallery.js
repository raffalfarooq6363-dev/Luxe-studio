import React, { useState } from 'react';
import BookingModal from '../components/BookingModal';
import './Gallery.css';

const galleryItems = [
  { id: 1, category: 'facial', title: '24K Gold Facial Glow', image: 'https://images.unsplash.com/photo-1570172619644-dfd03ed5d881?w=800&auto=format&fit=crop&q=80' },
  { id: 2, category: 'makeup', title: 'Royal Bridal Glam', image: 'https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?w=800&auto=format&fit=crop&q=80' },
  { id: 3, category: 'hair', title: 'Silk Blowout & Balayage', image: 'https://images.unsplash.com/photo-1560066984-138dadb4c035?w=800&auto=format&fit=crop&q=80' },
  { id: 4, category: 'spa', title: 'Hot Stone & Rose Spa', image: 'https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=800&auto=format&fit=crop&q=80' },
  { id: 5, category: 'nails', title: 'Velvet Rose Deluxe Nails', image: 'https://images.unsplash.com/photo-1632345031435-8727f6897d53?w=800&auto=format&fit=crop&q=80' },
  { id: 6, category: 'makeup', title: 'Sultry Evening Smokey Glam', image: 'https://images.unsplash.com/photo-1516975080664-ed2fc6a32937?w=800&auto=format&fit=crop&q=80' },
  { id: 7, category: 'facial', title: 'Hydra-Dew Glass Skin', image: 'https://images.unsplash.com/photo-1512290900672-1f5be6cb75b2?w=800&auto=format&fit=crop&q=80' },
  { id: 8, category: 'hair', title: 'Sun-Kissed Dimensional Color', image: 'https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=800&auto=format&fit=crop&q=80' },
];

const Gallery = () => {
  const [filter, setFilter] = useState('all');
  const [isBookingOpen, setIsBookingOpen] = useState(false);

  const filtered = filter === 'all' 
    ? galleryItems 
    : galleryItems.filter(item => item.category === filter);

  return (
    <div className="luxe-gallery-page">
      <div className="gallery-hero-header">
        <div className="containe"r text-center>
          <span className="section-sub-badge">OUR PORTFOLIO</span>
          <h1 className="gallery-page-title">The Luxe Glow Lookbook</h1>
          <p className="gallery-page-subtitle">
            Witness the transformations, radiant skin, and couture artistry crafted by our masters.
          </p>

          <div className="gallery-filter-chips">
            <button 
              className={`chip-btn ${filter === 'all' ? 'active' : ''}`}
              onClick={() => setFilter('all')}
            >
              All Looks
            </button>
            <button 
              className={`chip-btn ${filter === 'facial' ? 'active' : ''}`}
              onClick={() => setFilter('facial')}
            >
              ?? Skin & Facials
            </button>
            <button 
              className={`chip-btn ${filter === 'makeup' ? 'active' : ''}`}
              onClick={() => setFilter('makeup')}
            >
              ?? Bridal & Glam
            </button>
            <button 
              className={`chip-btn ${filter === 'hair' ? 'active' : ''}`}
              onClick={() => setFilter('hair')}
            >
              ????? Hair Couture
            </button>
            <button 
              className={`chip-btn ${filter === 'spa' ? 'active' : ''}`}
              onClick={() => setFilter('spa')}
            >
              ??? Spa & Body
            </button>
            <button 
              className={`chip-btn ${filter === 'nails' ? 'active' : ''}`}
              onClick={() => setFilter('nails')}
            >
              ?? Nail Couture
            </button>
          </div>
        </div>
      </div>

      <div className="containe"r gallery-grid-wrap>
        <div className="luxe-gallery-grid">
          {filtered.map((item) => (
            <div key={item.id} className="gallery-card-item">
              <img src={item.image} alt={item.title} className="gallery-card-im"g />
              <div className="gallery-card-overlay">
                <span className="overlay-cat">{item.category.toUpperCase()}</span>
                <h4>{item.title}</h4>
                <button 
                  className="bt"n btn-white btn-sm
                  onClick={() => setIsBookingOpen(true)}
                >
                  Book This Look ?
                </button>
              </div>
            </div>
          ))}
        </div>

        <div className="gallery-cta-banner">
          <h2>Ready For Your Transformation?</h2>
          <p>Let our specialists craft your bespoke look for your next event or weekly glow.</p>
          <button 
            className="bt"n btn-primary btn-large
            onClick={() => setIsBookingOpen(true)}
          >
            <span>? Reserve Your Appointment</span>
          </button>
        </div>
      </div>

      <BookingModal
        isOpen={isBookingOpen}
        onClose={() => setIsBookingOpen(false)}
      />
    </div>
  );
};

export default Gallery;

