# ------------------------------------------
# 1) Base Build Stage (Restores Dependencies First)
# ------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app

# Copy only the project files to leverage caching
COPY BeCRM.sln .
COPY Data/Data.csproj Data/
COPY Server.Tests/Server.Tests.csproj Server.Tests/
COPY Server/Server.csproj Server/
COPY ServerLibrary/ServerLibrary.csproj ServerLibrary/

# Restore dependencies separately
RUN dotnet restore BeCRM.sln

# ------------------------------------------
# 2) Full Build Stage
# ------------------------------------------
FROM base AS build

# Copy the rest of the source code
COPY . .

# Publish the Server project
RUN dotnet publish Server/Server.csproj -c Release -o /publish 


# Ensure Templates folder is copied correctly
RUN mkdir -p /app/ServerLibrary/Templates
COPY ServerLibrary/Templates /app/ServerLibrary/Templates

# Debugging: Check if Templates folder exists
RUN ls -la /app/ServerLibrary/Templates
# ------------------------------------------
# 3) Runtime Stage
# ------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /publish .
# Copy the Templates folder again to make sure it's available at runtime
COPY --from=build /app/ServerLibrary/Templates /app/ServerLibrary/Templates
# Expose port 5000 for HTTP
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
# Run the main Server project
CMD ["dotnet", "Server.dll"]
