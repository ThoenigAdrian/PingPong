using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ClientSourcePackages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stresstester
{
    class ScenarioProvider
    {
        public List<List<PackageInterface>> Scenarios = new List<List<PackageInterface>>();
        public ScenarioProvider()
        {
            List<PackageInterface>  ConnectOnly = new List<PackageInterface>();
            Scenarios.Add(ConnectOnly);

            List<PackageInterface> SessionRequest = new List<PackageInterface>();
            ClientSessionRequest sessionRequest = new ClientSessionRequest();
            sessionRequest.Reconnect = false;
            sessionRequest.ReconnectSessionID = 0;
            SessionRequest.Add(sessionRequest);
            Scenarios.Add(SessionRequest);

            List<PackageInterface> ReconnectWithPreviousSession = new List<PackageInterface>();
            ReconnectWithPreviousSession.Add(sessionRequest);
            Scenarios.Add(ReconnectWithPreviousSession);

        }
    }
}
