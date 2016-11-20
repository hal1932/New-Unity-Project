using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorUtil
{
    public class Disposable : IDisposable
    {
        public Disposable(Action onDispose = null)
        {
            _onDisposed = onDispose;
        }

        protected void SetDisposingAction(Action action)
        {
            _onDisposed = action;
        }

        ~Disposable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            lock (_disposingLock)
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;

                _onDisposed();
            }
        }

        private Action _onDisposed;
        private bool _disposed;
        private object _disposingLock = new object();
    }
}
