# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only sln + project files first for better caching
COPY ./MiniLoan.sln ./
COPY ./src/MiniLoan.Api/MiniLoan.Api.csproj ./src/MiniLoan.Api/

# Restore just the API project (avoids needing test csproj in image)
RUN dotnet restore ./src/MiniLoan.Api/MiniLoan.Api.csproj


# Now copy the rest of the source
COPY . .

# Publish (Release), smaller & faster startup
RUN dotnet publish ./src/MiniLoan.Api/MiniLoan.Api.csproj \
    -c Release \
    -o /app/publish \
    -p:PublishReadyToRun=true \
    -p:UseAppHost=false

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# (Optional) Drop privileges – create a non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish ./

# Bind to 0.0.0.0:8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# (Optional) lightweight healthcheck hitting the root (or /swagger)
# HEALTHCHECK --interval=30s --timeout=3s CMD wget -qO- http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "MiniLoan.Api.dll"]
