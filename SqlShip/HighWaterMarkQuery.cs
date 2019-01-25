using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Timers;
using Newtonsoft.Json;
using NPoco;
using SqlShip.Interfaces;

namespace SqlShip
{
    public sealed class HighWaterMarkQuery
    {
        public string ConnectionString { get; set; }
        public string Query {get; set; }
        public int EveryNSeconds { get; set; }
        public string HighWaterMark { get; set; }
        public Type HighWaterMarkType { get; set; }
        public string HighWaterMarkColumn { get; set; }
        public string ApiUserName { get; set; }
        public string ApiPassword { get; set; }
        public string TargetUrl { get; set; }

        public event EventHandler ValueChanged;

        private readonly Timer _timer = new Timer();
        private ILogger _logger;

        public void Start(ILogger logger)
        {
            _logger = logger;
            _timer.Interval = TimeSpan.FromSeconds(EveryNSeconds).TotalMilliseconds;
            _timer.AutoReset = false;
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                using (var con = new SqlConnection(ConnectionString))
                {
                    if (con.State != ConnectionState.Open)
                        con.Open();
                    using (IDatabase database = new Database(con))
                    {
                        var highWaterMark = Convert.ChangeType(HighWaterMark, HighWaterMarkType);
                        var results = database.Fetch<Dictionary<string, object>>(Query, highWaterMark);
                        var newHighWaterMark = highWaterMark;
                        foreach (var result in results)
                        {
                            newHighWaterMark = result[HighWaterMarkColumn];
                            if (PostData(result)) continue;
                            _logger.Error($"There was an error while posting the data.");
                            break;
                        }

                        if (HighWaterMark == newHighWaterMark.ToString()) return;
                        HighWaterMark = newHighWaterMark.ToString();
                        OnValueChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                OnValueChanged();
                _logger.Error(ex, $"Error while running query.");
            }
            finally
            {
                _timer.Start();
            }
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private readonly HttpClient _client = new HttpClient();
        private static readonly Encoding Ascii = new ASCIIEncoding();

        
        private bool PostData(Dictionary<string, object> data)
        {
            var jsonObject = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonObject, Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Ascii.GetBytes($"{ApiUserName}:{ApiPassword}")));
            var result = _client.PostAsync(TargetUrl, content).Result;
            if (!result.IsSuccessStatusCode)
            {
                _logger.Error($"Received {result.StatusCode} while posting data to URL {TargetUrl}. {result.Content.ReadAsStringAsync().Result}");
            }
            return result.IsSuccessStatusCode;
        }

    }
}
