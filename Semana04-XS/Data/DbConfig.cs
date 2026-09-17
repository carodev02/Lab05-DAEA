namespace Semana04_XS.Data;

public static class DbConfig
{
    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("NEPTUNO_CONNECTION_STRING")
        ?? @"Server=(localdb)\MSSQLLocalDB;Database=NeptunoDB;Trusted_Connection=True;TrustServerCertificate=True;";
}
