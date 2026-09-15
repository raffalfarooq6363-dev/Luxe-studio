export const categories = [
  { id: 'all', name: 'All Services', icon: '✨' },
  { id: 'facial', name: 'Facial & Skin Glow', icon: '🌸' },
  { id: 'hair', name: 'Hair Care & Styling', icon: '💇‍♀️' },
  { id: 'makeup', name: 'Bridal & Makeup', icon: '💄' },
  { id: 'spa', name: 'Spa & Relaxation', icon: '🕯️' },
  { id: 'nails', name: 'Nail Art & Care', icon: '💅' }
];

export const practitioners = [
  {
    id: 1,
    name: 'Dr. Sophia Miller',
    role: 'Lead Aesthetician & Skin Specialist',
    rating: 4.9,
    reviews: 142,
    image: 'https://images.unsplash.com/photo-1594744803329-e58b31de8bf5?w=400&auto=format&fit=crop&q=80',
    specialty: 'facial'
  },
  {
    id: 2,
    name: 'Elena Rostova',
    role: 'Master Hair Stylist & Colorist',
    rating: 5.0,
    reviews: 188,
    image: 'https://images.unsplash.com/photo-1580489944761-15a19d654956?w=400&auto=format&fit=crop&q=80',
    specialty: 'hair'
  },
  {
    id: 3,
    name: 'Zara Khan',
    role: 'Celebrity Makeup & Bridal Artist',
    rating: 4.9,
    reviews: 210,
    image: 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=400&auto=format&fit=crop&q=80',
    specialty: 'makeup'
  },
  {
    id: 4,
    name: 'Maya Lin',
    role: 'Senior Spa & Holistic Therapist',
    rating: 4.8,
    reviews: 95,
    image: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=400&auto=format&fit=crop&q=80',
    specialty: 'spa'
  },
  {
    id: 5,
    name: 'Chloe Bennett',
    role: 'Nail Couture & Care Specialist',
    rating: 4.9,
    reviews: 120,
    image: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=400&auto=format&fit=crop&q=80',
    specialty: 'nails'
  }
];

export const servicesData = [
  {
    id: 1,
    name: '24K Gold Luxury Facial',
    category: 'facial',
    description: 'Infusion of pure 24K gold serum with micro-needling & lymphatic drainage for instant youth glow.',
    price: 120,
    durationMinutes: 75,
    isPopular: true,
    rating: 4.9,
    reviewsCount: 89,
    image: 'https://images.unsplash.com/photo-1570172619644-dfd03ed5d881?w=600&auto=format&fit=crop&q=80',
    features: ['Hydrating Mask', '24K Gold Flakes', 'Neck & Shoulder Massage', 'UV Shield Treatment']
  },
  {
    id: 2,
    name: 'Hydra-Dew Glass Skin Facial',
    category: 'facial',
    description: 'Deep pore vacuum extraction with multi-vitamin hyaluronic acid infusion for glassy, poreless skin.',
    price: 95,
    durationMinutes: 60,
    isPopular: true,
    rating: 4.8,
    reviewsCount: 114,
    image: 'https://images.unsplash.com/photo-1512290900672-1f5be6cb75b2?w=600&auto=format&fit=crop&q=80',
    features: ['AHA/BHA Exfoliation', 'Pore Vacuuming', 'LED Light Therapy', 'Cooling Collagen Mask']
  },
  {
    id: 3,
    name: 'Royal Bridal Glow & Makeup',
    category: 'makeup',
    description: 'Complete high-definition bridal glam with luxury airbrushing, lashes, and 12-hour setting.',
    price: 250,
    durationMinutes: 150,
    isPopular: true,
    rating: 5.0,
    reviewsCount: 162,
    image: 'https://images.unsplash.com/photo-1487412720507-e7ab37603c6f?w=600&auto=format&fit=crop&q=80',
    features: ['HD Airbrush Makeup', 'Mink Lashes Included', 'Hair Sculpting & Veil Setting', 'Touch-up Kit']
  },
  {
    id: 4,
    name: 'Silk Infusion Hair Spa & Blowdry',
    category: 'hair',
    description: 'Keratin & argan oil steam treatment restoring frizz-damaged hair followed by signature blowout.',
    price: 85,
    durationMinutes: 60,
    isPopular: true,
    rating: 4.9,
    reviewsCount: 76,
    image: 'https://images.unsplash.com/photo-1560066984-138dadb4c035?w=600&auto=format&fit=crop&q=80',
    features: ['Deep Steam Massage', 'Moroccan Argan Elixir', 'Split-end Trim', 'Signature Volume Blowout']
  },
  {
    id: 5,
    name: 'Balayage & Couture Hair Color',
    category: 'hair',
    description: 'Custom sun-kissed hand-painted balayage with Olaplex bond builder and toner gloss.',
    price: 180,
    durationMinutes: 140,
    isPopular: false,
    rating: 4.8,
    reviewsCount: 53,
    image: 'https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=600&auto=format&fit=crop&q=80',
    features: ['Color Consultation', 'Olaplex Protection', 'Custom Gloss & Toner', 'Styling Finish']
  },
  {
    id: 6,
    name: 'Rose Quartz Aromatherapy Spa',
    category: 'spa',
    description: 'Relaxing full-body hot stone massage with organic rose and jasmine essential oils.',
    price: 140,
    durationMinutes: 90,
    isPopular: true,
    rating: 5.0,
    reviewsCount: 98,
    image: 'https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=600&auto=format&fit=crop&q=80',
    features: ['Warm Rose Oil', 'Himalayan Hot Stones', 'Aromatherapy Diffuser', 'Herbal Detox Tea']
  },
  {
    id: 7,
    name: 'Velvet Rose Deluxe Mani-Pedi',
    category: 'nails',
    description: 'Petal soak, sugar scrub exfoliation, cuticle therapy and long-lasting luxury gel polish.',
    price: 65,
    durationMinutes: 50,
    isPopular: false,
    rating: 4.7,
    reviewsCount: 64,
    image: 'https://images.unsplash.com/photo-1632345031435-8727f6897d53?w=600&auto=format&fit=crop&q=80',
    features: ['Rose Petal Soak', 'Organic Sugar Scrub', 'Gel Coat Finish', 'Paraffin Wax Hydration']
  },
  {
    id: 8,
    name: 'Evening Party Glam Makeup',
    category: 'makeup',
    description: 'Soft glam or sultry smokey eye with contouring and camera-ready highlighter.',
    price: 90,
    durationMinutes: 60,
    isPopular: false,
    rating: 4.8,
    reviewsCount: 42,
    image: 'https://images.unsplash.com/photo-1516975080664-ed2fc6a32937?w=600&auto=format&fit=crop&q=80',
    features: ['Full Face Glam', 'Custom Lash Application', 'Lip Liner & Gloss Bar', 'Fixing Spray']
  }
];

export const timeSlots = [
  '09:00 AM',
  '10:30 AM',
  '12:00 PM',
  '01:30 PM',
  '03:00 PM',
  '04:30 PM',
  '06:00 PM',
  '07:30 PM'
];
