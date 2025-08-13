# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY MedicalCatalogService/MedicalCatalogService.csproj MedicalCatalogService/
RUN dotnet restore "MedicalCatalogService/MedicalCatalogService.csproj"
COPY MedicalCatalogService/ ./MedicalCatalogService/
WORKDIR /src/MedicalCatalogService
RUN dotnet build "MedicalCatalogService.csproj" -c Release -o /app/build
RUN dotnet publish "MedicalCatalogService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8083
EXPOSE 8083
RUN adduser --disabled-password --gecos "" appuser \
    && mkdir -p /home/appuser/.azure \
    && chown -R appuser:appuser /home/appuser
USER appuser
VOLUME ["/home/appuser/.azure"]
ENTRYPOINT ["dotnet", "MedicalCatalogService.dll"]
