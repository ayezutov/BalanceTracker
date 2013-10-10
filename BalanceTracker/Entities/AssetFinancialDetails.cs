namespace BalanceTracker.Entities
{
    public class AssetFinancialDetails
    {
        public double Available { get; set; }

        public double? Limit { get; set; }

        public double AbsoluteAmount
        {
            get { return Available - (Limit ?? 0); }
        }
    }
}