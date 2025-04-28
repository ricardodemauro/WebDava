# WebDAV Server Specification  
## Method: PROPPATCH

---

## 1. Purpose

The `PROPPATCH` HTTP method allows clients to modify properties of a resource on the server. This is a key feature of WebDAV, enabling metadata management for files and directories.

---

## 2. Requirements

### Functional

- Accept `PROPPATCH` requests to update or remove properties of a resource at a specified path.
- Validate the XML request body to ensure it conforms to the WebDAV schema.
- Support both setting and removing properties.
- Return appropriate status codes for each property operation.

### Non-Functional

- Ensure proper error handling for invalid XML or unsupported properties.
- Log all `PROPPATCH` operations for auditing purposes.

---

## 3. Request Specification

**Example:**
```http
PROPPATCH /webdav/folder1/file.txt HTTP/1.1
Host: example.com
Content-Type: application/xml; charset="utf-8"
Content-Length: 234

<?xml version="1.0" encoding="utf-8" ?>
<D:propertyupdate xmlns:D="DAV:">
  <D:set>
    <D:prop>
      <D:author>John Doe</D:author>
    </D:prop>
  </D:set>
  <D:remove>
    <D:prop>
      <D:deprecated/>
    </D:prop>
  </D:remove>
</D:propertyupdate>
```

- The request body contains XML specifying the properties to set or remove.
- The `Content-Type` header must be `application/xml`.

---

## 4. Response Specification

**Success Response:**
```http
HTTP/1.1 207 Multi-Status
Content-Type: application/xml; charset="utf-8"
Content-Length: 345

<?xml version="1.0" encoding="utf-8" ?>
<D:multistatus xmlns:D="DAV:">
  <D:response>
    <D:href>/webdav/folder1/file.txt</D:href>
    <D:propstat>
      <D:prop>
        <D:author/>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
    <D:propstat>
      <D:prop>
        <D:deprecated/>
      </D:prop>
      <D:status>HTTP/1.1 404 Not Found</D:status>
    </D:propstat>
  </D:response>
</D:multistatus>
```

**Headers**

| Header              | Value                               | Description |
|---------------------|-------------------------------------|-------------|
| `Content-Type`      | `application/xml; charset="utf-8"` | Specifies the response format. |

---

### Status Codes

| Status Code | Reason |
|-------------|--------|
| 207 Multi-Status | Indicates multiple property operation results. |
| 400 Bad Request | Invalid XML or unsupported property. |
| 403 Forbidden | Insufficient permissions to modify properties. |
| 404 Not Found | Resource does not exist. |
| 507 Insufficient Storage | Unable to persist the property changes. |

---

## 5. Error Responses

| Status Code | Reason |
|-------------|--------|
| 400 Bad Request | Malformed XML or invalid headers. |
| 403 Forbidden | Write access denied for the specified path. |
| 404 Not Found | Resource does not exist. |
| 507 Insufficient Storage | Storage quota exceeded or disk full. |

---

## 6. Implementation Notes

- Validate the XML body to ensure it conforms to the WebDAV schema.
- Support both `set` and `remove` operations for properties.
- Return a `207 Multi-Status` response with detailed results for each property operation.
- Log all property updates and removals, including the path and outcome.

---

## 7. Example Variants

### Setting a Property:
```http
PROPPATCH /webdav/file.txt HTTP/1.1
Host: example.com
Content-Type: application/xml; charset="utf-8"
Content-Length: 123

<?xml version="1.0" encoding="utf-8" ?>
<D:propertyupdate xmlns:D="DAV:">
  <D:set>
    <D:prop>
      <D:author>John Doe</D:author>
    </D:prop>
  </D:set>
</D:propertyupdate>
```

**Response:**
```http
HTTP/1.1 207 Multi-Status
Content-Type: application/xml; charset="utf-8"
Content-Length: 234

<?xml version="1.0" encoding="utf-8" ?>
<D:multistatus xmlns:D="DAV:">
  <D:response>
    <D:href>/webdav/file.txt</D:href>
    <D:propstat>
      <D:prop>
        <D:author/>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
</D:multistatus>
```

### Removing a Property:
```http
PROPPATCH /webdav/file.txt HTTP/1.1
Host: example.com
Content-Type: application/xml; charset="utf-8"
Content-Length: 123

<?xml version="1.0" encoding="utf-8" ?>
<D:propertyupdate xmlns:D="DAV:">
  <D:remove>
    <D:prop>
      <D:deprecated/>
    </D:prop>
  </D:remove>
</D:propertyupdate>
```

**Response:**
```http
HTTP/1.1 207 Multi-Status
Content-Type: application/xml; charset="utf-8"
Content-Length: 234

<?xml version="1.0" encoding="utf-8" ?>
<D:multistatus xmlns:D="DAV:">
  <D:response>
    <D:href>/webdav/file.txt</D:href>
    <D:propstat>
      <D:prop>
        <D:deprecated/>
      </D:prop>
      <D:status>HTTP/1.1 404 Not Found</D:status>
    </D:propstat>
  </D:response>
</D:multistatus>
```

---

## 8. References

- [RFC 4918 - HTTP Extensions for Web Distributed Authoring and Versioning (WebDAV)](https://datatracker.ietf.org/doc/html/rfc4918)

---

# Conclusion

The `PROPPATCH` method is essential for managing resource properties in a WebDAV server. Proper implementation ensures compatibility with WebDAV clients and robust handling of property updates and removals.