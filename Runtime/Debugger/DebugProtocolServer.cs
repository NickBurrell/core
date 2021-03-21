using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jint.DebugAgent;
using Jint.DebugAgent.Domains;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ReactUnity.Debugger
{
    /// <summary>
    /// Implements the messaging for the chrome debug protocol. The commands are implemented (by domain) in the ...Domain classes
    /// </summary>
    internal class ChromeDebugProtocolServer : IDebugProtocolServer
    {
        private readonly IProtocolServerOwner owner;
        private readonly DomainBase[] domains;
        private WebSocketServer server;

        /// <summary/>
        public ChromeDebugProtocolServer(IProtocolServerOwner owner, params DomainBase[] domains)
        {
            this.owner = owner;
            this.domains = domains;
        }

        /// <summary>
        /// Starts the server. This method only returns when the cancellation token is set to cancelled
        /// </summary>
        public Task Start(CancellationToken cancellationToken, int port)
        {
            return Task.Run(() =>
            {
                server = new WebSocketServer(port);
                server.AddWebSocketService("/", () => new SocketHandler(owner, domains));
                server.Start();

                cancellationToken.WaitHandle.WaitOne();
                server.Stop();
            }, cancellationToken);
        }

        /// <summary>
        /// Sends debug protocol messages throught the websocket
        /// </summary>
        public void Transmit(string domain, string method, JObject parameter)
        {
            string response = JsonConvert.SerializeObject(new JObject(
                new JProperty("method", string.Join(".", domain, method)),
                new JProperty("params", parameter)
                ));
            server.WebSocketServices.Broadcast(response);
        }

        private class SocketHandler : WebSocketBehavior
        {
            private readonly IProtocolServerOwner owner;
            private readonly DomainBase[] domains;
            private readonly object sendLock = new object();

            public SocketHandler(IProtocolServerOwner owner, DomainBase[] domains)
            {
                this.domains = domains;
                this.owner = owner;
            }

            protected override async void OnMessage(MessageEventArgs e)
            {
                var MessageText = e.Data;

                JObject Message = JsonConvert.DeserializeObject<JObject>(MessageText);
                int MessageId = Message["id"].Value<int>();
                string[] Method = Message["method"].Value<string>().Split('.');
                JObject Parameter = Message["params"]?.Value<JObject>();
                try
                {
                    JObject Result = await this.ProcessMessageAsync(Method[0], Method[1], Parameter);
                    if (Result != null)
                    {
                        JProperty IdProperty = new JProperty("id", MessageId);
                        JProperty ResultProperty = Result.HasValues ? new JProperty("result", Result) : null;
                        JObject Response = ResultProperty != null
                            ? new JObject(IdProperty, ResultProperty)
                            : new JObject(IdProperty);
                        SendMessage(Response);
                    }
                    else
                    {
                        //Ignore error or null results
                    }
                }
                catch
                {
                    //Ignore
                }
            }

            protected override void OnOpen()
            {
                this.owner.NotifyConnected();
            }

            protected override void OnClose(CloseEventArgs e)
            {
                this.owner.NotifyDisconnected();
            }

            protected override void OnError(ErrorEventArgs e)
            {
            }


            /// <summary>
            /// Processes a debug method request by forwarding ti to the corresponding domain implementation
            /// </summary>
            private async Task<JObject> ProcessMessageAsync(string domain, string method, JObject parameter)
            {
                DomainBase Domain = this.domains.FirstOrDefault(_ => _.Name == domain);
                if (Domain != null)
                {
                    return await Domain.ProcessMessageAsync(method, parameter);
                }
                else
                {
                    //Domain not supported; ignore
                    return null;
                }
            }

            private void SendMessage(JObject message)
            {
                string ResponseText = JsonConvert.SerializeObject(message);
                Send(ResponseText);
            }
        }
    }
}
