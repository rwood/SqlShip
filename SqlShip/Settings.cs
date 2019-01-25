using SqlShip.Interfaces;

namespace SqlShip
{
    public class Settings : IUpdaterConfig
    {
        public string UpdateUrl { get; set; } = "http://tamarau.com/SqlShip/";
        public double UpdateIntervalCheckMinutes { get; set; } = 5;
    }
}