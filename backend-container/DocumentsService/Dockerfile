# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DocumentsService/DocumentsService.csproj DocumentsService/
RUN dotnet restore DocumentsService/DocumentsService.csproj

COPY DocumentsService/ DocumentsService/
RUN dotnet publish DocumentsService/DocumentsService.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN useradd -m appuser || adduser -D appuser
USER appuser
COPY --from=build /app/publish .
EXPOSE 8084
ENV ASPNETCORE_URLS=http://+:8084
ENTRYPOINT ["dotnet", "DocumentsService.dll"]
