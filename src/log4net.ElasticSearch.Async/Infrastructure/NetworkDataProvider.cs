namespace CentauroTech.Log4net.ElasticSearch.Async.Infrastructure
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    using log4net.Core;

    /// <summary>Network data provider for log events.</summary>
    internal class NetworkDataProvider
    {
        /// <summary>The default external IP check address.</summary>
        public const string DefaultExternalIpCheckAddress = "8.8.8.8";

        /// <summary>log4net error handler.</summary>
        private readonly IErrorHandler errorHandler;

        /// <summary>Initializes a new instance of the <see cref="NetworkDataProvider"/> class.</summary>
        /// <param name="errorHandler">log4net error handler.</param>
        public NetworkDataProvider(IErrorHandler errorHandler)
        {
            this.errorHandler = errorHandler;
        }

        /// <summary>Gets external machine IP using simple UDP connection</summary>
        /// <param name="externalIpCheckAddress">External IP to be used by address check.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetMachineIp(string externalIpCheckAddress = DefaultExternalIpCheckAddress)
        {
            string localIp = null;
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect(externalIpCheckAddress, 80); // doesn't matter what it connects to
                    if (socket.LocalEndPoint is IPEndPoint endPoint)
                    {
                        localIp = endPoint.Address.ToString(); //ipv4
                    }
                }
            }
            catch (Exception ex)
            {
                this.errorHandler.Error("Failed to get IP address of the local machine.", ex);
            }

            return localIp;
        }
    }
}
