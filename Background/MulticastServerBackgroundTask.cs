using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Background
{
    public sealed class MulticastServerBackgroundTask : IBackgroundTask
    {
        public static Guid Register()
        {
            foreach (var current in BackgroundTaskRegistration.AllTasks)
            {
                if (current.Value.Name == nameof(MulticastServerBackgroundTask))
                {
                    return current.Value.TaskId;
                }
            }

            BackgroundTaskBuilder socketTaskBuilder = new BackgroundTaskBuilder
            {
                Name = nameof(MulticastServerBackgroundTask),
                TaskEntryPoint = typeof(MulticastServerBackgroundTask).FullName
            };
            SocketActivityTrigger trigger = new SocketActivityTrigger();
            socketTaskBuilder.SetTrigger(trigger);

            var task = socketTaskBuilder.Register();

            return task.TaskId;
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var details = taskInstance.TriggerDetails as SocketActivityTriggerDetails;
            var socketInformation = details.SocketInformation;

            switch (details.Reason)
            {
                case SocketActivityTriggerReason.SocketActivity:
                    BackgroundMulticastServer server = new BackgroundMulticastServer(socketInformation, taskInstance);
                    break;
                default:
                    break;
            }
        }
    }
}
