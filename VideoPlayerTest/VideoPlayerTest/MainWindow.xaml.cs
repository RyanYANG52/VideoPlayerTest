using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VideoPlayerTest.Players;

namespace VideoPlayerTest
{
    public partial class MainWindow : Window
    {
        private readonly Stopwatch _watch = new Stopwatch();
        private TimeSpan _watchStartTime;
        private readonly VideoPlayerBase _player;
        private TimeSpan WatchTime => _watchStartTime + _watch.Elapsed;

        public MainWindow()
        {
            InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            //_player = new WpfVideoPlayer();
            _player = new FFMEVideoPlayer();
            PlayerHost.Content = _player;
        }

        private async void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Video Files | *.mp4; *.avi; *.mkv; *.mpeg; *.wmv",
            };
            if (dialog.ShowDialog() == true && await _player.LoadMedia(dialog.FileName))
            {
                PositionSlider.Minimum = 0;
                PositionSlider.Maximum = _player.Duration.TotalSeconds;
                PositionSlider.IsEnabled = true;
                DurationText.Text = _player.Duration.ToString(@"hh\:mm\:ss\.fff");
            }
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_watch.IsRunning)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_watch.IsRunning)
            {
                TimeSpan watchTime = WatchTime;
                if (watchTime.TotalSeconds >= PositionSlider.Maximum) //play to the end, pause
                {
                    Console.WriteLine("Play To End");
                    PauseWatch();
                    _watchStartTime = TimeSpan.FromSeconds(PositionSlider.Maximum);
                    UpdateUI(WatchTime, TimeSpan.Zero);
                    Pause();
                }
                else // update slider and text while playing
                {
                    TimeSpan offsetTime = watchTime - _player.Position;
                    if (Math.Abs(offsetTime.TotalSeconds) > 0.5) // clock and player is unsync
                    {
                        Console.WriteLine($"Correct Clock: {offsetTime}");

                        // correct clock to sync with player
                        PauseWatch();
                        _watchStartTime = _player.Position;
                        StartWatch();
                    }
                    UpdateUI(watchTime, offsetTime);
                }
            }
        }

        private async void PositionSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(PositionSlider);
            TimeSpan time = TimeSpan.FromSeconds(PositionSlider.Maximum * pos.X / PositionSlider.ActualWidth);

            if (_watch.IsRunning) // seek while playing
            {
                PauseWatch();
                _watchStartTime = time;
                UpdateUI(WatchTime, TimeSpan.Zero);
                Stopwatch sw = Stopwatch.StartNew();
                await _player.SeekAsync(time); //using ffme Position do not change immediately after seek
                sw.Stop();                
                Console.WriteLine($"OFFSET: {time - _player.Position} SEEK Duration:{sw.Elapsed}");
                StartWatch();
            }
            else // seek while paused
            {
                _watchStartTime = time;
                _player.Position = _watchStartTime;
               // await _player.SeekAsync(time);
               // Console.WriteLine($"OFFSET: {time - _player.Position}");
                UpdateUI(WatchTime, TimeSpan.Zero);
            }
            e.Handled = true;
        }

        private void Play()
        {
            _player.Play();
            StartWatch();
            PlayBtn.Content = "Pause";
        }

        private void Pause()
        {
            _player.Pause();
            _player.Position = WatchTime;
            PauseWatch();
            PlayBtn.Content = "Play";
        }

        private void UpdateUI(TimeSpan time, TimeSpan offsetTime)
        {
            PositionSlider.Value = time.TotalSeconds;
            PositionText.Text = time.ToString(@"hh\:mm\:ss\.fff");
            OffsetText.Text = offsetTime.ToString(@"hh\:mm\:ss\.fff");
        }

        private void PauseWatch()
        {
            if (_watch.IsRunning)
            {
                _watchStartTime += _watch.Elapsed;
                _watch.Reset();
            }
        }

        private void StartWatch()
        {
            if (!_watch.IsRunning)
            {
                _watch.Start();
            }
        }
    }
}
