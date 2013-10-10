using System;
using BalanceTracker.Http;
using BalanceTracker.Mint.Workflow;

namespace BalanceTracker.Mint
{
    public class MintSession : HttpSession
    {
        private readonly Random random;

        protected MintSession()
        {
            random = new Random();
        }

        public string Token { get; set; }

        public static MintSession Start(string user, string password)
        {
            var mintSession = new MintSession();
            new LoginWorkflow(mintSession).Login(user, password);
            return mintSession;
        }

        public int GetRandomHundredThousands()
        {
            return random.Next(100000, 1000000);
        }
    }
}