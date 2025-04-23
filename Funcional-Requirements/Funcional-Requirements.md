### **WebDAV Core Operations Table**

| HTTP Method   | Purpose                                                   | Required? | Description                                                                 |
|---------------|------------------------------------------------------------|-----------|-----------------------------------------------------------------------------|
| `OPTIONS`     | Discover server capabilities                               | Yes       | Used by clients to check supported methods and DAV extensions.             |
| `PROPFIND`    | Retrieve properties (metadata) and directory listings      | Yes       | Allows listing directories and reading file/folder metadata.               |
| `PROPPATCH`   | Update or remove properties                                | Yes       | Modify file/folder metadata (e.g., author, title).                         |
| `MKCOL`       | Create a new collection (folder)                           | Yes       | Creates a directory at the given path.                                     |
| `GET`         | Download a file                                            | Yes       | Retrieves the contents of a resource (like a file).                        |
| `PUT`         | Upload a file                                              | Yes       | Uploads a new file or replaces an existing one.                            |
| `DELETE`      | Delete a file or folder                                    | Yes       | Deletes a resource (file or folder recursively).                           |
| `COPY`        | Copy a resource to a new location                          | Yes       | Copies a file or folder on the server.                                     |
| `MOVE`        | Move/rename a resource                                     | Yes       | Moves or renames a file/folder on the server.                              |
| `LOCK`        | Lock a resource for editing                                | Optional* | Prevents simultaneous editing. Needed for collaborative use.               |
| `UNLOCK`      | Unlock a previously locked resource                        | Optional* | Releases a lock on a file/folder.                                          |

---

### Notes:

- **`OPTIONS`** is essential for feature discovery—clients use this to check what’s supported.
- **`PROPFIND`** is critical for showing file lists and reading custom or default metadata.
- **`LOCK`/`UNLOCK`** are **optional in RFC 4918**, but often required for compatibility with clients like **Windows**, **macOS**, or **Microsoft Office**.
- Your server should also handle normal HTTP status codes (`200 OK`, `201 Created`, `204 No Content`, `403 Forbidden`, etc.) and appropriate **HTTP headers** like `ETag`, `If-Match`, and `Depth`.
