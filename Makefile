.PHONY: up down build restart logs shell migrate clean

# Iniciar containers
up:
	docker-compose up -d

# Parar containers
down:
	docker-compose down

# Parar e remover volumes
down-volumes:
	docker-compose down -v

# Construir imagens
build:
	docker-compose build

# Reiniciar containers
restart:
	docker-compose restart

# Ver logs
logs:
	docker-compose logs -f api

# Ver logs do postgres
logs-db:
	docker-compose logs -f postgres

# Acessar shell do container da API
shell:
	docker-compose exec api bash

# Aplicar migrations
migrate:
	docker-compose exec api dotnet ef database update

# Criar nova migration
migration:
	@read -p "Nome da migration: " name; \
	docker-compose exec api dotnet ef migrations add $$name

# Limpar tudo (containers, volumes, imagens)
clean:
	docker-compose down -v --rmi local

# Iniciar e aplicar migrations
start: up
	@echo "Aguardando serviços iniciarem..."
	@sleep 10
	@echo "Aplicando migrations..."
	@docker-compose exec api dotnet ef database update || echo "Migrations já aplicadas ou erro ao conectar"

