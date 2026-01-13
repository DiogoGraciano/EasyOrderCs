# EasyOrderCs

Sistema de gerenciamento de pedidos desenvolvido em ASP.NET Core, projetado para facilitar o controle de pedidos entre empresas e clientes.

## 📋 Sobre o Projeto

O **EasyOrderCs** é uma API REST desenvolvida em C# (.NET 10.0) que fornece uma solução completa para gerenciamento de pedidos. O sistema permite que empresas cadastrem produtos, clientes realizem pedidos e o sistema gerencie todo o fluxo de processamento.

### Principais Funcionalidades

- **Autenticação e Autorização**: Sistema de autenticação JWT com controle de acesso baseado em roles
- **Gerenciamento de Usuários**: Cadastro e controle de usuários do sistema
- **Gerenciamento de Empresas**: Cadastro de empresas com validação de CNPJ
- **Gerenciamento de Clientes**: Cadastro de clientes com validação de CPF e telefone
- **Gerenciamento de Produtos**: CRUD completo de produtos com controle de estoque
- **Gerenciamento de Pedidos**: Sistema completo de pedidos com múltiplos itens e controle de status
- **Upload de Arquivos**: Integração com Cloudflare R2 para armazenamento de imagens
- **Validação de Dados**: Validação robusta usando FluentValidation
- **Documentação de API**: Documentação interativa com Scalar/OpenAPI

## 🏗️ Arquitetura do Projeto

O projeto segue uma arquitetura em camadas (Layered Architecture) com separação clara de responsabilidades:

```
┌─────────────────────────────────────┐
│         Controllers Layer           │  ← Endpoints da API
├─────────────────────────────────────┤
│         Services Layer              │  ← Lógica de negócio
│      (Interfaces + Implementations) │
├─────────────────────────────────────┤
│         Data Layer                  │  ← Entity Framework Core
│    (DbContext + Migrations)         │
├─────────────────────────────────────┤
│         Models Layer                │  ← Entidades do domínio
├─────────────────────────────────────┤
│         DTOs Layer                  │  ← Data Transfer Objects
├─────────────────────────────────────┤
│         Helpers Layer               │  ← Utilitários e validadores
└─────────────────────────────────────┘
```

### Camadas da Arquitetura

#### 1. **Controllers** (`/Controllers`)
Responsáveis por receber requisições HTTP e coordenar a resposta. Cada controller representa um recurso da API:
- `AuthController`: Autenticação e autorização
- `CustomerController`: Gerenciamento de clientes
- `EnterpriseController`: Gerenciamento de empresas
- `ProductController`: Gerenciamento de produtos
- `OrderController`: Gerenciamento de pedidos
- `HealthController`: Health checks

#### 2. **Services** (`/Services`)
Contém a lógica de negócio da aplicação. Implementa o padrão de injeção de dependência através de interfaces:
- `AuthService`: Lógica de autenticação e gerenciamento de usuários
- `CustomerService`: Regras de negócio para clientes
- `EnterpriseService`: Regras de negócio para empresas
- `ProductService`: Regras de negócio para produtos
- `OrderService`: Regras de negócio para pedidos
- `FileUploadService`: Upload e gerenciamento de arquivos

#### 3. **Data** (`/Data`)
Camada de acesso a dados usando Entity Framework Core:
- `ApplicationDbContext`: Contexto do banco de dados com configurações das entidades
- Migrations: Versionamento do esquema do banco de dados

#### 4. **Models** (`/Models`)
Entidades do domínio que representam as tabelas do banco de dados:
- `User`: Usuários do sistema
- `Customer`: Clientes
- `Enterprise`: Empresas
- `Product`: Produtos
- `Order`: Pedidos
- `OrderItem`: Itens de pedidos
- `OrderStatus`: Enum de status de pedidos

#### 5. **DTOs** (`/Dtos`)
Data Transfer Objects para comunicação entre camadas e com clientes da API:
- `Auth/`: DTOs de autenticação (Login, Register, AuthResponse)
- `Customer/`: DTOs de clientes (Create, Update)
- `Enterprise/`: DTOs de empresas (Create, Update)
- `Product/`: DTOs de produtos (Create, Update)
- `Order/`: DTOs de pedidos (Create, Update, CreateOrderItem)

