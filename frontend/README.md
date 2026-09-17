# Luxe Glow Studio - Frontend

A modern, professional React application for the Luxe Glow Studio Parlour Management System.

## 🚀 Features

### Public Pages
- **Home**: Hero section, features, popular services, and CTAs
- **About**: Company story, mission, values, and team
- **Services**: Complete service catalog with pricing and booking
- **Gallery**: Showcase of work and transformations
- **Contact**: Contact form and business information
- **Login/Register**: Secure authentication system

### Customer Dashboard
- Overview with booking statistics
- Book new services
- View and manage appointments
- Cancel/reschedule bookings
- Profile management
- Payment history

### Admin Dashboard
- Complete analytics and statistics
- User management (customers, practitioners)
- Booking management and assignment
- Service CRUD operations
- Payment tracking and reports
- System configuration

### Practitioner Dashboard
- Daily schedule and appointments
- Client management
- Availability settings
- Performance metrics
- Profile and portfolio management

## 🛠️ Tech Stack

- **React 19.3.0** - UI Framework
- **React Router DOM** - Routing
- **Axios** - HTTP Client
- **Stripe** - Payment Processing
- **React Toastify** - Notifications
- **CSS3** - Styling (Gradient designs, animations)

## 📁 Project Structure

```
src/
├── config/
│   └── api.js                 # Axios configuration
├── services/
│   ├── authService.js         # Authentication API calls
│   ├── bookingService.js      # Booking API calls
│   └── adminService.js        # Admin API calls
├── context/
│   └── AuthContext.js         # Authentication state management
├── components/
│   ├── Navbar.js/css          # Navigation bar
│   ├── Footer.js/css          # Footer
│   └── ProtectedRoute.js      # Route protection
├── pages/
│   ├── Home.js/css            # Landing page
│   ├── About.js/css           # About page
│   ├── Services.js/css        # Services listing
│   ├── Gallery.js/css         # Image gallery
│   ├── Contact.js/css         # Contact form
│   ├── Login.js               # Login page
│   ├── Register.js            # Registration page
│   ├── Auth.css               # Auth pages styling
│   ├── customer/
│   │   └── CustomerDashboard.js/css
│   ├── admin/
│   │   └── AdminDashboard.js/css
│   └── practitioner/
│       └── PractitionerDashboard.js/css
├── App.js                     # Main app component
├── App.css                    # App styling
├── index.js                   # Entry point
└── index.css                  # Global styles
```

## 🔧 Setup Instructions

### Prerequisites
- Node.js (v16 or higher)
- npm or yarn

### Installation

1. **Install Dependencies**
   ```bash
   cd frontend
   npm install
   ```

2. **Environment Configuration**
   
   Create/Update `.env` file:
   ```env
   REACT_APP_API_URL=http://localhost:5299/api
   REACT_APP_STRIPE_PUBLIC_KEY=your_stripe_public_key_here
   ```

3. **Start Development Server**
   ```bash
   npm start
   ```
   
   The app will open at `http://localhost:3000`

4. **Build for Production**
   ```bash
   npm run build
   ```

### Booking Email Notifications

The backend sends email when a customer creates a booking, when an admin confirms it, and when an admin rejects it. Rejected-booking emails include the reason entered by the admin.

Configure SMTP in the backend `appsettings.Development.json` or with environment variables before starting the API:

```json
{
   "Email": {
      "SmtpHost": "smtp.gmail.com",
      "SmtpPort": 587,
      "EnableSsl": true,
      "Username": "studio@example.com",
      "Password": "your-app-password",
      "From": "studio@example.com"
   }
}
```

For Gmail, use a Google App Password rather than the normal account password. Keep SMTP credentials out of source control.

## 🔐 Authentication Flow

1. User registers/logins through `/register` or `/login`
2. JWT token is stored in localStorage
3. Protected routes check authentication status
4. API calls automatically include the token
5. Token expiry redirects to login

## 🎨 Design Features

### Color Palette
- **Primary**: `#667eea` to `#764ba2` (Purple gradient)
- **Success**: `#28a745`
- **Warning**: `#ffc107`
- **Danger**: `#dc3545`
- **Info**: `#17a2b8`

### Key UI Elements
- **Gradient backgrounds** for hero sections
- **Card-based layouts** for content organization
- **Smooth animations** on hover and transitions
- **Responsive design** for mobile, tablet, desktop
- **Professional shadows** for depth
- **Rounded corners** for modern look

## 📱 Responsive Breakpoints

- **Desktop**: > 1200px
- **Tablet**: 768px - 1200px
- **Mobile**: < 768px

## 🔗 API Integration

All API calls are centralized in service files:

### Auth Service
```javascript
authService.register(userData)
authService.login(email, password)
authService.adminRegister(userData)
authService.logout()
```

### Booking Service
```javascript
bookingService.searchBookings(params)
bookingService.getAvailability(serviceId, date)
bookingService.createBooking(data)
bookingService.updateBooking(id, data)
bookingService.cancelBooking(id)
```

### Admin Service
```javascript
adminService.getDashboard()
adminService.getAnalytics(startDate, endDate)
adminService.getUsers(params)
adminService.createService(data)
```

## 🚦 Available Routes

### Public Routes
- `/` - Home
- `/about` - About
- `/services` - Services
- `/gallery` - Gallery
- `/contact` - Contact
- `/login` - Login
- `/register` - Register

### Protected Routes
- `/customer/*` - Customer Dashboard (Customer role)
- `/admin/*` - Admin Dashboard (Admin role)
- `/practitioner/*` - Practitioner Dashboard (Practitioner role)

## 🧩 Components Usage

### Protected Route
```jsx
<ProtectedRoute role="customer">
  <CustomerDashboard />
</ProtectedRoute>
```

### Auth Context
```jsx
const { user, login, logout, isAuthenticated } = useAuth();
```

## 🎯 Key Features Implementation

### 1. **Role-Based Access Control**
   - Routes protected by user role
   - Different dashboards for each role
   - Conditional UI elements based on permissions

### 2. **Booking System**
   - Check service availability
   - Create bookings with date/time selection
   - Cancel/reschedule appointments
   - View booking history

### 3. **Admin Panel**
   - Dashboard with statistics
   - User management
   - Service management
   - Booking oversight
   - Payment tracking

### 4. **Responsive Navigation**
   - Sticky header
   - Hamburger menu for mobile
   - User profile dropdown
   - Role-based navigation items

## 🔮 Future Enhancements

- [ ] Real-time notifications
- [ ] Live chat support
- [ ] Service reviews and ratings
- [ ] Photo upload for gallery
- [ ] Payment integration (Stripe)
- [ ] Email notifications
- [ ] SMS reminders
- [ ] Social media integration
- [ ] Multi-language support
- [ ] Dark mode theme

## 🐛 Common Issues & Solutions

### Port Already in Use
```bash
# Kill process on port 3000
npx kill-port 3000

# Or start on different port
PORT=3001 npm start
```

### CORS Errors
Ensure backend CORS is configured to allow:
```
http://localhost:3000
http://localhost:3001
```

### API Connection Failed
1. Check backend is running on port 5299
2. Verify `.env` file has correct API_URL
3. Check browser console for errors

## 📞 Support

For issues or questions:
- Email: dev@luxeglowstudio.com
- Documentation: See project wiki

## 📄 License

Copyright © 2026 Luxe Glow Studio. All rights reserved.

---

**Built with ❤️ by the Luxe Glow Studio Development Team**
