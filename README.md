# LintingResultsViewer

A web application for viewing and managing linting results for repositories. The application provides a user interface to browse linting results and receives new linting data via Azure Service Bus messaging.

## Overview

LintingResultsViewer is an ASP.NET Core 8.0 Blazor Server application that:
- Receives linting results from Azure Service Bus
- Stores results in a SQLite database
- Provides a web UI for viewing and browsing linting results by repository
- Offers an HTTP API for programmatic access to linting data

## Building

### Prerequisites
- .NET 8.0 SDK or later

### Build Commands

Build the project:
```bash
dotnet build
```

Publish for production:
```bash
dotnet publish -c Release
```

Build Docker image:
```bash
docker build -t linting-results -f LintingResults/Dockerfile .
```

## Running

### Running Locally

Run the application in development mode:
```bash
cd LintingResults
dotnet run
```

The application will be available at `http://localhost:5000` (or the URL shown in the console output).

### Running with Docker

Using Docker:
```bash
docker run -p 8080:8080 \
  -e ServiceBusConnectionString="<your-connection-string>" \
  -e ConnectionStrings__DefaultConnection="DataSource=/data/app.db;Cache=Shared" \
  -e DefaultDisabledLintingRules="3,37" \
  -v linting-data:/data \
  linting-results
```

Using Docker Compose:
```bash
export ServiceBusConnectionString="<your-connection-string>"
export IMAGE_TAG="latest"
export DefaultDisabledLintingRules="3,37"
docker-compose up
```

The application will be available at `http://localhost:8092`.

## Configuration

The application is configured via `appsettings.json` or environment variables. Below are the available configuration values:

| Configuration Key | Description | Example Value | Required |
|------------------|-------------|---------------|----------|
| `ServiceBusConnectionString` | Azure Service Bus connection string for receiving linting results | `Endpoint=sb://...` | Yes |
| `ConnectionStrings__DefaultConnection` | SQLite database connection string | `DataSource=Data/app.db;Cache=Shared` | Yes |
| `WACSUrl` | Base URL for WACS (Wycliffe Associates Content Server) links | `https://content.bibletranslationtools.org` | No |
| `DefaultDisabledLintingRules` | Comma-separated list of linting rule IDs to disable by default | `3,37` | No |
| `Logging__LogLevel__Default` | Default logging level | `Information` | No |
| `Logging__LogLevel__Microsoft.AspNetCore` | ASP.NET Core logging level | `Warning` | No |

### Setting Configuration

**Via appsettings.json:**
```json
{
  "ServiceBusConnectionString": "Endpoint=sb://...",
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=Data/app.db;Cache=Shared"
  },
  "WACSUrl": "https://content.bibletranslationtools.org",
  "DefaultDisabledLintingRules": "3,37"
}
```

**Via Environment Variables:**
```bash
export ServiceBusConnectionString="Endpoint=sb://..."
export ConnectionStrings__DefaultConnection="DataSource=Data/app.db;Cache=Shared"
export WACSUrl="https://content.bibletranslationtools.org"
export DefaultDisabledLintingRules="3,37"
```

### DefaultDisabledLintingRules

This optional configuration controls which linting rules are disabled by default when viewing linting results. 

- **Format:** Comma-separated list of rule IDs (e.g., `"3,37"`)
- **Default Behavior:** If not specified, all linting rules are enabled by default
- **User Control:** Users can still manually enable or disable any rules through the UI regardless of this setting

**Examples:**
- Disable rules 3 and 37: `"DefaultDisabledLintingRules": "3,37"`
- Disable rules 3, 37, and 5: `"DefaultDisabledLintingRules": "3,37,5"`
- Enable all rules by default: Don't set this configuration or set it to an empty string `""`

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
