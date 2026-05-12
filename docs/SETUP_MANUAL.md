# ⚙️ Configuración manual requerida

Este manual está dividido en dos partes independientes:

- **Parte 1 — Desarrollo local:** lo que hacés para correr el proyecto en tu PC.
- **Parte 2 — Producción en Railway:** lo que hacés cuando querés deployar a internet.

Podés hacer la Parte 1 sin tener nada en Railway todavía.

---

# PARTE 1 — Desarrollo local

---

## Paso 1 — Instalar el .NET 10 SDK

Si no lo tenés instalado, abrí PowerShell y ejecutá:

```powershell
winget install Microsoft.DotNet.SDK.10
```

Cerrá y volvé a abrir PowerShell cuando termine. Verificá con:

```powershell
dotnet --version
```

Debe mostrar algo que empiece con `10.`.

---

## Paso 2 — Crear la base de datos local en pgAdmin 4

La API necesita una base de datos PostgreSQL vacía para arrancar. EF Core se encarga de crear todas las tablas automáticamente cuando corrás el proyecto por primera vez, pero la base de datos en sí la tenés que crear vos.

1. Abrí **pgAdmin 4**.
2. En el panel izquierdo → expandí **Servers → PostgreSQL** (la versión que tengas instalada).
3. Click derecho en **Databases → Create → Database...**
4. En el campo **Database** escribí: `etereo`
5. En **Owner** dejá: `postgres`
6. Click en **Save**.

La base de datos `etereo` queda creada vacía — listo, no hay nada más que hacer acá.

> **¿Olvidaste la contraseña de PostgreSQL?** Es la que pusiste al instalar PostgreSQL en tu PC. Si no la recordás, buscá "resetear contraseña postgres windows" — el proceso varía según la versión.

---

## Paso 3 — Configurar User Secrets

### ¿Qué son User Secrets?

