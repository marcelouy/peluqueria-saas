# ====================================================
# SCRIPT DEBUG VENTAS NAVIGATION
# Diagnóstica problema redirect /Ventas/Details/6 → /Ventas
# ====================================================

Write-Host "🔍 INICIANDO DEBUG VENTAS NAVIGATION..." -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Gray

# URL base de la aplicación
$baseUrl = "http://localhost:5043"
$cookieContainer = New-Object System.Net.CookieContainer

# ====================================================
# FUNCIÓN: Hacer request con análisis completo
# ====================================================
function Test-HttpRequest {
    param(
        [string]$Url,
        [string]$Description
    )
    
    Write-Host "`n🎯 $Description" -ForegroundColor Yellow
    Write-Host "URL: $Url" -ForegroundColor Gray
    Write-Host "-----------------------------------" -ForegroundColor Gray
    
    try {
        # Preparar request
        $request = [System.Net.HttpWebRequest]::Create($Url)
        $request.Method = "GET"
        $request.AllowAutoRedirect = $false
        $request.CookieContainer = $cookieContainer
        $request.UserAgent = "Debug-PowerShell/1.0"
        
        # Ejecutar request
        $response = $request.GetResponse()
        
        # Analizar respuesta
        Write-Host "✅ STATUS: $($response.StatusCode) ($([int]$response.StatusCode))" -ForegroundColor Green
        
        # Verificar redirects
        if ($response.StatusCode -eq [System.Net.HttpStatusCode]::Redirect -or 
            $response.StatusCode -eq [System.Net.HttpStatusCode]::Found -or
            $response.StatusCode -eq [System.Net.HttpStatusCode]::MovedPermanently) {
            
            $location = $response.Headers["Location"]
            Write-Host "🔄 REDIRECT TO: $location" -ForegroundColor Magenta
        }
        
        # Headers importantes
        Write-Host "📋 HEADERS:" -ForegroundColor Cyan
        foreach ($header in $response.Headers.AllKeys) {
            if ($header -in @("Location", "Content-Type", "Content-Length", "Server")) {
                Write-Host "   $header`: $($response.Headers[$header])" -ForegroundColor White
            }
        }
        
        $response.Close()
        return $true
        
    } catch [System.Net.WebException] {
        $errorResponse = $_.Exception.Response
        if ($errorResponse) {
            Write-Host "❌ ERROR: $($errorResponse.StatusCode) ($([int]$errorResponse.StatusCode))" -ForegroundColor Red
            Write-Host "   Description: $($errorResponse.StatusDescription)" -ForegroundColor Red
        } else {
            Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        }
        return $false
    } catch {
        Write-Host "❌ EXCEPTION: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# ====================================================
# FUNCIÓN: Verificar servidor activo
# ====================================================
function Test-ServerRunning {
    Write-Host "🚀 VERIFICANDO SERVIDOR..." -ForegroundColor Yellow
    
    try {
        $response = Invoke-WebRequest -Uri $baseUrl -Method HEAD -TimeoutSec 5 -ErrorAction Stop
        Write-Host "✅ Servidor ASP.NET Core funcionando en $baseUrl" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "❌ Servidor NO disponible en $baseUrl" -ForegroundColor Red
        Write-Host "   Asegúrate que dotnet run está ejecutándose" -ForegroundColor Yellow
        return $false
    }
}

# ====================================================
# FUNCIÓN: Test específico routing ASP.NET
# ====================================================
function Test-RoutingPattern {
    Write-Host "`n🛣️ TESTING ROUTING PATTERNS..." -ForegroundColor Yellow
    
    $routes = @(
        @{ Url = "$baseUrl/Ventas"; Desc = "Index Ventas (should work)" },
        @{ Url = "$baseUrl/Ventas/Details"; Desc = "Details sin ID (should error)" },
        @{ Url = "$baseUrl/Ventas/Details/1"; Desc = "Details con ID=1" },
        @{ Url = "$baseUrl/Ventas/Details/6"; Desc = "Details con ID=6 (problema reportado)" },
        @{ Url = "$baseUrl/Ventas/Details/999"; Desc = "Details con ID inexistente" }
    )
    
    foreach ($route in $routes) {
        Test-HttpRequest -Url $route.Url -Description $route.Desc
        Start-Sleep -Milliseconds 500
    }
}

# ====================================================
# FUNCIÓN: Analizar cookies/session
# ====================================================
function Test-SessionCookies {
    Write-Host "`n🍪 TESTING SESSION/COOKIES..." -ForegroundColor Yellow
    
    # Primero ir a /Ventas para establecer session
    try {
        $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
        $response = Invoke-WebRequest -Uri "$baseUrl/Ventas" -WebSession $session -ErrorAction Stop
        
        Write-Host "✅ Session establecida con /Ventas" -ForegroundColor Green
        Write-Host "📋 COOKIES:" -ForegroundColor Cyan
        
        foreach ($cookie in $session.Cookies.GetCookies($baseUrl)) {
            Write-Host "   $($cookie.Name): $($cookie.Value.Substring(0, [Math]::Min(50, $cookie.Value.Length)))..." -ForegroundColor White
        }
        
        # Ahora test Details con session
        Write-Host "`n🎯 Testing Details con session establecida..."
        $detailsResponse = Invoke-WebRequest -Uri "$baseUrl/Ventas/Details/6" -WebSession $session -MaximumRedirection 0 -ErrorAction SilentlyContinue
        
        if ($detailsResponse) {
            Write-Host "✅ Details response: $($detailsResponse.StatusCode)" -ForegroundColor Green
        }
        
    } catch {
        Write-Host "❌ Error en session test: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ====================================================
# MAIN EXECUTION
# ====================================================

# 1. Verificar servidor
if (-not (Test-ServerRunning)) {
    Write-Host "`n❌ ABORTANDO: Servidor no disponible" -ForegroundColor Red
    exit 1
}

# 2. Test routing patterns
Test-RoutingPattern

# 3. Test session/cookies
Test-SessionCookies

# ====================================================
# RESUMEN Y RECOMENDACIONES
# ====================================================
Write-Host "`n" + "="*50 -ForegroundColor Gray
Write-Host "📊 RESUMEN DEBUG RESULTS" -ForegroundColor Cyan
Write-Host "="*50 -ForegroundColor Gray

Write-Host "`n🔍 ANÁLISIS ESPERADO:" -ForegroundColor Yellow
Write-Host "   • /Ventas → 200 OK (funciona)"
Write-Host "   • /Ventas/Details/6 → 200 OK (debería funcionar)"
Write-Host "   • Si hay 302 redirect → problema identificado"
Write-Host "   • Si hay 404 → routing problem"
Write-Host "   • Si hay 500 → controller exception"

Write-Host "`n🚨 SI VES REDIRECTS 302:" -ForegroundColor Red
Write-Host "   → Controller Details() está ejecutando RedirectToAction"
Write-Host "   → Verificar venta.VentaId existe y EsActivo=true"
Write-Host "   → Verificar Include VentaDetalles no causa exception"

Write-Host "`n📋 PRÓXIMOS PASOS:" -ForegroundColor Green
Write-Host "   1. Ejecutar este script"
Write-Host "   2. Analizar status codes y redirects"
Write-Host "   3. Identificar causa específica problema"
Write-Host "   4. Aplicar fix correspondiente"

Write-Host "`n🔧 SCRIPT COMPLETADO" -ForegroundColor Cyan