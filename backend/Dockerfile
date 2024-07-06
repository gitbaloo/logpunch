FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /logpunch

# Copy everything
COPY . ./

# Restore as distinct layers
WORKDIR /logpunch/src/server
RUN dotnet restore

# Build and publish a release
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:latest AS runtime
WORKDIR /logpunch
COPY --from=build /logpunch/src/server/out ./
EXPOSE 7206
ENV ASPNETCORE_URLS=http://+:7206
ENTRYPOINT ["dotnet", "logpunch.dll"]
