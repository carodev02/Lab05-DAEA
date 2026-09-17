using System.Data;
using Microsoft.Data.SqlClient;
using Semana04_XS.Models;

namespace Semana04_XS.Data;

public sealed class NeptunoRepository : INeptunoRepository
{
    private readonly string _connectionString;
    public NeptunoRepository(string connectionString) => _connectionString = connectionString;

    private SqlConnection Connection() => new(_connectionString);
    private static object Db(object? value) => value ?? DBNull.Value;
    private static void Add(SqlCommand cmd, string name, SqlDbType type, object? value, int size = 0)
    {
        var parameter = size == 0 ? cmd.Parameters.Add(name, type) : cmd.Parameters.Add(name, type, size);
        parameter.Value = Db(value);
    }

    private async Task<List<T>> QueryAsync<T>(string procedure, Action<SqlCommand>? parameters, Func<SqlDataReader, T> map)
    {
        await using var connection = Connection();
        await using var command = new SqlCommand(procedure, connection) { CommandType = CommandType.StoredProcedure };
        parameters?.Invoke(command);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        var result = new List<T>();
        while (await reader.ReadAsync()) result.Add(map(reader));
        return result;
    }

    private async Task<int> InsertAsync(string procedure, Action<SqlCommand> parameters)
    {
        await using var connection = Connection();
        await using var command = new SqlCommand(procedure, connection) { CommandType = CommandType.StoredProcedure };
        parameters(command);
        var newId = command.Parameters.Add("@NuevoID", SqlDbType.Int);
        newId.Direction = ParameterDirection.Output;
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        return newId.Value == DBNull.Value ? 0 : Convert.ToInt32(newId.Value);
    }

