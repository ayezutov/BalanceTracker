using System.Net;

namespace BalanceTracker.Http
{
    public class HttpSession
    {
        public CookieContainer Cookies { get; private set; }

        protected HttpSession()
        {
            Cookies = new CookieContainer();
        }
    }
}