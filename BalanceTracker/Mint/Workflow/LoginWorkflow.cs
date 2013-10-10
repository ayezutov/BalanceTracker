using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace BalanceTracker.Mint.Workflow
{
    public class LoginWorkflow
    {
        private readonly MintSession session;
        private readonly MintRepository mintRepository;

        public LoginWorkflow(MintSession mintSession)
        {
            session = mintSession;
            mintRepository = new MintRepository(session);
        }

        public void Login(string user, string password)
        {
            var loginInfo =
                GetAndParseLoginPage("https://wwws.mint.com/login.event");

            loginInfo.Inputs.Add("username", user);
            loginInfo.Inputs.Add("password", password);
            loginInfo.Inputs["task"] = "L";

            PerformAuthentication(loginInfo);
        }

        private void PerformAuthentication(LoginInformation loginInfo)
        {
            var postRequest = mintRepository.GetResponseBody(HttpMethod.Post, loginInfo.LoginPostUri, loginInfo.Inputs);

            var match = new Regex(@"<input[^<]+?id=\""javascript-token\"".+?value=\""(?<value>.*?)\"".+?\/\>").Match(postRequest);

            if (!match.Success)
            {
                throw new InvalidProgramException("'javascript-token' was not found in page after log in");
            }

            session.Token = match.Groups["value"].Value;
        }

        private LoginInformation GetAndParseLoginPage(string url)
        {
            var loginInfo = new LoginInformation();
            
            var request = mintRepository.GetHttpRequest(HttpMethod.Get, new Uri(url));

            var response = request.GetResponse();
            var responseBody = new StreamReader(response.GetResponseStream()).ReadToEnd();
            response.Close();

            var formRegex = new Regex("<form .+? id=\"form\\-login\" .+?</form>", RegexOptions.Singleline);

            loginInfo.Form = formRegex.Match(responseBody).Value;
            loginInfo.LoginPageUri = response.ResponseUri;

            var actionRegex = new Regex("<form[^>]+?action=(?:'|\")(?<action>[^'\"]+?)(?:'|\")[^>]+?>");

            var inputsRegex =
                new Regex(
                    "<input[^>]+name=(?:'|\")(?<name>[^'\"]+?)(?:'|\")[^>]+?(?:value=(?:'|\")(?<value>[^'\"]+?)(?:'|\")){0,1}[^>]+?>",
                    RegexOptions.Singleline);

            var pairs = inputsRegex.Matches(loginInfo.Form).Cast<Match>()
                                   .Where(
                                       m =>
                                       !new[]
                                            {
                                                "password", 
                                                "username"
                                            }.Any(x =>
                                                  x.Equals(m.Groups["name"].Value,
                                                           StringComparison.InvariantCultureIgnoreCase)))
                                   .Select(
                                       m =>
                                       new KeyValuePair<string, string>(
                                           m.Groups["name"].Value,
                                           m.Groups["value"].Success
                                                ? m.Groups["value"].Value
                                                : string.Empty)).ToList();

            loginInfo.Inputs = new NameValueCollection();
            foreach (var pair in pairs)
            {
                loginInfo.Inputs.Add(pair.Key, pair.Value);
            }

            loginInfo.Action = actionRegex.Match(loginInfo.Form).Groups["action"].Value;

            Uri postUri;
            if (!Uri.TryCreate(loginInfo.LoginPageUri, loginInfo.Action, out postUri))
            {
                throw new InvalidDataException("Could not construct Uri to post login information");
            }

            loginInfo.LoginPostUri = postUri;

            return loginInfo;
        }
    }
}