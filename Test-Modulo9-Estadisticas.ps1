###############################################################################
#  Módulo 9 — Estadísticas & Dashboard
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

Write-Host "`n[LOGIN]"
$login = Invoke-RestMethod "$base/auth/login" -Method POST -ContentType "application/json" `
    -Body '{"email":"admin@etereo.com","password":"Admin1234!"}' -ErrorAction SilentlyContinue
$token = $login.data.accessToken
if (-not $token) { Write-Host "  LOGIN FALLIDO" -ForegroundColor Red; exit 1 }
$hA = @{ Authorization = "Bearer $token" }
Write-Host "  [OK] Login admin" -ForegroundColor Green

$hoy      = (Get-Date).ToString("yyyy-MM-dd")
$hace30   = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
$hace365  = (Get-Date).AddDays(-365).ToString("yyyy-MM-dd")

###############################################################################
#  RESUMEN
###############################################################################
Write-Host "`n[RESUMEN]"
$r = Invoke-WebRequest "$base/estadisticas/resumen" -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET resumen" 200 $r.StatusCode
$res = ($r.Content | ConvertFrom-Json).data
Write-Host "    → TurnosHoy: $($res.turnosHoy) | TurnosMes: $($res.turnosMes)"
Write-Host "    → IngresosMes: $($res.ingresosMes) | EgresosMes: $($res.egresosMes) | BalanceMes: $($res.balanceMes)"
Write-Host "    → Promedio calificación: $($res.promedioCalificacionGlobal) ($($res.totalCalificaciones) cal.)"
Write-Host "    → Estados este mes:"
$res.turnosPorEstado | ForEach-Object { Write-Host "      $($_.estado): $($_.cantidad) ($($_.porcentaje)%)" }

###############################################################################
#  EVOLUCIÓN
###############################################################################
Write-Host "`n[EVOLUCIÓN]"

$r = Invoke-WebRequest "$base/estadisticas/evolucion?fechaDesde=$hace30&fechaHasta=$hoy&agrupacion=dia" `
    -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET evolución por día" 200 $r.StatusCode
$puntos = ($r.Content | ConvertFrom-Json).data
Write-Host "    → $($puntos.Count) punto(s) diarios"

$r = Invoke-WebRequest "$base/estadisticas/evolucion?fechaDesde=$hace365&fechaHasta=$hoy&agrupacion=mes" `
    -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET evolución por mes" 200 $r.StatusCode

$r = Invoke-WebRequest "$base/estadisticas/evolucion?fechaDesde=$hace30&fechaHasta=$hoy&agrupacion=semana" `
    -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET evolución por semana" 200 $r.StatusCode

# Agrupación inválida
$r = Invoke-WebRequest "$base/estadisticas/evolucion?fechaDesde=$hace30&fechaHasta=$hoy&agrupacion=hora" `
    -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET evolución agrupación inválida → 400" 400 $r.StatusCode

###############################################################################
#  RANKING SERVICIOS
###############################################################################
Write-Host "`n[RANKING SERVICIOS]"

$r = Invoke-WebRequest "$base/estadisticas/servicios" -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET ranking servicios (sin filtro)" 200 $r.StatusCode
$rank = ($r.Content | ConvertFrom-Json).data
Write-Host "    → $($rank.Count) servicio(s) con turnos realizados"
$rank | Select-Object -First 3 | ForEach-Object {
    Write-Host "      $($_.nombreServicio): $($_.cantidadTurnos) turnos | $$($_.ingresoTotal)"
}

$r = Invoke-WebRequest "$base/estadisticas/servicios?fechaDesde=$hace30&fechaHasta=$hoy" `
    -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET ranking servicios (con filtro fecha)" 200 $r.StatusCode

###############################################################################
#  ESTADÍSTICAS OPERARIAS
###############################################################################
Write-Host "`n[OPERARIAS]"

$r = Invoke-WebRequest "$base/estadisticas/operarias" -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET estadísticas operarias (mes actual)" 200 $r.StatusCode
$ops = ($r.Content | ConvertFrom-Json).data
Write-Host "    → $($ops.Count) operaria(s)"
$ops | ForEach-Object {
    Write-Host "      $($_.nombre): $($_.turnosMes) turnos | Realizados: $($_.turnosRealizados) | Ingresos: $$($_.ingresosMes) | Prom.Cal: $($_.promedioCalificacion)"
}

$r = Invoke-WebRequest "$base/estadisticas/operarias?fechaDesde=$hace30&fechaHasta=$hoy" `
    -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET estadísticas operarias (con rango)" 200 $r.StatusCode

###############################################################################
#  OCUPACIÓN DIARIA
###############################################################################
Write-Host "`n[OCUPACIÓN]"

$r = Invoke-WebRequest "$base/estadisticas/ocupacion?fechaDesde=$hace30&fechaHasta=$hoy" `
    -Headers $hA -ErrorAction SilentlyContinue
Test-Endpoint "GET ocupación diaria" 200 $r.StatusCode
$ocup = ($r.Content | ConvertFrom-Json).data
Write-Host "    → $($ocup.Count) día(s) con actividad"

###############################################################################
#  RESUMEN FINAL
###############################################################################
Write-Host "`n─────────────────────────────────────────"
$total = $ok + $fail
Write-Host "  Resultado: $ok/$total tests pasaron" -ForegroundColor ($fail -eq 0 ? "Green" : "Yellow")
if ($fail -gt 0) { Write-Host "  $fail test(s) fallaron" -ForegroundColor Red }
