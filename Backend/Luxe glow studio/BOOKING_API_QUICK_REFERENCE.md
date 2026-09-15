# Booking API - Quick Reference

## Base URL
```
https://localhost:7xxx/api/booking
```

## Endpoints Overview

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/booking` | Create single booking |
| POST | `/api/booking/multi-service` | Create multi-service booking |
| POST | `/api/booking/waiting-list` | Add to waiting list |
| POST | `/api/booking/validate` | Validate booking |
| GET | `/api/booking/{id}` | Get booking by ID |
| GET | `/api/booking` | Get all bookings (filtered) |
| GET | `/api/booking/availability` | Check time slot availability |
| GET | `/api/booking/upcoming/{userId}` | Get upcoming appointments |
| GET | `/api/booking/stats` | Get booking statistics |
| PUT | `/api/booking/{id}/reschedule` | Reschedule booking |
| PUT | `/api/booking/{id}/confirm` | Confirm booking |
| PUT | `/api/booking/{id}/status` | Update booking status |
| DELETE | `/api/booking/{id}` | Cancel booking |

## Common Request Examples

### 1. Create Booking
```json
POST /api/booking
{
  "userId": 1,
  "serviceId": 5,
  "appointmentDate": "2026-09-20",
  "startTime": "10:00:00",
  "notes": "First visit",
  "clientPhone": "+1234567890",
  "assignedStaffMember": "Sarah"
}
```

### 2. Check Availability
```
GET /api/booking/availability?serviceId=5&date=2026-09-20&staffMember=Sarah
```

### 3. Multi-Service Booking
```json
POST /api/booking/multi-service
{
  "userId": 1,
  "serviceIds": [5, 8, 12],
  "appointmentDate": "2026-09-20",
  "startTime": "10:00:00"
}
```

### 4. Reschedule
```json
PUT /api/booking/123/reschedule
{
  "newAppointmentDate": "2026-09-21",
  "newStartTime": "14:00:00",
  "reason": "Schedule conflict"
}
```

### 5. Get Bookings with Filters
```
GET /api/booking?status=Pending&startDate=2026-09-01&endDate=2026-09-30&page=1&pageSize=20
```

### 6. Add to Waiting List
```json
POST /api/booking/waiting-list
{
  "userId": 1,
  "serviceId": 5,
  "preferredDate": "2026-09-20",
  "preferredTimeRange": "Morning"
}
```

## Response Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request (validation failed) |
| 404 | Not Found |
| 500 | Server Error |

## Common Error Messages

| Message | Reason |
|---------|--------|
| "User not found" | Invalid userId |
| "Service not found or not available" | Invalid/inactive serviceId |
| "Time slot is not available" | Already booked |
| "Cannot book appointments in the past" | Date is in the past |
| "Service requires at least X hours advance booking" | Too close to appointment time |
| "Cannot book more than X days in advance" | Too far in future |
| "Booking time must be between 9 AM and 6 PM" | Outside business hours |

## Booking Status Flow

```
Pending → Confirmed → InProgress → Completed
   ↓
Cancelled
```

## Time Format
- Date: `YYYY-MM-DD` (e.g., "2026-09-20")
- Time: `HH:MM:SS` (e.g., "10:00:00")

## Filter Parameters

### For GET /api/booking
- `startDate`: Filter from date
- `endDate`: Filter to date
- `status`: Pending|Confirmed|Completed|Cancelled
- `userId`: User ID
- `serviceId`: Service ID
- `staffMember`: Staff name
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)

## Business Rules

1. **Business Hours**: 9:00 AM - 6:00 PM
2. **Time Slots**: 30-minute intervals
3. **Buffer Time**: 15 minutes between services (multi-service)
4. **Reminders**: Sent 24 hours before appointment
5. **Minimum Advance**: Service-specific (usually 2 hours)
6. **Maximum Advance**: Service-specific (usually 30 days)

## Notification Events

| Event | Trigger |
|-------|---------|
| Booking Confirmation | Booking created |
| Booking Confirmed | Status changed to Confirmed |
| Booking Rescheduled | Booking rescheduled |
| Booking Cancelled | Booking cancelled |
| Appointment Reminder | 24 hours before (6 PM daily job) |
| Slot Available | Added to waiting list & slot freed |

## Testing URLs

### Development
```
https://localhost:7xxx/api/booking
```

### Swagger UI
```
https://localhost:7xxx/swagger
```

## Quick Test Sequence

1. **Check availability**: GET `/availability?serviceId=5&date=2026-09-20`
2. **Validate booking**: POST `/validate` with booking details
3. **Create booking**: POST `/booking` with full details
4. **View booking**: GET `/booking/{id}`
5. **Reschedule if needed**: PUT `/booking/{id}/reschedule`
6. **Confirm**: PUT `/booking/{id}/confirm`

## Common Workflows

### Customer Booking Flow
1. Browse services
2. Check availability
3. Create booking
4. Receive confirmation
5. Get reminder (24h before)
6. Complete appointment

### Admin Management Flow
1. View all bookings (with filters)
2. Confirm pending bookings
3. Manage staff assignments
4. Handle cancellations
5. View statistics

### Rescheduling Flow
1. Customer requests reschedule
2. Check new slot availability
3. Reschedule booking
4. Send notification
5. Confirm new appointment

## Integration Points

### Required Integrations
- [ ] Payment Gateway (Stripe/PayPal)
- [ ] Email Service (SendGrid/AWS SES)
- [ ] SMS Service (Twilio/AWS SNS)

### Optional Integrations
- [ ] Calendar Sync (Google/Outlook)
- [ ] CRM System
- [ ] Analytics Platform
- [ ] Mobile Push Notifications

## Need Help?

- Full Documentation: `BOOKING_SYSTEM_GUIDE.md`
- Implementation Summary: `BOOKING_SYSTEM_SUMMARY.md`
- Swagger UI: `/swagger`
