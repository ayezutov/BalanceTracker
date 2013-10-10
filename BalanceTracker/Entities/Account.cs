using System.Collections.Generic;
using System.Linq;

namespace BalanceTracker.Entities
{
    public class Account
    {
        public Account()
        {
            Assets = new List<Asset>();
        }

        public IList<Asset> Assets { get; set; }

        public double EffectiveBalance
        {
            get { return Assets.Sum(a => a.Financial.AbsoluteAmount); }
        }
    }
}