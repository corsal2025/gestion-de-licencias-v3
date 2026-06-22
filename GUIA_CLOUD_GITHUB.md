# Cloud-first con GitHub (sin operación local en PC)

Esta guía deja el flujo de trabajo en la nube usando GitHub Codespaces + GitHub Actions.

## Qué queda en nube

- Código fuente, commits, ramas y PRs: GitHub
- Entorno de desarrollo, terminal, paquetes y ejecución: GitHub Codespaces
- Compilación automática por push/PR: GitHub Actions

## Límites reales (importante)

No existe operación 100% sin rastro local cuando usás una PC. Siempre puede haber metadatos mínimos del sistema o navegador.

Lo que sí se logra con esta guía: que el trabajo de desarrollo y los datos operativos del proyecto no se guarden como archivos locales del repositorio.

## 1) Habilitar trabajo solo en Codespaces

1. Subí este repo a GitHub.
2. Abrí el repo en GitHub y creá un Codespace:
   1. `Code` -> `Codespaces` -> `Create codespace on main`
3. Trabajá siempre dentro del navegador (`github.dev` / Codespaces web).
4. Evitá clonar este repo en disco local.

## 2) Variables y secretos

### Repository secrets (GitHub)

Configurá en `Settings -> Secrets and variables -> Actions`:

- `PROD_CONNECTION_STRING` (si usás BD cloud)
- Cualquier token/API requerido por despliegue

### Variables en Codespaces

Si querés forzar BD efímera dentro del contenedor:

- `ConnectionStrings__DefaultConnection=Data Source=/tmp/gestionlicencias.db`

Esto evita crear `gestionlicencias.db` en la carpeta del repo.

## 3) Flujo diario recomendado

1. Abrir Codespace en navegador.
2. Desarrollar y ejecutar desde terminal del Codespace.
3. Commit + push.
4. Validar CI en la pestaña `Actions`.
5. Cerrar Codespace al terminar.

## 4) Política de higiene para evitar rastros

- Usar navegador en modo privado o perfil de trabajo aislado.
- No descargar artefactos al equipo local.
- No copiar secretos en archivos del repo.
- No ejecutar el proyecto fuera de Codespaces.

## 5) Verificación rápida

- Hay workflow de CI en `.github/workflows/ci.yml`
- Hay configuración de devcontainer en `.devcontainer/devcontainer.json`
- `.gitignore` bloquea archivos `.db`, logs y temporales
