FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Temasek.Calendarr/Temasek.Calendarr/Temasek.Calendarr.csproj", "Temasek.Calendarr/Temasek.Calendarr/"]
RUN dotnet restore "Temasek.Calendarr/Temasek.Calendarr/Temasek.Calendarr.csproj"
COPY . .
WORKDIR "/src/Temasek.Calendarr/Temasek.Calendarr"
RUN dotnet build "./Temasek.Calendarr.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Temasek.Calendarr.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN mkdir -p /app/publish/assets
RUN wget -O /app/publish/assets/vocab.txt "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/raw/main/vocab.txt"
RUN wget -O /app/publish/assets/model.onnx "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/onnx/model_qint8_avx512.onnx"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Temasek.Calendarr.dll"]
