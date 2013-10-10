using BalanceTracker.Mint;

namespace BalanceTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new Configuration();
            var session = MintSession.Start(config.UserName, config.Password);

            var accountTask = new MintRepository(session).UpdateAccount();

            var account = accountTask.Result;
            
        }
    }
}
