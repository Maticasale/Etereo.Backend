# ─────────────────────────────────────────────────────────────────────────────
# Test-Modulo0-Setup.ps1
# Valida el setup inicial del backend Etereo (Módulo 0).
# Cambiar $baseUrl para probar contra Railway en vez de localhost.
# ─────────────────────────────────────────────────────────────────────────────

$baseUrl = "http://localhost:8080"

$passed = 0
$failed = 0

function Write-Pass($msg) {
    Write-Host "  [PASS] $msg" -ForegroundColor Green
    $script:passed++
}

function Write-Fail($msg) {
    Write-Host "  [FAIL] $msg" -ForegroundColor Red
    $script:failed++
}

function Write-Section($title) {
    Write-Host ""
    Write-Host "── $title ─────────────────────────────────────────────" -ForegroundColor Cyan
}

# ─────────────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "Etereo Backend — Test Suite Módulo 0: Setup" -ForegroundColor Yellow
Write-Host "Base URL: $baseUrl"
Write-Host ""

# ── Test 1: GET /health → 200 { data: "ok" } ─────────────────────────────
Write-Section "1. Health Check"

try {
    $resp = Invoke-WebRequest -Uri "$baseUrl/health" -Method GET -ErrorAction Stop
    $body = $resp.Content | ConvertFrom-Json

    if ($resp.StatusCode -eq 200) {
        Write-Pass "GET /health devuelve 200"
    } else {
        Write-Fail "GET /health devolvió $($resp.StatusCode), esperado 200"
    }

    if ($body.data -eq "ok") {
        Write-Pass "Body contiene { data: 'ok' }"
    } else {
        Write-Fail "Body incorrecto: $($resp.Content)"
    }
}
catch {
    Write-Fail "GET /health lanzó excepción: $_"
}

# ── Test 2: POST a ruta protegida sin JWT → 401 ───────────────────────────
Write-Section "2. Ruta protegida sin JWT → 401"

try {
    $resp = Invoke-WebRequest `
        -Uri "$baseUrl/api/v1/usuarios" `
        -Method GET `
        -ErrorAction Stop

    Write-Fail "Se esperaba 401 pero devolvió $($resp.StatusCode)"
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__

    if ($statusCode -eq 401) {
        Write-Pass "GET /api/v1/usuarios sin JWT devuelve 401"

        try {
            $rawBody = $_.ErrorDetails.Message | ConvertFrom-Json
            if ($rawBody.error.codigo) {
                Write-Pass "Body de error contiene campo 'error.codigo': $($rawBody.error.codigo)"
            } else {
                Write-Fail "Body de error no contiene 'error.codigo'"
            }
        }
        catch {
            Write-Fail "No se pudo parsear el body del error 401"
        }
    } else {
        Write-Fail "Se esperaba 401 pero devolvió $statusCode"
    }
}

# ── Test 3: POST a ruta protegida con JWT inválido → 401 ─────────────────
Write-Section "3. Ruta protegida con JWT inválido → 401"

$fakeJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
           "eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkZha2UiLCJpYXQiOjE1MTYyMzkwMjJ9." +
           "SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"

$headers = @{ Authorization = "Bearer $fakeJwt" }

try {
    $resp = Invoke-WebRequest `
        -Uri "$baseUrl/api/v1/usuarios" `
        -Method GET `
        -Headers $headers `
        -ErrorAction Stop

    Write-Fail "Se esperaba 401 pero devolvió $($resp.StatusCode)"
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__

    if ($statusCode -eq 401) {
        Write-Pass "GET /api/v1/usuarios con JWT inválido devuelve 401"

        try {
            $rawBody = $_.ErrorDetails.Message | ConvertFrom-Json
            if ($rawBody.error.codigo) {
                Write-Pass "Body de error contiene campo 'error.codigo': $($rawBody.error.codigo)"
            } else {
                Write-Fail "Body de error no contiene 'error.codigo'"
            }
        }
        catch {
            Write-Fail "No se pudo parsear el body del error 401"
        }
    } else {
        Write-Fail "Se esperaba 401 pero devolvió $statusCode"
    }
}

# ── Resumen ───────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
$total = $passed + $failed
Write-Host "Resultado: $passed/$total tests pasaron" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Yellow" })

if ($failed -gt 0) {
    Write-Host "$failed test(s) fallaron." -ForegroundColor Red
    exit 1
} else {
    Write-Host "Todos los tests pasaron." -ForegroundColor Green
    exit 0
}
