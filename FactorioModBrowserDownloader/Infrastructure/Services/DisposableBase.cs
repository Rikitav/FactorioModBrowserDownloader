using System.Runtime.CompilerServices;

namespace FactorioNexus.Infrastructure.Services
{
    public abstract class DisposableBase<T> : IDisposable where T : DisposableBase<T>
    {
        private bool isDisposed = false;

        public bool IsDisposed
        {
            get => isDisposed;
            private set => isDisposed = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EnsureAlive()
        {
            ObjectDisposedException.ThrowIf(isDisposed, typeof(T));
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            GC.SuppressFinalize(this);
            isDisposed = true;
        }

        protected abstract void Dispose(bool disposing);
    }
}
