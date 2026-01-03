# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef --version 10.0.1
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy csproj and restore dependencies
COPY ["EasyOrderCs.csproj", "./"]
RUN dotnet restore "EasyOrderCs.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "EasyOrderCs.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "EasyOrderCs.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install EF Core tools for migrations
RUN dotnet tool install --global dotnet-ef --version 10.0.1
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy published app
COPY --from=publish /app/publish .

# Copy entrypoint script
COPY docker-entrypoint.sh /app/
RUN chmod +x /app/docker-entrypoint.sh

ENTRYPOINT ["/app/docker-entrypoint.sh"]

