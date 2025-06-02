# Add support for sending images and video in chat

- Blazor UI for uploading/capturing images and video (with preview).
- Backend API to receive, compress, and store media (preferably in Azure Blob Storage).
- Generate thumbnails and serve optimized previews in chat.
- Secure access to media files (temporary or authenticated URLs).
- Update chat message model to include media attachments.
- Handle media in SignalR messages (e.g., display images/videos inline).