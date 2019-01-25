namespace SqlShip.Interfaces
{
    public interface IUpdaterConfig
    {
        string UpdateUrl { get; set; }
        double UpdateIntervalCheckMinutes { get; set; }
    }
}