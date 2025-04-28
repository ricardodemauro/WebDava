# WebDAV Server Specification  
## Method: PUT

---

## 1. Purpose

The `PUT` HTTP method allows clients to upload or replace a resource (file) on the server. It is a fundamental operation for WebDAV servers, enabling file creation and updates.

---

## 2. Requirements

### Functional

- Accept `PUT` requests to create or replace files at a specified path.
- Validate the request headers, including `Content-Length` and `Content-Type`.
- Support conditional requests using `If-Match` and `If-None-Match` headers.
- Ensure atomicity: the file must not be partially written.
- Handle overwriting behavior based on the `Overwrite` header.

### Non-Functional

- Stream the file content directly to storage without buffering the entire file in memory.
- Ensure proper error handling for storage-related issues (e.g., insufficient space).
- Log all `PUT` operations for auditing purposes.

---

## 3. Request Specification

**Example:**
```http
PUT /webdav/folder1/file.txt HTTP/1.1
Host: example.com
Content-Type: text/plain
Content-Length: 1234

<file content>
```

- The request body contains the file content to be uploaded.
- The `Content-Type` header specifies the MIME type of the file.
- The `Content-Length` header indicates the size of the file in bytes.

---

## 4. Response Specification

**Success Response:**
```http
HTTP/1.1 201 Created
ETag: "<etag-value>"
```

**Headers**

| Header              | Value                               | Description |
|---------------------|-------------------------------------|-------------|
| `ETag`             | Unique identifier for the resource  | Used for caching and conditional requests. |

---

### Status Codes

| Status Code | Reason |
|-------------|--------|
| 201 Created | File successfully created. |
| 204 No Content | File successfully replaced. |
| 400 Bad Request | Invalid request headers or body. |
| 403 Forbidden | Insufficient permissions to write to the path. |
| 409 Conflict | Parent directory does not exist. |
| 507 Insufficient Storage | Not enough space to complete the operation. |

---

## 5. Error Responses

| Status Code | Reason |
|-------------|--------|
| 400 Bad Request | Malformed PUT request or invalid headers. |
| 403 Forbidden | Write access denied for the specified path. |
| 409 Conflict | Parent directory does not exist. |
| 507 Insufficient Storage | Storage quota exceeded or disk full. |

---

## 6. Implementation Notes

- Validate the `Content-Length` header to prevent oversized uploads.
- Use a temporary file during the upload process to ensure atomicity.
- Generate and return a strong `ETag` for the uploaded file.
- Ensure proper cleanup of temporary files in case of errors or cancellations.
- Respect the `If-Match` and `If-None-Match` headers for conditional requests.

---

## 7. Example Variants

### Successful File Creation:
```http
PUT /webdav/newfile.txt HTTP/1.1
Host: example.com
Content-Type: text/plain
Content-Length: 11

Hello World
```

**Response:**
```http
HTTP/1.1 201 Created
ETag: "123456789abcdef"
```

### File Replacement:
```http
PUT /webdav/existingfile.txt HTTP/1.1
Host: example.com
Content-Type: text/plain
Content-Length: 5

Hello
```

**Response:**
```http
HTTP/1.1 204 No Content
ETag: "abcdef123456789"
```

---

## 8. References

- [RFC 7231 - Hypertext Transfer Protocol (HTTP/1.1): Semantics and Content](https://datatracker.ietf.org/doc/html/rfc7231)
- [RFC 4918 - HTTP Extensions for Web Distributed Authoring and Versioning (WebDAV)](https://datatracker.ietf.org/doc/html/rfc4918)

---

# Conclusion

The `PUT` method is essential for enabling file uploads and updates in a WebDAV server. Proper implementation ensures compatibility with a wide range of clients and robust handling of file operations.