#### 6. **Helpers** (`/Helpers`)
Utilitários e validadores auxiliares:
- `CpfValidator`: Validação de CPF brasileiro
- `CnpjValidator`: Validação de CNPJ brasileiro
- `PhoneValidator`: Validação de telefone

## 🗄️ Modelo de Dados

O sistema utiliza as seguintes entidades principais e seus relacionamentos:

```
User (Usuários do sistema)
├── Id (Guid, PK)
├── Name, Email, Password
├── Role, IsActive
└── CreatedAt, UpdatedAt

Customer (Clientes)
├── Id (Guid, PK)
├── Name, Email, Phone, CPF
├── Address, Photo
├── Orders (1:N)
└── CreatedAt, UpdatedAt

Enterprise (Empresas)
├── Id (Guid, PK)
├── LegalName, TradeName, CNPJ
├── Address, Logo, FoundationDate
├── Orders (1:N)
├── Products (1:N)
└── CreatedAt, UpdatedAt

Product (Produtos)
├── Id (Guid, PK)
├── Name, Description, Price
├── Stock, Photo
├── EnterpriseId (FK)
├── OrderItems (1:N)
└── CreatedAt, UpdatedAt

Order (Pedidos)
├── Id (Guid, PK)
├── OrderNumber (único)
├── OrderDate, Status
├── CustomerId (FK)
├── EnterpriseId (FK)
├── TotalAmount, Notes
├── Items (1:N)
└── CreatedAt, UpdatedAt

OrderItem (Itens de Pedido)
├── Id (Guid, PK)
├── OrderId (FK)
├── ProductId (FK)
├── ProductName, Quantity
├── UnitPrice, Subtotal
└── CreatedAt, UpdatedAt
```

### Relacionamentos

- **Customer ↔ Order**: Um cliente pode ter múltiplos pedidos (1:N)
- **Enterprise ↔ Order**: Uma empresa pode ter múltiplos pedidos (1:N)
- **Enterprise ↔ Product**: Uma empresa pode ter múltiplos produtos (1:N)
- **Order ↔ OrderItem**: Um pedido pode ter múltiplos itens (1:N)
- **Product ↔ OrderItem**: Um produto pode estar em múltiplos itens de pedido (1:N)

## 🛠️ Tecnologias Utilizadas

### Framework e Linguagem
- **.NET 10.0**: Framework principal
- **C#**: Linguagem de programação
- **ASP.NET Core**: Framework web

### Banco de Dados
- **PostgreSQL 18**: Banco de dados relacional
- **Entity Framework Core 10.0.1**: ORM para acesso a dados
- **Npgsql.EntityFrameworkCore.PostgreSQL**: Provider PostgreSQL para EF Core

### Autenticação e Segurança
- **JWT (JSON Web Tokens)**: Autenticação baseada em tokens
- **BCrypt.Net-Next**: Hash de senhas
- **Microsoft.AspNetCore.Authentication.JwtBearer**: Middleware de autenticação JWT

### Validação e Documentação
- **FluentValidation.AspNetCore**: Validação de dados
- **Swashbuckle.AspNetCore**: Geração de documentação Swagger
- **Scalar.AspNetCore**: Interface alternativa para documentação da API

### Cloud e Storage
- **AWSSDK.S3**: SDK para integração com Cloudflare R2 (compatível com S3)

### Testes
- **xUnit** (implícito): Framework de testes unitários

## 📁 Estrutura de Diretórios

