#!/bin/bash
set -e

echo "Aguardando PostgreSQL estar pronto..."
# Aguarda o PostgreSQL estar disponível (o docker-compose já faz isso com depends_on)
sleep 3

echo "Aplicando migrations..."
export PATH="$PATH:/root/.dotnet/tools"
cd /app
dotnet ef database update || echo "Aviso: Falha ao aplicar migrations. Você pode aplicar manualmente com: docker-compose exec api dotnet ef database update"

echo "Iniciando aplicação em modo desenvolvimento..."
# Se o comando foi passado como argumento, usa ele (para watch)
if [ $# -gt 0 ]; then
  exec "$@"
else
  exec dotnet watch run --urls "http://0.0.0.0:8080"
fi

