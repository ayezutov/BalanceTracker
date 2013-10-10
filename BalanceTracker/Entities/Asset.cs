using System;

namespace BalanceTracker.Entities
{
    public class Asset
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public AssetFinancialDetails Financial { get; set; }

        public DateTime? DueDate { get; set; }
    }
}