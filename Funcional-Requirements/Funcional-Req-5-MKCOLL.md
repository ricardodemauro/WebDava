Alright, let’s stay sharp and professional like we agreed.

Here's the **technical documentation** for the **MKCOL** operation, following the style you used for previous methods:

---

# MKCOL — Method Documentation

## Purpose
`MKCOL` (Make Collection) is used to create a **new collection** (i.e., a folder) at a specified URI on the server.

It is essential for allowing clients to organize resources into directories.

---

## MKCOL Request Example
```http
MKCOL /webdav/newfolder/ HTTP/1.1
Host: example.com
User-Agent: Microsoft-WebDAV-MiniRedir/10.0.19041
Content-Length: 0
```

- `Content-Length: 0` — In most cases, the request body is empty.
- The target URI must **not already exist** — otherwise, the server must respond with a `405 Method Not Allowed` or `409 Conflict`.

---

## MKCOL Response Example (Success)
```http
HTTP/1.1 201 Created
```

**Explanation:**
- `201 Created` — Indicates that the new collection (folder) was successfully created at the requested URI.

---

## MKCOL Response Example (Failure — Folder Already Exists)
```http
HTTP/1.1 405 Method Not Allowed
Allow: OPTIONS, GET, HEAD, POST, PUT, DELETE, TRACE, COPY, MOVE, MKCOL, PROPFIND, PROPPATCH
```

**Explanation:**
- `405 Method Not Allowed` — The target resource already exists and thus a new collection cannot be created at that URI.

---

## Server Behavior Rules for MKCOL

| Rule | Description |
|:-----|:------------|
| Path must not exist | If the target path already exists, reject the request. |
| Parent must exist | If the **parent folder** does not exist, respond with `409 Conflict`. |
| No request body expected | An empty body is assumed. If a body exists, you **may** reject it with `415 Unsupported Media Type` depending on policy. (RFC4918 allows but does not require body handling.) |
| Proper status codes | Respond accurately with `201 Created`, `405 Method Not Allowed`, or `409 Conflict`. |

---

## Server Response Codes

| Status Code | Meaning |
|:------------|:--------|
| `201 Created` | Folder successfully created. |
| `405 Method Not Allowed` | Target already exists and is not a collection. |
| `409 Conflict` | One or more intermediate collections do not exist. |
| `415 Unsupported Media Type` | (Optional) If you reject non-empty request bodies. |

---

## Important Notes:
- If you receive `MKCOL /webdav/newfolder/subfolder/`, but `newfolder/` does not exist yet, you must **reject** it with `409 Conflict`.
- Some clients (like older Windows versions) may send unnecessary headers — your server must be tolerant to unknown headers.

---

## Quick XML Note:
- **MKCOL does not require XML parsing** if the body is empty (which is 99% of cases).
- Only WebDAV extensions (like `MKCOL` with body) introduce XML payloads, but you can ignore this complexity for now.
