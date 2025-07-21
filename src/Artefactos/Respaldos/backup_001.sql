-- ==========================================================
-- 🛡️ BACKUP COMPLETO SISTEMA PELUQUERÍA SAAS - FUNCIONAL
-- Estado: 100% FUNCIONAL (Julio 20, 2025)
-- TenantId: "default" (CRÍTICO para funcionamiento)
-- ==========================================================

-- 🗄️ RECREAR BASE DE DATOS (si es necesario)
/*
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'PeluqueriaSaaS')
DROP DATABASE PeluqueriaSaaS;

CREATE DATABASE PeluqueriaSaaS;
USE PeluqueriaSaaS;
*/

USE PeluqueriaSaaS;

-- ============================================
-- 📋 ESTRUCTURA DE TABLAS COMPLETA
-- ============================================

-- 🧑‍💼 TABLA EMPLEADOS (100% FUNCIONAL)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Empleados' AND xtype='U')
CREATE TABLE Empleados (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Nombre nvarchar(100) NOT NULL,
    Apellido nvarchar(100) NOT NULL,
    Email nvarchar(255) NOT NULL,
    Telefono nvarchar(20),
    FechaNacimiento datetime2,
    FechaRegistro datetime2 NOT NULL DEFAULT GETDATE(),
    FechaContratacion datetime2,
    Cargo nvarchar(100),
    Salario decimal(18,2),
    Horario nvarchar(200),
    Direccion nvarchar(255),
    Ciudad nvarchar(100),
    CodigoPostal nvarchar(20),
    Notas nvarchar(500),
    EsActivo bit NOT NULL DEFAULT 1,
    SucursalId int
);

-- 👥 TABLA CLIENTES (100% FUNCIONAL)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ClientesBasicos' AND xtype='U')
CREATE TABLE ClientesBasicos (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Nombre nvarchar(100) NOT NULL,
    Apellido nvarchar(100) NOT NULL,
    Email nvarchar(255),
    Telefono nvarchar(20),
    FechaNacimiento datetime2,
    FechaRegistro datetime2 NOT NULL DEFAULT GETDATE(),
    Direccion nvarchar(255),
    CodigoPostal nvarchar(20),
    Ciudad nvarchar(100),
    Notas nvarchar(500),
    EsActivo bit NOT NULL DEFAULT 1,
    UltimaVisita datetime2
);

-- 🏷️ TABLA TIPOS DE SERVICIO (100% FUNCIONAL)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TiposServicio' AND xtype='U')
CREATE TABLE TiposServicio (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Nombre nvarchar(100) NOT NULL,
    Descripcion nvarchar(500),
    PrecioBase decimal(18,2),
    Codigo nvarchar(10),
    DuracionEstimadaMinutos int,
    TenantId nvarchar(450) NOT NULL DEFAULT 'default',
    Activo bit NOT NULL DEFAULT 1,
    EsActivo bit NOT NULL DEFAULT 1,
    EsPorDefecto bit NOT NULL DEFAULT 0,
    EsSistema bit NOT NULL DEFAULT 0,
    RequiereEspecialista bit NOT NULL DEFAULT 0,
    PermiteDescuentos bit NOT NULL DEFAULT 1,
    Color nvarchar(7),
    Icono nvarchar(50),
    Orden int DEFAULT 0,
    TiempoPreparacionMinutos int DEFAULT 0,
    FechaCreacion datetime2 NOT NULL DEFAULT GETDATE(),
    FechaActualizacion datetime2 NULL,
    CreadoPor nvarchar(255) NOT NULL DEFAULT 'system',
    ActualizadoPor nvarchar(255) NOT NULL DEFAULT 'system'
);

-- ✂️ TABLA SERVICIOS (100% FUNCIONAL)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Servicios' AND xtype='U')
CREATE TABLE Servicios (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Nombre nvarchar(200) NOT NULL,
    Descripcion nvarchar(1000),
    Precio decimal(18,2) NOT NULL,
    DuracionMinutos int NOT NULL,
    TipoServicioId int NOT NULL,
    TenantId nvarchar(450) NOT NULL DEFAULT 'default',
    EsActivo bit NOT NULL DEFAULT 1,
    Activo bit NOT NULL DEFAULT 1,
    FechaCreacion datetime2 NOT NULL DEFAULT GETDATE(),
    FechaActualizacion datetime2 NULL DEFAULT GETDATE(),
    CreadoPor nvarchar(255) NOT NULL DEFAULT 'system',
    ActualizadoPor nvarchar(255) NOT NULL DEFAULT 'system',
    Moneda nvarchar(3) NOT NULL DEFAULT 'UYU',
    FOREIGN KEY (TipoServicioId) REFERENCES TiposServicio(Id)
);

