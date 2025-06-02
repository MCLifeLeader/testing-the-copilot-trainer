# Allow multiple device sessions per user

- List active sessions in UI and allow remote logout.
- Invalidate tokens upon logout or password change.
- Store session tokens in a database with user ID and device info.
- Implement session management API for listing and revoking sessions.
- Ensure session tokens are securely stored and transmitted.
- Use JWT or similar tokens with expiration for session management.
- Consider using refresh tokens for long-lived sessions.
