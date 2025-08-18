# MessagingService

This service manages internal messaging, threads, and communication between users in the Medicare application.

## Features

- Direct messaging between users
- Message threads with multiple participants
- Message read receipts
- Message categorization (General, Appointment, Medical, System)
- Priority levels
- Conversation history

## API Endpoints

### Messages
- `POST /api/messaging/messages` - Send message
- `GET /api/messaging/messages/{id}` - Get message by ID
- `GET /api/messaging/messages/inbox/{userId}` - Get user inbox
- `GET /api/messaging/messages/sent/{userId}` - Get sent messages
- `GET /api/messaging/messages/unread/{userId}` - Get unread messages
- `PUT /api/messaging/messages/{id}/read` - Mark message as read
- `GET /api/messaging/messages/conversation/{userId1}/{userId2}` - Get conversation

### Threads
- `POST /api/messaging/threads` - Create message thread
- `GET /api/messaging/threads/{id}` - Get thread by ID
- `GET /api/messaging/threads/user/{userId}` - Get user threads
- `POST /api/messaging/threads/{id}/messages` - Send thread message
- `GET /api/messaging/threads/{id}/messages` - Get thread messages
- `GET /api/messaging/threads/{id}/participants` - Get thread participants

## Database Schema

- `messaging.Message` - Direct messages
- `messaging.Message_Thread` - Message threads
- `messaging.Thread_Participant` - Thread participants
- `messaging.Thread_Message` - Messages in threads
- `messaging.Message_Receipt` - Read receipts

## Port

- Development: 8086
