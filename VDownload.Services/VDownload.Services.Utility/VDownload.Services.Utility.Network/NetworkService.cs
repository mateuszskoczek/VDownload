using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace VDownload.Services.Utility.Network
{
    public interface INetworkService
    {
        bool IsMetered { get; }
    }



    public class NetworkService : INetworkService
    {
        #region FIELDS

        protected readonly IEnumerable<NetworkCostType> _notMeteredTypes = [
            NetworkCostType.Unknown,
            NetworkCostType.Unrestricted
        ];

        #endregion



        #region PROPERTIES

        public bool IsMetered => !_notMeteredTypes.Contains(NetworkInformation.GetInternetConnectionProfile().GetConnectionCost().NetworkCostType);

        #endregion
    }
}
