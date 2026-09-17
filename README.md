# Laboratorio 05 NeptunoDB

Aplicación WPF desarrollada con MVVM, ADO.NET y procedimientos almacenados para administrar productos, categorías, proveedores y pedidos en la base de datos NeptunoDB.

## Solución implementada

### Base de datos

Se agregó el campo `Activo` de tipo `BIT` en las tablas `Productos`, `Categorias`, `Proveedores` y `Pedidos`. El campo se crea con valor predeterminado `1`, por lo que los nuevos registros quedan activos.

Los scripts de la base de datos se encuentran en la carpeta `database`:

- `01-NeptunoDB.sql`: crea las tablas y carga los datos iniciales.
- `03-Lab05-Activo.sql`: agrega el campo `Activo` a las tablas existentes.
- `02-Procedimientos.sql`: crea los procedimientos almacenados del laboratorio.

### Operaciones de escritura

El acceso a datos se concentra en `Semana04-XS/Data/NeptunoRepository.cs`. Las operaciones de alta, actualización y baja crean comandos parametrizados y ejecutan `ExecuteNonQueryAsync`.

Las operaciones implementadas son:

- Alta, actualización y baja lógica de productos.
- Alta, actualización y baja lógica de categorías.
- Alta, actualización y baja lógica de proveedores.
- Alta, actualización y baja lógica de pedidos.

En las altas, el procedimiento almacenado devuelve el nuevo identificador mediante el parámetro de salida `@NuevoID`.

### Eliminación lógica

Los procedimientos de eliminación no utilizan `DELETE`. Actualizan el campo `Activo` a `0`:

```sql
UPDATE dbo.Productos SET Activo = 0
WHERE ProductoID = @ProductoID;
```

Este mismo criterio se aplica a categorías, proveedores y pedidos. Los registros permanecen almacenados para conservar la información histórica.

Los procedimientos de listado y búsqueda utilizan `WHERE Activo = 1`. De esta manera, los registros inactivos no aparecen en la aplicación ni en las consultas del reporte.

### Proveedores y reporte

Los proveedores se pueden buscar mediante filtros parciales por `NombreContacto` y `Ciudad`. La búsqueda solo devuelve proveedores activos.

El reporte de detalles recibe una fecha inicial y una fecha final. Utiliza un `INNER JOIN` entre `DetallePedidos`, `Pedidos` y `Productos`, y excluye los pedidos cuyo campo `Activo` sea `0`.

### Aplicación WPF

La interfaz contiene pestañas para productos, categorías, proveedores, pedidos y reporte por fechas. Cada pantalla está enlazada al `MainViewModel`, que coordina los comandos, validaciones, operaciones de escritura y actualización de las tablas.

La aplicación utiliza una interfaz morada con mensajes de estado y validación para evitar operaciones cuando no se ha seleccionado un registro.
