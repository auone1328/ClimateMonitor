namespace Infrastructure
{
    public class CleanupOptions
    {
        public int RetentionDays { get; set; } = 7;
        public int IntervalMinutes { get; set; } = 60;
    }
}