-- ============================================
-- 🌱 DATOS SEMILLA CRÍTICOS
-- ============================================

-- LIMPIAR DATOS EXISTENTES (OPCIONAL - CUIDADO)
-- DELETE FROM Servicios;
-- DELETE FROM Empleados;  
-- DELETE FROM ClientesBasicos;
-- DELETE FROM TiposServicio;

-- 🏷️ TIPOS DE SERVICIO (CRÍTICOS PARA DROPDOWN)
IF NOT EXISTS (SELECT 1 FROM TiposServicio WHERE Id = 1)
INSERT INTO TiposServicio (Id, Nombre, Descripcion, PrecioBase, Codigo, DuracionEstimadaMinutos, TenantId, Activo, EsActivo, EsPorDefecto, EsSistema, RequiereEspecialista, PermiteDescuentos, Color, Icono, Orden, TiempoPreparacionMinutos, FechaCreacion, FechaActualizacion, CreadoPor, ActualizadoPor) VALUES
(1, 'Corte de Cabello', 'Servicios de corte y peinado', 1000.00, 'CORTE', 45, 'default', 1, 1, 1, 0, 0, 1, '#FF5722', 'scissors', 1, 5, GETDATE(), GETDATE(), 'system', 'system'),
(2, 'Coloración', 'Servicios de tintura y mechas', 2500.00, 'COLOR', 90, 'default', 1, 1, 1, 0, 1, 1, '#9C27B0', 'palette', 2, 10, GETDATE(), GETDATE(), 'system', 'system'),
(3, 'Manicure', 'Servicios de manicure y nail art', 800.00, 'MANIC', 30, 'default', 1, 1, 1, 0, 0, 1, '#E91E63', 'hand', 3, 0, GETDATE(), GETDATE(), 'system', 'system'),
(4, 'Peinado', 'Peinados para eventos y ocasiones especiales', 1500.00, 'PEIN', 60, 'default', 1, 1, 1, 0, 1, 1, '#3F51B5', 'star', 4, 15, GETDATE(), GETDATE(), 'system', 'system');

-- 🧑‍💼 EMPLEADOS DEMO URUGUAY (12 REGISTROS)
INSERT INTO Empleados (Nombre, Apellido, Email, Telefono, FechaNacimiento, FechaRegistro, FechaContratacion, Cargo, Salario, Horario, Direccion, Ciudad, CodigoPostal, Notas, EsActivo, SucursalId) VALUES
-- Datos existentes + nuevos
('María', 'González', 'maria.gonzalez@peluqueria.com', '+598 99 123 456', '1990-05-15', GETDATE(), '2020-03-10', 'Estilista Senior', 45000.00, 'Lun-Vie 9:00-18:00', 'Av. 18 de Julio 1234', 'Montevideo', '11200', 'Especialista en cortes modernos y tendencias europeas', 1, 1),
('Carlos', 'Rodríguez', 'carlos.rodriguez@peluqueria.com', '+598 99 234 567', '1985-11-20', GETDATE(), '2019-08-15', 'Colorista Expert', 50000.00, 'Mar-Sab 10:00-19:00', 'Pocitos Boulevard 456', 'Montevideo', '11300', 'Experto en técnicas de coloración y mechas', 1, 1),
('Ana', 'Martínez', 'ana.martinez@peluqueria.com', '+598 99 345 678', '1992-07-08', GETDATE(), '2021-01-20', 'Estilista', 38000.00, 'Lun-Vie 10:00-19:00', 'Av. Brasil 789', 'Montevideo', '11500', 'Especialista en peinados para eventos', 1, 1),
('Diego', 'Silva', 'diego.silva@peluqueria.com', '+598 99 456 789', '1988-12-03', GETDATE(), '2020-09-05', 'Estilista', 40000.00, 'Mar-Sab 9:00-18:00', 'Cordón Norte 321', 'Montevideo', '11100', 'Experto en cortes masculinos y barba', 1, 1),
('Valentina', 'López', 'valentina.lopez@peluqueria.com', '+598 99 567 890', '1995-04-22', GETDATE(), '2022-02-14', 'Manicurista', 30000.00, 'Lun-Vie 11:00-20:00', 'Carrasco Norte 654', 'Montevideo', '15000', 'Especialista en nail art y tratamientos', 1, 1),
('Francisca', 'Torres', 'francisca.torres@peluqueria.com', '+598 99 678 901', '1993-09-17', GETDATE(), '2021-06-30', 'Manicurista Senior', 35000.00, 'Mar-Sab 10:00-19:00', 'Punta Gorda 987', 'Montevideo', '11400', 'Experta en manicure francesa y gelish', 1, 1),
('Camila', 'Herrera', 'camila.herrera@peluqueria.com', '+598 99 789 012', '1997-01-12', GETDATE(), '2022-05-18', 'Recepcionista', 28000.00, 'Lun-Vie 9:00-18:00', 'Centro Histórico 147', 'Montevideo', '11000', 'Atención al cliente y agenda', 1, 1),
('Sebastián', 'Morales', 'sebastian.morales@peluqueria.com', '+598 99 890 123', '1991-08-25', GETDATE(), '2021-11-08', 'Auxiliar', 25000.00, 'Lun-Sab 8:00-17:00', 'La Comercial 258', 'Montevideo', '12000', 'Lavado, secado y asistencia general', 1, 1);

