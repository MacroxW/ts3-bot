.PHONY: all build build-bot build-web setup-opus run clean help

# Default target
all: build

# Help command to list targets
help:
	@echo "Available targets:"
	@echo "  make build       - Build both the bot (C#) and the Web Interface (TypeScript/Vue)"
	@echo "  make build-bot   - Build the C# TS3AudioBot"
	@echo "  make build-web   - Install dependencies and build the Web Interface"
	@echo "  make setup-opus  - Automatically link the system libopus to the bot directory"
	@echo "  make run         - Setup Opus, then start the bot"
	@echo "  make clean       - Clean C# build files and Web Interface dist directory"

# Build both C# bot and WebInterface
build: build-bot build-web

# Build C# TS3AudioBot
build-bot:
	dotnet build TS3AudioBot.sln

# Build Web Interface
build-web:
	cd WebInterface && npm install && npm run build

# Automatically find and link libopus.so to the local execution folder
setup-opus:
	@echo "Setting up Opus library link..."
	@mkdir -p TS3AudioBot/bin/Debug/net10.0
	@OPUS_PATH=$$(ldconfig -p | grep -oE '/.*libopus\.so[^ ]*' | head -n 1); \
	if [ -n "$$OPUS_PATH" ]; then \
		echo "Found system Opus at $$OPUS_PATH, linking..."; \
		ln -sf $$OPUS_PATH TS3AudioBot/bin/Debug/net10.0/libopus.so; \
	else \
		echo "WARNING: libopus not found in system library cache. You might need to install it first."; \
	fi

# Setup dependencies and run the bot ## alternative dotnet run --project TS3AudioBot
run: setup-opus
	@echo "Killing any existing TS3AudioBot instances..."
	@pkill -x TS3AudioBot || true
	./TS3AudioBot/bin/Debug/net10.0/TS3AudioBot

# Clean build outputs
clean:
	dotnet clean TS3AudioBot.sln
	rm -rf WebInterface/dist
	rm -rf TS3AudioBot/bin TS3AudioBot/obj
