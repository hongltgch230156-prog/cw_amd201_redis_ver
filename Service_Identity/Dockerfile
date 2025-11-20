# Stage 1: Base image cho runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy .csproj và restore dependencies
COPY ["Service_Identity.csproj", "./"]
RUN dotnet restore "./Service_Identity.csproj"

# Copy toàn bộ source code và build
COPY . .
RUN dotnet build "./Service_Identity.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "./Service_Identity.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service_Identity.dll"]
