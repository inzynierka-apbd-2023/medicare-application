# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY BillingService/BillingService.csproj BillingService/
RUN dotnet restore BillingService/BillingService.csproj
COPY BillingService/ BillingService/
RUN dotnet publish BillingService/BillingService.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN useradd -m appuser || adduser -D appuser
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8085
EXPOSE 8085
USER appuser
ENTRYPOINT ["dotnet", "BillingService.dll"]
