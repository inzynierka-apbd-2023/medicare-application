FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MessagingService/MessagingService.csproj", "MessagingService/"]
RUN dotnet restore "MessagingService/MessagingService.csproj"

COPY MessagingService/ ./MessagingService/
WORKDIR "/src/MessagingService"
RUN dotnet build "MessagingService.csproj" -c Release -o /app/build
RUN dotnet publish "MessagingService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8086
ENV ASPNETCORE_URLS=http://+:8086
RUN adduser --disabled-password --gecos "" appuser \
    && mkdir -p /home/appuser/.azure \
    && chown -R appuser:appuser /home/appuser
USER appuser
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MessagingService.dll"]
