using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using BalanceTracker.Entities;
using BalanceTracker.Http;
using BalanceTracker.Mint.Api;
using BalanceTracker.Mint.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BalanceTracker.Mint
{
    public class MintRepository : HttpRepository<HttpSession>
    {
        private readonly MintSession session;

        public MintRepository(MintSession session)
            : base(session)
        {
            this.session = session;
        }

        public async Task<Account> UpdateAccount(bool refresh = true)
        {
            var result = await Task.Run(() =>
                                            {
                                                if (refresh)
                                                {
                                                    GetResponseBody(HttpMethod.Post, 
                                                                    GetUri("refreshFILogins.xevent"), 
                                                                    new NameValueCollection
                                                                        {
                                                                            {
                                                                                "token", 
                                                                                session
                                                                                .Token
                                                                            }
                                                                        });

                                                    var random = new Random();
                                                    UserStatus userStatus;
                                                    do
                                                    {
                                                        var userStatusJson =
                                                            GetResponseBody(HttpMethod.Get, 
                                                                            GetUri(string.Format(
                                                                                "userStatus.xevent?rnd={0}", 
                                                                                random.Next(100000, 99999999))));
                                                        userStatus =
                                                            JsonConvert.DeserializeObject<UserStatus>(userStatusJson);
                                                    }
 while (userStatus.IsRefreshing);
                                                }

                                                return GetAccountInformation();
                                            });
            return result;
        }

        public Account GetAccountInformation()
        {
            var result = PostMintBundledRequest<MintAccountService_getAccountsSorted>(new MintRequest
                                                                                          {
                                                                                              Service =
                                                                                                  "MintAccountService", 
                                                                                              Task = "getAccountsSorted", 
                                                                                              Args = new
                                                                                                         {
                                                                                                             types =
                                                                                                  new[]
                                                                                                      {
                                                                                                          "BANK", 
                                                                                                          "CREDIT", 
                                                                                                          "INVESTMENT", 
                                                                                                          
// "LOAN", 
                                                                                                          // "MORTGAGE", 
                                                                                                          // "OTHER_PROPERTY", 
                                                                                                          // "REAL_ESTATE", 
                                                                                                          // "VEHICLE", 
                                                                                                          // "UNCLASSIFIED"
                                                                                                      }
                                                                                                         }
                                                                                          });

            return ConvertToAccount(result.First());
        }

        private AssetFinancialDetails GetAccountFinancialDetails(int id)
        {
            var json = GetResponseBody(HttpMethod.Get, 
                                       GetUri(
                                           string.Format(
                                               " /listTransaction.xevent?accountId={0}&queryNew=&offset=0&comparableType=8&acctChanged=T&rnd={1}", 
                                               id, session.GetRandomHundredThousands())));

            var details = JsonConvert.DeserializeObject<AccountDetails>(json);

            return ParseAccountDetails(details.accountHeader);
        }

        private static AssetFinancialDetails ParseAccountDetails(string html)
        {
            var doc = XDocument.Parse("<root>" + html + "</root>");
            var table = doc.XPathSelectElement("//table[@class='account']");

            var ths = table.XPathSelectElements("//th").ToList();
            var tds = table.XPathSelectElements("//td").ToList();

            var available = GetTdValue(ths, tds, "Available");
            if (available == null)
                return null;

            return new AssetFinancialDetails
                       {
                           Available = available.Value, 
                           Limit = GetTdValue(ths, tds, "Total Credit")
                       };
        }

        private static double? GetTdValue(List<XElement> ths, List<XElement> tds, string value)
        {
            var index = ths.FindIndex(th => th.Value.Contains(value));

            return index < 0 ? (double?) null : double.Parse(tds[index].Value, NumberStyles.Currency);
        }

        private Account ConvertToAccount(MintAccountService_getAccountsSorted ma)
        {
            return new Account
                       {
                           Assets = ma.Select(a =>
                                                  {
                                                      var financial = GetAccountFinancialDetails(a.accountId);
                                                      DateTime date;
                                                      return new Asset
                                                                 {
                                                                     Id = a.accountId, 
                                                                     Name = a.accountName, 
                                                                     DueDate =
                                                                         DateTime.TryParse(a.dueDate, out date)
                                                                             ? (DateTime?) date
                                                                             : null, 
                                                                     Financial = financial
                                                                 };
                                                  }).ToList()
                       };
        }

        private IEnumerable<TResult> PostMintBundledRequest<TResult>(params MintRequest[] mintRequest)
        {
            var json = JsonConvert.SerializeObject(mintRequest);

            var request = GetHttpRequest(HttpMethod.Post, 
                                         GetUri(string.Format("/bundledServiceController.xevent?token={0}", 
                                                              session.Token)), new NameValueCollection {{"input", json}});

            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string jsonString = reader.ReadToEnd();

                    var obj = (JContainer) JsonConvert.DeserializeObject(jsonString);

                    return mintRequest.Select(mr =>
                                                  {
                                                      var root = obj["response"];
                                                      var value = root[mr.Id].ToString();

                                                      var responseRoot = JsonConvert.DeserializeObject<Response>(value);

                                                      Type type =
                                                          Assembly.GetExecutingAssembly()
                                                                  .GetTypes()
                                                                  .First(t => t.Name.Equals(responseRoot.responseType));

                                                      var result =
                                                          JsonConvert.DeserializeObject(
                                                              responseRoot.response.ToString(), type);

                                                      return result;
                                                  }).Cast<TResult>();
                }
            }
        }

        private Uri GetUri(string relative)
        {
            return new Uri(new Uri("https://wwws.mint.com"), relative);
        }
    }
}