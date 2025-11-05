-- Crear base de datos
CREATE DATABASE InventoryDb;
GO

USE InventoryDb;
GO

-- Tabla TipoMovimiento
CREATE TABLE TipoMovimiento (
    IdTipoMovimiento INT PRIMARY KEY,
    Tipo NVARCHAR(50) NOT NULL
);
GO

INSERT INTO TipoMovimiento VALUES (1, 'Entrada'), (2, 'Salida');
GO

-- Tabla Productos
CREATE TABLE Productos (
    IdProducto INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(255),
    PrecioVenta DECIMAL(18,2) NOT NULL,
    MinimoExistencia INT NOT NULL DEFAULT 0,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    UltimaFechaActualizacion DATETIME DEFAULT GETDATE()
);
GO

-- Tabla Inventario
CREATE TABLE Inventario (
    IdProducto INT PRIMARY KEY,
    Existencia INT NOT NULL DEFAULT 0,
    UltimaFechaActualizacion DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto)
);
GO

-- Tabla MovimientosInventario
CREATE TABLE MovimientosInventario (
    IdMovimiento INT IDENTITY(1,1) PRIMARY KEY,
    IdProducto INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Cantidad INT NOT NULL,
    IdTipoMovimiento INT NOT NULL,
    UltimaFechaActualizacion DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto),
    FOREIGN KEY (IdTipoMovimiento) REFERENCES TipoMovimiento(IdTipoMovimiento)
);
GO

-- Procedimientos almacenados para Productos

CREATE PROCEDURE GetProductos
AS
BEGIN
    SELECT * FROM Productos;
END
GO

CREATE PROCEDURE GetProducto @IdProducto INT
AS
BEGIN
    SELECT * FROM Productos WHERE IdProducto = @IdProducto;
END
GO

CREATE PROCEDURE InsertProducto @Nombre NVARCHAR(100), @Descripcion NVARCHAR(255), @PrecioVenta DECIMAL(18,2), @MinimoExistencia INT
AS
BEGIN
    INSERT INTO Productos (Nombre, Descripcion, PrecioVenta, MinimoExistencia, FechaCreacion, UltimaFechaActualizacion)
    VALUES (@Nombre, @Descripcion, @PrecioVenta, @MinimoExistencia, GETDATE(), GETDATE());
    SELECT SCOPE_IDENTITY() AS IdProducto;
END
GO

CREATE PROCEDURE UpdateProducto @IdProducto INT, @Nombre NVARCHAR(100), @Descripcion NVARCHAR(255), @PrecioVenta DECIMAL(18,2), @MinimoExistencia INT
AS
BEGIN
    UPDATE Productos SET Nombre = @Nombre, Descripcion = @Descripcion, PrecioVenta = @PrecioVenta, MinimoExistencia = @MinimoExistencia, UltimaFechaActualizacion = GETDATE()
    WHERE IdProducto = @IdProducto;
END
GO

CREATE PROCEDURE DeleteProducto @IdProducto INT
AS
BEGIN
    DELETE FROM Productos WHERE IdProducto = @IdProducto;
END
GO

-- Procedimientos para Inventario (ejemplo básico)

CREATE PROCEDURE GetInventario
AS
BEGIN
    SELECT * FROM Inventario;
END
GO

CREATE PROCEDURE GetInventarioByProducto @IdProducto INT
AS
BEGIN
    SELECT * FROM Inventario WHERE IdProducto = @IdProducto;
END
GO

-- Etc. para otros