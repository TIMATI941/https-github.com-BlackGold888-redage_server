using LinqToDB.Configuration;
using System;
using MySqlConnector;

namespace NeptuneEvo.Database
{
    class ConnectionInfo: IConnectionStringSettings
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public string ProviderName { get; set; }
        public bool IsGlobal => false;

        public ConnectionInfo(string connectionName, string host, string user, string password, string database, string port)
        {
            this.Name = connectionName;
            this.ProviderName = "MySqlConnector";
            this.ConnectionString = $"SERVER={host};DATABASE={database};UID={user};PASSWORD={password};Port={port};SSLMode=none;";
        }
    }
}
