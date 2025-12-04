# SingleOne - Sistema de Gestão

## 📋 Descrição
Sistema completo de gestão com backend em ASP.NET Core 6.0 e frontend em Angular 10, utilizando PostgreSQL como banco de dados.

## 🏗️ Arquitetura

### Backend (ASP.NET Core 6.0)
- **Framework**: .NET 6.0
- **ORM**: Entity Framework Core
- **Banco**: PostgreSQL
- **Autenticação**: JWT Bearer
- **Documentação**: Swagger/OpenAPI

### Frontend (Angular 10)
- **Framework**: Angular 10
- **UI**: Angular Material
- **Build**: Nginx (Docker)

## 🚀 Configuração Local (100% Local)

### Pré-requisitos
- .NET 6.0 SDK
- Node.js 10+
- PostgreSQL
- PowerShell (Windows)

### 1. Configuração Rápida

```powershell
# 1. Testar configuração
.\test-setup.ps1

# 2. Configurar PostgreSQL
.\setup-postgres.ps1

# 3. Configurar ambiente local
.\setup-local.ps1

# 4. Executar backend (Terminal 1)
.\run-backend.ps1

# 5. Executar frontend (Terminal 2)
.\run-frontend.ps1
```

### 2. Instalação Manual

#### Instalar .NET 6.0 SDK
1. Baixe do site oficial: https://dotnet.microsoft.com/download
2. Execute o instalador
3. Verifique: `dotnet --version`

#### Instalar Node.js
1. Baixe do site oficial: https://nodejs.org/
2. Execute o instalador
3. Verifique: `node --version` e `npm --version`

#### Instalar PostgreSQL
1. Baixe do site oficial: https://www.postgresql.org/download/windows/
2. Execute o instalador
3. Use a senha: `password`
4. Mantenha a porta padrão: `5432`
5. Instale o pgAdmin (opcional)

### 3. Configuração do Banco de Dados

```sql
-- Conectar ao PostgreSQL
psql -h localhost -U postgres

-- Criar banco de dados
CREATE DATABASE singleone;

-- Verificar se foi criado
\l

-- Sair
\q
```

### 4. Configuração do Backend

```powershell
cd SingleOne_Backend\SingleOneAPI

# Restaurar dependências
dotnet restore

# Executar migrations (se existirem)
dotnet ef database update

# Executar aplicação
dotnet run
```

### 5. Configuração do Frontend

```powershell
cd SingleOne_Frontend

# Instalar dependências
npm install

# Executar em desenvolvimento
npm start
```

## 🔧 Variáveis de Ambiente

Copie o arquivo `env.example` para `.env` e configure:

### Backend
- `DB_HOST`: Host do PostgreSQL (padrão: localhost)
- `DB_USER`: Usuário do PostgreSQL (padrão: postgres)
- `DB_PASSWORD`: Senha do PostgreSQL (padrão: password)
- `SITE_URL`: URL do site (padrão: http://localhost:4200)
- `SMTP_HOST`: Host do servidor SMTP (padrão: localhost)
- `SMTP_PORT`: Porta do servidor SMTP (padrão: 587)
- `SMTP_LOGIN`: Login do SMTP (opcional)
- `SMTP_PASSWORD`: Senha do SMTP (opcional)
- `SMTP_FROM`: Email remetente (padrão: noreply@localhost)

### Frontend
- `API_URL`: URL da API do backend (padrão: http://localhost:5000/api/)

## 📊 Status da Configuração

### ✅ Configurado Corretamente
- [x] PostgreSQL no backend
- [x] Entity Framework Core
- [x] JWT Authentication
- [x] CORS configurado
- [x] Swagger/OpenAPI
- [x] Angular Material
- [x] Environment variables
- [x] Scripts de automação
- [x] Configuração 100% local

### ⚠️ Pontos de Atenção
- [ ] Migrations do Entity Framework (verificar se existem)
- [ ] Configuração de produção do frontend
- [ ] Testes automatizados
- [ ] CI/CD pipeline

## 🔍 Endpoints Principais

### Desenvolvimento Local
- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **PostgreSQL**: localhost:5432

## 📝 Scripts Disponíveis

- `test-setup.ps1`: Testa se tudo está configurado corretamente
- `setup-postgres.ps1`: Configura o PostgreSQL localmente
- `setup-local.ps1`: Configura o ambiente de desenvolvimento
- `run-backend.ps1`: Executa apenas o backend
- `run-frontend.ps1`: Executa apenas o frontend

## 🐛 Troubleshooting

### Problemas Comuns

1. **PostgreSQL não conecta**
   ```powershell
   # Verificar se o serviço está rodando
   Get-Service -Name "postgresql*"
   
   # Iniciar serviço
   Start-Service "postgresql-x64-13"
   ```

2. **Porta 5000 já em uso**
   ```powershell
   # Encontrar processo
   netstat -ano | findstr :5000
   
   # Matar processo
   taskkill /PID <PID> /F
   ```

3. **Node modules não encontrado**
   ```powershell
   cd SingleOne_Frontend
   Remove-Item -Recurse -Force node_modules
   Remove-Item package-lock.json
   npm install
   ```

4. **.NET não encontrado**
   ```powershell
   # Verificar instalação
   dotnet --version
   
   # Se não encontrado, reinstale o .NET 6.0 SDK
   ```

5. **Erro de migrations**
   ```powershell
   # Instalar Entity Framework Tools
   dotnet tool install --global dotnet-ef
   
   # Executar migrations
   dotnet ef database update
   ```

## 📞 Suporte

Para dúvidas ou problemas, consulte a documentação ou entre em contato com a equipe de desenvolvimento. 