FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MedicalRecordsService/MedicalRecordsService.csproj", "MedicalRecordsService/"]
RUN dotnet restore "MedicalRecordsService/MedicalRecordsService.csproj"

COPY MedicalRecordsService/ ./MedicalRecordsService/
WORKDIR "/src/MedicalRecordsService"
RUN dotnet build "MedicalRecordsService.csproj" -c Release -o /app/build
RUN dotnet publish "MedicalRecordsService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8088
ENV ASPNETCORE_URLS=http://+:8088
RUN adduser --disabled-password --gecos "" appuser \
    && mkdir -p /home/appuser/.azure \
    && chown -R appuser:appuser /home/appuser
USER appuser
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MedicalRecordsService.dll"]
