###############################################################################
#  Módulo 8 — Emails & Notificaciones
#  Cubre: configuración email, historial, calificaciones
#  NOTA: el envío real de emails requiere RESEND_API_KEY configurada.
#        Los tests validan que los endpoints respondan correctamente.
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
if (-not $token) { Write-Host "  LOGIN FALLIDO" -ForegroundColor Red; exit 1 }
$hA = @{ Authorization = "Bearer $token" }
Write-Host "  [OK] Login admin" -ForegroundColor Green

###############################################################################
#  CONFIGURACIÓN EMAIL
###############################################################################
Write-Host "`n[CONFIGURACIÓN EMAIL]"

$r = Invoke-WebRequest "$base/emails/configuracion" -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET configuración" 200 $r.StatusCode
$cfg = ($r.Content | ConvertFrom-Json).data
Write-Host "    → EmailsActivos: $($cfg.emailsActivos) | RecordatorioDiasAntes: $($cfg.recordatorioDiasAntes)"

# Actualizar configuración
$body = '{"recordatorioDiasAntes":2,"postturnoHorasDespues":3,"emailsActivos":false}'
$r = Invoke-WebRequest "$base/emails/configuracion" -Method PUT -Headers $hA `
    -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
Test-Endpoint "PUT actualizar configuración" 200 $r.StatusCode
$cfg2 = ($r.Content | ConvertFrom-Json).data
Write-Host "    → EmailsActivos actualizado: $($cfg2.emailsActivos)"

# Reactivar emails
$r = Invoke-WebRequest "$base/emails/configuracion" -Method PUT -Headers $hA `
    -ContentType "application/json" `
    -Body '{"recordatorioDiasAntes":1,"postturnoHorasDespues":2,"emailsActivos":true}' `
    -ErrorAction SilentlyContinue
Test-Endpoint "PUT reactivar emails" 200 $r.StatusCode

###############################################################################
#  HISTORIAL
###############################################################################
Write-Host "`n[HISTORIAL DE EMAILS]"

$r = Invoke-WebRequest "$base/emails/historial" -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET historial (vacío)" 200 $r.StatusCode
$count = (($r.Content | ConvertFrom-Json).data).Count
Write-Host "    → $count email(s) en historial"

# Filtros no rompen el endpoint
$r = Invoke-WebRequest "$base/emails/historial?tipo=ConfirmacionTurno&estado=Enviado" `
    -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET historial con filtros" 200 $r.StatusCode

###############################################################################
#  CAMPAÑA
###############################################################################
Write-Host "`n[CAMPAÑA]"

$body = @{
    emails   = @("test@example.com")
    asunto   = "Novedades Etéreo"
    contenido = "<p>Tenemos nuevos servicios disponibles. ¡Reservá tu turno!</p>"
} | ConvertTo-Json
$r = Invoke-WebRequest "$base/emails/campana" -Method POST -Headers $hA `
    -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
Test-Endpoint "POST enviar campaña" 200 $r.StatusCode

# Sin destinatarios → error
$body = @{ emails = @(); asunto = "x"; contenido = "y" } | ConvertTo-Json
$r = Invoke-WebRequest "$base/emails/campana" -Method POST -Headers $hA `
    -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
Test-Endpoint "POST campaña sin destinatarios → error" 400 $r.StatusCode

###############################################################################
#  CALIFICACIONES
###############################################################################
Write-Host "`n[CALIFICACIONES]"

# Listar (vacío)
$r = Invoke-WebRequest "$base/calificaciones" -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET calificaciones (vacío)" 200 $r.StatusCode

# Necesitamos un turno REALIZADO para calificar → buscamos uno existente
$turnos = (Invoke-WebRequest "$base/turnos" -Headers $hA -ErrorAction SilentlyContinue).Content | ConvertFrom-Json
$turnoRealizado = $turnos.data | Where-Object { $_.estado -eq "Realizado" } | Select-Object -First 1

if ($turnoRealizado) {
    Write-Host "    → Turno realizado encontrado: #$($turnoRealizado.id)"

    # Login como el cliente del turno
    $loginC = Invoke-RestMethod "$base/auth/login" -Method POST -ContentType "application/json" `
        -Body '{"email":"cliente@test.com","password":"Cli123!"}' -ErrorAction SilentlyContinue
    $hC = @{ Authorization = "Bearer $($loginC.data.accessToken)" }

    $body = @{ turnoId = $turnoRealizado.id; puntuacion = 5; comentario = "Excelente servicio" } | ConvertTo-Json
    $r = Invoke-WebRequest "$base/calificaciones" -Method POST -Headers $hC `
        -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
    Test-Endpoint "POST crear calificación" 200 $r.StatusCode

    # Doble calificación → error
    $r = Invoke-WebRequest "$base/calificaciones" -Method POST -Headers $hC `
        -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
    Test-Endpoint "POST doble calificación → error" 400 $r.StatusCode

    # Promedio operario
    $opId = $turnoRealizado.operarioId
    $r = Invoke-WebRequest "$base/calificaciones/promedio/$opId" -Headers $hA -ErrorAction SilentlyContinue
    Test-Endpoint "GET promedio operario" 200 $r.StatusCode
    $prom = ($r.Content | ConvertFrom-Json).data
    Write-Host "    → Promedio operario #$opId : $($prom.promedio) ($($prom.totalCalificaciones) calificación/es)"
} else {
    Write-Host "    [SKIP] No hay turnos realizados — calificar después de usar módulo 5" -ForegroundColor Yellow
}

# Puntuación inválida → error
$loginC2 = Invoke-RestMethod "$base/auth/login" -Method POST -ContentType "application/json" `
    -Body '{"email":"cliente@test.com","password":"Cli123!"}' -ErrorAction SilentlyContinue
$hC2 = @{ Authorization = "Bearer $($loginC2.data.accessToken)" }
$body = '{"turnoId":999,"puntuacion":10,"comentario":"mal"}'
$r = Invoke-WebRequest "$base/calificaciones" -Method POST -Headers $hC2 `
    -ContentType "application/json" -Body $body -ErrorAction SilentlyContinue
Test-Endpoint "POST puntuación inválida → error" 400 $r.StatusCode

###############################################################################
#  RESUMEN FINAL
###############################################################################
Write-Host "`n─────────────────────────────────────────"
$total = $ok + $fail
Write-Host "  Resultado: $ok/$total tests pasaron" -ForegroundColor ($fail -eq 0 ? "Green" : "Yellow")
if ($fail -gt 0) { Write-Host "  $fail test(s) fallaron" -ForegroundColor Red }
