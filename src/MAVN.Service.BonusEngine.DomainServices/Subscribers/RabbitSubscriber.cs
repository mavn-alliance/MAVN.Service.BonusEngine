using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace MAVN.Service.BonusEngine.DomainServices.Subscribers
{
    public abstract class RabbitSubscriber<TMessage> : IStartStop
    {
        private readonly ILogFactory _logFactory;
        private readonly string _connectionString;
        private readonly string _exchangeName;
        private readonly string _contextName;

        private RabbitMqSubscriber<TMessage> _subscriber;

        protected readonly ILog Log;
        protected IList<string> GuidsFieldsToValidate { get; set; } = new List<string>();

        protected RabbitSubscriber(string connectionString, string exchangeName, ILogFactory logFactory)
        {
            Log = logFactory.CreateLog(this);
            _logFactory = logFactory;

            _connectionString = connectionString;
            _exchangeName = exchangeName;
            _contextName = GetType().Name;
        }

        public void Start()
        {
            var rabbitMqSubscriptionSettings = RabbitMqSubscriptionSettings.ForSubscriber(_connectionString,
                    _exchangeName,
                    "bonusengine")
                .MakeDurable();

            _subscriber = new RabbitMqSubscriber<TMessage>(
                    _logFactory,
                    rabbitMqSubscriptionSettings,
                    new ResilientErrorHandlingStrategy(
                        _logFactory,
                        rabbitMqSubscriptionSettings,
                        TimeSpan.FromSeconds(10)))
                .SetMessageDeserializer(new JsonMessageDeserializer<TMessage>())
                .Subscribe(StartProcessingAsync)
                .CreateDefaultBinding()
                .Start();
        }

        public void Stop()
        {
            _subscriber.Stop();
        }

        public void Dispose()
        {
            _subscriber.Dispose();
        }

        public async Task StartProcessingAsync(TMessage message)
        {
            Log.Info($"{_contextName} event received", message);

            if (!ValidateIdentifiers(message))
            {
                return;
            }

            var result = await ProcessMessageAsync(message);

            if (!result.isSuccessful)
            {
                Log.Error(message: $"{_contextName} event was not processed", context: new
                {
                    Event = message,
                    ErrorMessage = result.errorMessage
                });

                return;
            }

            Log.Info($"{_contextName} event was processed", message);
        }

        private bool ValidateIdentifiers(TMessage message)
        {
            var messageType = typeof(TMessage);
            var isMessageValid = true;

            foreach (var fieldName in GuidsFieldsToValidate)
            {
                var propertyInfo = messageType.GetProperty(fieldName);

                var fieldValue = (string)propertyInfo.GetValue(message, null);

                if (!Guid.TryParse(fieldValue, out _))
                {
                    Log.Error(message: $"{fieldName} has invalid format in {nameof(message)}", context: message);
                    isMessageValid = false;
                }
            }

            return isMessageValid;
        }

        protected abstract Task<(bool isSuccessful, string errorMessage)> ProcessMessageAsync(TMessage message);
    }
}
