#!/bin/bash

echo "🚀 Iniciando ambiente de desenvolvimento EasyOrder..."

# Verifica se o Docker está rodando
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker não está rodando. Por favor, inicie o Docker primeiro."
    exit 1
fi

# Constrói as imagens se necessário
echo "📦 Construindo imagens..."
docker-compose build

# Inicia os containers
echo "🔧 Iniciando containers..."
docker-compose up -d

# Aguarda os serviços estarem prontos
echo "⏳ Aguardando serviços iniciarem..."
sleep 10

# Aplica migrations
echo "🗄️  Aplicando migrations..."
docker-compose exec -T api dotnet ef database update || echo "⚠️  Aviso: Não foi possível aplicar migrations automaticamente. Execute manualmente com: docker-compose exec api dotnet ef database update"

echo ""
echo "✅ Ambiente iniciado com sucesso!"
echo ""
echo "📋 Serviços disponíveis:"
echo "   - API: http://localhost:8080"
echo "   - API Docs: http://localhost:8080/scalar/v1"
echo "   - PostgreSQL: localhost:5432"
echo ""
echo "📝 Comandos úteis:"
echo "   - Ver logs: docker-compose logs -f api"
echo "   - Parar: docker-compose down"
echo "   - Shell: docker-compose exec api bash"
echo ""

