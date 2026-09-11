import React from 'react';
import './Gallery.css';

const Gallery = () => {
  // Sample gallery data - will be replaced with API call
  const galleryImages = [
    { id: 1, category: 'Facial', alt: 'Facial Treatment' },
    { id: 2, category: 'Hair', alt: 'Hair Styling' },
    { id: 3, category: 'Makeup', alt: 'Bridal Makeup' },
    { id: 4, category: 'Nails', alt: 'Nail Art' },
    { id: 5, category: 'Spa', alt: 'Spa Treatment' },
    { id: 6, category: 'Hair', alt: 'Hair Coloring' },
    { id: 7, category: 'Makeup', alt: 'Party Makeup' },
    { id: 8, category: 'Facial', alt: 'Skin Care' },
  ];

  return (
    <div className="gallery-page">
      <div className="gallery-hero">
        <h1>Our Gallery</h1>
        <p>Explore our work and see the transformations</p>
      </div>

      <div className="container">
        <div className="gallery-grid">
          {galleryImages.map((image) => (
            <div key={image.id} className="gallery-item">
              <div className="gallery-image">
                <div className="image-placeholder">
                  <span>{image.category}</span>
                </div>
              </div>
              <div className="gallery-overlay">
                <p>{image.alt}</p>
              </div>
            </div>
          ))}
        </div>

        <div className="gallery-cta">
          <h2>Love What You See?</h2>
          <p>Book your appointment and let us create magic for you</p>
          <a href="/register" className="btn btn-primary btn-large">
            Book Now
          </a>
        </div>
      </div>
    </div>
  );
};

export default Gallery;
