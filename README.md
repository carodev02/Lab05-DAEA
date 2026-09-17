# Laboratorio 05 NeptunoDB

Aplicación WPF con MVVM, ADO.NET y procedimientos almacenados para productos, categorías, proveedores y pedidos. Incluye búsqueda de proveedores y reporte de detalles por fechas.

## Estructura

- `Semana04-XS/Models`: entidades.
- `Semana04-XS/Data`: configuración, contrato y repositorio ADO.NET.
- `Semana04-XS/ViewModels`: estado y comandos.
- `Semana04-XS/Themes`: recursos visuales compartidos.
- `database`: creación, datos iniciales y procedimientos.
- `.github/workflows`: compilación automática en Windows.

## Ejecutar en Windows

1. Instalar Visual Studio con **Desarrollo de escritorio de .NET** y SQL Server LocalDB.
2. Ejecutar `database/01-NeptunoDB.sql`, `database/03-Lab05-Activo.sql` y después `database/02-Procedimientos.sql`.
3. La configuración predeterminada usa `SQLEXPRESS`. Para otra instancia:

```powershell
$env:NEPTUNO_CONNECTION_STRING="Server=SERVIDOR;Database=NeptunoDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

4. Abrir `Semana04-XS.slnx`, restaurar paquetes y ejecutar.

## Contribuir desde macOS

WPF no se ejecuta en macOS, pero `EnableWindowsTargeting` permite compilar para detectar errores. Instala Git y .NET 8 SDK:

```bash
git clone https://github.com/carodev02/Laboratorio-04---DAEA.git
cd Laboratorio-04---DAEA
git switch -c feature/nombre-del-cambio
chmod +x scripts/check-mac.sh
./scripts/check-mac.sh
```

Si no tienes acceso de escritura, crea un fork y configura los remotos:

```bash
git remote rename origin upstream
git remote add origin https://github.com/TU_USUARIO/Laboratorio-04---DAEA.git
git push -u origin feature/nombre-del-cambio
```

Abre un Pull Request hacia `carodev02/Laboratorio-04---DAEA:master`. Para actualizar tu rama:

```bash
git fetch upstream
git rebase upstream/master
```

## División del equipo

- macOS: SQL, modelos, repositorio ADO.NET, validaciones, documentación y revisión.
- Windows: SQL Server, ejecución WPF, pruebas integrales y capturas.

No suban contraseñas ni conexiones personales. Usen `NEPTUNO_CONNECTION_STRING`.

## Alcance implementado

- CRUD de productos, categorías, proveedores y pedidos con `ExecuteNonQuery`.
- Bajas lógicas mediante el campo `Activo`; ningún botón de eliminar ejecuta `DELETE`.
- Proveedores filtrados por nombre de contacto y ciudad.
- Detalles unidos con pedidos y productos, filtrados entre fechas.

SQL Server protege las relaciones al eliminar categorías, proveedores o productos en uso. Al eliminar un pedido, el procedimiento elimina primero sus detalles.
