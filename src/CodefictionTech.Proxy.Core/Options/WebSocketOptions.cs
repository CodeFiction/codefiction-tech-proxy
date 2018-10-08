using System;

namespace CodefictionTech.Proxy.Core.Options
{
    /// <summary>
    /// Shared Proxy Options
    /// </summary>
    public class WebSocketOptions
    {
        private int _webSocketBufferSize;

        public WebSocketOptions()
        {
            _webSocketBufferSize = 4096;
        }

        /// <summary>
        /// Keep-alive interval for proxied Web Socket connections.
        /// </summary>
        public TimeSpan? WebSocketKeepAliveInterval { get; set; }

        /// <summary>
        /// Internal send and receive buffers size for proxied Web Socket connections.
        /// </summary>
        public int WebSocketBufferSize
        {
            get => _webSocketBufferSize;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _webSocketBufferSize = value;
            }
        }
    }
}