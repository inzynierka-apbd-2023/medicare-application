# BillingService

.NET 8 Web API for payments and subscriptions.

- Schema: `billing`
- Tables: Payment_Method, Subscription_Contract, Payment_Intent, Payment_Transaction, Appointment_Payment, Subscription_Payment, Psp_Webhook_Event, Outbox_Event
- Views: `billing.vw_Patient_Billing_Summary`, `billing.vw_Doctor_Revenue_Dashboard` (keyless mappings)

APIs
- POST /api/payments/intents — create payment intent (appointment/subscription)
- GET /api/payments/intents/{id} — get intent
- POST /api/payments/intents/{id}/transactions — record ledger transaction (+enqueue outbox event)
- POST /api/payments/subscriptions/{contractId}/renewals — create next-period intent and emit renewal-due event
- POST /api/payments/webhooks/{provider} — idempotent inbox of PSP events

Events (Outbox Type)
- billing.appointment.paid
- billing.subscription.paid
- billing.payment.failed
- billing.subscription.renewal_due

Run via Docker Compose; supports Azure SQL AAD token auth like other services.
