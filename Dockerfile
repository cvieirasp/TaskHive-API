# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia a solução e os arquivos de projeto
COPY ./TaskHive.sln ./
COPY ./src/TaskHive.API/TaskHive.API.csproj ./src/TaskHive.API/
COPY ./src/TaskHive.Application/TaskHive.Application.csproj ./src/TaskHive.Application/
COPY ./src/TaskHive.Domain/TaskHive.Domain.csproj ./src/TaskHive.Domain/
COPY ./src/TaskHive.Infra/TaskHive.Infra.csproj ./src/TaskHive.Infra/
COPY ./tests/TaskHive.UnitTests/TaskHive.UnitTests.csproj ./tests/TaskHive.UnitTests/
COPY ./tests/TaskHive.IntegrationTests/TaskHive.IntegrationTests.csproj ./tests/TaskHive.IntegrationTests/

# Restaura dependências
RUN dotnet restore

# Copia tudo
COPY . .

# Publica a aplicação
RUN dotnet publish ./src/TaskHive.API/TaskHive.API.csproj -c Release -o /app/publish

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# Configure environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl --fail http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "TaskHive.API.dll"]
