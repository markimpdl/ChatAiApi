# ChatAI API

A minimal ASP.NET Core 8 Web API that exposes an AI chat endpoint backed by [OpenRouter](https://openrouter.ai), supporting free models, conversation history, and a configurable system prompt.

Built as a portfolio project to demonstrate clean architecture in a real-world AI integration.

> **Frontend:** The Angular frontend for this project is available at [markimpdl/ChatAiFront](https://github.com/markimpdl/ChatAiFront).

---

## Features

- Single `POST /api/chatai` endpoint for AI chat completions
- Multi-turn conversation support via optional `history` payload
- Configurable system prompt, model, and metadata via `appsettings.json`
- Named `HttpClient` with DI — no static clients
- `IOpenRouterService` abstraction for full testability
- Swagger UI with XML docs enabled
- Unit tests with xUnit + Moq (9 tests, 0 dependencies on real HTTP)
- Docker support

---

## Project Structure

```
ChatLAiApi/
├── Controllers/
│   └── ChatAiController.cs
├── Models/
│   └── ChatModels.cs
├── Services/
│   ├── IOpenRouterService.cs
│   └── OpenRouterService.cs
├── appsettings.json
├── Program.cs
├── Dockerfile
└── ChatAIApi.csproj

ChatAIApi.Tests/
├── Controllers/
│   └── ChatAiControllerTests.cs
└── Services/
    └── OpenRouterServiceTests.cs
```

---

## Getting Started

### 1. Get a free OpenRouter API key

Sign up at [openrouter.ai](https://openrouter.ai) — no credit card required for free-tier models.

### 2. Configure the API key

**Option A — User Secrets (recommended for local dev):**
```bash
dotnet user-secrets set "OpenRouter:ApiKey" "sk-or-..."
```

**Option B — Environment variable:**
```bash
export OpenRouter__ApiKey="sk-or-..."
```

**Option C — `appsettings.Development.json`** *(never commit this)*:
```json
{
  "OpenRouter": {
    "ApiKey": "sk-or-..."
  }
}
```

### 3. Run

```bash
cd ChatLAiApi
dotnet run
```

Swagger UI will be available at `http://localhost:5096/swagger`.

---

## API

### `POST /api/chatai`

**Request body:**
```json
{
  "question": "What is the capital of France?",
  "history": [
    { "role": "user", "content": "Hello!" },
    { "role": "assistant", "content": "Hi! How can I help?" }
  ]
}
```

> `history` is optional. Omit it for single-turn questions.

**Response `200 OK`:**
```json
{
  "answer": "The capital of France is Paris."
}
```

**Error responses:**

| Status | Reason |
|--------|--------|
| `400` | Empty question |
| `502` | Model returned no content |
| `4xx/5xx` | OpenRouter API error (status forwarded) |

---

## Configuration (`appsettings.json`)

| Key | Default | Description |
|-----|---------|-------------|
| `OpenRouter:ApiKey` | *(required)* | Your OpenRouter API key |
| `OpenRouter:Model` | `nvidia/nemotron-3-super-120b-a12b:free` | Model ID from OpenRouter |
| `OpenRouter:SystemPrompt` | `You are a helpful assistant...` | System prompt prepended to every conversation |
| `OpenRouter:Referer` | `http://localhost` | Required by OpenRouter |
| `OpenRouter:AppTitle` | `ChatAIApi` | Required by OpenRouter |

---

## Running Tests

```bash
cd ChatAIApi.Tests
dotnet test
```

---

## Docker

```bash
docker build -t chatai-api .
docker run -e OpenRouter__ApiKey="sk-or-..." -p 8080:8080 chatai-api
```

---

## Tech Stack

- .NET 8 / ASP.NET Core
- xUnit + Moq
- Swashbuckle (Swagger)
- OpenRouter API (free LLM models)
