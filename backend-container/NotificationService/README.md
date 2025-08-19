# NotificationService

A lightweight service that persists notifications from RabbitMQ and exposes a read API.

- Queue: `notifications.events` (JSON payload with RecipientUserId, Description, Type, SourceService, ActionUrl, PriorityLevel, ExpiresAt)
- Database: Azure SQL (AAD token optional) schema `notifications` table `Notification`
- Health: `/health`
- Swagger (dev): `/swagger`

API

- GET `/api/notifications?recipientUserId={id}&unreadOnly=false&page=1&pageSize=20`
- POST `/api/notifications/{id}/read`