```
EasyOrderCs/
├── Controllers/          # Controladores da API
│   ├── AuthController.cs
│   ├── CustomerController.cs
│   ├── EnterpriseController.cs
│   ├── HealthController.cs
│   ├── OrderController.cs
│   └── ProductController.cs
├── Data/                 # Camada de acesso a dados
│   └── ApplicationDbContext.cs
├── Dtos/                 # Data Transfer Objects
│   ├── Auth/
│   ├── Customer/
│   ├── Enterprise/
│   ├── Order/
│   └── Product/
├── Helpers/              # Utilitários e validadores
│   ├── CnpjValidator.cs
│   ├── CpfValidator.cs
│   └── PhoneValidator.cs
├── Migrations/           # Migrations do Entity Framework
├── Models/               # Entidades do domínio
│   ├── Customer.cs
│   ├── Enterprise.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── OrderStatus.cs
│   ├── Product.cs
│   └── User.cs
├── Services/             # Serviços e lógica de negócio
│   ├── Interfaces/       # Interfaces dos serviços
│   ├── AuthService.cs
│   ├── CustomerService.cs
│   ├── EnterpriseService.cs
│   ├── FileUploadService.cs
│   ├── OrderService.cs
│   └── ProductService.cs
├── Tests/                # Testes unitários
│   ├── Helpers/
│   └── Services/
├── Properties/           # Configurações do projeto
├── Program.cs            # Ponto de entrada da aplicação
├── appsettings.json      # Configurações da aplicação
├── docker-compose.yml    # Configuração Docker Compose
├── Dockerfile            # Imagem Docker para produção
├── Dockerfile.dev        # Imagem Docker para desenvolvimento
├── Makefile              # Comandos auxiliares
└── EasyOrderCs.csproj   # Arquivo de projeto
```

## 🚀 Como Executar

### Pré-requisitos

- .NET 10.0 SDK
- PostgreSQL 18 (ou usar Docker)
- Docker e Docker Compose (opcional, mas recomendado)

### Execução Local

1. **Clone o repositório** (se aplicável)

2. **Configure o banco de dados**

   Edite o arquivo `appsettings.json` ou configure as variáveis de ambiente:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=easy_order;Username=postgres;Password=password"
     }
   }
   ```

3. **Aplique as migrations**

   ```bash
   dotnet ef database update
   ```

4. **Execute a aplicação**

   ```bash
   dotnet run
   ```

   A API estará disponível em `http://localhost:5000` ou `https://localhost:5001`

### Execução com Docker

O projeto inclui configuração Docker Compose para facilitar o desenvolvimento:

1. **Inicie os containers**

   ```bash
   docker-compose up -d
   ```

   Ou usando o Makefile:
   ```bash
   make up
   ```

2. **Aplique as migrations**

   ```bash
   docker-compose exec api dotnet ef database update
   ```

   Ou usando o Makefile:
   ```bash
   make migrate
   ```

3. **Acesse a API**

   - API: `http://localhost:8080`
   - Documentação: `http://localhost:8080/scalar` (em desenvolvimento)

### Comandos Úteis (Makefile)

```bash
make up              # Inicia os containers
make down            # Para os containers
make build           # Constrói as imagens
make restart         # Reinicia os containers
make logs            # Mostra logs da API
make logs-db         # Mostra logs do PostgreSQL
make shell           # Acessa o shell do container
make migrate         # Aplica migrations
make migration       # Cria nova migration (interativo)
make clean           # Remove tudo (containers, volumes, imagens)
make start           # Inicia e aplica migrations automaticamente
```

## 🔐 Autenticação

O sistema utiliza autenticação JWT. Para acessar endpoints protegidos:

1. **Registre um usuário**:
   ```
   POST /api/auth/register
   {
     "name": "Nome do Usuário",
     "email": "usuario@example.com",
     "password": "senha123"
   }
   ```

2. **Faça login**:
   ```
   POST /api/auth/login
   {
     "email": "usuario@example.com",
     "password": "senha123"
   }
   ```

3. **Use o token** nas requisições subsequentes:
   ```
   Authorization: Bearer {token}
   ```

## 📡 Endpoints da API

### Autenticação (`/api/auth`)
- `POST /register` - Registrar novo usuário
- `POST /login` - Fazer login
- `GET /profile` - Obter perfil do usuário autenticado (requer autenticação)
- `POST /logout` - Fazer logout (requer autenticação)

### Clientes (`/api/customer`)
- `GET /` - Listar clientes
- `GET /{id}` - Obter cliente por ID
- `POST /` - Criar cliente
- `PUT /{id}` - Atualizar cliente
- `DELETE /{id}` - Deletar cliente

