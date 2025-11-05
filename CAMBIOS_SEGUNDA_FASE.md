# Documentación de Cambios - Segunda Fase

## Requisitos Implementados

### R3: Agregar campo MinimoExistencia al catálogo de Productos ✅
- Agregado campo `MinimoExistencia` (int, default 0) al modelo `Producto.cs`.
- Actualizado procedimiento almacenado `InsertProducto` para recibir parámetro `@MinimoExistencia`.
- Actualizado procedimiento almacenado `UpdateProducto` para manejar `@MinimoExistencia`.
- Actualizado `DatabaseSetup.sql` con la nueva columna en la tabla `Productos`.

### R4: Lógica de Transacciones de Inventario ✅
- **Entrada:** Al registrar una transacción tipo "Entrada", se suma la cantidad a la existencia del producto.
- **Salida:** Al registrar una transacción tipo "Salida", se resta la cantidad de la existencia.
- Implementado en `MovimientosInventarioController.cs` en el método `PostMovimiento`.

### R5: Validaciones y Notificaciones ✅
- **Validación de Suficiencia:** Si al registrar una salida el inventario quedaría negativo, se rechaza la transacción con mensaje de error.
- **Notificación de Mínimo:** Si la transacción deja el inventario en o por debajo de `MinimoExistencia` (y es > 0), se retorna una notificación de advertencia en la respuesta.
- **Bloqueo en Cero:** Si `MinimoExistencia = 0`, se activa como límite mínimo absoluto (no permite negativo).
- Implementado en `MovimientosInventarioController.cs`.

## Cambios en Archivos

### 1. `Models/Producto.cs`
```csharp
// Añadido campo:
[Required]
public int MinimoExistencia { get; set; } = 0;
```

### 2. `DatabaseSetup.sql`
- Agregada columna `MinimoExistencia INT NOT NULL DEFAULT 0` a tabla `Productos`.
- Actualizado SP `InsertProducto` para incluir `@MinimoExistencia`.
- Actualizado SP `UpdateProducto` para incluir `@MinimoExistencia`.

### 3. `Controllers/ProductosController.cs`
- Actualizado `PostProducto` para pasar `MinimoExistencia` al SP.
- Actualizado `PutProducto` para pasar `MinimoExistencia` al SP.

### 4. `Controllers/MovimientosInventarioController.cs` (NUEVO)
- Controlador completo para gestionar movimientos de inventario.
- Endpoints:
  - `GET /api/MovimientosInventario` — Listar todos los movimientos.
  - `GET /api/MovimientosInventario/{id}` — Obtener un movimiento específico.
  - `GET /api/MovimientosInventario/por-producto/{idProducto}` — Listar movimientos de un producto.
  - `POST /api/MovimientosInventario` — Crear movimiento (aplica lógica R4 y R5).
  - `PUT /api/MovimientosInventario/{id}` — Actualizar movimiento.
  - `DELETE /api/MovimientosInventario/{id}` — Eliminar movimiento.

## Ejemplo de Uso

### 1. Crear un Producto con MinimoExistencia
```json
POST /api/productos
{
  "nombre": "Laptop",
  "descripcion": "Laptop de negocios",
  "precioVenta": 1500.00,
  "minimoExistencia": 5
}
```

### 2. Registrar una Entrada de Inventario
```json
POST /api/movimientosInventario
{
  "idProducto": 1,
  "cantidad": 20,
  "idTipoMovimiento": 1
}
```
**Respuesta:**
```json
{
  "movimiento": { ... },
  "inventarioActual": 20,
  "notificacion": null
}
```

### 3. Registrar una Salida de Inventario (Advertencia)
```json
POST /api/movimientosInventario
{
  "idProducto": 1,
  "cantidad": 16,
  "idTipoMovimiento": 2
}
```
**Respuesta (inventario = 4, mínimo = 5):**
```json
{
  "movimiento": { ... },
  "inventarioActual": 4,
  "notificacion": "⚠️ ADVERTENCIA: Inventario por debajo del mínimo. Existencia actual: 4, Mínimo requerido: 5"
}
```

### 4. Intentar Salida que Deje Inventario Negativo
```json
POST /api/movimientosInventario
{
  "idProducto": 1,
  "cantidad": 10,
  "idTipoMovimiento": 2
}
```
**Respuesta (ERROR - 400 Bad Request):**
```json
{
  "mensaje": "No hay suficiente inventario para realizar esta transacción de salida."
}
```

## Pruebas Sugeridas

1. Crear varios productos con diferentes `MinimoExistencia`.
2. Registrar entradas y salidas, verificar que:
   - Las existencias se actualizan correctamente.
   - Las advertencias se muestran al alcanzar el mínimo.
   - Las salidas se rechazan si dejarían el inventario negativo.
3. Usar Swagger en `http://localhost:5000/swagger` para probar interactivamente.

## Próximos Pasos

- Si es necesario, crear endpoint para listar inventario actual (estado).
- Considerar agregar reportes de movimientos por rango de fechas.
- Implementar autenticación/autorización si aplica.
