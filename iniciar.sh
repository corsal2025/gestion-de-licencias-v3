#!/usr/bin/env bash
# Levanta el sistema de Gestión de Licencias en el puerto 5029.
# Uso: ./iniciar.sh
cd "$(dirname "$0")"

# Añadir rutas donde puede estar .NET 10 (instalado por devcontainer feature o manualmente)
export PATH="$HOME/.dotnet:/usr/local/dotnet:$PATH"

# Detener una instancia anterior si existe
pkill -f 'GestionLicencias.Web' 2>/dev/null || true
sleep 1

nohup dotnet run --project src/GestionLicencias.Web --launch-profile http \
  > /tmp/gestionlicencias.log 2>&1 &

echo "Iniciando servidor (log: /tmp/gestionlicencias.log)..."
for i in $(seq 1 60); do
  if curl -sf -o /dev/null http://localhost:5029/ 2>/dev/null; then
    echo "Sistema disponible en http://localhost:5029"
    break
  fi
  sleep 1
done

# En Codespaces: dejar el puerto accesible desde el navegador
if [ -n "$CODESPACE_NAME" ] && command -v gh >/dev/null 2>&1; then
  gh codespace ports visibility 5029:public -c "$CODESPACE_NAME" 2>/dev/null || true
  echo "URL pública: https://${CODESPACE_NAME}-5029.app.github.dev/"
fi
