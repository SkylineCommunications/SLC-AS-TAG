namespace TAG_GQI_Retrieve_Layouts_1.RealTimeUpdates
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.ManagerStore;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using SLDataGateway.API.Querying;

    public sealed class ElementTableCache : IDisposable
    {
        private readonly ElementTableWatcher _watcher;

        private readonly int _dataminerId;
        private readonly int _elementId;
        private readonly int _tableId;

        private readonly GQIDMS _gqiDms;

        private ParameterValue _cachedTable;

        public ElementTableCache(Connection connection, GQIDMS gqiDms, int dataminerId, int elementId, int tableId, string subscripionId = "1")
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (gqiDms is null)
            {
                throw new ArgumentNullException(nameof(gqiDms));
            }

            if (dataminerId == default || elementId == default || tableId == default)
            {
                throw new ArgumentException($"Dataminer ID, element ID and table ID cannot contain any default value.");
            }

            _dataminerId = dataminerId;
            _elementId = elementId;
            _tableId = tableId;
            _gqiDms = gqiDms;

            _watcher = new ElementTableWatcher(connection, dataminerId, elementId, tableId, subscripionId);
            _watcher.Changed += Watcher_OnChanged;

            FillCache();
        }

        public event EventHandler<ParameterTableUpdateEventMessage> Changed;

        public ParameterValue[] GetData()
        {
            return _cachedTable.ArrayValue;
        }

        public void Dispose()
        {
            _watcher.Changed -= Watcher_OnChanged;
            _watcher?.Dispose();
        }

        private void FillCache()
        {
            var table = GetTableColumns();
            if (table is null)
            {
                return;
            }

            _cachedTable = table;
        }

        private void UpdateCache(ParameterTableUpdateEventMessage message)
        {
            var newTable = message.NewValue;
            if (newTable is null || message.ParameterID != _tableId) return;

            _cachedTable.ApplyUpdate(message);
        }

        private ParameterValue GetTableColumns()
        {
            var getPartialTableMessage = new GetPartialTableMessage(_dataminerId, _elementId, _tableId, new[] { "forceFullTable=true" });
            var parameterChangeEventMessage = (ParameterChangeEventMessage)_gqiDms.SendMessage(getPartialTableMessage);
            if (parameterChangeEventMessage.NewValue?.ArrayValue == null)
            {
                return null;
            }

            var table = parameterChangeEventMessage.NewValue;

            int lengthCheck = (_tableId == 14100 || _tableId == 15100) ? 7 : 4;
            if (table.ArrayValue.Length < lengthCheck)
            {
                return null;
            }

            return table;
        }

        private void Watcher_OnChanged(object sender, ParameterTableUpdateEventMessage e)
        {
            UpdateCache(e);

            Changed?.Invoke(this, e);
        }
    }
}
