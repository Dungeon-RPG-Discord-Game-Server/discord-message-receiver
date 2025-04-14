# 💬 DiscordMessageReceiver

DiscordMessageReceiver is the **Discord interaction layer** for a turn-based text RPG.  
This service handles Discord messages and buttons, translates them into HTTP requests, and communicates with the backend **GameStateService** through secured API calls.

Built with **.NET 8** and **Discord.NET**, this service is containerized and deployed via **Azure Container Apps**, with full observability and performance optimizations.

## ⚙️ Technologies Used

- **.NET 8** – Discord bot and HTTP client
- **Discord.NET** – Handles commands, buttons, and DM messages
- **HTTP Communication** – All commands are forwarded to GameStateService via REST
- **Azure Container Apps** – Lightweight hosting for the bot logic
- **Azure Key Vault** – Secure Admin Key storage
- **In-Memory Cache** – Temporary API Key caching
- **OpenTelemetry** – Distributed tracing across bot and game server
- **GitHub Actions** – CI/CD pipeline for Docker image and Azure push

## 🔑 Authentication Flow

- On startup, fetch **permanent Admin API Key** from Key Vault
- Request a **temporary API Key** from GameStateService
- Store issued key in **in-memory cache**
- All game-related API calls use this key
- When key expires or is invalid → re-request from server

## 📌 Features

- Supports both **prefix commands (!menu, !save)** and **slash commands (/start)**
- All gameplay interactions handled via Discord **DMs**
- Command modules organized by function (Game, Admin, General)
- Slash commands used for entry point and user onboarding
- Richly formatted **embedded responses** with icons and status

All interactions are translated to HTTP requests using `HttpClient`.


## 🔐 Security

- Uses **API Key-based auth** for lightweight, secure communication
- All keys issued via secured **Key Vault + cache** structure
- Invalid keys rejected with proper status

## 🎯 Design Principles

- Fully stateless
- Optimized for latency and cost
- Graceful error handling for all Discord interactions
- Designed for extensibility and future feature modules

## 🔥 Performance Optimizations

- API call time reduced from 200-300ms → **1~2ms** via local cache
- All commands are **non-blocking** and respond instantly
- Lightweight bot service with minimal external dependency

## 🚀 Deployment

1. Containerized with Docker
2. GitHub Actions builds and pushes to ACR
3. Deployed to Azure Container App with `minReplicas: 1` (Always On)

---

> “The Discord bot is just the messenger — all the brains live in the game server.  
But together, they make a scalable, fast, and robust RPG backend.”  
— Built for performance, clarity, and production readiness.
