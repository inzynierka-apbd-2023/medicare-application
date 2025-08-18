# Appointment Analytics Feature

## Overview

This feature implements a comprehensive appointment analytics system using CQRS architecture and the MediatR pattern. It provides detailed insights into appointment data, doctor performance, specialization statistics, and time slot analysis.

## Architecture

### CQRS Implementation
- **Queries**: Separate query models for different analytics requirements
- **Handlers**: Individual handlers for each query type using MediatR
- **DTOs**: Data transfer objects that match the frontend requirements
- **Validation**: Request validation for all analytics endpoints

### Key Components

1. **Controllers**
   - `AnalyticsController`: Main API controller with role-based authorization
   - Supports filtering by date range, doctor, specialization, and status

2. **Queries**
   - `GetAppointmentAnalyticsQuery`: Comprehensive dashboard data
   - `GetAppointmentMetricsQuery`: Key performance metrics
   - `GetAppointmentTrendsQuery`: Trend analysis over time
   - `GetDoctorPerformanceQuery`: Individual doctor performance
   - `GetSpecializationStatsQuery`: Specialization-wide statistics
   - `GetTimeSlotAnalysisQuery`: Time-based appointment analysis

3. **Handlers**
   - Individual handlers for each query type
   - Optimized database queries with proper filtering
   - Parallel execution for comprehensive analytics

4. **DTOs**
   - Aligned with frontend requirements from `analyticsApi.ts`
   - Proper data structure for metrics, trends, performance data

## API Endpoints

### Main Analytics Dashboard
```
GET /api/appointment/analytics/dashboard
```
**Parameters:**
- `startDate` (optional): Start date for analytics period
- `endDate` (optional): End date for analytics period  
- `doctorId` (optional): Filter by specific doctor
- `specialization` (optional): Filter by specialization
- `status` (optional): Filter by appointment status

**Response:** Complete analytics response with all data types

### Individual Endpoints
- `GET /api/appointment/analytics/metrics` - Key metrics only
- `GET /api/appointment/analytics/trends` - Trend data over time
- `GET /api/appointment/analytics/doctor-performance` - Doctor performance data
- `GET /api/appointment/analytics/specialization-stats` - Specialization statistics
- `GET /api/appointment/analytics/time-slot-analysis` - Time slot analysis

## Authorization & Security

### Role-Based Access Control
- **Admin/Manager**: Full access to all analytics data
- **Doctor**: Restricted to their own data only
- **Receptionist**: Access to general analytics (implementation dependent)
- **Patient**: No access to analytics endpoints

### Data Security
- All endpoints require authentication via JWT tokens
- User roles validated on each request
- Automatic data filtering based on user permissions
- Input validation on all parameters

## Data Sources

The analytics system queries data from multiple database tables:
- `Schedule_Appointment`: Main appointment data
- `Schedule_Appointment_Status`: Appointment status information
- `User`, `User_Profile`: User information for doctors and patients
- `Doctor`, `Doctor_Specialization`, `Specialization`: Doctor and specialization data
- `Appointment_Payment`: Payment and revenue data
- `Rate`: Rating and feedback data

## Notifications

Analytics access is tracked via the notification system:
- Notification created when analytics dashboard is accessed
- Includes user information and timestamp
- Configurable notification types and priorities

## Validation

Comprehensive request validation includes:
- Date range validation (max 365 days, no future dates)
- Doctor ID format validation (must be valid GUID)
- Specialization name validation (letters and spaces only)
- Status validation (must be valid appointment status)

## Testing

### Unit Tests
- Handler tests with in-memory database
- Controller tests with mocked dependencies
- Validation tests for all input parameters
- Authorization tests for different user roles

### Integration Tests
- End-to-end API testing
- Database integration testing
- Authentication and authorization testing

## Performance Considerations

1. **Optimized Queries**
   - Proper indexing on date and user ID fields
   - Efficient joins and filtering
   - Parallel execution of independent queries

2. **Caching Strategy**
   - Consider implementing response caching for frequently accessed data
   - Cache invalidation on data updates

3. **Database Performance**
   - Indexed columns: `Day`, `Doctor_User_Id`, `Patient_User_Id`, `Status_Id`
   - Proper query optimization for large datasets

## Configuration

### Required Services
- MediatR for CQRS pattern
- Entity Framework Core for database access
- JWT Bearer authentication
- Custom notification service

### Database Schema
Requires access to the main Medicare database schema with read permissions on:
- User management tables
- Appointment tables  
- Payment tables
- Rating tables

## Deployment Notes

1. **Database Access**: Ensure proper connection strings and permissions
2. **Authentication**: Configure JWT settings properly
3. **Logging**: Comprehensive logging for monitoring and debugging
4. **Error Handling**: Graceful error handling with proper HTTP status codes

## Future Enhancements

1. **Real-time Analytics**: WebSocket support for live updates
2. **Advanced Filtering**: More sophisticated filtering options
3. **Export Functionality**: CSV/PDF export of analytics data
4. **Scheduled Reports**: Automated report generation
5. **Predictive Analytics**: Machine learning integration for forecasting
