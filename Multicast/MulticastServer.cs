using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Multicast
{
    public class MulticastServer : AbstractMulticastSocket
    {
        public MulticastServer(Guid taskId)
        {
            this.taskId = taskId;
        }

        protected MulticastServer()
        {
        }

        public async Task TransferOwnership()
        {
            socket.MessageReceived -= OnMessageReceived;
            await socket.CancelIOAsync();

            IBuffer data = information.Serialize();
            SocketActivityContext context = new SocketActivityContext(data);

            socket.TransferOwnership(nameof(MulticastServer), context);
            socket = null;
        }

        public void ReclaimOwnership()
        {
            if (SocketActivityInformation.AllSockets.TryGetValue(nameof(MulticastServer), out SocketActivityInformation socketInformation))
            {
                socket = socketInformation.DatagramSocket;
                socket.MessageReceived += OnMessageReceived;
            }
        }

        protected override async void OnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            await ProcessMessage(args);
        }

        protected async Task ProcessMessage(DatagramSocketMessageReceivedEventArgs args)
        {
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
                            Name = GetComputerName(),
                            Address = information.LocalAddress,
                            Type = EndpointType.Server
                        };

                        await SendMessage(message);
                    }
                }
            }
        }
    }
}
