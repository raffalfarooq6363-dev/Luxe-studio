import React, { useState, useEffect } from 'react';
import { servicesData, categories, practitioners, timeSlots } from '../data/store';
import { saveNewBooking } from '../data/bookingStore';
import { useAuth } from '../context/AuthContext';
import './BookingModal.css';

const BookingModal = ({ isOpen, onClose, initialServiceId = null, onBookingSuccess }) => {
  const { user } = useAuth();
  const [step, setStep] = useState(1);
  const [selectedCategory, setSelectedCategory] = useState('all');
  const [selectedService, setSelectedService] = useState(null);
  const [selectedPractitioner, setSelectedPractitioner] = useState(null);
  const [appointmentDate, setAppointmentDate] = useState(
    new Date(Date.now() + 86400000).toISOString().split('T')[0]
  );
  const [selectedTimeSlot, setSelectedTimeSlot] = useState(timeSlots[1]);
  const [serviceType, setServiceType] = useState('Studio Appointment');
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    phone: '',
    notes: '',
    paymentMethod: 'salon'
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [confirmedBooking, setConfirmedBooking] = useState(null);

  useEffect(() => {
    if (user) {
      setFormData(prev => ({
        ...prev,
        name: user.firstName ? `${user.firstName} ${user.lastName || ''}`.trim() : prev.name,
        email: user.email || prev.email,
        phone: user.phoneNumber || prev.phone
      }));
    }
  }, [user]);

  useEffect(() => {
    if (isOpen) {
      if (initialServiceId) {
        const found = servicesData.find(s => s.id === Number(initialServiceId));
        if (found) {
          setSelectedService(found);
          setStep(2);
        } else {
          setSelectedService(servicesData[0]);
          setStep(1);
        }
      } else {
        if (!selectedService) {
          setSelectedService(servicesData[0]);
        }
        setStep(1);
      }
      setConfirmedBooking(null);
    }
  }, [isOpen, initialServiceId, selectedService]);

  if (!isOpen) return null;

  const filteredServices = selectedCategory === 'all'
    ? servicesData
    : servicesData.filter(s => s.category === selectedCategory);

  const handleNextStep = () => {
    if (step === 1 && !selectedService) {
      alert('Please select a service to proceed');
      return;
    }
    if (step === 3 && (!appointmentDate || !selectedTimeSlot)) {
      alert('Please select both a date and a time slot');
      return;
    }
    setStep(prev => prev + 1);
  };

  const handlePrevStep = () => {
    setStep(prev => Math.max(1, prev - 1));
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleConfirmBooking = (e) => {
    e.preventDefault();
    if (!formData.name || !formData.email || !formData.phone) {
      alert('Please provide your name, email, and phone number.');
      return;
    }

    setIsSubmitting(true);
    setTimeout(() => {
      const practitionerName = selectedPractitioner 
        ? selectedPractitioner.name 
        : 'First Available Specialist';
      const practitionerRole = selectedPractitioner 
        ? selectedPractitioner.role 
        : 'Senior Beauty Specialist';

      const bookingPayload = {
        serviceId: selectedService.id,
        serviceName: selectedService.name,
        category: selectedService.category,
        practitionerName,
        practitionerRole,
        appointmentDate,
        timeSlot: selectedTimeSlot,
        customerName: formData.name,
        customerEmail: formData.email,
        customerPhone: formData.phone,
        notes: formData.notes,
        serviceType,
        totalPrice: selectedService.price,
        paymentMethod: formData.paymentMethod
      };

      const result = saveNewBooking(bookingPayload);
      setConfirmedBooking(result);
      setIsSubmitting(false);
      setStep(5);
      if (onBookingSuccess) onBookingSuccess(result);
    }, 600);
  };

  const resetAndClose = () => {
    setStep(1);
    setConfirmedBooking(null);
    onClose();
  };

  return (
    <div className="booking-modal-overlay" onClick={resetAndClose}>
      <div className="booking-modal-container" onClick={e => e.stopPropagation()}>
        {/* Modal Header */}
        <div className="booking-modal-header">
          <div className="header-brand">
            <span className="brand-badge">✨ LUXE EXPERIENCE</span>
            <h2>Book Your Treatment</h2>
          </div>
          <button className="close-btn" onClick={resetAndClose} aria-label="Close modal">
            ✕
          </button>
        </div>

        {/* Step Indicator */}
        {step <= 4 && (
          <div className="booking-progress-bar">
            <div className={`progress-step ${step >= 1 ? 'active' : ''}`}>
              <span className="step-num">1</span>
              <span className="step-label">Service</span>
            </div>
            <div className={`progress-line ${step >= 2 ? 'active' : ''}`}></div>
            <div className={`progress-step ${step >= 2 ? 'active' : ''}`}>
              <span className="step-num">2</span>
              <span className="step-label">Specialist</span>
            </div>
            <div className={`progress-line ${step >= 3 ? 'active' : ''}`}></div>
            <div className={`progress-step ${step >= 3 ? 'active' : ''}`}>
              <span className="step-num">3</span>
              <span className="step-label">Schedule</span>
            </div>
            <div className={`progress-line ${step >= 4 ? 'active' : ''}`}></div>
            <div className={`progress-step ${step >= 4 ? 'active' : ''}`}>
              <span className="step-num">4</span>
              <span className="step-label">Details</span>
            </div>
          </div>
        )}

        {/* Modal Body */}
        <div className="booking-modal-body">
          {/* STEP 1: SELECT SERVICE */}
          {step === 1 && (
            <div className="step-content">
              <h3 className="step-title">Choose Your Luxury Service</h3>
              <p className="step-subtitle">Select the bespoke treatment crafted for your glow.</p>

              <div className="category-pills">
                {categories.map(cat => (
                  <button
                    key={cat.id}
                    className={`cat-pill ${selectedCategory === cat.id ? 'active' : ''}`}
                    onClick={() => setSelectedCategory(cat.id)}
                  >
                    <span>{cat.icon}</span> {cat.name}
                  </button>
                ))}
              </div>

              <div className="services-selection-grid">
                {filteredServices.map(service => (
                  <div
                    key={service.id}
                    className={`service-select-card ${selectedService?.id === service.id ? 'selected' : ''}`}
                    onClick={() => setSelectedService(service)}
                  >
                    <div className="service-card-img" style={{ backgroundImage: `url(${service.image})` }}>
                      {service.isPopular && <span className="tag-popular">Popular</span>}
                      <span className="service-duration">⏱️ {service.durationMinutes}m</span>
                    </div>
                    <div className="service-card-details">
                      <h4>{service.name}</h4>
                      <p className="desc">{service.description}</p>
                      <div className="card-foot">
                        <span className="price">${service.price}</span>
                        <span className="radio-indicator">
                          {selectedService?.id === service.id ? '✓ Selected' : 'Select'}
                        </span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* STEP 2: SELECT SPECIALIST */}
          {step === 2 && (
            <div className="step-content">
              <h3 className="step-title">Select Your Beauty Specialist</h3>
              <p className="step-subtitle">Hand-picked, certified masters in cosmetology & wellness.</p>

              <div className="practitioners-grid">
                <div
                  className={`practitioner-card ${selectedPractitioner === null ? 'selected' : ''}`}
                  onClick={() => setSelectedPractitioner(null)}
                >
                  <div className="practitioner-avatar-placeholder">
                    <span>✨</span>
                  </div>
                  <div className="practitioner-info">
                    <h4>First Available Specialist</h4>
                    <p className="role">Recommended for quickest availability</p>
                    <span className="badge-any">⭐ Best Matching Stylist</span>
                  </div>
                </div>

                {practitioners.map(spec => (
                  <div
                    key={spec.id}
                    className={`practitioner-card ${selectedPractitioner?.id === spec.id ? 'selected' : ''}`}
                    onClick={() => setSelectedPractitioner(spec)}
                  >
                    <img src={spec.image} alt={spec.name} className="practitioner-avatar" />
                    <div className="practitioner-info">
                      <h4>{spec.name}</h4>
                      <p className="role">{spec.role}</p>
                      <div className="practitioner-rating">
                        <span>⭐ {spec.rating}</span>
                        <span className="reviews">({spec.reviews} reviews)</span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* STEP 3: DATE & TIME */}
          {step === 3 && (
            <div className="step-content">
              <h3 className="step-title">Select Date & Time</h3>
              <p className="step-subtitle">Choose when you would like your luxurious session.</p>

              <div className="schedule-layout">
                <div className="date-selection-box">
                  <label>📅 Preferred Date</label>
                  <input
                    type="date"
                    className="custom-date-input"
                    value={appointmentDate}
                    min={new Date().toISOString().split('T')[0]}
                    onChange={(e) => setAppointmentDate(e.target.value)}
                  />
                  <div className="location-type-toggle">
                    <label>📍 Service Location</label>
                    <div className="toggle-group">
                      <button
                        type="button"
                        className={`toggle-btn ${serviceType === 'Studio Appointment' ? 'active' : ''}`}
                        onClick={() => setServiceType('Studio Appointment')}
                      >
                        🏛️ Salon Studio
                      </button>
                      <button
                        type="button"
                        className={`toggle-btn ${serviceType === 'VIP Home Service' ? 'active' : ''}`}
                        onClick={() => setServiceType('VIP Home Service')}
                      >
                        🏡 VIP Home Visit
                      </button>
                    </div>
                  </div>
                </div>

                <div className="time-slots-box">
                  <label>⏰ Available Slots for {appointmentDate}</label>
                  <div className="slots-grid">
                    {timeSlots.map(slot => (
                      <button
                        key={slot}
                        type="button"
                        className={`slot-chip ${selectedTimeSlot === slot ? 'active' : ''}`}
                        onClick={() => setSelectedTimeSlot(slot)}
                      >
                        {slot}
                      </button>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* STEP 4: CONTACT & CONFIRMATION */}
          {step === 4 && (
            <div className="step-content">
              <h3 className="step-title">Guest Details & Preferences</h3>
              <p className="step-subtitle">Almost there! Provide your details to secure your reservation.</p>

              <div className="booking-summary-banner">
                <div className="summary-col">
                  <span className="label">Treatment</span>
                  <span className="val">{selectedService?.name}</span>
                </div>
                <div className="summary-col">
                  <span className="label">Specialist</span>
                  <span className="val">{selectedPractitioner?.name || 'Any Expert'}</span>
                </div>
                <div className="summary-col">
                  <span className="label">Date & Time</span>
                  <span className="val">{appointmentDate} at {selectedTimeSlot}</span>
                </div>
                <div className="summary-col">
                  <span className="label">Total Due</span>
                  <span className="val price-tag">${selectedService?.price}</span>
                </div>
              </div>

              <form onSubmit={handleConfirmBooking} className="booking-form-grid">
                <div className="input-field">
                  <label>Full Name *</label>
                  <input
                    type="text"
                    name="name"
                    required
                    placeholder="e.g. Jessica Taylor"
                    value={formData.name}
                    onChange={handleInputChange}
                  />
                </div>
                <div className="input-field">
                  <label>Email Address *</label>
                  <input
                    type="email"
                    name="email"
                    required
                    placeholder="jessica@example.com"
                    value={formData.email}
                    onChange={handleInputChange}
                  />
                </div>
                <div className="input-field">
                  <label>Phone Number *</label>
                  <input
                    type="tel"
                    name="phone"
                    required
                    placeholder="+1 (555) 000-0000"
                    value={formData.phone}
                    onChange={handleInputChange}
                  />
                </div>
                <div className="input-field">
                  <label>Payment Preference</label>
                  <select
                    name="paymentMethod"
                    value={formData.paymentMethod}
                    onChange={handleInputChange}
                  >
                    <option value="salon">💳 Pay at Studio / On Arrival</option>
                    <option value="card">✨ Credit / Debit Card (Online Pre-pay)</option>
                    <option value="applepay">🍎 Apple Pay / Google Pay</option>
                  </select>
                </div>
                <div className="input-field full-width">
                  <label>Special Requests or Skin Allergies (Optional)</label>
                  <textarea
                    name="notes"
                    rows="2"
                    placeholder="Any specific skincare concerns, fragrance sensitivities, or requests..."
                    value={formData.notes}
                    onChange={handleInputChange}
                  ></textarea>
                </div>
              </form>
            </div>
          )}

          {/* STEP 5: BOOKING SUCCESS CELEBRATION */}
          {step === 5 && confirmedBooking && (
            <div className="step-content success-step">
              <div className="success-icon-wrapper">
                <div className="sparkle-circle">✨</div>
              </div>
              <h3 className="success-heading">Appointment Confirmed!</h3>
              <p className="success-sub">
                Thank you, <strong>{confirmedBooking.customerName}</strong>! Your luxury reservation has been booked.
              </p>

              <div className="confirmation-ticket">
                <div className="ticket-header">
                  <span>LUXE GLOW STUDIO RESERVATION</span>
                  <span className="booking-code">ID: {confirmedBooking.id}</span>
                </div>
                <div className="ticket-body">
                  <div className="ticket-row">
                    <span>Service</span>
                    <strong>{confirmedBooking.serviceName}</strong>
                  </div>
                  <div className="ticket-row">
                    <span>Specialist</span>
                    <strong>{confirmedBooking.practitionerName}</strong>
                  </div>
                  <div className="ticket-row">
                    <span>Date & Time</span>
                    <strong>{confirmedBooking.appointmentDate} • {confirmedBooking.timeSlot}</strong>
                  </div>
                  <div className="ticket-row">
                    <span>Location</span>
                    <strong>{confirmedBooking.serviceType}</strong>
                  </div>
                  <div className="ticket-row total">
                    <span>Total Amount</span>
                    <strong className="pink-highlight">${confirmedBooking.totalPrice}</strong>
                  </div>
                </div>
              </div>

              <div className="success-actions">
                <button
                  className="btn-pink-primary"
                  onClick={resetAndClose}
                >
                  Done & Back to Studio
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Modal Footer Controls */}
        {step <= 4 && (
          <div className="booking-modal-footer">
            {step > 1 ? (
              <button className="btn-back" onClick={handlePrevStep} disabled={isSubmitting}>
                ← Back
              </button>
            ) : <div></div>}

            <div className="footer-right">
              {step < 4 ? (
                <button className="btn-pink-primary" onClick={handleNextStep}>
                  Continue →
                </button>
              ) : (
                <button
                  className="btn-pink-primary"
                  onClick={handleConfirmBooking}
                  disabled={isSubmitting}
                >
                  {isSubmitting ? 'Securing Slot...' : '✨ Confirm & Reserve'}
                </button>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default BookingModal;
