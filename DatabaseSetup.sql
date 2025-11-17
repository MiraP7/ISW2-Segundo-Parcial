-- Crear base de datos
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'InventoryDb')
BEGIN
    CREATE DATABASE InventoryDb;
END
GO

USE InventoryDb;
GO

-- Tabla TipoMovimiento
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TipoMovimiento')
BEGIN
    CREATE TABLE TipoMovimiento (
        IdTipoMovimiento INT PRIMARY KEY,
        Tipo NVARCHAR(50) NOT NULL
    );
    INSERT INTO TipoMovimiento VALUES (1, 'Entrada'), (2, 'Salida');
END
GO

-- Tabla Productos (con CodigoProducto y Eliminado)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Productos')
BEGIN
    CREATE TABLE Productos (
        IdProducto INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        CodigoProducto NVARCHAR(50) NOT NULL UNIQUE,
        Descripcion NVARCHAR(255),
        PrecioVenta DECIMAL(18,2) NOT NULL,
        MinimoExistencia INT NOT NULL DEFAULT 0,
        Eliminado BIT NOT NULL DEFAULT 0,
        FechaCreacion DATETIME DEFAULT GETUTCDATE(),
        UltimaFechaActualizacion DATETIME DEFAULT GETUTCDATE()
    );
END
GO

-- Tabla Inventario
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Inventario')
BEGIN
    CREATE TABLE Inventario (
        IdProducto INT PRIMARY KEY,
        Existencia INT NOT NULL DEFAULT 0,
        UltimaFechaActualizacion DATETIME DEFAULT GETUTCDATE(),
        FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto)
    );
END
GO

-- Tabla MovimientosInventario
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MovimientosInventario')
BEGIN
    CREATE TABLE MovimientosInventario (
        IdMovimiento INT IDENTITY(1,1) PRIMARY KEY,
        IdProducto INT NOT NULL,
        Fecha DATETIME NOT NULL,
        Cantidad INT NOT NULL,
        IdTipoMovimiento INT NOT NULL,
        UltimaFechaActualizacion DATETIME DEFAULT GETUTCDATE(),
        FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto),
        FOREIGN KEY (IdTipoMovimiento) REFERENCES TipoMovimiento(IdTipoMovimiento)
    );
END
GO

-- =====================================================
-- STORED PROCEDURES para Productos
-- =====================================================

-- SP: GetProductos (solo no eliminados)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetProductos')
    DROP PROCEDURE GetProductos;
GO
CREATE PROCEDURE GetProductos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdProducto, Nombre, CodigoProducto, Descripcion, PrecioVenta, MinimoExistencia, Eliminado, FechaCreacion, UltimaFechaActualizacion
    FROM Productos
    WHERE Eliminado = 0;
END
GO

-- SP: GetProducto
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetProducto')
    DROP PROCEDURE GetProducto;
GO
CREATE PROCEDURE GetProducto @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdProducto, Nombre, CodigoProducto, Descripcion, PrecioVenta, MinimoExistencia, Eliminado, FechaCreacion, UltimaFechaActualizacion
    FROM Productos
    WHERE IdProducto = @IdProducto AND Eliminado = 0;
END
GO

-- SP: InsertProducto (genera CodigoProducto automáticamente)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'InsertProducto')
    DROP PROCEDURE InsertProducto;
GO
CREATE PROCEDURE InsertProducto 
    @Nombre NVARCHAR(100), 
    @Descripcion NVARCHAR(255), 
    @PrecioVenta DECIMAL(18,2), 
    @MinimoExistencia INT,
    @IdProducto INT OUTPUT,
    @CodigoProducto NVARCHAR(50) OUTPUT,
    @Mensaje NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NextId INT;
    DECLARE @GeneratedCode NVARCHAR(50);
    
    -- Obtener el siguiente ID disponible
    SELECT @NextId = ISNULL(MAX(IdProducto), 0) + 1 FROM Productos;
    
    -- Generar código con formato PROD-XXXX (4 dígitos con ceros a la izquierda)
    SET @GeneratedCode = 'PROD-' + RIGHT('0000' + CAST(@NextId AS NVARCHAR), 4);
    
    -- Insertar el producto
    INSERT INTO Productos (Nombre, CodigoProducto, Descripcion, PrecioVenta, MinimoExistencia, Eliminado, FechaCreacion, UltimaFechaActualizacion)
    VALUES (@Nombre, @GeneratedCode, @Descripcion, @PrecioVenta, @MinimoExistencia, 0, GETUTCDATE(), GETUTCDATE());
    
    SET @IdProducto = SCOPE_IDENTITY();
    SET @CodigoProducto = @GeneratedCode;
    SET @Mensaje = 'Producto creado exitosamente con código ' + @GeneratedCode;
