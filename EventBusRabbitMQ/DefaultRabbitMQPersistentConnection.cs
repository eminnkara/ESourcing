using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace EventBusRabbitMQ.Producer
{
    public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        private readonly IConnectionFactory connectionFactory;
        private IConnection connection;
        private readonly int retryCount;
        private readonly ILogger<DefaultRabbitMQPersistentConnection> logger;
        private bool disposed;

        public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount, ILogger<DefaultRabbitMQPersistentConnection> logger)
        {
            this.connectionFactory = connectionFactory;
            this.retryCount = retryCount;
            this.logger = logger;
        }

        public bool IsConnected
        {
            get
            {
                return connection != null && connection.IsOpen && !disposed;
            }
        }

        public bool TryConnect()
        {
            logger.LogInformation("RabbitMQ client is trying to connect");
            var policy = RetryPolicy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                 {
                     logger.LogWarning(ex, "RabbitMQ Client could not connect after {TimeOut}s {{ExceptionMessage}}", $"{time.TotalMilliseconds:n1}", ex.Message);
                 });
            
            policy.Execute(() => {
                connection = connectionFactory.CreateConnection();
            });


            if (IsConnected)
            {
                connection.ConnectionShutdown += OnConnectionShutdown;
                connection.CallbackException += OnCallbackException;
                connection.ConnectionBlocked += OnConnectionBlocked;

                logger.LogInformation("RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events", connection.Endpoint.HostName);

                return true;
            }
            else
            {
                logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");

                return false;
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (disposed) return;

            logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (disposed) return;

            logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (disposed) return;

            logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            TryConnect();
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return connection.CreateModel();
        }

        public void Dispose()
        {
            if (disposed) return;

            disposed = true;
            try
            {
                connection.Dispose();
            }
            catch (IOException ex)
            {
                logger.LogCritical(ex.ToString());
            }
        }
    }
}
