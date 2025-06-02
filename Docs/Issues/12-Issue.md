# Enforce HTTPS for all endpoints

- Implement end-to-end encryption for messages (use libraries such as libsodium/net).
- Encrypt media files at rest in storage.
- Store all authentication tokens securely (never expose secrets in client).
- Use secure cookies for session management.
- Implement rate limiting and IP blocking for abuse prevention.