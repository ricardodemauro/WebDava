# WebDAV Server Specification  
## Method: LOCK

---

## 1. Purpose

The `LOCK` HTTP method allows clients to lock a resource on the server to prevent conflicting modifications. This is particularly useful in collaborative environments where multiple users may attempt to edit the same resource.

---

## 2. Requirements

### Functional

- Accept `LOCK` requests to lock a resource at a specified path.
- Support both exclusive and shared locks as per [RFC 4918](https://datatracker.ietf.org/doc/html/rfc4918).
- Generate a unique lock token for each lock.
- Respect the `Timeout` header to set the lock duration.
- Return the lock token and lock information in the response.

### Non-Functional

- Ensure that locks are stored persistently and survive server restarts.
- Handle concurrent lock requests gracefully.
- Log all `LOCK` operations for auditing purposes.

---

## 3. Request Specification

**Example:**
```http
LOCK /webdav/folder1/file.txt HTTP/1.1
Host: example.com
Timeout: Second-3600
Content-Type: application/xml; charset="utf-8"
Content-Length: 123

<?xml version="1.0" encoding="utf-8" ?>
<D:lockinfo xmlns:D="DAV:">
  <D:lockscope><D:exclusive/></D:lockscope>
  <D:locktype><D:write/></D:locktype>
  <D:owner>
    <D:href>http://example.com/users/johndoe</D:href>
  </D:owner>
</D:lockinfo>
```

- The request body contains XML specifying the lock scope, type, and owner.
- The `Timeout` header specifies the lock duration (e.g., `Second-3600` for 1 hour).
- The `Content-Type` header must be `application/xml`.

---

## 4. Response Specification

**Success Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/xml; charset="utf-8"
Content-Length: 234

<?xml version="1.0" encoding="utf-8" ?>
<D:prop xmlns:D="DAV:">
  <D:lockdiscovery>
    <D:activelock>
      <D:locktype><D:write/></D:locktype>
      <D:lockscope><D:exclusive/></D:lockscope>
      <D:depth>infinity</D:depth>
      <D:owner>
        <D:href>http://example.com/users/johndoe</D:href>
      </D:owner>
      <D:timeout>Second-3600</D:timeout>
      <D:locktoken>
        <D:href>opaquelocktoken:1234abcd-5678-efgh-ijkl-9876mnopqrst</D:href>
      </D:locktoken>
    </D:activelock>
  </D:lockdiscovery>
</D:prop>
```

**Headers**

| Header              | Value                               | Description |
|---------------------|-------------------------------------|-------------|
| `Content-Type`      | `application/xml; charset="utf-8"` | Specifies the response format. |
| `Lock-Token`        | `opaquelocktoken:<unique-id>`       | Unique identifier for the lock. |

---

### Status Codes

| Status Code | Reason |
|-------------|--------|
| 200 OK | Lock successfully created. |
| 400 Bad Request | Invalid request body or headers. |
| 403 Forbidden | Insufficient permissions to lock the resource. |
| 423 Locked | Resource is already locked. |
| 507 Insufficient Storage | Unable to persist the lock. |

---

## 5. Error Responses

| Status Code | Reason |
|-------------|--------|
| 400 Bad Request | Malformed XML or invalid headers. |
| 403 Forbidden | Write access denied for the specified path. |
| 423 Locked | Resource is already locked by another client. |
| 507 Insufficient Storage | Unable to persist the lock due to storage issues. |

---

## 6. Implementation Notes

- Validate the XML body to ensure it conforms to the WebDAV schema.
- Generate a unique lock token for each lock and include it in the response.
- Respect the `Timeout` header and set a default timeout if it is not provided.
- Store lock information persistently to survive server restarts.
- Ensure that locks are released when the timeout expires or when explicitly unlocked.

---

## 7. Example Variants

### Exclusive Lock:
```http
LOCK /webdav/file.txt HTTP/1.1
Host: example.com
Timeout: Second-3600
Content-Type: application/xml; charset="utf-8"
Content-Length: 123

<?xml version="1.0" encoding="utf-8" ?>
<D:lockinfo xmlns:D="DAV:">
  <D:lockscope><D:exclusive/></D:lockscope>
  <D:locktype><D:write/></D:locktype>
  <D:owner>
    <D:href>http://example.com/users/johndoe</D:href>
  </D:owner>
</D:lockinfo>
```

**Response:**
```http
HTTP/1.1 200 OK
Lock-Token: opaquelocktoken:1234abcd-5678-efgh-ijkl-9876mnopqrst
Content-Type: application/xml; charset="utf-8"
Content-Length: 234

<?xml version="1.0" encoding="utf-8" ?>
<D:prop xmlns:D="DAV:">
  <D:lockdiscovery>
    <D:activelock>
      <D:locktype><D:write/></D:locktype>
      <D:lockscope><D:exclusive/></D:lockscope>
      <D:depth>infinity</D:depth>
      <D:owner>
        <D:href>http://example.com/users/johndoe</D:href>
      </D:owner>
      <D:timeout>Second-3600</D:timeout>
      <D:locktoken>
        <D:href>opaquelocktoken:1234abcd-5678-efgh-ijkl-9876mnopqrst</D:href>
      </D:locktoken>
    </D:activelock>
  </D:lockdiscovery>
</D:prop>
```

---

## 8. References

- [RFC 4918 - HTTP Extensions for Web Distributed Authoring and Versioning (WebDAV)](https://datatracker.ietf.org/doc/html/rfc4918)

---

# Conclusion

The `LOCK` method is essential for enabling collaborative editing and preventing conflicting modifications. Proper implementation ensures compatibility with WebDAV clients and robust handling of resource locks.