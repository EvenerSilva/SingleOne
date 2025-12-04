# ==========================================
# Dockerfile - SingleOne Backend API
# .NET 6.0 Runtime
# ==========================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copiar arquivos do projeto
COPY SingleOneAPI/*.csproj SingleOneAPI/
WORKDIR /app/SingleOneAPI
RUN dotnet restore

# Copiar todo o código fonte
WORKDIR /app
COPY SingleOneAPI/ SingleOneAPI/

# Build da aplicação
WORKDIR /app/SingleOneAPI
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Instalar dependências necessárias (PostgreSQL, wkhtmltopdf, etc)
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    curl \
    libgdiplus \
    libc6-dev \
    libx11-6 \
    libxrender1 \
    libfontconfig1 \
    fontconfig \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Copiar aplicação compilada
COPY --from=build /app/publish .

# Garantir permissão de execução do wkhtmltopdf
RUN chmod +x /app/Rotativa/Linux/wkhtmltopdf || true

# Criar diretórios necessários
RUN mkdir -p /app/logs && \
    mkdir -p /app/wwwroot/logos && \
    mkdir -p /app/evidencias && \
    chmod 777 /app/logs

# Variáveis de ambiente padrão (podem ser sobrescritas)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000
ENV DB_HOST=postgres
ENV DB_PORT=5432
ENV DB_NAME=singleone
ENV DB_USER=postgres
ENV DB_PASSWORD=postgres
ENV SITE_URL=http://localhost:4200

# Expor porta
EXPOSE 5000

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5000/api/health || exit 1

# Comando de inicialização
ENTRYPOINT ["dotnet", "SingleOneAPI.dll"]
