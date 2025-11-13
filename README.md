# LintingResultsViewer

A web application for viewing and filtering linting results from Bible translation projects.

## Configuration

### Environment Variables

The following environment variables can be configured:

- **ServiceBusConnectionString**: Connection string for Azure Service Bus (optional)
- **ConnectionStrings__DefaultConnection**: Database connection string
- **DefaultDisabledLintingRules**: Comma-separated list of linting rule IDs to disable by default (optional)
  - Example: `"3,37"` to disable rules 3 and 37 by default
  - If not specified, all linting rules will be enabled by default
  - Users can still manually enable or disable any rules through the UI

### Docker Compose

To configure default disabled linting rules when using Docker Compose, set the `DefaultDisabledLintingRules` environment variable:

```bash
export DefaultDisabledLintingRules="3,37"
docker-compose up
```

Or create a `.env` file:

```
DefaultDisabledLintingRules=3,37
```