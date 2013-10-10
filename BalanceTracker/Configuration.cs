using System.Configuration;

namespace BalanceTracker
{
    public class Configuration
    {
        public string UserName
        {
            get { return ConfigurationManager.AppSettings["username"]; }
        }

        public string Password
        {
            get { return ConfigurationManager.AppSettings["password"]; }
        }
    }
}