# Discord Message Receiver

A .NET 8 microservice that handles all Discord-related functionalities for the text-based dungeon RPG. Built with `Discord.NET`, this service listens to user commands, manages messaging workflows, and securely communicates with the Game State Service.

## Features

- ✅ Slash commands and legacy `!`-prefixed commands
- ✅ Runs as a background Discord bot using `DiscordSocketClient`
- ✅ Secure API Key-based communication with the Game State Service
- ✅ Command routing via modular CommandService and interaction handlers
- ✅ OpenTelemetry tracing support for observability
- ✅ Deployed as an Azure Container App

## Tech Stack

- .NET 8
- Discord.NET
- Azure Container Apps
- Azure Key Vault
- OpenTelemetry
- In-Memory Cache

## Commands

### Slash Commands
- `/start` – Sends a DM to begin your adventure
- `/help` – Provides command information

### Legacy `!` Commands (DM only)
- `!menu` – Starts a new game or resumes saved progress
- `!save` – Saves current game state
- `!status` – Displays current player info
- `!quit` – Ends the current game session

## API Communication

- All messages and actions are sent to the Game State Service via HTTP API
- Authenticated using short-lived API Keys (stored in memory)

## Deployment

Dockerized and deployed through GitHub Actions:

- Docker image pushed to Azure Container Registry
- `az containerapp update` used to apply new versions

---

> ✨ Designed for fast iteration, secure communication, and lightweight runtime. This service is your gateway to the dungeon.

