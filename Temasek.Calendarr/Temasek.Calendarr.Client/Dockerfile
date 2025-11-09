FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Temasek.Calendarr/Temasek.Calendarr.Client/Temasek.Calendarr.Client.csproj", "Temasek.Calendarr/Temasek.Calendarr.Client/"]
RUN dotnet restore "Temasek.Calendarr/Temasek.Calendarr.Client/Temasek.Calendarr.Client.csproj"
COPY . .
WORKDIR "/src/Temasek.Calendarr/Temasek.Calendarr.Client"
RUN dotnet build "./Temasek.Calendarr.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Temasek.Calendarr.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Temasek.Calendarr.Client.dll"]
