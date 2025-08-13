FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PractitionerService/PractitionerService.csproj", "PractitionerService/"]
RUN dotnet restore "PractitionerService/PractitionerService.csproj"

COPY PractitionerService/ ./PractitionerService/
WORKDIR "/src/PractitionerService"
RUN dotnet build "PractitionerService.csproj" -c Release -o /app/build
RUN dotnet publish "PractitionerService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8081
RUN adduser --disabled-password --gecos "" appuser \
    && mkdir -p /home/appuser/.azure \
    && chown -R appuser:appuser /home/appuser
USER appuser
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PractitionerService.dll"]
