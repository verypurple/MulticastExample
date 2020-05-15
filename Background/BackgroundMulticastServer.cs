using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.ApplicationModel.Background;
using Windows.Storage.Streams;

namespace Background
{
    using Multicast;

    internal class BackgroundMulticastServer : MulticastServer
    {
        private readonly BackgroundTaskDeferral deferral;

        public BackgroundMulticastServer(SocketActivityInformation socketInformation, IBackgroundTaskInstance taskInstance)
        {
            socket = socketInformation.DatagramSocket;
            information = ServerInformation.Deserialize(socketInformation.Context.Data);
            deferral = taskInstance.GetDeferral();

            socket.MessageReceived += OnMessageReceived;
        }

        protected async void OnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            socket.MessageReceived -= OnMessageReceived;

            string remoteAddress = args.RemoteAddress.CanonicalName;

            // Reject messages from this computer
            if (remoteAddress == information.LocalAddress)
            {
                return;
            }

            using (DataReader reader = args.GetDataReader())
            {
                byte[] data = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(data);

                EndpointInformation message = EndpointInformation.Deserialize(data);

                if (message != null)
                {
                    // Did message originate from a client?
                    if (message.Type == EndpointType.Client)
                    {
                        message = new EndpointInformation()
                        {
                            Address = information.LocalAddress,
                            Type = EndpointType.Server
                        };

                        await SendMessage(message);
                    }
                }
            }

            await TransferOwnership();

            deferral.Complete();
        }
    }
}
