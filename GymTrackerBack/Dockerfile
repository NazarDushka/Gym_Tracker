# syntax=docker/dockerfile:1

# --- Build & publish stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем весь контекст (GymTrackerBack/*) и публикуем конкретный проект
COPY . .
RUN dotnet restore "GymTracker/GymTracker.csproj"
RUN dotnet publish "GymTracker/GymTracker.csproj" -c Release -o /app/publish --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "GymTracker.dll"]