    private async Task ExecuteAsync(string procedure, Action<SqlCommand> parameters)
    {
        await using var connection = Connection();
        await using var command = new SqlCommand(procedure, connection) { CommandType = CommandType.StoredProcedure };
        parameters(command);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public Task<List<Producto>> ListarProductosAsync() => QueryAsync("dbo.usp_Producto_Listar", null, r => new Producto
    {
        ProductoID = r.GetInt32(r.GetOrdinal("ProductoID")),
        NombreProducto = r.GetString(r.GetOrdinal("NombreProducto")),
        ProveedorID = r.IsDBNull(r.GetOrdinal("ProveedorID")) ? null : r.GetInt32(r.GetOrdinal("ProveedorID")),
        CategoriaID = r.IsDBNull(r.GetOrdinal("CategoriaID")) ? null : r.GetInt32(r.GetOrdinal("CategoriaID")),
        CantidadPorUnidad = r.IsDBNull(r.GetOrdinal("CantidadPorUnidad")) ? null : r.GetString(r.GetOrdinal("CantidadPorUnidad")),
        PrecioUnidad = r.GetDecimal(r.GetOrdinal("PrecioUnidad")),
        UnidadesEnExistencia = r.GetInt16(r.GetOrdinal("UnidadesEnExistencia")),
        UnidadesEnPedido = r.GetInt16(r.GetOrdinal("UnidadesEnPedido")),
        NivelDeReorden = r.GetInt16(r.GetOrdinal("NivelDeReorden")),
        Descontinuado = r.GetBoolean(r.GetOrdinal("Descontinuado")), Activo = r.GetBoolean(r.GetOrdinal("Activo"))
    });

    public Task<int> CrearProductoAsync(Producto x) => InsertAsync("dbo.usp_Producto_Crear", c => ProductoParams(c, x, false));
    public Task ActualizarProductoAsync(Producto x) => ExecuteAsync("dbo.usp_Producto_Actualizar", c => ProductoParams(c, x, true));
    public Task EliminarProductoAsync(int id) => ExecuteAsync("dbo.usp_Producto_Eliminar", c => Add(c, "@ProductoID", SqlDbType.Int, id));
    private static void ProductoParams(SqlCommand c, Producto x, bool id)
    {
        if (id) Add(c, "@ProductoID", SqlDbType.Int, x.ProductoID);
        Add(c, "@NombreProducto", SqlDbType.NVarChar, x.NombreProducto, 60);
        Add(c, "@ProveedorID", SqlDbType.Int, x.ProveedorID);
        Add(c, "@CategoriaID", SqlDbType.Int, x.CategoriaID);
        Add(c, "@CantidadPorUnidad", SqlDbType.NVarChar, x.CantidadPorUnidad, 30);
        Add(c, "@PrecioUnidad", SqlDbType.Decimal, x.PrecioUnidad);
        Add(c, "@UnidadesEnExistencia", SqlDbType.SmallInt, x.UnidadesEnExistencia);
        Add(c, "@UnidadesEnPedido", SqlDbType.SmallInt, x.UnidadesEnPedido);
        Add(c, "@NivelDeReorden", SqlDbType.SmallInt, x.NivelDeReorden);
        Add(c, "@Descontinuado", SqlDbType.Bit, x.Descontinuado);
    }

    public Task<List<Categoria>> ListarCategoriasAsync() => QueryAsync("dbo.usp_Categoria_Listar", null, r => new Categoria
    {
        CategoriaID = r.GetInt32(r.GetOrdinal("CategoriaID")),
        NombreCategoria = r.GetString(r.GetOrdinal("NombreCategoria")),
        Descripcion = r.IsDBNull(r.GetOrdinal("Descripcion")) ? null : r.GetString(r.GetOrdinal("Descripcion")), Activo = r.GetBoolean(r.GetOrdinal("Activo"))
    });
    public Task<int> CrearCategoriaAsync(Categoria x) => InsertAsync("dbo.usp_Categoria_Crear", c => CategoriaParams(c, x, false));
    public Task ActualizarCategoriaAsync(Categoria x) => ExecuteAsync("dbo.usp_Categoria_Actualizar", c => CategoriaParams(c, x, true));
    public Task EliminarCategoriaAsync(int id) => ExecuteAsync("dbo.usp_Categoria_Eliminar", c => Add(c, "@CategoriaID", SqlDbType.Int, id));
    private static void CategoriaParams(SqlCommand c, Categoria x, bool id)
    {
        if (id) Add(c, "@CategoriaID", SqlDbType.Int, x.CategoriaID);
        Add(c, "@NombreCategoria", SqlDbType.NVarChar, x.NombreCategoria, 30);
        Add(c, "@Descripcion", SqlDbType.NVarChar, x.Descripcion, 200);
    }

    public Task<List<Proveedor>> ListarProveedoresAsync(string? contacto = null, string? ciudad = null) =>
        QueryAsync("dbo.usp_Proveedor_Buscar", c =>
        {
            Add(c, "@NombreContacto", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(contacto) ? null : contacto, 40);
            Add(c, "@Ciudad", SqlDbType.NVarChar, string.IsNullOrWhiteSpace(ciudad) ? null : ciudad, 30);
        }, MapProveedor);
    private static Proveedor MapProveedor(SqlDataReader r) => new()
    {
        ProveedorID = r.GetInt32(r.GetOrdinal("ProveedorID")), CompaniaNombre = r.GetString(r.GetOrdinal("CompaniaNombre")), Activo = r.GetBoolean(r.GetOrdinal("Activo")),
        NombreContacto = Str(r, "NombreContacto"), CargoContacto = Str(r, "CargoContacto"), Direccion = Str(r, "Direccion"),
        Ciudad = Str(r, "Ciudad"), CodigoPostal = Str(r, "CodigoPostal"), Pais = Str(r, "Pais"), Telefono = Str(r, "Telefono"), Fax = Str(r, "Fax")
    };
    private static string? Str(SqlDataReader r, string n) => r.IsDBNull(r.GetOrdinal(n)) ? null : r.GetString(r.GetOrdinal(n));
    public Task<int> CrearProveedorAsync(Proveedor x) => InsertAsync("dbo.usp_Proveedor_Crear", c => ProveedorParams(c, x, false));
    public Task ActualizarProveedorAsync(Proveedor x) => ExecuteAsync("dbo.usp_Proveedor_Actualizar", c => ProveedorParams(c, x, true));
    public Task EliminarProveedorAsync(int id) => ExecuteAsync("dbo.usp_Proveedor_Eliminar", c => Add(c, "@ProveedorID", SqlDbType.Int, id));
    private static void ProveedorParams(SqlCommand c, Proveedor x, bool id)
    {
        if (id) Add(c, "@ProveedorID", SqlDbType.Int, x.ProveedorID);
        Add(c, "@CompaniaNombre", SqlDbType.NVarChar, x.CompaniaNombre, 60); Add(c, "@NombreContacto", SqlDbType.NVarChar, x.NombreContacto, 40);
        Add(c, "@CargoContacto", SqlDbType.NVarChar, x.CargoContacto, 40); Add(c, "@Direccion", SqlDbType.NVarChar, x.Direccion, 80);
        Add(c, "@Ciudad", SqlDbType.NVarChar, x.Ciudad, 30); Add(c, "@CodigoPostal", SqlDbType.NVarChar, x.CodigoPostal, 10);
        Add(c, "@Pais", SqlDbType.NVarChar, x.Pais, 30); Add(c, "@Telefono", SqlDbType.NVarChar, x.Telefono, 24); Add(c, "@Fax", SqlDbType.NVarChar, x.Fax, 24);
    }

    public Task<List<Pedido>> ListarPedidosAsync() => QueryAsync("dbo.usp_Pedido_Listar", null, r => new Pedido
    {
        PedidoID = r.GetInt32(r.GetOrdinal("PedidoID")), ClienteID = NullableInt(r, "ClienteID"), EmpleadoID = NullableInt(r, "EmpleadoID"),
        FechaPedido = r.GetDateTime(r.GetOrdinal("FechaPedido")), FechaRequerida = NullableDate(r, "FechaRequerida"), FechaEnvio = NullableDate(r, "FechaEnvio"),
        TransportistaID = NullableInt(r, "TransportistaID"), Destinatario = Str(r, "Destinatario"), CiudadDestino = Str(r, "CiudadDestino"), PaisDestino = Str(r, "PaisDestino"), Activo = r.GetBoolean(r.GetOrdinal("Activo"))
    });
    private static int? NullableInt(SqlDataReader r, string n) => r.IsDBNull(r.GetOrdinal(n)) ? null : r.GetInt32(r.GetOrdinal(n));
    private static DateTime? NullableDate(SqlDataReader r, string n) => r.IsDBNull(r.GetOrdinal(n)) ? null : r.GetDateTime(r.GetOrdinal(n));
    public Task<int> CrearPedidoAsync(Pedido x) => InsertAsync("dbo.usp_Pedido_Crear", c => PedidoParams(c, x, false));
    public Task ActualizarPedidoAsync(Pedido x) => ExecuteAsync("dbo.usp_Pedido_Actualizar", c => PedidoParams(c, x, true));
    public Task EliminarPedidoAsync(int id) => ExecuteAsync("dbo.usp_Pedido_Eliminar", c => Add(c, "@PedidoID", SqlDbType.Int, id));
    private static void PedidoParams(SqlCommand c, Pedido x, bool id)
    {
        if (id) Add(c, "@PedidoID", SqlDbType.Int, x.PedidoID);
        Add(c, "@ClienteID", SqlDbType.Int, x.ClienteID); Add(c, "@EmpleadoID", SqlDbType.Int, x.EmpleadoID);
        Add(c, "@FechaPedido", SqlDbType.Date, x.FechaPedido); Add(c, "@FechaRequerida", SqlDbType.Date, x.FechaRequerida);
        Add(c, "@FechaEnvio", SqlDbType.Date, x.FechaEnvio); Add(c, "@TransportistaID", SqlDbType.Int, x.TransportistaID);
        Add(c, "@Destinatario", SqlDbType.NVarChar, x.Destinatario, 60); Add(c, "@CiudadDestino", SqlDbType.NVarChar, x.CiudadDestino, 30);
        Add(c, "@PaisDestino", SqlDbType.NVarChar, x.PaisDestino, 30);
    }

    public Task<List<DetallePedidoReporte>> ReporteDetallesAsync(DateTime desde, DateTime hasta) =>
        QueryAsync("dbo.usp_DetallePedido_ListarPorFechas", c => { Add(c, "@FechaDesde", SqlDbType.Date, desde); Add(c, "@FechaHasta", SqlDbType.Date, hasta); }, r => new DetallePedidoReporte
        {
            PedidoID = r.GetInt32(r.GetOrdinal("PedidoID")), FechaPedido = r.GetDateTime(r.GetOrdinal("FechaPedido")),
            Destinatario = Str(r, "Destinatario"), ProductoID = r.GetInt32(r.GetOrdinal("ProductoID")),
            NombreProducto = r.GetString(r.GetOrdinal("NombreProducto")), PrecioUnidad = r.GetDecimal(r.GetOrdinal("PrecioUnidad")),
            Cantidad = r.GetInt16(r.GetOrdinal("Cantidad")), Descuento = r.GetDecimal(r.GetOrdinal("Descuento")), Total = r.GetDecimal(r.GetOrdinal("Total"))
        });
}
