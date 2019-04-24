using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VideoPlayerTest.Players
{
    public class WpfVideoPlayer : VideoPlayerBase
    {
        private readonly MediaElement _mediaElement;
        private TaskCompletionSource<bool> _loadTcs;
        private bool _isPlaying;
        public override bool IsPlaying => _isPlaying;

        public override TimeSpan Position
        {
            get
            {
                if (IsMediaLoaded) return _mediaElement.Position;
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
                if (IsMediaLoaded && _mediaElement.NaturalDuration.HasTimeSpan) return _mediaElement.NaturalDuration.TimeSpan;
                return TimeSpan.Zero;
            }
        }

        public WpfVideoPlayer()
        {
            _mediaElement = new MediaElement
            {
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Manual,
                ScrubbingEnabled = true
            };
            Content = _mediaElement;

            _mediaElement.MediaOpened += MediaElement_MediaOpened;
            _mediaElement.MediaFailed += MediaElement_MediaFailed;
            _mediaElement.MediaEnded += MediaElement_MediaEnded;
        }

        public override void DisposePlayer()
        {
            _mediaElement.MediaOpened -= MediaElement_MediaOpened;
            _mediaElement.MediaFailed -= MediaElement_MediaFailed;
            _mediaElement.MediaEnded -= MediaElement_MediaEnded;
            _mediaElement.Close();
            IsMediaLoaded = false;
        }

        public override Task<bool> LoadMedia(string filePath)
        {
            UnloadMedia();
            _loadTcs = null;
            _loadTcs = new TaskCompletionSource<bool>();

            _mediaElement.Source = new Uri(filePath);
            _mediaElement.Play();
            return _loadTcs.Task;
        }

        public override void UnloadMedia()
        {
            if (IsMediaLoaded)
            {
                IsMediaLoaded = false;
                _mediaElement.Stop();
                _mediaElement.Source = null;
            }
        }

        public override void Play()
        {
            if (!IsMediaLoaded) return;
            if (!IsPlaying)
            {
                _mediaElement.Play();
                _isPlaying = true;
            }
        }

        public override void Pause()
        {
            if (!IsMediaLoaded) return;
            if (IsPlaying)
            {
                _mediaElement.Pause();
                _isPlaying = false;
            }
        }

        public override Task SeekAsync(TimeSpan time)
        {
            Position = time;
            return Task.CompletedTask;
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (_loadTcs != null && _mediaElement.HasVideo)
            {
                _mediaElement.Pause();
                _loadTcs.TrySetResult(true);
                _loadTcs = null;
                IsMediaLoaded = true;
            }
        }

        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _loadTcs?.TrySetResult(false);
            _loadTcs = null;
            IsMediaLoaded = false;
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Pause();
        }
    }
}
