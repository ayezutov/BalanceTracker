using System;
using System.Collections.Specialized;

namespace BalanceTracker.Mint.Workflow
{
    public class LoginInformation
    {
        public string Form { get; set; }

        public Uri LoginPageUri { get; set; }

        public Uri LoginPostUri { get; set; }

        public NameValueCollection Inputs { get; set; }

        public string Action { get; set; }
    }
}