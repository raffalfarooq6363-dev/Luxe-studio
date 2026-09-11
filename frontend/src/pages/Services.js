import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../config/api';
import './Services.css';

const Services = () => {
  const [services, setServices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchServices();
  }, []);

  const fetchServices = async () => {
    try {
      // This endpoint needs to be created in the backend
      const response = await api.get('/services');
      setServices(response.data);
    } catch (err) {
      setError('Failed to load services. Showing sample data.');
      // Sample data for now
      setServices([
        {
          id: 1,
          name: 'Classic Facial',
          description: 'Deep cleansing facial treatment for glowing skin',
          price: 80,
          durationMinutes: 60,
          isPopular: true,
        },
        {
          id: 2,
          name: 'Hair Styling',
          description: 'Professional hair cut and styling',
          price: 60,
          durationMinutes: 45,
          isPopular: true,
        },
        {
          id: 3,
          name: 'Bridal Makeup',
          description: 'Complete bridal makeup package',
          price: 150,
          durationMinutes: 120,
          isPopular: true,
        },
        {
          id: 4,
          name: 'Manicure & Pedicure',
          description: 'Complete nail care and polish',
          price: 50,
          durationMinutes: 60,
        },
        {
          id: 5,
          name: 'Spa Package',
          description: 'Relaxing full body spa treatment',
          price: 200,
          durationMinutes: 180,
        },
        {
          id: 6,
          name: 'Hair Coloring',
          description: 'Professional hair coloring service',
          price: 120,
          durationMinutes: 120,
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="services-page">
        <div className="loading">Loading services...</div>
      </div>
    );
  }

  return (
    <div className="services-page">
      <div className="services-hero">
        <h1>Our Services</h1>
        <p>Discover our range of premium beauty and wellness services</p>
      </div>

      {error && <div className="info-message">{error}</div>}

      <div className="container">
        <div className="services-list">
          {services.map((service) => (
            <div key={service.id} className="service-item">
              {service.isPopular && <span className="popular-badge">Popular</span>}
              <div className="service-info">
                <h3>{service.name}</h3>
                <p>{service.description}</p>
                <div className="service-meta">
                  <span className="price">${service.price}</span>
                  <span className="duration">{service.durationMinutes} mins</span>
                </div>
              </div>
              <div className="service-actions">
                <Link to="/register" className="btn btn-primary">
                  Book Now
                </Link>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default Services;
