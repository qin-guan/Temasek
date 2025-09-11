FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Temasek.Operatorr/Temasek.Operatorr.csproj", "Temasek.Operatorr/"]
RUN dotnet restore "Temasek.Operatorr/Temasek.Operatorr.csproj"
COPY . .
WORKDIR "/src/Temasek.Operatorr"
RUN dotnet build "./Temasek.Operatorr.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Temasek.Operatorr.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Temasek.Operatorr.dll"]
