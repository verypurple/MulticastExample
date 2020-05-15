using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using System.Threading;
using Windows.ApplicationModel.Background;

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

        protected override async void OnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            socket.MessageReceived -= OnMessageReceived;

            await ProcessMessage(args);
            await TransferOwnership();

            deferral.Complete();
        }
    }
}
