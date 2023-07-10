using LinqToDB.Configuration;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
namespace NeptuneEvo.Database
{
    class DatabaseSettings : ILinqToDBSettings
    {
        public IEnumerable<IDataProviderSettings> DataProviders => Enumerable.Empty<IDataProviderSettings>();

        public string DefaultConfiguration => "MySqlConnector";
        public string DefaultDataProvider => "MySqlConnector";

        private readonly IConnectionStringSettings[] _connectionStrings;

        public DatabaseSettings(IConnectionStringSettings[] connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }

        public IEnumerable<IConnectionStringSettings> ConnectionStrings => _connectionStrings;
    }
}
