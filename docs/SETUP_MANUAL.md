# ⚙️ Configuración manual requerida

Pasos que **no puede hacer el código** y que tenés que hacer vos antes de correr el proyecto por primera vez.

---

## 1. Crear proyecto en Railway y configurar PostgreSQL

1. Ir a [railway.app](https://railway.app) → **New Project → Deploy PostgreSQL**.
2. Una vez creado el plugin de Postgres, hacer clic en él → pestaña **Connect**.
3. Copiar el valor de **DATABASE_URL** (formato `postgresql://user:pass@host:port/dbname`).
4. **Importante:** Npgsql espera el formato con `;` en vez de URL. Railway también ofrece las variables individuales. Podés usarlas así:

   ```
   DATABASE_URL=Host=<host>;Port=<port>;Database=<dbname>;Username=<user>;Password=<pass>;SSL Mode=Require;Trust Server Certificate=true
   ```

   O podés dejar el formato `postgresql://...` y usar `NpgsqlDataSourceBuilder` con `UseNpgsqlNativeDatabase()` — pero la forma más simple es el connection string con `;`.

---

## 2. Configurar variables de entorno en Railway

1. En el proyecto Railway → seleccionar el servicio de la API → pestaña **Variables**.
2. Agregar **una por una** las siguientes variables:

   | Variable | Valor |
   |---|---|
   | `DATABASE_URL` | El connection string del paso 1 |
   | `JWT_SECRET_KEY` | String aleatorio de 64+ chars — generalo con: `openssl rand -base64 48` |
   | `JWT_ISSUER` | `etereo-api` |
   | `JWT_AUDIENCE` | `etereo-app` |
   | `ADMIN_PASSWORD` | Contraseña segura para el admin inicial (mín. 8 chars) |
   | `RESEND_API_KEY` | Ver paso 4 |
   | `RESEND_FROM_EMAIL` | `Etereo <noreply@tudominio.com>` |
   | `GOOGLE_CLIENT_ID` | Ver paso 3 |
   | `CORS_ALLOWED_ORIGINS` | URL del frontend en producción, ej: `https://etereo.com` |

3. Railway inyecta estas variables automáticamente como variables de entorno en el contenedor.

---

## 3. Crear proyecto en Google Cloud Console para OAuth

1. Ir a [console.cloud.google.com](https://console.cloud.google.com).
2. Crear un proyecto nuevo → nombre: `Etereo`.
3. En el menú lateral → **APIs & Services → OAuth consent screen**:
   - Tipo: **External**
   - Nombre de la app: `Etereo`
   - Email de soporte: tu email
   - Guardar y continuar (las demás opciones se pueden dejar vacías por ahora).
4. En **APIs & Services → Credentials → Create Credentials → OAuth client ID**:
   - Tipo de aplicación: **Web application**
   - Nombre: `Etereo Web`
   - Authorized JavaScript origins: `https://tudominio.com` y `http://localhost:5173`
   - Authorized redirect URIs: dejar vacío por ahora (Google OAuth con `idToken` no usa redirect)
5. Copiar el **Client ID** (formato `xxxxx.apps.googleusercontent.com`).
6. Pegarlo como `GOOGLE_CLIENT_ID` en Railway.

---

## 4. Crear cuenta en Resend y verificar dominio

1. Ir a [resend.com](https://resend.com) → crear cuenta gratuita.
2. Plan gratuito: 3.000 emails/mes permanente — suficiente para empezar.
3. En el dashboard → **Domains → Add Domain**:
   - Ingresar tu dominio (ej: `etereo.com`).
   - Resend te dará registros DNS para agregar en tu proveedor (Cloudflare, GoDaddy, etc.).
   - Agregar los registros y esperar verificación (puede tardar hasta 24 hs).
4. Una vez verificado el dominio → **API Keys → Create API Key**:
   - Nombre: `etereo-backend-prod`
   - Permiso: **Sending access**
5. Copiar la clave (formato `re_xxxx`) → pegarla como `RESEND_API_KEY` en Railway.
6. Configurar `RESEND_FROM_EMAIL` como `Etereo <noreply@tudominio.com>` (el dominio tiene que estar verificado).

> **Nota:** Mientras no tengas dominio verificado, podés usar `onboarding@resend.dev` para testear, pero solo manda a tu propio email registrado en Resend.

---

## 5. Conectar el repositorio GitHub a Railway y hacer el primer deploy

1. En Railway → **New Project → Deploy from GitHub repo**.
2. Seleccionar el repo `Maticasale/Etereo.Backend`.
3. Railway detecta el `Dockerfile` automáticamente en la raíz.
4. En **Settings → Deploy**:
   - Root directory: `/` (dejar vacío)
   - Dockerfile path: `Dockerfile` (detección automática)
   - Branch: `main`
5. Agregar todas las variables de entorno del paso 2.
6. Click en **Deploy** → Railway buildea la imagen y la despliega.
7. En **Settings → Networking → Generate Domain** para obtener la URL pública.
8. El primer deploy ejecuta automáticamente las migraciones y el seed.

> **Verificar deploy exitoso:** `GET https://tu-dominio.railway.app/health` debe devolver `{ "data": "ok" }`.

---

## 6. Correr el proyecto localmente por primera vez (PowerShell)

### Prerequisitos
- .NET 10 SDK instalado: `winget install Microsoft.DotNet.SDK.10`
- PostgreSQL local corriendo (o usar Railway como DB de desarrollo)
- Tener creado el archivo `.env.local` (copiar desde `.env.example` y completar)

### Cargar variables de entorno desde `.env.local`

```powershell
# En la raíz del repo
Get-Content .env.local | ForEach-Object {
    if ($_ -match '^\s*([^#][^=]+)=(.+)$') {
        [System.Environment]::SetEnvironmentVariable($matches[1].Trim(), $matches[2].Trim(), 'Process')
    }
}
```

### Generar la primera migración

```powershell
# Desde la raíz del repo
dotnet ef migrations add InitialCreate `
    --project src/Etereo.Infrastructure `
    --startup-project src/Etereo.Api `
    --output-dir Persistence/Migrations
```

### Correr el proyecto

```powershell
dotnet run --project src/Etereo.Api
```

El servidor arranca en `http://localhost:8080`.  
Las migraciones se aplican automáticamente al boot.  
El seed crea el admin y carga todos los servicios.

### Verificar que funciona

```powershell
# Health check
Invoke-RestMethod -Uri http://localhost:8080/health -Method GET
# Debe devolver: @{data=ok}

# Correr el test suite completo
.\Test-Modulo0-Setup.ps1
```

### Acceder a Swagger

Abrir en el browser: `http://localhost:8080/swagger`

---

## Notas adicionales

- **`.env.local`** está en `.gitignore` — nunca lo commitees.
- **`.env.example`** sí se commitea y documenta todas las variables necesarias.
- Las migraciones de EF Core se generan con `dotnet ef` y se commitean al repo (carpeta `Migrations/`).
- El seeder es idempotente — puede correr múltiples veces sin duplicar datos.
