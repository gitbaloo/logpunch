# Use the official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the solution and project files
COPY *.sln ./
COPY src/Server/Server.csproj ./src/Server/
COPY src/Domain/Domain.csproj ./src/Domain/
COPY src/Persistence/Persistence.csproj ./src/Persistence/
COPY src/Service/Service.csproj ./src/Service/
COPY src/Shared/Shared.csproj ./src/Shared/
COPY src/Logpunch.Tests/Logpunch.Tests.csproj ./src/Logpunch.Tests/

# Restore dependencies
RUN dotnet restore

# Copy the entire source code
COPY . ./

# Build the project
RUN dotnet publish src/Server/Server.csproj -c Release -o out

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose the port the app runs on
EXPOSE 7206

# Set the entry point for the container
ENTRYPOINT ["dotnet", "LogpunchBackend.dll"]
