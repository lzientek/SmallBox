using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using SmallBox.Storage.Provider;

namespace SmallBox.BackupWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);
        private readonly StorageProvider _storageProvider = new StorageProvider();

        /// <summary>
        /// Time between two backups.
        /// </summary>
        public const int N = 60;

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole is running");

            try
            {
                RunAsync(_cancellationTokenSource.Token).Wait();
            }
            finally
            {
                _runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 1;

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            _cancellationTokenSource.Cancel();
            _runCompleteEvent.WaitOne();
            base.OnStop();
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool result = await _storageProvider.ZipAllToBackup();
                if (result)
                { Trace.TraceInformation("Working"); }
                else { Trace.TraceError("Error during the backup"); }

                await Task.Delay(N * 1000);
            }
        }

    }
}
