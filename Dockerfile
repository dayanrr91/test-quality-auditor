# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["TestQualityAuditor.Web/TestQualityAuditor.Web.csproj", "TestQualityAuditor.Web/"]
COPY ["TestQualityAuditor.Core/TestQualityAuditor.Core.csproj", "TestQualityAuditor.Core/"]
COPY ["TestQualityAuditor.sln", "."]

# Restore dependencies
RUN dotnet restore "TestQualityAuditor.Web/TestQualityAuditor.Web.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/TestQualityAuditor.Web"
RUN dotnet build "TestQualityAuditor.Web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TestQualityAuditor.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "TestQualityAuditor.Web.dll"]
