-- =============================================
-- 📊 DATOS DEMO URUGUAY - EMPLEADOS Y SERVICIOS
-- Archivo: datos_demo_001.sql  
-- Fecha: 2025-07-20
-- =============================================

-- 🇺🇾 EMPLEADOS DEMO URUGUAY (8 registros)
INSERT INTO Empleados (Nombre, Apellido, Email, Telefono, FechaNacimiento, FechaRegistro, FechaContratacion, Cargo, Salario, Horario, Direccion, Ciudad, CodigoPostal, Notas, EsActivo, SucursalId) VALUES

-- Estilistas Senior
('María', 'González', 'maria.gonzalez@peluqueria.com', '+598 99 123 456', '1990-05-15', GETDATE(), '2020-03-10', 'Estilista Senior', 45000.00, 'Lun-Vie 9:00-18:00', 'Av. 18 de Julio 1234', 'Montevideo', '11200', 'Especialista en cortes modernos y tendencias europeas', 1, 1),

('Carlos', 'Rodríguez', 'carlos.rodriguez@peluqueria.com', '+598 99 234 567', '1985-11-20', GETDATE(), '2019-08-15', 'Colorista Expert', 50000.00, 'Mar-Sab 10:00-19:00', 'Pocitos Boulevard 456', 'Montevideo', '11300', 'Experto en técnicas de coloración y mechas', 1, 1),

-- Estilistas
('Ana', 'Martínez', 'ana.martinez@peluqueria.com', '+598 99 345 678', '1992-07-08', GETDATE(), '2021-01-20', 'Estilista', 38000.00, 'Lun-Vie 10:00-19:00', 'Av. Brasil 789', 'Montevideo', '11500', 'Especialista en peinados para eventos', 1, 1),

('Diego', 'Silva', 'diego.silva@peluqueria.com', '+598 99 456 789', '1988-12-03', GETDATE(), '2020-09-05', 'Estilista', 40000.00, 'Mar-Sab 9:00-18:00', 'Cordón Norte 321', 'Montevideo', '11100', 'Experto en cortes masculinos y barba', 1, 1),

-- Manicuristas  
('Valentina', 'López', 'valentina.lopez@peluqueria.com', '+598 99 567 890', '1995-04-22', GETDATE(), '2022-02-14', 'Manicurista', 30000.00, 'Lun-Vie 11:00-20:00', 'Carrasco Norte 654', 'Montevideo', '15000', 'Especialista en nail art y tratamientos', 1, 1),

('Francisca', 'Torres', 'francisca.torres@peluqueria.com', '+598 99 678 901', '1993-09-17', GETDATE(), '2021-06-30', 'Manicurista Senior', 35000.00, 'Mar-Sab 10:00-19:00', 'Punta Gorda 987', 'Montevideo', '11400', 'Experta en manicure francesa y gelish', 1, 1),

-- Recepcionista y Auxiliar
('Camila', 'Herrera', 'camila.herrera@peluqueria.com', '+598 99 789 012', '1997-01-12', GETDATE(), '2022-05-18', 'Recepcionista', 28000.00, 'Lun-Vie 9:00-18:00', 'Centro Histórico 147', 'Montevideo', '11000', 'Atención al cliente y agenda', 1, 1),

('Sebastián', 'Morales', 'sebastian.morales@peluqueria.com', '+598 99 890 123', '1991-08-25', GETDATE(), '2021-11-08', 'Auxiliar', 25000.00, 'Lun-Sab 8:00-17:00', 'La Comercial 258', 'Montevideo', '12000', 'Lavado, secado y asistencia general', 1, 1);


-- 🇺🇾 SERVICIOS DEMO URUGUAY (8 registros)
INSERT INTO Servicios (Nombre, Descripcion, Precio, DuracionMinutos, TipoServicioId, TenantId, EsActivo, FechaCreacion, FechaActualizacion, CreadoPor, ActualizadoPor, Moneda, Activo) VALUES

-- CORTES (TipoServicioId = 1)
('Corte Moderno Mujer', 'Corte de tendencia con técnicas actuales y peinado incluido', 1200.00, 45, 1, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),

('Corte Clásico Hombre', 'Corte tradicional masculino con arreglo de barba', 800.00, 30, 1, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),

-- COLORACIÓN (TipoServicioId = 2)  
('Mechas Californianas', 'Técnica de iluminación natural con degradé perfecto', 2500.00, 120, 2, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),

('Coloración Completa', 'Tintura completa del cabello con tratamiento hidratante', 2000.00, 90, 2, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),

('Balayage Premium', 'Técnica francesa de coloración natural y elegante', 3200.00, 150, 2, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),

-- MANICURE (TipoServicioId = 3)
('Manicure Francesa', 'Manicure clásica con esmaltado francés', 600.00, 45, 3, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),

('Nail Art Personalizado', 'Diseños únicos y creativos en uñas', 900.00, 60, 3, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1),

-- PEINADOS (TipoServicioId = 4)
('Peinado Eventos', 'Peinado elegante para bodas, graduaciones y eventos especiales', 1800.00, 75, 4, 'default', 1, GETDATE(), GETDATE(), 'system', 'system', 'UYU', 1);


-- ⚠️ FIX APLICADO POSTERIORMENTE:
-- Los registros se insertaron con FechaActualizacion = NULL
-- Se ejecutó: UPDATE Servicios SET FechaActualizacion = GETDATE() WHERE FechaActualizacion IS NULL;

-- 🔍 VERIFICACIÓN FINAL:
SELECT 'EMPLEADOS DEMO' as Tipo, COUNT(*) as Total FROM Empleados WHERE FechaRegistro >= CAST(GETDATE() AS DATE);
SELECT 'SERVICIOS DEMO' as Tipo, COUNT(*) as Total FROM Servicios WHERE CreadoPor = 'system' AND FechaCreacion >= CAST(GETDATE() AS DATE);