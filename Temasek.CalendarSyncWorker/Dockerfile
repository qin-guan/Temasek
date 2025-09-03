# Use the official .NET runtime as base image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["Temasek.CalendarSyncWorker.csproj", "Temasek.CalendarSyncWorker/"]
RUN dotnet restore "Temasek.CalendarSyncWorker/Temasek.CalendarSyncWorker.csproj"

# Copy source code and build
COPY . Temasek.CalendarSyncWorker/
WORKDIR "/src/Temasek.CalendarSyncWorker"
RUN dotnet build "Temasek.CalendarSyncWorker.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Temasek.CalendarSyncWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Temasek.CalendarSyncWorker.dll"]