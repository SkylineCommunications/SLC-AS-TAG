namespace TAG_GQI_Retrieve_Layouts_1.RealTimeUpdates
{
    using System;
    using System.Threading;

    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net;

    internal static class StaticDataProvider
    {
        private static Lazy<DataProvider> _lazyDataProvider = new Lazy<DataProvider>(CreateInstance);
        private static GQIDMS _gqiDms;
        private static int _dataminerId;
        private static int _elementId;

        public static DataProvider Instance => _lazyDataProvider.Value;

        public static void Initialize(GQIDMS gqiDms, int dataminerId, int elementId)
        {
            _gqiDms = gqiDms ?? throw new ArgumentNullException(nameof(gqiDms));
            _dataminerId = dataminerId;
            _elementId = elementId;
        }

        public static void Reset()
        {
            var newLazy = new Lazy<DataProvider>(CreateInstance);
            var oldLazy = Interlocked.Exchange(ref _lazyDataProvider, newLazy);

            if (oldLazy.IsValueCreated &&
                oldLazy.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private static DataProvider CreateInstance()
        {
            if (_gqiDms == null)
            {
                throw new InvalidOperationException("Initialize method should be called first");
            }

            var connection = CreateConnection(_gqiDms);
            var dataProvider = new DataProvider(connection, _gqiDms, _dataminerId, _elementId);

            return dataProvider;
        }

        private static Connection CreateConnection(GQIDMS gqiDms)
        {
            var connection = ConnectionHelper.CreateConnection(gqiDms, "TAG-AllLayouts-Subscription(GQIDS)");
            connection.OnClose += (reason) => Reset();
            connection.OnAbnormalClose += (s, e) => Reset();
            connection.OnEventsDropped += (s, e) => Reset();
            connection.OnForcedLogout += (s, e) => Reset();

            return connection;
        }
    }
}
