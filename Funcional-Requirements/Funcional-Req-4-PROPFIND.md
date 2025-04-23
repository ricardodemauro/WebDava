Perfect—`PROPFIND` is **one of the most important WebDAV operations**, especially for clients like Windows Explorer. It's how clients **list directories** and **read metadata** (like file size, modification date, custom properties, etc.).

---

### **Basic Use of `PROPFIND`:**
- It's like an enhanced `GET` for folders (and sometimes files).
- Returns XML that describes the contents and properties of a resource.
- Supports recursive queries with the `Depth` header:
  - `Depth: 0` = only the target
  - `Depth: 1` = target + immediate children
  - `Depth: infinity` = full recursion (rarely used, often blocked)

---

### **Example: Client Requests Folder Contents**

#### **PROPFIND Request**
```http
PROPFIND /webdav/documents/ HTTP/1.1
Host: example.com
User-Agent: Microsoft-WebDAV-MiniRedir/10.0.19041
Depth: 1
Content-Type: application/xml
Content-Length: 0
```

> Note: The body can be empty to request **all default properties**, or contain XML to request specific ones.

---

### **PROPFIND Response (XML)**
```http
HTTP/1.1 207 Multi-Status
Content-Type: application/xml; charset="utf-8"

<?xml version="1.0" encoding="utf-8"?>
<D:multistatus xmlns:D="DAV:">
  <D:response>
    <D:href>/webdav/documents/</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>documents</D:displayname>
        <D:creationdate>2025-04-21T10:00:00Z</D:creationdate>
        <D:getlastmodified>Mon, 21 Apr 2025 10:34:00 GMT</D:getlastmodified>
        <D:resourcetype><D:collection/></D:resourcetype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>

  <D:response>
    <D:href>/webdav/documents/report.pdf</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>report.pdf</D:displayname>
        <D:getcontentlength>84321</D:getcontentlength>
        <D:getlastmodified>Mon, 21 Apr 2025 10:34:00 GMT</D:getlastmodified>
        <D:resourcetype/>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
</D:multistatus>
```

---

### Explanation:

| Tag                  | Meaning                             |
|----------------------|-------------------------------------|
| `<D:href>`           | Resource URL                        |
| `<D:displayname>`    | Human-readable name                 |
| `<D:resourcetype>`   | Tells whether it's a file or folder |
| `<D:getcontentlength>` | File size (only for files)        |
| `<D:getlastmodified>` | Last modified timestamp            |
| `<D:collection/>`    | Indicates it's a folder             |

---

### Server Response Code:
- Always returns `**207 Multi-Status**` for `PROPFIND`, even if some items fail.

---

Would you like a sample implementation of `PROPFIND` in C# or another language? Or should we move on to the next operation like `PROPPATCH` or `MKCOL`?