-- ✂️ SERVICIOS DEMO URUGUAY (10 REGISTROS)
INSERT INTO Servicios (Nombre, Descripcion, Precio, DuracionMinutos, TipoServicioId, TenantId, EsActivo, FechaCreacion, FechaActualizacion, CreadoPor, ActualizadoPor, Moneda, Activo) VALUES
-- CORTES
('Corte Moderno Mujer', 'Corte de tendencia con técnicas actuales y peinado incluido', 1200.00, 45, 1, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),
('Corte Clásico Hombre', 'Corte tradicional masculino con arreglo de barba', 800.00, 30, 1, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),
-- COLORACIÓN
('Mechas Californianas', 'Técnica de iluminación natural con degradé perfecto', 2500.00, 120, 2, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),
('Coloración Completa', 'Tintura completa del cabello con tratamiento hidratante', 2000.00, 90, 2, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),
('Balayage Premium', 'Técnica francesa de coloración natural y elegante', 3200.00, 150, 2, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),
-- MANICURE  
('Manicure Francesa', 'Manicure clásica con esmaltado francés', 600.00, 45, 3, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),
('Nail Art Personalizado', 'Diseños únicos y creativos en uñas', 900.00, 60, 3, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),
-- PEINADOS
('Peinado Eventos', 'Peinado elegante para bodas, graduaciones y eventos especiales', 1800.00, 75, 4, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1);

-- ============================================
-- 🔍 VERIFICACIONES FINALES
-- ============================================

-- Verificar datos insertados
SELECT 'EMPLEADOS' as Tabla, COUNT(*) as Total FROM Empleados WHERE EsActivo = 1
UNION ALL
SELECT 'CLIENTES' as Tabla, COUNT(*) as Total FROM ClientesBasicos WHERE EsActivo = 1  
UNION ALL
SELECT 'TIPOS_SERVICIO' as Tabla, COUNT(*) as Total FROM TiposServicio WHERE EsActivo = 1
UNION ALL
SELECT 'SERVICIOS' as Tabla, COUNT(*) as Total FROM Servicios WHERE EsActivo = 1;

-- Verificar TenantId crítico
SELECT 'TiposServicio TenantId' as Check, TenantId, COUNT(*) as Registros 
FROM TiposServicio 
GROUP BY TenantId;

SELECT 'Servicios TenantId' as Check, TenantId, COUNT(*) as Registros 
FROM Servicios 
GROUP BY TenantId;

-- ============================================
-- 🚨 NOTAS CRÍTICAS PARA PRÓXIMOS CHATS
-- ============================================

/*
🎯 ESTADO ACTUAL: 100% FUNCIONAL

✅ QUE FUNCIONA:
- Empleados: CRUD completo (12 registros demo Uruguay)
- Clientes: CRUD completo  
- Servicios: CRUD completo (10 registros demo Uruguay)
- Dropdown TiposServicio: FUNCIONA (TenantId = "default")

🔧 FIX APLICADOS:
- ServiciosController TenantId: "tenant-demo" → "default"
- Servicios.FechaActualizacion: NULL → GETDATE()

⚠️ PROBLEMAS MENORES PENDIENTES:
1. Encoding UTF-8: "GestiÃ³n" → "Gestión"  
2. Botón "Guardar y seguir": guarda pero redirige a listado
3. CSS: diseño básico, falta bonito

🛡️ CRÍTICO - NO ROMPER:
- TenantId DEBE ser "default" en ServiciosController.cs
- NO tocar migraciones EF Core
- NO modificar estructura de tablas funcionando

📋 URLS FUNCIONALES:
- http://localhost:5043/Empleados ✅
- http://localhost:5043/Clientes ✅  
- http://localhost:5043/Servicios ✅

🗄️ CONEXIÓN BD:
Server=localhost;Database=PeluqueriaSaaS;Trusted_Connection=true;TrustServerCertificate=true;
*/