END
GO

-- SP: UpdateProducto (solo actualiza campos editables, CodigoProducto no se modifica)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'UpdateProducto')
    DROP PROCEDURE UpdateProducto;
GO
CREATE PROCEDURE UpdateProducto 
    @IdProducto INT, 
    @Nombre NVARCHAR(100), 
    @Descripcion NVARCHAR(255), 
    @PrecioVenta DECIMAL(18,2), 
    @MinimoExistencia INT,
    @Mensaje NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificar que el producto existe y no está eliminado
    IF NOT EXISTS (SELECT 1 FROM Productos WHERE IdProducto = @IdProducto AND Eliminado = 0)
    BEGIN
        SET @Mensaje = 'Producto no encontrado.';
        RETURN;
    END
    
    -- Actualizar solo los campos permitidos (CodigoProducto NO se modifica)
    UPDATE Productos
    SET Nombre = @Nombre,
        Descripcion = @Descripcion,
        PrecioVenta = @PrecioVenta,
        MinimoExistencia = @MinimoExistencia,
        UltimaFechaActualizacion = GETUTCDATE()
    WHERE IdProducto = @IdProducto;
    
    SET @Mensaje = 'Producto actualizado exitosamente.';
END
GO

-- SP: DeleteProducto (borrado lógico)
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'DeleteProducto')
    DROP PROCEDURE DeleteProducto;
GO
CREATE PROCEDURE DeleteProducto 
    @IdProducto INT,
    @Mensaje NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Productos WHERE IdProducto = @IdProducto)
    BEGIN
        SET @Mensaje = 'Producto no encontrado.';
        RETURN;
    END
    
    UPDATE Productos 
    SET Eliminado = 1, 
        UltimaFechaActualizacion = GETUTCDATE()
    WHERE IdProducto = @IdProducto;
    
    SET @Mensaje = 'Producto eliminado (borrado lógico).';
END
GO

-- =====================================================
-- STORED PROCEDURES para Inventario
-- =====================================================

-- SP: GetInventario
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetInventario')
    DROP PROCEDURE GetInventario;
GO
CREATE PROCEDURE GetInventario
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        i.IdProducto,
        p.Nombre AS Producto,
        i.Existencia,
        p.MinimoExistencia,
        CASE 
            WHEN i.Existencia > p.MinimoExistencia THEN 'Normal'
            WHEN i.Existencia = p.MinimoExistencia THEN 'En Mínimo'
            ELSE 'Bajo Mínimo'
        END AS Estado,
        i.UltimaFechaActualizacion
    FROM Inventario i
    INNER JOIN Productos p ON i.IdProducto = p.IdProducto
    WHERE p.Eliminado = 0;
END
GO

-- SP: GetInventarioByProducto
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetInventarioByProducto')
    DROP PROCEDURE GetInventarioByProducto;
GO
CREATE PROCEDURE GetInventarioByProducto @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        i.IdProducto,
        p.Nombre AS Producto,
        i.Existencia,
        p.MinimoExistencia,
        CASE 
            WHEN i.Existencia > p.MinimoExistencia THEN 'Normal'
            WHEN i.Existencia = p.MinimoExistencia THEN 'En Mínimo'
            ELSE 'Bajo Mínimo'
        END AS Estado,
        i.UltimaFechaActualizacion
    FROM Inventario i
    INNER JOIN Productos p ON i.IdProducto = p.IdProducto
    WHERE i.IdProducto = @IdProducto AND p.Eliminado = 0;
END
GO