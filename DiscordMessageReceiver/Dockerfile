FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5048

ENV ASPNETCORE_URLS=http://+:5048

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["DiscordMessageReceiver/DiscordMessageReceiver.csproj", "DiscordMessageReceiver/"]
RUN dotnet restore "DiscordMessageReceiver/DiscordMessageReceiver.csproj"
COPY . .
WORKDIR "/src/DiscordMessageReceiver"
RUN dotnet build "DiscordMessageReceiver.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "DiscordMessageReceiver.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiscordMessageReceiver.dll"]
