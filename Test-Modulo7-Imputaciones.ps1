###############################################################################
#  Módulo 7 — Imputaciones & Catálogos
#  Cubre: categorías-imputacion, métodos-pago, motivos-bloqueo-salon,
#         imputaciones (CRUD + resumen)
###############################################################################

$base = "http://localhost:5000/api/v1"
$ok   = 0
$fail = 0

function Test-Endpoint {
    param($Name, $Expected, $Actual, $Body = $null)
    if ($Actual -eq $Expected) {
        Write-Host "  [OK] $Name" -ForegroundColor Green
        $script:ok++
    } else {
        Write-Host "  [FAIL] $Name — esperado $Expected, obtenido $Actual" -ForegroundColor Red
        if ($Body) { Write-Host "        $Body" -ForegroundColor DarkRed }
        $script:fail++
    }
}

# ── Login admin ───────────────────────────────────────────────────────────────
Write-Host "`n[LOGIN]"
$loginRes = Invoke-RestMethod "$base/auth/login" -Method POST -ContentType "application/json" `
    -Body '{"email":"admin@etereo.com","password":"Admin1234!"}' -ErrorAction SilentlyContinue
$token = $loginRes.data.accessToken
if (-not $token) { Write-Host "  LOGIN FALLIDO — detener pruebas" -ForegroundColor Red; exit 1 }
$h = @{ Authorization = "Bearer $token" }
Write-Host "  [OK] Login admin" -ForegroundColor Green

###############################################################################
#  CATEGORÍAS DE IMPUTACIÓN
###############################################################################
Write-Host "`n[CATEGORÍAS DE IMPUTACIÓN]"

# Listar (puede estar vacío)
$r = Invoke-WebRequest "$base/categorias-imputacion" -Headers $h -ErrorAction SilentlyContinue
Test-Endpoint "GET categorías" 200 $r.StatusCode

# Crear categoría Ingreso
$body = '{"nombre":"Servicios de Belleza","tipo":"Ingreso","descripcion":"Ingresos por servicios"}'
$r = Invoke-WebRequest "$base/categorias-imputacion" -Method POST -Headers $h `
    -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
Test-Endpoint "POST crear categoría Ingreso" 200 $r.StatusCode
$catIngresoId = ($r.Content | ConvertFrom-Json).data.id

# Crear categoría Egreso
$body = '{"nombre":"Comisión Operaria","tipo":"Egreso","descripcion":"Comisiones a operarias"}'
$r = Invoke-WebRequest "$base/categorias-imputacion" -Method POST -Headers $h `
    -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
Test-Endpoint "POST crear categoría Egreso" 200 $r.StatusCode
$catEgresoId = ($r.Content | ConvertFrom-Json).data.id

# Actualizar nombre
$body = '{"nombre":"Servicios de Belleza Actualizado","descripcion":null}'
$r = Invoke-WebRequest "$base/categorias-imputacion/$catIngresoId" -Method PUT -Headers $h `
    -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
Test-Endpoint "PUT actualizar categoría" 200 $r.StatusCode

# Cambiar estado (desactivar)
$r = Invoke-WebRequest "$base/categorias-imputacion/$catIngresoId/estado" -Method PATCH -Headers $h `
    -ContentType "application/json" -Body '{"activo":false}' -ErrorAction SilentlyContinue
Test-Endpoint "PATCH estado categoría (desactivar)" 200 $r.StatusCode

# Reactivar
$r = Invoke-WebRequest "$base/categorias-imputacion/$catIngresoId/estado" -Method PATCH -Headers $h `
    -ContentType "application/json" -Body '{"activo":true}' -ErrorAction SilentlyContinue
Test-Endpoint "PATCH estado categoría (reactivar)" 200 $r.StatusCode

###############################################################################
#  MÉTODOS DE PAGO
###############################################################################
Write-Host "`n[MÉTODOS DE PAGO]"

$r = Invoke-WebRequest "$base/metodos-pago" -Headers $h -ErrorAction SilentlyContinue
Test-Endpoint "GET métodos de pago" 200 $r.StatusCode

$r = Invoke-WebRequest "$base/metodos-pago" -Method POST -Headers $h `
    -ContentType "application/json" -Body '{"nombre":"Efectivo"}' -ErrorAction SilentlyContinue
Test-Endpoint "POST crear método Efectivo" 200 $r.StatusCode
$metodoPagoId = ($r.Content | ConvertFrom-Json).data.id

$r = Invoke-WebRequest "$base/metodos-pago" -Method POST -Headers $h `
    -ContentType "application/json" -Body '{"nombre":"Transferencia"}' -ErrorAction SilentlyContinue
Test-Endpoint "POST crear método Transferencia" 200 $r.StatusCode

$r = Invoke-WebRequest "$base/metodos-pago/$metodoPagoId" -Method PUT -Headers $h `
    -ContentType "application/json" -Body '{"nombre":"Efectivo (AR$)"}' -ErrorAction SilentlyContinue
Test-Endpoint "PUT actualizar método de pago" 200 $r.StatusCode

$r = Invoke-WebRequest "$base/metodos-pago/$metodoPagoId/estado" -Method PATCH -Headers $h `
    -ContentType "application/json" -Body '{"activo":false}' -ErrorAction SilentlyContinue
Test-Endpoint "PATCH estado método de pago" 200 $r.StatusCode

###############################################################################
#  MOTIVOS DE BLOQUEO DE SALÓN
###############################################################################
Write-Host "`n[MOTIVOS DE BLOQUEO DE SALÓN]"

