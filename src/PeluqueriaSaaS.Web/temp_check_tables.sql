SELECT 
    TABLE_NAME as 'Tabla',
    TABLE_TYPE as 'Tipo'
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
AND TABLE_NAME IN (
    'Empleados', 
    'EmpleadosBasicos',
    'Clientes',
    'ClientesBasicos', 
    'Servicios',
    'TiposServicio',
    'Sucursales',
    'Citas'
)
ORDER BY TABLE_NAME;
