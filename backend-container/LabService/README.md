# LabService

This service manages laboratory orders, tests, results, and reviews for the Medicare application.

## Features

- Lab order management
- Lab test tracking
- Lab result recording
- Result review workflow
- LOINC code integration

## API Endpoints

### Lab Results
- `GET /api/lab/labresults/patient/{patientId}` - Get patient lab results
- `GET /api/lab/labresults/{id}` - Get lab result by ID
- `GET /api/lab/labresults/{id}/detail` - Get detailed result with test and order info
- `POST /api/lab/labresults` - Create lab result
- `GET /api/lab/labresults/pending-review` - Get pending review results
- `POST /api/lab/labresults/{id}/review` - Review lab result

### Lab Orders
- `POST /api/lab/laborders` - Create lab order
- `GET /api/lab/laborders/{id}` - Get lab order by ID
- `GET /api/lab/laborders/patient/{patientId}` - Get patient lab orders
- `GET /api/lab/laborders/{id}/tests` - Get order tests
- `POST /api/lab/laborders/{id}/tests` - Add test to order
- `PUT /api/lab/laborders/{id}/status` - Update order status

## Database Schema

- `lab.Lab_Order` - Laboratory orders
- `lab.Lab_Test` - Individual tests with LOINC codes
- `lab.Lab_Result` - Test results
- `lab.Lab_Result_Review` - Doctor reviews of results

## Port

- Development: 8089
