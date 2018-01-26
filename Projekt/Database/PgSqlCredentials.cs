
namespace Database
{
    public static class PgSqlCredentials
    {
        public static readonly string Host = "127.0.0.1";
        public static readonly string Port = "5432";
        public static readonly string User = "postgres";
        //public static readonly string Pass = "admin";
        public static readonly string Pass = "mk";
        public static readonly string DataBaseName = "GeoSus";

        public static readonly string RoadsTable = "\"Projekt\".\"Roads\"";
        public static readonly string BuildingsTable = "\"Projekt\".\"BuildingsB\"";
        public static readonly string ResultTable = "\"Projekt\".\"Result\"";

        public static readonly string ConnectionString =
                                      $"Server={Host};" +
                                      $"Port={Port}; " +
                                      $"User Id={User}; " +
                                      $"Password={Pass}; " +
                                      $"Database={DataBaseName};";
    }
}