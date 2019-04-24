using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VideoPlayerTest.Players
{
    public abstract class VideoPlayerBase:ContentPresenter, IDisposable
    {
        public abstract TimeSpan Position { get; set; }
        public abstract bool IsPlaying { get; }
        public abstract TimeSpan Duration { get; }
        public bool IsMediaLoaded { get; protected set; }
        public abstract Task<bool> LoadMedia(string filePath);
        public abstract void UnloadMedia();
        public abstract void Play();
        public abstract void Pause();
        public abstract Task SeekAsync(TimeSpan time);
        public abstract void DisposePlayer();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposePlayer();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~VideoPlayerBase()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
