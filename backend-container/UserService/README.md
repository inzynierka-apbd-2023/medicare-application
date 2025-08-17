# UserService

ASP.NET Core 8 Web API handling authentication and user profiles. Uses Azure SQL (schema `[user]`) and publishes domain events via a transactional outbox.

## Messaging (Transactional Outbox ? RabbitMQ)

- Outbox table: `[user].[Outbox_Event]` (Id, Type, OccurredAt, PayloadJson, PublishedAt).
- A hosted background publisher reads unpublished rows and publishes to RabbitMQ:
	- Exchange: `user.events` (type: topic, durable)
	- Routing key: taken from `Outbox_Event.Type` (e.g., `user.created`)
	- MessageId: set to the outbox `Id` for end-to-end idempotency

Environment variables (docker-compose already sets these):

- `RABBITMQ__HOST` (default `rabbitmq`)
- `RABBITMQ__USERNAME` (default `guest`)
- `RABBITMQ__PASSWORD` (default `guest`)

## Key endpoints

- POST `/api/auth/login` – returns JWT for existing user
- POST `/api/auth/register` – creates user and enqueues `user.created` outbox event
- GET `/api/admin/outbox` – diagnostic endpoint listing recent outbox entries

### Users (new)

- GET `/api/users/{id}` – fetch profile (JWT required)
- PUT `/api/users/{id}` – update profile fields including `firstName`, `lastName`, and optional `avatarUrl` (JWT required)
- GET `/api/users/availability?email=&username=` – anonymous availability check for sign-up. Response shape:

	```json
	{ "emailExists": true|false, "usernameExists": true|false }
	```

Notes

- The registration flow returns `{ token, user }`; the `user.id` can be used immediately to update the profile.

## Local URLs

- Base: <http://localhost:8080>
- Swagger: <http://localhost:8080/swagger>
- Health: <http://localhost:8080/health>

## Quick test

1) Ensure `rabbitmq` and `user-service` are running via docker-compose.
2) Register a user; this creates an outbox row and publishes `user.created`:
	- Body requires: `username`, `password`, `email`, `firstName`, `lastName`.

Notes

- In non-production, EF Core migrations auto-apply on startup.
