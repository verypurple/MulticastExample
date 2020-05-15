using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MulticastExample
{
    using Multicast;
    using Background;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Scenario_Server : Page
    {
        MulticastServer server;

        public Scenario_Server()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string localAddress = e.Parameter as string;

            var taskId = MulticastServerBackgroundTask.Register();
            server = new MulticastServer(taskId);
            await server.Bind(localAddress, "224.1.0.0", "9000");

            Application.Current.Suspending += OnSuspending;
            Application.Current.Resuming += OnResuming;
        }

        private void OnResuming(object sender, object e)
        {
            server.ReclaimOwnership();
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await server.TransferOwnership();
            deferral.Complete();
        }
    }
}
