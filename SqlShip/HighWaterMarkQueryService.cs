using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Newtonsoft.Json;
using NPoco;
using SqlShip.Helpers;
using SqlShip.Interfaces;

namespace SqlShip
{
    public class HighWaterMarkQueryService
    {
        private readonly ILogger _logger;
        private readonly List<HighWaterMarkQuery> _configs;
        private readonly FileInfo _configFile;
        private readonly object _syncRoot = new object();


        public HighWaterMarkQueryService(ILogger logger)
        {
            _logger = logger;
            _configFile = new FileInfo(Path.Combine(WellKnown.GetDataDirectory(), "queries.json"));
            if (!_configFile.Exists)
            {
                var example = new HighWaterMarkQuery()
                {
                    ConnectionString = "Data Source=;Initial Catalog=;User=;Password=;",
                    Query = "SELECT * FROM Table WHERE ID > @0",
                    EveryNSeconds = 60,
                    HighWaterMarkType = typeof(int),
                    HighWaterMark = "0",
                    HighWaterMarkColumn = "ID",
                    ApiUserName = "MyUserName",
                    ApiPassword = "MyPassword"
                };
                var list = new List<HighWaterMarkQuery>() { example };
                File.WriteAllText(_configFile.FullName, JsonConvert.SerializeObject(list, Formatting.Indented));
                _logger.Warning($"No query config found writing example to {_configFile.FullName}.");
                return;
            }

            var queryConfigContents = File.ReadAllText(_configFile.FullName);
            _configs = JsonConvert.DeserializeObject<List<HighWaterMarkQuery>>(queryConfigContents);

        }

        public void Start()
        {
            if (_configs == null) return;
            foreach (var config in _configs)
            {
                try
                {
                    using (var con = new SqlConnection(config.ConnectionString))
                    {
                        if(con.State != ConnectionState.Open)
                            con.Open();
                        using (IDatabase database = new Database(con, DatabaseType.SqlServer2012))
                        {
                            var _ = database.ExecuteScalar<int>("SELECT 1");
                        }
                    }

                    config.ValueChanged += ConfigOnValueChanged;
                    config.Start(_logger);
                }
                catch (SqlException e)
                {
                    _logger.Warning(e, $"Connection string is not valid, skipping this query. {config.Query}");
                }
            }
        }

        

        private void ConfigOnValueChanged(object sender, EventArgs eventArgs)
        {
            lock(_syncRoot)
                File.WriteAllText(_configFile.FullName, JsonConvert.SerializeObject(_configs, Formatting.Indented));
        }

        public void Stop()
        {
            if (_configs == null) return;
            foreach (var highWaterMarkQuery in _configs)
            {
                highWaterMarkQuery.Stop();
            }
        }
    }
}
