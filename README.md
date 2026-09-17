# Laboratorio 05 NeptunoDB

Aplicación de escritorio WPF para administrar productos, categorías, proveedores y pedidos usando MVVM, ADO.NET y procedimientos almacenados. El laboratorio implementa altas, consultas, actualizaciones, bajas lógicas, búsqueda de proveedores y un reporte de detalles de pedidos por fechas.

## Requisitos del laboratorio

- Agregar el campo `Activo` a productos, categorías, proveedores y pedidos.
- Implementar CRUD mediante procedimientos almacenados.
- Ejecutar las operaciones de escritura con `ExecuteNonQueryAsync`.
- Reemplazar la eliminación física por eliminación lógica.
- Buscar proveedores por nombre de contacto y ciudad.
- Consultar detalles de pedidos dentro de un intervalo de fechas.
- Excluir de los listados y reportes los registros con `Activo = 0`.

## Cómo se aplicó el Lab 05

### Base de datos

La base `NeptunoDB` se prepara con estos scripts, en este orden:

1. `database/01-NeptunoDB.sql` crea las tablas y carga los datos iniciales.
2. `database/03-Lab05-Activo.sql` agrega el campo `Activo BIT NOT NULL DEFAULT 1` a las cuatro tablas solicitadas. El script verifica si la columna ya existe para poder ejecutarse sin duplicarla.
3. `database/02-Procedimientos.sql` crea o actualiza los procedimientos almacenados del laboratorio.

### Operaciones de escritura

El archivo `Semana04-XS/Data/NeptunoRepository.cs` centraliza el acceso ADO.NET. Las altas utilizan `InsertAsync` y las actualizaciones y bajas utilizan `ExecuteAsync`. Ambos métodos crean un `SqlCommand`, asignan parámetros, abren la conexión y ejecutan:

```csharp
await command.ExecuteNonQueryAsync();
```

Este patrón se utiliza para productos, categorías, proveedores y pedidos. Las altas reciben el nuevo ID mediante el parámetro de salida `@NuevoID`.

### Eliminación lógica

Los procedimientos `usp_Producto_Eliminar`, `usp_Categoria_Eliminar`, `usp_Proveedor_Eliminar` y `usp_Pedido_Eliminar` no ejecutan `DELETE`. En su lugar, actualizan el registro:

```sql
UPDATE dbo.Productos SET Activo = 0
WHERE ProductoID = @ProductoID;
```

Los procedimientos de listado y búsqueda utilizan `WHERE Activo = 1`. Por eso un registro dado de baja permanece almacenado en la base de datos, pero deja de aparecer en la aplicación. El reporte también excluye los pedidos inactivos mediante la condición `p.Activo = 1`.

### Funcionalidades de la aplicación

- Productos: crear, editar y dar de baja lógicamente.
- Categorías: crear, editar y dar de baja lógicamente.
- Proveedores: crear, editar, dar de baja y buscar por contacto y ciudad.
- Pedidos: crear, editar y dar de baja lógicamente.
- Reporte: consultar detalles de pedidos mediante un rango de fechas e `INNER JOIN` con pedidos y productos.
- Interfaz: WPF con diseño morado, tablas de resultados, mensajes de estado y validación cuando no se selecciona un registro.

## Cómo abrir el proyecto

1. Abrir Visual Studio en Windows.
2. Seleccionar **Archivo > Abrir > Proyecto o solución**.
3. Abrir `Semana04-XS.slnx`.
4. Restaurar los paquetes y presionar `F5` para ejecutar.

La conexión predeterminada utiliza SQL Server LocalDB:

```text
Server=(localdb)\\MSSQLLocalDB;Database=NeptunoDB;Trusted_Connection=True;TrustServerCertificate=True;
```

Para utilizar otra instancia, definir la variable de entorno `NEPTUNO_CONNECTION_STRING` con una cadena de conexión válida.

## Evidencias de prueba

Las evidencias recomendadas son:

1. Producto creado y visible en el listado.
2. Producto actualizado mediante el botón **Guardar**.
3. Producto seleccionado y eliminado desde la aplicación.
4. Consulta SQL que demuestre que el registro permanece con `Activo = 0`.
5. Código C# donde se visualice `ExecuteNonQueryAsync`.
6. Procedimientos SQL que muestren `UPDATE ... SET Activo = 0` y los filtros `WHERE Activo = 1`.
7. Búsqueda de proveedores con los filtros de contacto y ciudad.
8. Reporte generado con fechas y detalles relacionados.

## Estructura principal

- `Semana04-XS/Models`: entidades de la aplicación.
- `Semana04-XS/Data`: configuración, contrato y repositorio ADO.NET.
- `Semana04-XS/ViewModels`: comandos, validaciones y estado de la interfaz.
- `Semana04-XS/Themes`: estilos y colores de la interfaz.
- `database`: scripts de tablas, datos, migración y procedimientos almacenados.
