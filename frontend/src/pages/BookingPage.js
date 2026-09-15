import React, { useState } from 'react';
import BookingModal from '../components/BookingModal';
import { useNavigate } from 'react-router-dom';

const BookingPage = () => {
  const navigate = useNavigate();
  const [isOpen, setIsOpen] = useState(true);

  const handleClose = () => {
    setIsOpen(false);
    navigate(-1);
  };

  return (
    <div style={{ minHeight: '80vh', padding: '4rem 1rem', background: '#faf7f9' }}>
      <BookingModal 
        isOpen={isOpen} 
        onClose={handleClose} 
        onBookingSuccess={() => {}}
      />
    </div>
  );
};

export default BookingPage;
