# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY PdfService/PdfService.csproj PdfService/
RUN dotnet restore PdfService/PdfService.csproj
COPY PdfService/ PdfService/
RUN dotnet publish PdfService/PdfService.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
# Install fonts to ensure QuestPDF can render text
RUN apt-get update \
	&& apt-get install -y --no-install-recommends fonts-dejavu-core fontconfig \
	&& fc-cache -f \
	&& rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PdfService.dll"]
