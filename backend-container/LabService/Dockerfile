FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LabService/LabService.csproj", "LabService/"]
RUN dotnet restore "LabService/LabService.csproj"

COPY LabService/ ./LabService/
WORKDIR "/src/LabService"
RUN dotnet build "LabService.csproj" -c Release -o /app/build
RUN dotnet publish "LabService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8089
ENV ASPNETCORE_URLS=http://+:8089
RUN adduser --disabled-password --gecos "" appuser \
    && mkdir -p /home/appuser/.azure \
    && chown -R appuser:appuser /home/appuser
USER appuser
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LabService.dll"]
