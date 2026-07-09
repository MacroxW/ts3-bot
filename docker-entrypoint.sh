#!/bin/bash
set -e

DATA_DIR="/data"
APP_DIR="/app"

echo "=== Dixibot Container Initialization ==="

# Ensure data directory exists
mkdir -p "$DATA_DIR"

# Copy default config if it doesn't exist in /data
if [ ! -f "$DATA_DIR/ts3audiobot.toml" ]; then
    echo "Initializing default ts3audiobot.toml..."
    cp "$APP_DIR/ts3audiobot.toml" "$DATA_DIR/ts3audiobot.toml"
fi

# Copy default rights if it doesn't exist in /data
if [ ! -f "$DATA_DIR/rights.toml" ]; then
    if [ -f "$APP_DIR/rights.toml" ]; then
        echo "Initializing default rights.toml..."
        cp "$APP_DIR/rights.toml" "$DATA_DIR/rights.toml"
    fi
fi

# Ensure Web Interface path in the config points to the container's static build
# We replace relative path with the absolute path /app/WebInterface/dist
if [ -f "$DATA_DIR/ts3audiobot.toml" ]; then
    sed -i 's|path = "WebInterface/dist"|path = "/app/WebInterface/dist"|g' "$DATA_DIR/ts3audiobot.toml"
fi

# Link system libopus so TS3AudioBot can resolve it
OPUS_PATH=$(ldconfig -p | grep -oE '/.*libopus\.so[^ ]*' | head -n 1)
if [ -n "$OPUS_PATH" ]; then
    echo "Linking system Opus ($OPUS_PATH) to $APP_DIR/libopus.so"
    ln -sf "$OPUS_PATH" "$APP_DIR/libopus.so"
else
    echo "WARNING: System libopus not found via ldconfig!"
fi

# Change working directory to /data so all relative paths (db, bots, logs, plugins)
# are resolved and saved natively inside the persistent volume.
cd "$DATA_DIR"

echo "Starting TS3AudioBot with config from $DATA_DIR/ts3audiobot.toml..."
exec "$APP_DIR/TS3AudioBot" --config "$DATA_DIR/ts3audiobot.toml" --non-interactive "$@"