Es el mecanismo de .NET para guardar contraseñas y claves **fuera del repositorio**. Los guarda en una carpeta de tu usuario de Windows (`AppData\Roaming\Microsoft\UserSecrets\`) que git nunca ve. La ventaja es que `dotnet run` los levanta solo — no necesitás ningún script previo cada vez que abrís una terminal.

En producción (Railway) los mismos valores vienen de las variables de entorno que configurás en Railway. El código los lee de la misma forma en ambos casos.

### Paso 3.1 — Inicializar User Secrets

Abrí PowerShell en la raíz del repo (`C:\Dev\Etereo\Etereo.Backend`):

```powershell
dotnet user-secrets init --project src/Etereo.Api
```

Va a decirte algo como `UserSecretsId already set` — no pasa nada, ya estaba configurado. Seguí al paso siguiente.

### Paso 3.2 — Generar el JWT Secret Key

El JWT Secret Key es una clave aleatoria que usa la API para firmar los tokens de login. Tiene que ser larga e impredecible. Generala con este comando de PowerShell:

```powershell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
```

Vas a ver un output similar a este (el tuyo va a ser diferente):

```
xK9mP2nQ7rT4vW1yA8bC5dF0gH3jL6oR+sU/eIiJkMnOpQrStUvWxYz==
```

**Copiá ese valor completo** — lo vas a usar en el comando del siguiente paso.

### Paso 3.3 — Cargar todos los secretos

Ejecutá cada comando por separado en PowerShell. Te explico de dónde sale cada valor:

**DATABASE_URL** — La dirección de tu base de datos local. Reemplazá `<TU_PASSWORD>` por la contraseña que pusiste al instalar PostgreSQL (si usás la instalación por defecto, el usuario es `postgres`):

```powershell
dotnet user-secrets set "DATABASE_URL" "Host=localhost;Port=5432;Database=etereo;Username=postgres;Password=<TU_PASSWORD>" --project src/Etereo.Api
```

**JWT_SECRET_KEY** — Pegá acá el valor que generaste en el paso 3.2:

```powershell
dotnet user-secrets set "JWT_SECRET_KEY" "<EL_VALOR_QUE_GENERASTE>" --project src/Etereo.Api
```

**JWT_ISSUER y JWT_AUDIENCE** — Son valores fijos, copiá exactamente así:

```powershell
dotnet user-secrets set "JWT_ISSUER"   "etereo-api" --project src/Etereo.Api
dotnet user-secrets set "JWT_AUDIENCE" "etereo-app" --project src/Etereo.Api
```

**ADMIN_PASSWORD** — La contraseña que va a tener el usuario administrador que crea el seed automáticamente. Elegí una contraseña de al menos 8 caracteres:

```powershell
dotnet user-secrets set "ADMIN_PASSWORD" "<CONTRASEÑA_QUE_ELIJAS>" --project src/Etereo.Api
```

**RESEND_API_KEY y RESEND_FROM_EMAIL** — Son para enviar emails. Por ahora cargá estos valores temporales que te permiten testear (solo mandan emails a tu propio correo de Resend):

```powershell
dotnet user-secrets set "RESEND_API_KEY"    "re_test_placeholder"   --project src/Etereo.Api
dotnet user-secrets set "RESEND_FROM_EMAIL" "onboarding@resend.dev" --project src/Etereo.Api
```

> Cuando tengas una cuenta real en Resend con dominio verificado, reemplazás estos valores. Ver Parte 2, Paso 3.

**GOOGLE_CLIENT_ID** — Es para el login con Google. Por ahora cargá un placeholder — no vas a poder usar login con Google hasta configurarlo, pero todo lo demás funciona:

```powershell
dotnet user-secrets set "GOOGLE_CLIENT_ID" "placeholder.apps.googleusercontent.com" --project src/Etereo.Api
```

> Cuando tengas el Client ID real de Google, reemplazás este valor. Ver Parte 2, Paso 2.

**CORS_ALLOWED_ORIGINS** — Qué frontend tiene permiso para llamar a la API. En desarrollo es el localhost de Vite:

```powershell
dotnet user-secrets set "CORS_ALLOWED_ORIGINS" "http://localhost:5173" --project src/Etereo.Api
```

### Paso 3.4 — Verificar que todo quedó cargado

```powershell
dotnet user-secrets list --project src/Etereo.Api
```

Deberías ver exactamente 9 líneas, una por cada secreto. Si falta alguno, repetí el comando `set` correspondiente.

---

## Paso 4 — Generar la primera migración de EF Core

EF Core necesita generar los archivos de migración — son los que le dicen a la base de datos qué tablas crear. Este comando los genera:

```powershell
dotnet ef migrations add InitialCreate `
    --project src/Etereo.Infrastructure `
    --startup-project src/Etereo.Api `
    --output-dir Persistence/Migrations
```

Cuando termine, vas a ver que se creó la carpeta `src/Etereo.Infrastructure/Persistence/Migrations/` con varios archivos `.cs`. **Commiteá esos archivos** — son parte del código.

> Si el comando falla con `dotnet ef not found`, instalá la herramienta con:
> ```powershell
> dotnet tool install --global dotnet-ef
> ```
> Y volvé a intentar.

---

## Paso 5 — Correr el proyecto por primera vez

```powershell
dotnet run --project src/Etereo.Api
```

Al arrancar, la API hace automáticamente:
1. Aplica las migraciones → crea todas las tablas en la base de datos `etereo`
2. Ejecuta el seed → crea el usuario admin y carga todos los servicios con precios

El servidor queda corriendo en `http://localhost:8080`.

---

## Paso 6 — Verificar que todo funciona

Con el servidor corriendo, abrí otra ventana de PowerShell y ejecutá:

```powershell
# Health check — debe devolver @{data=ok}
Invoke-RestMethod -Uri http://localhost:8080/health -Method GET
```

```powershell
# Test suite completo
.\Test-Modulo0-Setup.ps1
```

Para explorar todos los endpoints disponibles, abrí en el browser:

```
http://localhost:8080/swagger
```

---

## Flujo día a día (una vez configurado)

Desde ese momento en adelante, para trabajar solo necesitás:

```powershell
dotnet run --project src/Etereo.Api
```

No hace falta cargar variables ni correr ningún script. Si en algún momento necesitás cambiar un secreto:

```powershell
# Ver todos los secretos actuales
dotnet user-secrets list --project src/Etereo.Api

# Cambiar un valor
dotnet user-secrets set "ADMIN_PASSWORD" "nuevo-valor" --project src/Etereo.Api
```

---
---

# PARTE 2 — Producción en Railway

Hacés esto cuando querés que la API esté disponible en internet. La Parte 1 (desarrollo local) es completamente independiente.

---

## Paso 1 — Crear la base de datos PostgreSQL en Railway y obtener el DATABASE_URL

1. Ir a [railway.app](https://railway.app) → loguearte → **New Project**.
2. Seleccionar **Deploy PostgreSQL**.
3. Railway crea una instancia de PostgreSQL. Hacé click en el servicio de Postgres que aparece.
4. Ir a la pestaña **Connect**.
5. Vas a ver varias variables. Buscá la que se llama **DATABASE_URL** — tiene un formato largo tipo `postgresql://postgres:xxxxx@monorail.proxy.rlwy.net:12345/railway`.

Ese valor lo vas a necesitar en el Paso 4, pero en un formato diferente. Convertilo así:

- Tomá los datos individuales que Railway también muestra (`PGHOST`, `PGPORT`, `PGDATABASE`, `PGUSER`, `PGPASSWORD`)
- Armá el connection string en este formato:

```
Host=<PGHOST>;Port=<PGPORT>;Database=<PGDATABASE>;Username=<PGUSER>;Password=<PGPASSWORD>;SSL Mode=Require;Trust Server Certificate=true
```

---

## Paso 2 — Configurar Google OAuth (para login con Google)

1. Ir a [console.cloud.google.com](https://console.cloud.google.com).
2. Crear un proyecto nuevo → nombre: `Etereo`.
3. En el menú lateral → **APIs & Services → OAuth consent screen**:
   - Tipo: **External**
   - Nombre de la app: `Etereo`
   - Email de soporte: tu email
   - Guardar y continuar (el resto se puede dejar vacío por ahora).
4. En **APIs & Services → Credentials → Create Credentials → OAuth client ID**:
   - Tipo de aplicación: **Web application**
   - Nombre: `Etereo Web`
   - Authorized JavaScript origins: `https://tudominio.com` y `http://localhost:5173`
   - Authorized redirect URIs: dejarlo vacío
5. Click en **Create**. Aparece un popup con el **Client ID** (formato `xxxxxx.apps.googleusercontent.com`).
6. Copialo — lo necesitás en el Paso 4.

---

## Paso 3 — Configurar Resend (para enviar emails)

1. Ir a [resend.com](https://resend.com) → crear cuenta gratuita (3.000 emails/mes permanente).
2. En el dashboard → **Domains → Add Domain**:
   - Ingresá tu dominio (ej: `etereo.com`).
   - Resend te da registros DNS para agregar en tu proveedor (Cloudflare, GoDaddy, etc.).
   - Agregá los registros y esperá la verificación (puede tardar hasta 24 hs).
3. Una vez verificado el dominio → **API Keys → Create API Key**:
   - Nombre: `etereo-backend-prod`
   - Permiso: **Sending access**
4. Copiá la clave (formato `re_xxxx`) — la necesitás en el Paso 4.

---

## Paso 4 — Configurar variables de entorno en Railway

1. En Railway → hacé click en el servicio de la **API** (no el de Postgres) → pestaña **Variables**.
2. Agregá una por una las siguientes variables:

   | Variable | De dónde sale el valor |
   |---|---|
   | `DATABASE_URL` | El connection string que armaste en el Paso 1 |
   | `JWT_SECRET_KEY` | Generalo con el comando de PowerShell del Paso 3.2 de la Parte 1 |
   | `JWT_ISSUER` | Escribí exactamente: `etereo-api` |
   | `JWT_AUDIENCE` | Escribí exactamente: `etereo-app` |
   | `ADMIN_PASSWORD` | Una contraseña segura para el admin (mín. 8 chars) |
   | `RESEND_API_KEY` | La clave `re_xxxx` del Paso 3 |
   | `RESEND_FROM_EMAIL` | `Etereo <noreply@tudominio.com>` (con el dominio que verificaste) |
   | `GOOGLE_CLIENT_ID` | El Client ID `xxxxx.apps.googleusercontent.com` del Paso 2 |
   | `CORS_ALLOWED_ORIGINS` | La URL del frontend en producción, ej: `https://etereo.com` |

---

## Paso 5 — Conectar el repo y hacer el primer deploy

1. En Railway → **New Project → Deploy from GitHub repo**.
2. Seleccioná el repo `Maticasale/Etereo.Backend`.
3. Railway detecta el `Dockerfile` automáticamente.
4. Asegurate de que el branch sea `main`.
5. Ya tenés las variables del Paso 4 cargadas → click en **Deploy**.
6. En **Settings → Networking → Generate Domain** para obtener la URL pública.

Al arrancar, Railway corre automáticamente las migraciones y el seed.

**Verificar que el deploy funcionó:**

```
GET https://tu-dominio.railway.app/health
```

Debe devolver: `{ "data": "ok" }`

---

## Notas finales

- **User Secrets** (desarrollo) viven en `C:\Users\Matias\AppData\Roaming\Microsoft\UserSecrets\4e8d3f2a-9b1c-4d5e-8f7a-2c3d4e5f6a7b\secrets.json` — nunca se commitean.
- **`.env.example`** en el repo documenta las 9 variables necesarias — sirve de referencia para otros devs.
- Las **migraciones de EF Core** sí se commitean (`src/Etereo.Infrastructure/Persistence/Migrations/`).
- El **seed es idempotente** — puede correr múltiples veces sin duplicar datos.
