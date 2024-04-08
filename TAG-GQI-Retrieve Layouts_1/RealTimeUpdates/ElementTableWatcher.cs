namespace TAG_GQI_Retrieve_Layouts_1.RealTimeUpdates
{
    using System;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.SubscriptionFilters;

    public sealed class ElementTableWatcher : IDisposable
    {
        private readonly Connection _connection;
        private readonly SubscriptionFilter _subscriptionFilter;
        private readonly string _subscriptionId;

        public ElementTableWatcher(Connection connection, int dataminerId, int elementId, int tableId, string subscriptionId = "1")
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _connection.OnNewMessage += Connection_OnNewMessage;

            _subscriptionId = subscriptionId;

            _subscriptionFilter = new SubscriptionFilterParameter(typeof(ParameterChangeEventMessage).Name, new string[0], dataminerId, elementId, tableId, index: null);
            _connection.AddSubscription(subscriptionId, _subscriptionFilter);

            _connection.Subscribe();
        }

        public event EventHandler<ParameterTableUpdateEventMessage> Changed;

        public void Dispose()
        {
            try
            {
                _connection.RemoveSubscription(_subscriptionId, _subscriptionFilter);
                _connection.OnNewMessage -= Connection_OnNewMessage;
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void Connection_OnNewMessage(object sender, NewMessageEventArgs e)
        {
            if (e.Message is ParameterTableUpdateEventMessage tableChange)
            {
                Changed?.Invoke(this, tableChange);
            }
        }
    }
}
