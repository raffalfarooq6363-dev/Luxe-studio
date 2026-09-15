# Luxe Glow Studio - Enhanced Booking System

## Overview
A comprehensive, production-ready booking system with advanced features for managing appointments, multi-service bookings, waiting lists, and automated notifications.

## Features

### 1. **Smart Booking Management**
- Single and multi-service bookings
- Automatic staff assignment
- Time slot validation
- Conflict prevention
- Business hours enforcement

### 2. **Waiting List System**
- Add customers to waiting list when slots are full
- Automatic notifications when slots become available
- Priority-based notification

### 3. **Automated Notifications**
- Booking confirmations
- Appointment reminders (24 hours before)
- Rescheduling notifications
- Cancellation notifications
- Background service for automated reminders

### 4. **Advanced Validation**
- Minimum advance booking time
- Maximum booking window
- Service-specific rules
- Staff availability checks
- Business hours validation

### 5. **Multi-Service Bookings**
- Book multiple services in one transaction
- Automatic time slot calculation
- Buffer time between services
- Cross-day booking support

## API Endpoints

### Booking Endpoints

#### POST `/api/booking`
Create a new single-service booking.

**Request Body:**
```json
{
  "userId": 1,
  "serviceId": 5,
  "appointmentDate": "2026-09-20",
  "startTime": "10:00:00",
  "notes": "First time customer",
  "specialRequests": "Please use hypoallergenic products",
  "clientPhone": "+1234567890",
  "clientEmail": "customer@example.com",
  "assignedStaffMember": "Sarah Johnson",
  "isHomeService": false,
  "offerCodeId": null
}
```

**Response:**
```json
{
  "id": 123,
  "userId": 1,
  "userName": "John Doe",
  "serviceId": 5,
  "serviceName": "Facial Treatment",
  "appointmentDate": "2026-09-20",
  "startTime": "10:00:00",
  "endTime": "11:00:00",
  "status": "Pending",
  "totalAmount": 85.00,
  "assignedStaffMember": "Sarah Johnson"
}
```

#### POST `/api/booking/multi-service`
Create a booking with multiple services.

**Request Body:**
```json
{
  "userId": 1,
  "serviceIds": [5, 8, 12],
  "appointmentDate": "2026-09-20",
  "startTime": "10:00:00",
  "notes": "Package booking",
  "clientPhone": "+1234567890",
  "clientEmail": "customer@example.com",
  "assignedStaffMember": "Sarah Johnson"
}
```

**Response:**
```json
{
  "bookings": [
    {
      "id": 123,
      "serviceName": "Facial Treatment",
      "startTime": "10:00:00",
      "endTime": "11:00:00"
    },
    {
      "id": 124,
      "serviceName": "Manicure",
      "startTime": "11:15:00",
      "endTime": "12:00:00"
    }
  ],
  "totalAmount": 150.00,
  "message": "Multi-service booking created successfully"
}
```

#### GET `/api/booking/availability`
Check available time slots for a service.

**Query Parameters:**
- `date`: Date to check (YYYY-MM-DD)
- `serviceId`: Service ID
- `staffMember`: (Optional) Specific staff member

**Response:**
```json
[
  {
    "startTime": "09:00:00",
    "endTime": "10:00:00",
    "isAvailable": true,
    "unavailableReason": null
  },
  {
    "startTime": "10:00:00",
    "endTime": "11:00:00",
    "isAvailable": false,
    "unavailableReason": "Already booked"
  }
]
```

#### POST `/api/booking/validate`
Validate if a booking can be made without actually creating it.

**Request Body:**
```json
{
  "serviceId": 5,
  "appointmentDate": "2026-09-20",
  "startTime": "10:00:00",
  "assignedStaffMember": "Sarah Johnson"
}
```

**Response:**
```json
{
  "message": "Booking is valid",
  "canBook": true
}
```

#### GET `/api/booking/{id}`
Get booking details by ID.

#### GET `/api/booking`
Get all bookings with filtering and pagination.

**Query Parameters:**
- `startDate`: Filter by start date
- `endDate`: Filter by end date
- `status`: Filter by status (Pending, Confirmed, Completed, Cancelled)
- `userId`: Filter by user
- `serviceId`: Filter by service
- `staffMember`: Filter by staff member
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)

#### GET `/api/booking/upcoming/{userId}`
Get all upcoming appointments for a user.

#### PUT `/api/booking/{id}/reschedule`
Reschedule an existing booking.

**Request Body:**
```json
{
  "newAppointmentDate": "2026-09-21",
  "newStartTime": "14:00:00",
  "reason": "Schedule conflict"
}
```

#### PUT `/api/booking/{id}/confirm`
Confirm a pending booking.

#### PUT `/api/booking/{id}/status`
Update booking status.

**Request Body:**
```json
{
  "status": "Completed",
  "reason": "Service completed successfully",
  "notes": "Customer was very satisfied"
}
```

#### DELETE `/api/booking/{id}`
Cancel a booking.

