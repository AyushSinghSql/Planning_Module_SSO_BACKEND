# Base image with ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image with SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Planning_API.csproj", "."]
RUN dotnet restore "./Planning_API.csproj"
COPY . .
RUN dotnet build "./Planning_API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish app
FROM build AS publish
RUN dotnet publish "./Planning_API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image with runtime + dependencies
FROM base AS final
WORKDIR /app

# Switch to root to install dependencies
USER root

# Install native dependencies
RUN apt-get update && apt-get install -y \
    libc6-dev \
    libgdiplus \
    libx11-dev \
    libxrender1 \
    libxtst6 \
    libxi6 \
    libz-dev \
    unzip \
 && ln -s /lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/libdl.so || true \
 && rm -rf /var/lib/apt/lists/*

# Copy app
COPY --from=publish /app/publish .

# Optional: Create a non-root user (Render runs as root by default unless overridden)
# RUN useradd -m dotnetuser
# USER dotnetuser

# Entry point
ENTRYPOINT ["dotnet", "Planning_API.dll"]
