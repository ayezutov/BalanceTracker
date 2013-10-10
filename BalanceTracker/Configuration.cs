using System.Configuration;

namespace BalanceTracker
{
    public class Configuration
    {
        public string MintUserName
        {
            get { return ConfigurationManager.AppSettings["mint.username"]; }
        }

        public string MintPassword
        {
            get { return ConfigurationManager.AppSettings["mint.password"]; }
        }

        public string TwilioAccountSid
        {
            get { return ConfigurationManager.AppSettings["twilio.accountsid"]; }
        }

        public string TwilioAuthToken
        {
            get { return ConfigurationManager.AppSettings["twilio.authtoken"]; }
        }

        public string TwilioSendFrom
        {
            get { return ConfigurationManager.AppSettings["twilio.number.from"]; }
        }

        public string TwilioSendTo
        {
            get { return ConfigurationManager.AppSettings["twilio.number.to"]; }
        }
    }
}