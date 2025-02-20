ARG BUILD_CONFIGURATION=Release

# Debug Stage (Used when debugging in Visual Studio)
FROM mcr.microsoft.com/dotnet/sdk:9.0-nanoserver-ltsc2022 AS debug
WORKDIR /app
COPY EGON.DiscordBot/ .
ENTRYPOINT ["dotnet", "EGON.DiscordBot.dll"]

# Build Stage (Compiles the application)
FROM mcr.microsoft.com/dotnet/sdk:9.0-nanoserver-ltsc2022 AS build
ARG BUILD_CONFIGURATION
WORKDIR /src
COPY EGON.DiscordBot/EGON.DiscordBot.csproj EGON.DiscordBot/
RUN dotnet restore "EGON.DiscordBot/EGON.DiscordBot.csproj"
COPY . .
WORKDIR "/src/EGON.DiscordBot"
RUN dotnet build "EGON.DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish Stage (Optimized Release Build)
FROM build AS publish
ARG BUILD_CONFIGURATION
RUN dotnet publish "EGON.DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final Runtime Stage (Smaller for Production)
FROM mcr.microsoft.com/dotnet/runtime:9.0-nanoserver-ltsc2022 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EGON.DiscordBot.dll"]
