FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PatientService/PatientService.csproj", "PatientService/"]
RUN dotnet restore "PatientService/PatientService.csproj"

COPY PatientService/ ./PatientService/
WORKDIR "/src/PatientService"
RUN dotnet build "PatientService.csproj" -c Release -o /app/build
RUN dotnet publish "PatientService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8082
ENV ASPNETCORE_URLS=http://+:8082
RUN adduser --disabled-password --gecos "" appuser \
    && mkdir -p /home/appuser/.azure \
    && chown -R appuser:appuser /home/appuser
USER appuser
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PatientService.dll"]
