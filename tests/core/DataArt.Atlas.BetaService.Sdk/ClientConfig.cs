using System;
using DataArt.Atlas.Infrastructure.Interfaces;

namespace DataArt.Atlas.BetaService.Sdk
{
    public class ClientConfig : ISdkConfig
    {
        public static string ServiceKey = "Beta";

        public string Key => ServiceKey;

        public Type[] Clients => new[]
        {
            typeof(Client)
        };
    }
}
