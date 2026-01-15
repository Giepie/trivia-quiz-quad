# Trivia Quiz Application

A web application for answering trivia questions from the Open Trivia Database. Built with C# ASP.NET Core backend and Tailwind CSS frontend.

**🌐 Live Demo:** [https://zealous-bay-05d98bc03.2.azurestaticapps.net/](https://zealous-bay-05d98bc03.2.azurestaticapps.net/)

## Overview

This application provides two endpoints:
- **GET /trivia/questions** - Fetches trivia questions
- **POST /trivia/checkanswers** - Validates submitted answers and returns score

The backend acts as an intermediary between the frontend and Open Trivia Database API, preventing users from seeing correct answers in the network requests.

## Tech Stack

**Backend:**
- ASP.NET Core 8.0 (C#)
- In-memory caching for session management
- xUnit for testing

**Frontend:**
- HTML5 + JavaScript
- Tailwind CSS 3.4
- Font Awesome icons

## How It Works

1. User clicks "Start Quiz"
2. Frontend requests questions from backend API
3. Backend fetches from Open Trivia Database
4. Backend generates unique session ID and stores correct answers in memory
5. Backend returns questions with **shuffled answers**.
6. User selects answers in frontend
7. Frontend submits answers with session ID
8. Backend validates against stored answers and returns detailed results

## Building and Running

### 1. Setup Frontend

```bash
cd frontend
npm install
npm run build:css
```

### 2. Run Backend

```bash
cd backend/TriviaApi
dotnet run
```

Backend runs at: `http://localhost:5000`

### 3. Run Frontend

Open `frontend/index.html` with **VS Code Live Server** (Right-click file → "Open with Live Server")

**Alternative:** `npx http-server -p 5500` in frontend folder

Open browser at: `http://localhost:5500`

### 4. Run Tests

```bash
cd backend/TriviaApi.Tests
dotnet test
```

Expected: 5 tests pass

## API Documentation

### GET /trivia/questions

**Query Parameters:**
- `amount` (optional, default: 10) - Number of questions (1-50)
- `difficulty` (optional) - "easy", "medium", or "hard"
- `type` (optional) - "multiple" or "boolean"

**Response:**
```json
{
  "sessionId": "uuid",
  "questions": [
    {
      "id": 0,
      "question": "What is the capital of France?",
      "answers": ["Paris", "London", "Berlin", "Madrid"],
      "category": "Geography",
      "difficulty": "easy"
    }
  ]
}
```

### POST /trivia/checkanswers

**Request:**
```json
{
  "sessionId": "uuid",
  "answers": {
    "0": "Paris",
    "1": "Blue"
  }
}
```

**Response:**
```json
{
  "score": 8,
  "totalQuestions": 10,
  "results": [
    {
      "questionId": 0,
      "isCorrect": true,
      "correctAnswer": "Paris",
      "userAnswer": "Paris"
    }
  ]
}
```