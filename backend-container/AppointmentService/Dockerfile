FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AppointmentService/AppointmentService.csproj", "AppointmentService/"]
RUN dotnet restore "AppointmentService/AppointmentService.csproj"

COPY AppointmentService/ ./AppointmentService/
WORKDIR "/src/AppointmentService"
RUN dotnet build "AppointmentService.csproj" -c Release -o /app/build
RUN dotnet publish "AppointmentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8087
ENV ASPNETCORE_URLS=http://+:8087
RUN adduser --disabled-password --gecos "" appuser \
    && mkdir -p /home/appuser/.azure \
    && chown -R appuser:appuser /home/appuser
USER appuser
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AppointmentService.dll"]
