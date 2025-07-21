-- =============================================
-- 🔧 FIX DROPDOWN TIPOS SERVICIO VACÍO
-- Archivo: fix_dropdown_001.sql
-- Fecha: 2025-07-20
-- =============================================

-- 🚨 PROBLEMA IDENTIFICADO:
-- ServiciosController buscaba TenantId = "tenant-demo"
-- Pero TiposServicio en BD tienen TenantId = "default"
-- Resultado: Dropdown vacío en Servicios/Create

-- ✅ SOLUCIÓN APLICADA:
-- Cambiar en: src\PeluqueriaSaaS.Web\Controllers\ServiciosController.cs
-- Línea 12:

-- ANTES:
-- private readonly string _tenantId = "tenant-demo";

-- DESPUÉS:
-- private readonly string _tenantId = "default";

-- 🔍 VERIFICACIÓN:
-- Confirmar que TiposServicio tienen TenantId = "default"
SELECT 'TiposServicio TenantId Check' as Verificacion, 
       TenantId, 
       COUNT(*) as Registros 
FROM TiposServicio 
GROUP BY TenantId;

-- ✅ RESULTADO ESPERADO:
-- TenantId = "default", 4 registros (Corte, Coloración, Manicure, Peinado)

-- 🎯 IMPACTO:
-- Dropdown TiposServicio en Servicios/Create ahora muestra las 4 opciones
-- Sistema Servicios 100% funcional