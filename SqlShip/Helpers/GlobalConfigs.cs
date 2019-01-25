namespace SqlShip.Helpers
{
    public enum ProcessState
    {
        Unknown,
        Console,
        Service,
        WinForm
    }
    public static class GlobalConfigs
    {
        public static ProcessState ProcessState { get; set; } = ProcessState.Unknown;
    }
}