# Docker Compose - Ambiente de Desenvolvimento

Este projeto inclui uma configuração Docker Compose focada em desenvolvimento com hot reload.

## Pré-requisitos

- Docker
- Docker Compose

## Como usar

### Iniciar o ambiente

```bash
# Construir e iniciar os containers
docker-compose up -d

# Ou usar o Makefile
make up
```

### Aplicar migrations

As migrations são aplicadas automaticamente quando o container inicia. Se precisar aplicar manualmente:

```bash
# Usando docker-compose
docker-compose exec api dotnet ef database update

# Ou usando Makefile
make migrate
```

### Ver logs

```bash
# Logs da API
docker-compose logs -f api

# Logs do PostgreSQL
docker-compose logs -f postgres

# Ou usando Makefile
make logs
```

### Acessar o shell do container

```bash
docker-compose exec api bash

# Ou usando Makefile
make shell
```

### Parar o ambiente

```bash
docker-compose down

# Ou remover volumes também
docker-compose down -v

# Usando Makefile
make down
# ou
make down-volumes
```

## Serviços

### API (Porta 8080)
- URL: http://localhost:8080
- Hot reload habilitado com `dotnet watch`
- Código fonte montado como volume para edições em tempo real

### PostgreSQL (Porta 5432)
- Host: localhost
- Porta: 5432
- Database: easy_order
- Usuário: postgres
- Senha: postgres

## Comandos úteis

### Criar nova migration

```bash
docker-compose exec api dotnet ef migrations add NomeDaMigration
```

### Reverter migration

```bash
docker-compose exec api dotnet ef migrations remove
```

### Rebuild completo

```bash
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

## Variáveis de Ambiente

As variáveis podem ser configuradas no `docker-compose.yml` ou através de um arquivo `.env`:

- `DB_HOST`: Host do PostgreSQL (padrão: postgres)
- `DB_PORT`: Porta do PostgreSQL (padrão: 5432)
- `DB_NAME`: Nome do banco (padrão: easy_order)
- `DB_USERNAME`: Usuário do banco (padrão: postgres)
- `DB_PASSWORD`: Senha do banco (padrão: postgres)
- `JWT_SECRET`: Chave secreta para JWT
- `JWT_EXPIRES_IN`: Tempo de expiração do token (padrão: 24h)
- `CORS_ORIGIN`: Origem permitida para CORS (padrão: http://localhost:8081)
- `R2_*`: Configurações do Cloudflare R2 (opcional)

## Troubleshooting

### Container não inicia

```bash
# Ver logs detalhados
docker-compose logs api

# Rebuild sem cache
docker-compose build --no-cache
```

### Erro de conexão com banco

```bash
# Verificar se o PostgreSQL está rodando
docker-compose ps

# Ver logs do PostgreSQL
docker-compose logs postgres
```

### Hot reload não funciona

Certifique-se de que:
- O código está montado como volume (já configurado)
- O `dotnet watch` está rodando (verifique os logs)
- Os arquivos estão sendo salvos corretamente

## Makefile

O projeto inclui um Makefile com comandos úteis:

- `make up` - Inicia os containers
- `make down` - Para os containers
- `make build` - Constrói as imagens
- `make restart` - Reinicia os containers
- `make logs` - Mostra logs da API
- `make logs-db` - Mostra logs do PostgreSQL
- `make shell` - Acessa o shell do container
- `make migrate` - Aplica migrations
- `make migration` - Cria nova migration (interativo)
- `make clean` - Remove tudo (containers, volumes, imagens)
- `make start` - Inicia e aplica migrations automaticamente

