# LintingResultsViewer

A web application for viewing and managing linting results for repositories.

## API Endpoints

### Get Most Recent Linting Result

Returns the most recent linting result for a specified repository.

**Endpoint:** `GET /api/linting/{username}/{reponame}`

**Parameters:**
- `username` - The repository owner/user name
- `reponame` - The repository name

**Response:**

Success (200 OK):
```json
{
  "repoId": 1,
  "lintingResultDBModelId": 123,
  "dateInserted": "2024-11-11T20:00:00Z",
  "lintingItems": {
    "book1": {
      "chapter1": [
        {
          "verse": "1",
          "message": "Error message",
          "errorId": "E001"
        }
      ]
    }
  }
}
```

Not Found (404):
```json
{
  "message": "No linting results found for {username}/{reponame}"
}
```

**Example:**
```bash
curl http://localhost:5000/api/linting/johndoe/myrepo
```