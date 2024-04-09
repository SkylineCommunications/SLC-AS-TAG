namespace TAG_GQI_Retrieve_Layouts_1.RealTimeUpdates
{
    using System;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net;

    internal sealed class DataProvider : IDisposable
    {
        private readonly int _dataminerId;
        private readonly int _elementId;
        private readonly GQIDMS _gqiDms;
        private readonly Connection _connection;
        private readonly bool _isMcs;

        public DataProvider(Connection connection, GQIDMS gqiDms, int dataminerId, int elementId, bool isMcs)
        {
            _connection = connection;
            _dataminerId = dataminerId;
            _elementId = elementId;
            _gqiDms = gqiDms;
            _isMcs = isMcs;
            InstantiateCache();
        }

        public ElementTableCache SourceTable { get; private set; }

        private void InstantiateCache()
        {
            if (_connection == null)
            {
                throw new ArgumentNullException(nameof(_connection));
            }

            if (_isMcs)
            {
                SourceTable = new ElementTableCache(_connection, _gqiDms, _dataminerId, _elementId, 5600, "1");
            }
            else
            {
                SourceTable = new ElementTableCache(_connection, _gqiDms, _dataminerId, _elementId, 10300, "1");
            }
        }

        public void Dispose()
        {
            SourceTable?.Dispose();
        }
    }
}
