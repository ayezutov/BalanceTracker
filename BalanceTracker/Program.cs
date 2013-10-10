using System;
using BalanceTracker.Mint;
using Twilio;

namespace BalanceTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new Configuration();
            var session = MintSession.Start(config.MintUserName, config.MintPassword);

            var accountTask = new MintRepository(session).UpdateAccount();

            var account = accountTask.Result;

            var twilio = new TwilioRestClient(config.TwilioAccountSid, config.TwilioAuthToken);
            var message = twilio.SendSmsMessage(config.TwilioSendFrom, config.TwilioSendTo, string.Format("Your current balance is {0:C}", account.EffectiveBalance));
        }
    }
}
