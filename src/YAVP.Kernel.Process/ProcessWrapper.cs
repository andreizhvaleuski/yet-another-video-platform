namespace YAVP.Kernel.Process
{
    public sealed class ProcessWrapper : IDisposable
    {
        private bool disposedValue;
        private System.Diagnostics.Process? internallProcess;

        public EventHandler<OutputReceivedEventArgs>? OutputReceived;

        private void OnErrorReceived(string message)
        {
            OutputReceived?.Invoke(this, new OutputReceivedEventArgs(message, isError: true));
        }

        private void OnOutputReceived(string message)
        {
            OutputReceived?.Invoke(this, new OutputReceivedEventArgs(message));
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    internallProcess?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ProcessWrapper()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public sealed class OutputReceivedEventArgs : EventArgs
    {
        public OutputReceivedEventArgs(string message, bool isError = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

            Message = message;
            IsError = isError;
        }

        public string Message { get; }

        public bool IsError { get; }
    }
}