**Query Parameters:**
- `reason`: Cancellation reason

#### GET `/api/booking/stats`
Get booking statistics.

**Query Parameters:**
- `startDate`: Start date for statistics
- `endDate`: End date for statistics

**Response:**
```json
{
  "totalBookings": 150,
  "pendingBookings": 25,
  "confirmedBookings": 45,
  "completedBookings": 70,
  "cancelledBookings": 10,
  "totalRevenue": 12500.00,
  "todayRevenue": 850.00,
  "todayBookings": 8,
  "weeklyStats": [
    {
      "date": "2026-09-15",
      "bookingCount": 12,
      "revenue": 1500.00
    }
  ]
}
```

### Waiting List Endpoints

#### POST `/api/booking/waiting-list`
Add a customer to the waiting list.

**Request Body:**
```json
{
  "userId": 1,
  "serviceId": 5,
  "preferredDate": "2026-09-20",
  "preferredTimeRange": "Morning",
  "notes": "Flexible with timing"
}
```

## Business Logic

### Booking Rules
1. **Minimum Advance Booking**: Services can define minimum hours required for booking in advance
2. **Maximum Booking Window**: Services can limit how far in advance bookings can be made
3. **Business Hours**: Bookings only allowed between 9 AM - 6 PM
4. **Buffer Time**: 15 minutes buffer between consecutive services in multi-service bookings
5. **Conflict Prevention**: Automatic check for overlapping appointments

### Booking Statuses
- **Pending**: Initial status when booking is created
- **Confirmed**: Admin/staff has confirmed the booking
- **InProgress**: Service is currently being provided
- **Completed**: Service has been completed
- **Cancelled**: Booking was cancelled
- **NoShow**: Customer didn't show up

### Staff Assignment
- Automatic assignment to available staff if not specified
- Based on staff specializations and availability
- Prevents double-booking of staff members

### Notification Flow
1. **Booking Created** → Immediate confirmation notification
2. **24 Hours Before** → Automated reminder (via background service)
3. **Booking Rescheduled** → Notification with new details
4. **Booking Cancelled** → Cancellation notification
5. **Slot Available** → Waiting list notification

## Background Services

### Booking Reminder Service
- Runs daily at 6 PM
- Sends reminders for next day appointments
- Supports email and SMS (integration required)
- Marks reminders as sent to prevent duplicates

## Integration Points

### Email Service
Implement in `NotificationService.SendEmailReminderAsync()`:
- SendGrid
- AWS SES
- Mailgun
- Custom SMTP

### SMS Service
Implement in `NotificationService.SendSMSReminderAsync()`:
- Twilio
- AWS SNS
- Nexmo
- Custom SMS gateway

### Payment Integration
Ready for payment gateway integration:
- Stripe
- PayPal
- Square
- Razorpay

## Configuration

### Business Hours
Default: 9 AM - 6 PM
Can be configured in the business settings.

### Reminder Timing
Default: 24 hours before appointment
Configurable in `BookingReminderBackgroundService`.

### Time Slot Intervals
Default: 30 minutes
Configured in `GenerateTimeSlots()` method.

## Database Migrations

To add the waiting list and enhanced booking features:

```bash
cd "Backend/Luxe glow studio"
dotnet ef migrations add AddWaitingListAndEnhancedBooking
dotnet ef database update
```

## Testing

### Test Scenarios
1. **Single Booking**
   - Create booking → Verify confirmation notification
   - Check time slot availability
   - Validate booking rules

2. **Multi-Service Booking**
   - Book 3 services → Verify sequential time slots
   - Check cross-day booking if needed
   - Validate total amount calculation

3. **Conflict Prevention**
   - Try to book same slot twice → Should fail
   - Verify staff double-booking prevention

4. **Rescheduling**
   - Reschedule to available slot → Should succeed
   - Reschedule to booked slot → Should fail
   - Verify notification sent

5. **Waiting List**
   - Add to waiting list when full
   - Cancel booking → Verify waiting list notification

6. **Reminders**
   - Create booking for tomorrow
   - Wait for 6 PM reminder service
   - Verify reminder sent and marked

## Security Considerations
- Validate user permissions for bookings
- Prevent unauthorized cancellations
- Rate limit booking attempts
- Validate date ranges to prevent abuse
- Sanitize all user inputs

## Performance Optimization
- Index on AppointmentDate for faster queries
- Cache available time slots
- Batch notification sending
- Use async/await throughout
- Implement pagination for large result sets

## Future Enhancements
- [ ] Recurring appointments
- [ ] Group bookings
- [ ] Deposit/prepayment system
- [ ] Loyalty points integration
- [ ] Customer reviews post-appointment
- [ ] Staff schedule management
- [ ] Mobile app push notifications
- [ ] Calendar sync (Google, Outlook)
- [ ] Video consultation booking
- [ ] Dynamic pricing based on demand

## Support
For questions or issues, contact the development team or refer to the main API documentation.
