# 🏷️ MiniHub API - Catálogo de Itens

API REST para gerenciamento de catálogo com autenticação JWT, auditoria em MongoDB, importação de dados externos e exportação de relatórios.

## 🚀 Tecnologias Utilizadas

- **.NET 9.0** - Framework principal
- **ASP.NET Core Identity** - Autenticação e autorização
- **JWT (JSON Web Tokens)** - Tokens de acesso
- **Entity Framework Core 9.0** - ORM e migrations
- **MySQL 8.0** - Banco de dados relacional
- **MongoDB 7.0** - Banco NoSQL para auditoria
- **Docker & Docker Compose** - Containerização
- **Swagger/OpenAPI** - Documentação da API
- **Pomelo.EntityFrameworkCore.MySql** - Provider MySQL para EF Core

## 📋 Funcionalidades

### 🔐 Autenticação e Autorização
- Registro e login de usuários com JWT
- Sistema de roles (Admin, Editor, Viewer)
- Tokens com expiração de 24 horas
- Endpoints protegidos por autorização baseada em roles

### 📊 Gestão de Catálogo
- CRUD completo de Itens, Categorias e Tags
- Busca avançada com filtros combináveis
- Paginação e ordenação personalizável
- Relacionamentos muitos-para-muitos (Item-Tag)

### 🔄 Integrações
- Importação de dados de API externa (MockAPI)
- Deduplicação por ExternalId
- Processamento assíncrono de lotes

### 📈 Relatórios e Auditoria
- Exportação de dados em JSON
- Dashboard com estatísticas
- Auditoria completa em MongoDB
- Logs de todas as ações com IP e User-Agent

### 🐳 Infraestrutura
- Docker Compose para MySQL e MongoDB
- Migrations automatizadas
- Seeds para dados iniciais
- Configuração por ambiente

## 🛠️ Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [MySQL 8.0+](https://dev.mysql.com/downloads/) (opcional, pode usar Docker)
- [MongoDB 7.0+](https://www.mongodb.com/try/download/community) (opcional, pode usar Docker)

## 🚀 Como Executar

### Método 1: Com Docker (Recomendado)

```bash
# 1. Clone o repositório
git clone https://github.com/seu-usuario/minihub-api.git
cd minihub-api

# 2. Inicie os containers
docker-compose up -d
