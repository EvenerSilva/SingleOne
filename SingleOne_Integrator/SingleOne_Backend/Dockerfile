# ============================================
# Dockerfile - SingleOne Backend (.NET 6.0)
# ============================================

# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copiar o arquivo .csproj e restaurar dependências
COPY SingleOneAPI/SingleOneAPI.csproj SingleOneAPI/
RUN dotnet restore "SingleOneAPI/SingleOneAPI.csproj"

# Copiar todo o código fonte
COPY . .

# Build da aplicação
WORKDIR /src/SingleOneAPI
RUN dotnet build "SingleOneAPI.csproj" -c Release -o /app/build

# Estágio 2: Publish
FROM build AS publish
RUN dotnet publish "SingleOneAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

# Instalar dependências necessárias para wkhtmltopdf
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libx11-6 \
    libxext6 \
    libxrender1 \
    libfontconfig1 \
    xfonts-75dpi \
    xfonts-base \
    fontconfig \
    && rm -rf /var/lib/apt/lists/*

# Copiar os arquivos publicados
COPY --from=publish /app/publish .

# Garantir permissões de execução para o wkhtmltopdf
RUN chmod +x /app/Rotativa/Linux/wkhtmltopdf || true

# Criar diretórios necessários
RUN mkdir -p /app/wwwroot/logos /app/evidencias

# Expor a porta
EXPOSE 5000
EXPOSE 443

# Variáveis de ambiente padrão
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Executar a aplicação
ENTRYPOINT ["dotnet", "SingleOneAPI.dll"]
