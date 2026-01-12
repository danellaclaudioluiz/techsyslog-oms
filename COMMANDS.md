# Guia de Execução - TechsysLog

## Execução Local

### 1. Banco de Dados (MongoDB)
```bash
docker run -d --name mongodb -p 27017:27017 -v mongodb_data:/data/db mongo
```

### 2. Backend API (.NET)
```bash
# Navegue até a pasta da API
cd src/TechsysLog.API

# Restaure as dependências
dotnet restore

# Execute a aplicação
dotnet run

# Ou para build
dotnet build

# Para executar os testes
dotnet test
```

**A API estará disponível em:** `http://localhost:5150`

### 3. Frontend (Angular)
```bash
# Navegue até a pasta do frontend
cd src/TechsysLog.Web

# Instale as dependências (primeira vez)
npm install

# Inicie o servidor de desenvolvimento
npm start
```

**O frontend estará disponível em:** `http://localhost:4200`

---

## Execução com Docker Compose

### Subir toda a stack (DB + API + Frontend + Seq)
```bash
# No diretório raiz do projeto
docker compose up --build
```

### Serviços disponíveis:

| Serviço | URL | Descrição |
|---------|-----|-----------|
| Frontend | http://localhost:4200 | Interface Web |
| API | http://localhost:5000 | Backend REST API |
| MongoDB | localhost:27017 | Banco de dados |
| Seq | http://localhost:8081 | Logs e observabilidade |

### Comandos úteis:
```bash
# Parar todos os serviços
docker compose down

# Ver logs em tempo real
docker compose logs -f

# Reconstruir apenas um serviço
docker compose up --build api

# Limpar volumes (remove dados do banco)
docker compose down -v
```

---

## Credenciais Padrão (Docker)

**MongoDB:**
- Usuário: `admin`
- Senha: `admin123`
- Database: `techsyslog`

**Seq:**
- Sem autenticação (desenvolvimento)
