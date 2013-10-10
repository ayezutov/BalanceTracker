namespace BalanceTracker.Mint.Entities
{
    public class UserStatus
    {
        public bool IsNew { get; set; } 

        public bool IsRefreshing { get; set; } 

        public int ErrorCount { get; set; } 

    }
}