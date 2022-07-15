using DnsClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Remore.WinUI.Helpers
{
    public static class NetworkUtility
    {


        public static (string Address, int Port) GetEndpointForHostname(string hostname)
        {
            var lookupClient = App.GetService<DnsClient.LookupClient>();

            var result = lookupClient.ResolveService(hostname, "Remore", ProtocolType.Tcp);
            var entry = result.FirstOrDefault();
            if (entry is null)
                return new("0.0.0.0", 0);

            if (entry.AddressList.Any())
                return new(entry.AddressList.First().ToString(), entry.Port);

            var entryAddressAnswers = lookupClient.Query(entry.HostName, QueryType.A).Answers;
            if (entryAddressAnswers.Any())
            {
                var aTarget = entryAddressAnswers.ARecords().First();
                return new(aTarget.Address.ToString(), entry.Port);
            }

            entryAddressAnswers = lookupClient.Query(entry.HostName, QueryType.AAAA).Answers;
            if (entryAddressAnswers.Any())
            {
                var aaaaTarget = entryAddressAnswers.AaaaRecords().First();
                return new(aaaaTarget.Address.ToString(), entry.Port);
            }

            return new("0.0.0.0", 0);
        }
    }
}