$r = Invoke-WebRequest "$base/motivos-bloqueo-salon" -Headers $h -ErrorAction SilentlyContinue
Test-Endpoint "GET motivos de bloqueo" 200 $r.StatusCode

$r = Invoke-WebRequest "$base/motivos-bloqueo-salon" -Method POST -Headers $h `
    -ContentType "application/json" -Body '{"nombre":"Feriado"}' -ErrorAction SilentlyContinue
Test-Endpoint "POST crear motivo Feriado" 200 $r.StatusCode
$motivoId = ($r.Content | ConvertFrom-Json).data.id

$r = Invoke-WebRequest "$base/motivos-bloqueo-salon" -Method POST -Headers $h `
    -ContentType "application/json" -Body '{"nombre":"Limpieza"}' -ErrorAction SilentlyContinue
Test-Endpoint "POST crear motivo Limpieza" 200 $r.StatusCode

$r = Invoke-WebRequest "$base/motivos-bloqueo-salon/$motivoId" -Method PUT -Headers $h `
    -ContentType "application/json" -Body '{"nombre":"Feriado Nacional"}' -ErrorAction SilentlyContinue
Test-Endpoint "PUT actualizar motivo" 200 $r.StatusCode

$r = Invoke-WebRequest "$base/motivos-bloqueo-salon/$motivoId/estado" -Method PATCH -Headers $h `
    -ContentType "application/json" -Body '{"activo":false}' -ErrorAction SilentlyContinue
Test-Endpoint "PATCH estado motivo" 200 $r.StatusCode

###############################################################################
#  IMPUTACIONES
###############################################################################
Write-Host "`n[IMPUTACIONES]"

# Reactivar catIngresoId (por si acaso)
Invoke-WebRequest "$base/categorias-imputacion/$catIngresoId/estado" -Method PATCH -Headers $h `
    -ContentType "application/json" -Body '{"activo":true}' -ErrorAction SilentlyContinue | Out-Null

$hoy = (Get-Date).ToString("yyyy-MM-dd")

# Crear imputación ingreso manual
$body = @{
    fecha       = $hoy
    tipo        = "Ingreso"
    categoriaId = $catIngresoId
    descripcion = "Cobro servicio manual"
    monto       = 15000
    turnoId     = $null
    operarioId  = $null
} | ConvertTo-Json
$r = Invoke-WebRequest "$base/imputaciones" -Method POST -Headers $h `
    -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
Test-Endpoint "POST crear imputación Ingreso" 200 $r.StatusCode
$impId = ($r.Content | ConvertFrom-Json).data.id

# Crear imputación egreso manual
$body = @{
    fecha       = $hoy
    tipo        = "Egreso"
    categoriaId = $catEgresoId
    descripcion = "Pago comisión"
    monto       = 3000
    turnoId     = $null
    operarioId  = $null
} | ConvertTo-Json
$r = Invoke-WebRequest "$base/imputaciones" -Method POST -Headers $h `
    -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
Test-Endpoint "POST crear imputación Egreso" 200 $r.StatusCode

# Listar imputaciones
$r = Invoke-WebRequest "$base/imputaciones" -Headers $h -ErrorAction SilentlyContinue
Test-Endpoint "GET listar imputaciones" 200 $r.StatusCode
$count = (($r.Content | ConvertFrom-Json).data).Count
Write-Host "    → $count imputación(es) encontrada(s)"

# Listar filtradas por tipo
$r = Invoke-WebRequest "$base/imputaciones?tipo=Ingreso" -Headers $h -ErrorAction SilentlyContinue
Test-Endpoint "GET listar imputaciones (filtro Ingreso)" 200 $r.StatusCode

# Resumen
$r = Invoke-WebRequest "$base/imputaciones/resumen" -Headers $h -ErrorAction SilentlyContinue
Test-Endpoint "GET resumen imputaciones" 200 $r.StatusCode
$resumen = ($r.Content | ConvertFrom-Json).data
Write-Host "    → Ingresos: $($resumen.totalIngresos) | Egresos: $($resumen.totalEgresos) | Balance: $($resumen.balance)"

# Actualizar imputación
$body = '{"descripcion":"Cobro servicio manual actualizado","monto":16000}'
$r = Invoke-WebRequest "$base/imputaciones/$impId" -Method PUT -Headers $h `
    -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
Test-Endpoint "PUT actualizar imputación" 200 $r.StatusCode

# Eliminar imputación de egreso (la segunda)
# Primero obtenemos todas para obtener el ID del egreso
$all = (Invoke-WebRequest "$base/imputaciones?tipo=Egreso" -Headers $h -ErrorAction SilentlyContinue).Content | ConvertFrom-Json
$egresoId = $all.data[0].id
$r = Invoke-WebRequest "$base/imputaciones/$egresoId" -Method DELETE -Headers $h -ErrorAction SilentlyContinue
Test-Endpoint "DELETE eliminar imputación" 200 $r.StatusCode

# Tipo inválido
$body = '{"fecha":"2025-01-01","tipo":"Invalido","categoriaId":1,"descripcion":"x","monto":100,"turnoId":null,"operarioId":null}'
$r = Invoke-WebRequest "$base/imputaciones" -Method POST -Headers $h `
    -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
Test-Endpoint "POST tipo inválido → error" 400 $r.StatusCode

###############################################################################
#  RESUMEN FINAL
###############################################################################
Write-Host "`n─────────────────────────────────────────"
$total = $ok + $fail
Write-Host "  Resultado: $ok/$total tests pasaron" -ForegroundColor ($fail -eq 0 ? "Green" : "Yellow")
if ($fail -gt 0) { Write-Host "  $fail test(s) fallaron" -ForegroundColor Red }
