# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NotificationService/NotificationService.csproj NotificationService/
RUN dotnet restore NotificationService/NotificationService.csproj

COPY NotificationService/ NotificationService/
RUN dotnet publish NotificationService/NotificationService.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN useradd -m appuser || adduser -D appuser
USER appuser
COPY --from=build /app/publish .
EXPOSE 8090
ENV ASPNETCORE_URLS=http://+:8090
ENTRYPOINT ["dotnet", "NotificationService.dll"]
