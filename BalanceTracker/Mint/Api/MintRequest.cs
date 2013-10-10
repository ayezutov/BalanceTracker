using System;
using Newtonsoft.Json;

namespace BalanceTracker.Mint.Api
{
    public class MintRequest
    {
        static Random random = new Random();

        public MintRequest()
        {
            Id = random.Next(100000, 999999).ToString();
        }

        [JsonProperty("args")]
        public object Args { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("task")]
        public string Task { get; set; }
    }
}