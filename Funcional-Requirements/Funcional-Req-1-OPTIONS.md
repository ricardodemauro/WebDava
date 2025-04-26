# WebDAV Server Specification  
## Method: OPTIONS

---

## 1. Purpose

The `OPTIONS` HTTP method allows clients to discover server-supported methods and WebDAV capabilities.  
Correct implementation is critical for client interoperability, especially with Windows WebDAV clients, macOS Finder, and Microsoft Office products.

---

## 2. Requirements

### Functional

- Respond to `OPTIONS` requests at any valid path (e.g., `/webdav/`, `/webdav/folder/`, `/webdav/file.txt`).
- Advertise only the HTTP methods that are fully supported.
- Accurately indicate WebDAV protocol compliance level.
- Provide Microsoft-specific discovery headers when configured.

### Non-Functional

- Response must be minimal (empty body, `Content-Length: 0`).
- Response must be fast (no backend storage access is allowed during OPTIONS handling).
- Server configuration must control:
  - Whether advanced features like `LOCK/UNLOCK` are advertised.
  - Whether Microsoft-specific headers are emitted.

---

## 3. Request Specification

**Example:**
```http
OPTIONS /webdav/folder1/ HTTP/1.1
Host: example.com
User-Agent: Microsoft-WebDAV-MiniRedir/10.0.19041
```

- No request body is expected.
- No special headers are required.

---

## 4. Response Specification

**Success Response:**
```http
HTTP/1.1 200 OK
Allow: OPTIONS, GET, HEAD, PUT, DELETE, COPY, MOVE, MKCOL, PROPFIND, PROPPATCH
DAV: 1
MS-Author-Via: DAV
Content-Length: 0
```

### Headers

| Header              | Value                               | Description |
|---------------------|-------------------------------------|-------------|
| `Allow`             | List of supported methods           | Only list methods fully implemented. |
| `DAV`               | 1 or 1,2                            | `1` for core WebDAV. `2` only if locking is supported. |
| `MS-Author-Via`     | `DAV`                               | Required for Microsoft client compatibility. Configurable. |
| `Content-Length`    | `0`                                 | Required, even for an empty body. |

---

### Method List: `Allow`

| Method     | Mandatory? | Notes |
|------------|------------|-------|
| OPTIONS    | Yes         | Required for discovery |
| GET        | Yes         | File retrieval |
| HEAD       | Strongly Recommended | Lightweight file check |
| PUT        | Yes         | File upload |
| DELETE     | Yes         | File/folder deletion |
| COPY       | Yes         | Resource duplication |
| MOVE       | Yes         | Rename or move resource |
| MKCOL      | Yes         | Create new collection (directory) |
| PROPFIND   | Yes         | Metadata and directory listing |
| PROPPATCH  | Yes         | Update metadata |
| LOCK       | Optional    | Only if locking is supported |
| UNLOCK     | Optional    | Only if locking is supported |

Methods like `POST`, `TRACE`, or `CONNECT` must **not** be included unless explicitly implemented and validated.

---

### DAV Compliance Level: `DAV`

| Level  | Meaning |
|--------|---------|
| `1`    | Core WebDAV (collections, properties) |
| `1,2`  | Core WebDAV + Locking Support |

Only advertise level `2` when the server **fully** supports LOCK and UNLOCK semantics per [RFC 4918].

---

## 5. Error Responses

| Status Code | Reason |
|-------------|--------|
| 400 Bad Request | Malformed OPTIONS request |
| 500 Internal Server Error | Unexpected failure when processing capabilities |
| 501 Not Implemented | If OPTIONS is somehow disabled (should not occur in WebDAV server) |

In normal cases, **always respond `200 OK`** even if the path does not exist.

---

## 6. Implementation Notes

- OPTIONS requests **must not** trigger storage access (path existence is irrelevant for OPTIONS).
- `Allow` list should be **dynamically generated** based on enabled server features.
- `MS-Author-Via` must be configurable per deployment or environment.
- Clients often aggressively cache OPTIONS responses; ensure headers reflect the real server state.

---

## 7. Example Variants

### Minimal Core WebDAV (no locking):
```http
Allow: OPTIONS, GET, HEAD, PUT, DELETE, COPY, MOVE, MKCOL, PROPFIND, PROPPATCH
DAV: 1
MS-Author-Via: DAV
Content-Length: 0
```

### Extended WebDAV (locking supported):
```http
Allow: OPTIONS, GET, HEAD, PUT, DELETE, COPY, MOVE, MKCOL, PROPFIND, PROPPATCH, LOCK, UNLOCK
DAV: 1,2
MS-Author-Via: DAV
Content-Length: 0
```

---

## 8. References

- [RFC 7231 - Hypertext Transfer Protocol (HTTP/1.1): Semantics and Content](https://datatracker.ietf.org/doc/html/rfc7231)
- [RFC 4918 - HTTP Extensions for Web Distributed Authoring and Versioning (WebDAV)](https://datatracker.ietf.org/doc/html/rfc4918)

---

# Conclusion

This specification **must be implemented before any client testing** with Windows Explorer, macOS Finder, or Microsoft Office.  
Any deviation (wrong DAV level, missing headers) will result in silent or visible failures on the client side.
