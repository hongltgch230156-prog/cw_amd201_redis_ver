FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# COPY .csproj từ thư mục con nếu cần
COPY ["Service_URL_Shorten.csproj", "."]
RUN dotnet restore "./Service_URL_Shorten.csproj"

# COPY toàn bộ project
COPY . .

RUN dotnet build "./Service_URL_Shorten.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Service_URL_Shorten.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service_URL_Shorten.dll"]
