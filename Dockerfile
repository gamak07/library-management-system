# Use the official .NET SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Use the smaller runtime image to run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy built files from build stage
COPY --from=build /app/out ./

# Expose port
EXPOSE 8080

# Set environment to production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Run the app
ENTRYPOINT ["dotnet", "LibraryApp.dll"]