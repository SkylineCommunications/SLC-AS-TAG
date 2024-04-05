namespace GQI_TAG_GetEndpoints_1.RealTimeUpdates
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Protobuf.Shared.IdObjects.v1;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class DataProvider : IDisposable
    {
        private readonly int _dataminerId;
        private readonly int _elementId;

        private readonly GQIDMS _gqiDms;
        private readonly Connection _connection;

        public DataProvider(Connection connection, GQIDMS gqiDms, int dataminerId, int elementId)
        {
            _connection = connection;
            _dataminerId = dataminerId;
            _elementId = elementId;
            _gqiDms = gqiDms;

            InstantiateCache();
        }

        public ElementTableCache AllLayoutsMcsTable { get; private set; }

        private void InstantiateCache()
        {
            if (_connection == null)
            {
                throw new ArgumentNullException(nameof(_connection));
            }

            AllLayoutsMcsTable = new ElementTableCache(_connection, _gqiDms, _dataminerId, _elementId, 5600, "1");
        }

        public void Dispose()
        {
            AllLayoutsMcsTable?.Dispose();
        }
    }
}
