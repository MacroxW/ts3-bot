# =========================================================
# Stage 1: Build the Web Interface (Vue/TypeScript)
# =========================================================
FROM node:20-bookworm-slim AS web-builder
WORKDIR /src/WebInterface

# Copy package files and install dependencies
COPY WebInterface/package*.json ./
RUN npm ci

# Copy web interface source and build
COPY WebInterface/ ./
RUN npm run build

# =========================================================
# Stage 2: Build the C# TS3AudioBot Application
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS bot-builder
WORKDIR /src

# Copy solution and project files first for NuGet restore caching
COPY TS3AudioBot.sln ./
COPY TS3AudioBot/TS3AudioBot.csproj TS3AudioBot/
COPY TSLib/TSLib.csproj TSLib/

# Restore dependencies
RUN dotnet restore TS3AudioBot/TS3AudioBot.csproj

# Copy the rest of the source code
COPY TS3AudioBot/ TS3AudioBot/
COPY TSLib/ TSLib/

# Copy the version metadata files (needed by the msbuild version generator target)
COPY version.txt ./
COPY build_number.txt ./

# Ensure the generate_version script has execution permissions
RUN chmod +x TS3AudioBot/generate_version.sh

# Publish C# application
RUN dotnet publish TS3AudioBot/TS3AudioBot.csproj -c Release -o /app/publish

# =========================================================
# Stage 3: Run the TS3AudioBot Application
# =========================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runner

# Install system dependencies (ffmpeg, libopus, python3/curl for yt-dlp, and node for youtube JS challenge)
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    ffmpeg \
    libopus-dev \
    python3 \
    python3-minimal \
    ca-certificates \
    && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Install yt-dlp
RUN curl -L https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp -o /usr/local/bin/yt-dlp \
    && chmod a+rx /usr/local/bin/yt-dlp

# Create the symlink for the bot's default configuration path to yt-dlp
# (This matches the default /home/macrox/.local/bin/yt-dlp configured in ts3audiobot.toml)
RUN mkdir -p /home/macrox/.local/bin \
    && ln -s /usr/local/bin/yt-dlp /home/macrox/.local/bin/yt-dlp

# Set up application directories
WORKDIR /app
COPY --from=bot-builder /app/publish/ .
COPY --from=web-builder /src/WebInterface/dist/ ./WebInterface/dist/

# Copy base config and rights files to have defaults if not mounted
COPY ts3audiobot.toml .
COPY rights.toml .

# Expose Web Panel / API port
EXPOSE 58913

# Create persistent data volume mountpoint
VOLUME /data
ENV DATA_PATH=/data

# Copy entrypoint script
COPY docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

ENTRYPOINT ["docker-entrypoint.sh"]
