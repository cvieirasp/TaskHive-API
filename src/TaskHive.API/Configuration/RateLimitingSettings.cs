namespace TaskHive.API.Configuration;

public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";
    
    public GlobalSettings Global { get; set; } = new();
    public PolicySettings SignInPolicy { get; set; } = new();
    public int DefaultRetryAfterSeconds { get; set; } = 5;

    public class GlobalSettings
    {
        public int PermitLimit { get; set; } = 100;
        public int WindowMinutes { get; set; } = 1;
    }

    public class PolicySettings
    {
        public int PermitLimit { get; set; } = 5;
        public int WindowMinutes { get; set; } = 15;
    }
} 