### Empresas (`/api/enterprise`)
- `GET /` - Listar empresas
- `GET /{id}` - Obter empresa por ID
- `POST /` - Criar empresa
- `PUT /{id}` - Atualizar empresa
- `DELETE /{id}` - Deletar empresa

### Produtos (`/api/product`)
- `GET /` - Listar produtos
- `GET /{id}` - Obter produto por ID
- `POST /` - Criar produto
- `PUT /{id}` - Atualizar produto
- `DELETE /{id}` - Deletar produto

### Pedidos (`/api/order`)
- `GET /` - Listar pedidos
- `GET /{id}` - Obter pedido por ID
- `POST /` - Criar pedido
- `PUT /{id}` - Atualizar pedido
- `DELETE /{id}` - Deletar pedido

### Health Check (`/api/health`)
- `GET /` - Verificar saúde da API

## ⚙️ Configurações

### Variáveis de Ambiente

O projeto suporta configuração via variáveis de ambiente ou `appsettings.json`:

- `DB_HOST`: Host do PostgreSQL (padrão: localhost)
- `DB_PORT`: Porta do PostgreSQL (padrão: 5432)
- `DB_NAME`: Nome do banco (padrão: easy_order)
- `DB_USERNAME`: Usuário do banco (padrão: postgres)
- `DB_PASSWORD`: Senha do banco (padrão: password)
- `JWT_SECRET`: Chave secreta para JWT (obrigatório em produção)
- `JWT_EXPIRES_IN`: Tempo de expiração do token (padrão: 24h)
- `CORS_ORIGIN`: Origem permitida para CORS (padrão: http://localhost:8081)
- `R2_ENDPOINT`: Endpoint do Cloudflare R2 (opcional)
- `R2_ACCESS_KEY_ID`: Access Key ID do R2 (opcional)
- `R2_SECRET_ACCESS_KEY`: Secret Access Key do R2 (opcional)
- `R2_BUCKET_NAME`: Nome do bucket R2 (opcional)
- `R2_PUBLIC_URL`: URL pública do R2 (opcional)

## 🧪 Testes

O projeto inclui testes unitários na pasta `Tests/`. Para executar:

```bash
dotnet test
```

Os testes cobrem os principais serviços:
- `AuthServiceTests`
- `CustomerServiceTests`
- `EnterpriseServiceTests`
- `OrderServiceTests`
- `ProductServiceTests`

## 🐳 Docker

### Imagens Docker

- **Dockerfile**: Imagem otimizada para produção
- **Dockerfile.dev**: Imagem para desenvolvimento com hot reload

### Docker Compose

O `docker-compose.yml` configura:
- **PostgreSQL**: Banco de dados na porta 5432
- **API**: Aplicação ASP.NET Core na porta 8080

Para mais detalhes sobre Docker, consulte [README.DOCKER.md](./README.DOCKER.md).

## 📝 Padrões e Boas Práticas

- **Injeção de Dependência**: Todos os serviços são registrados via DI
- **Repository Pattern**: Uso de DbContext para acesso a dados
- **DTO Pattern**: Separação entre modelos de domínio e DTOs
- **Service Layer**: Lógica de negócio isolada em serviços
- **Validação**: Validação de dados com FluentValidation
- **Async/Await**: Operações assíncronas para melhor performance
- **CORS**: Configuração adequada para desenvolvimento e produção

## 🔒 Segurança

- Senhas são hasheadas com BCrypt
- Autenticação JWT com validação de token
- Validação de CPF e CNPJ brasileiros
- Proteção contra SQL Injection via Entity Framework
- CORS configurado para origens específicas

## 📚 Documentação da API

Em ambiente de desenvolvimento, a documentação interativa está disponível em:
- **Scalar**: `http://localhost:8080/scalar`
- **Swagger**: `http://localhost:8080/swagger` (se configurado)

## 🤝 Contribuindo

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT.

## 👥 Autores

Desenvolvido como parte do projeto EasyOrder.

---

Para mais informações sobre Docker e desenvolvimento, consulte [README.DOCKER.md](./README.DOCKER.md).

