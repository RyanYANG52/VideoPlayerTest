using System;
using System.Threading.Tasks;
using Unosquare.FFME;

namespace VideoPlayerTest.Players
{
    public class FFMEVideoPlayer : VideoPlayerBase
    {
        private readonly MediaElement _mediaElement;
        private TaskCompletionSource<bool> _loadTcs;
        public override bool IsPlaying => _mediaElement.IsPlaying;

        public override TimeSpan Position
        {
            get
            {
                if (IsMediaLoaded || _mediaElement.ActualPosition.HasValue) return _mediaElement.ActualPosition.Value;
                return TimeSpan.Zero;
            }
            set
            {
                if (IsMediaLoaded && value <= Duration) _mediaElement.Position = value;
            }
        }

        public override TimeSpan Duration
        {
            get
            {
                if (IsMediaLoaded && _mediaElement.NaturalDuration.HasValue) return _mediaElement.NaturalDuration.Value;
                return TimeSpan.Zero;
            }
        }

        public FFMEVideoPlayer()
        {
            Library.FFmpegDirectory = @"C:\ffmpeg";
            _mediaElement = new MediaElement
            {
                LoadedBehavior = Unosquare.FFME.Common.MediaPlaybackState.Manual,
                UnloadedBehavior = Unosquare.FFME.Common.MediaPlaybackState.Manual,
                ScrubbingEnabled = true,
            };
            Content = _mediaElement;

            _mediaElement.MediaOpening += MediaElement_MediaOpening;
            _mediaElement.MediaOpened += MediaElement_MediaOpened;
            _mediaElement.MediaFailed += MediaElement_MediaFailed;
            _mediaElement.MediaEnded += MediaElement_MediaEnded;
        }

        public override void DisposePlayer()
        {
            _mediaElement.MediaOpened -= MediaElement_MediaOpened;
            _mediaElement.MediaFailed -= MediaElement_MediaFailed;
            _mediaElement.MediaEnded -= MediaElement_MediaEnded;
            _mediaElement.MediaOpening -= MediaElement_MediaOpening;
            _mediaElement.Close();
            _mediaElement.Dispose();
        }

        public override Task<bool> LoadMedia(string filePath)
        {
            UnloadMedia();
            _loadTcs = null;
            _loadTcs = new TaskCompletionSource<bool>();

            //_mediaElement.Source = new Uri(filePath);
            _mediaElement.Open(new Uri(filePath));
            return _loadTcs.Task;
        }

        public override void UnloadMedia()
        {
            if (IsMediaLoaded)
            {
                IsMediaLoaded = false;
                //_mediaElement.Stop();
                //_mediaElement.Source = null;
                _mediaElement.Close();
            }
        }

        public override void Play()
        {
            if (!IsMediaLoaded) return;
            if (!IsPlaying)
            {
                _mediaElement.Play();
            }
        }

        public override void Pause()
        {
            if (!IsMediaLoaded) return;
            if (IsPlaying)
            {
                _mediaElement.Pause();
            }
        }

        public async override Task SeekAsync(TimeSpan time)
        {
            if (!IsMediaLoaded) return;

            // HACK, maybe will work
            //await _mediaElement.Pause();
            //await Dispatcher.BeginInvoke(new Action(() => Position = time));
            //await Task.Delay(20);
            //await _mediaElement.Play();
            //await _mediaElement.Pause();
            await _mediaElement.Seek(time);
            //Position = time; // To change Position property immediately after Seek
                             //
            //await _mediaElement.Play();
        }

        private void MediaElement_MediaOpening(object sender, Unosquare.FFME.Common.MediaOpeningEventArgs e)
        {
            e.Options.IsFluidSeekingDisabled = true;
        }

        private void MediaElement_MediaOpened(object sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        {
            if (_loadTcs != null && _mediaElement.HasVideo)
            {
                Position = TimeSpan.Zero;
                _loadTcs.TrySetResult(true);
                _loadTcs = null;
                IsMediaLoaded = true;
            }
        }

        private void MediaElement_MediaFailed(object sender, Unosquare.FFME.Common.MediaFailedEventArgs e)
        {
            _loadTcs?.TrySetResult(false);
            _loadTcs = null;
            IsMediaLoaded = false;
        }

        private void MediaElement_MediaEnded(object sender, EventArgs e)
        {
            Pause();
        }
    }
}
