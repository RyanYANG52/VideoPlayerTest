using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VideoPlayerTest.Players;

namespace VideoPlayerTest
{
    public partial class MainWindow : Window
    {
        private readonly Stopwatch _watch = new Stopwatch();
        private TimeSpan _watchStartTime;
        private readonly VideoPlayerBase _player;
        private TimeSpan WatchTime => _watchStartTime + _watch.Elapsed;
        private DispatcherTimer _testTimer;

        public MainWindow()
        {
            InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            //_player = new WpfVideoPlayer();
            _player = new FFMEVideoPlayer();
            PlayerHost.Content = _player;
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMedia(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\SampleVideo_1280x720_30mb.mp4"));
        }

        private async void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Video Files | *.mp4; *.avi; *.mkv; *.mpeg; *.wmv",
            };
            if (dialog.ShowDialog() == true)
            {
                await LoadMedia(dialog.FileName);
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
                await SeekAsync(time);
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
        private async Task LoadMedia(string filePath)
        {
            if (await _player.LoadMedia(filePath))
            {
                PositionSlider.Minimum = 0;
                PositionSlider.Maximum = _player.Duration.TotalSeconds;
                PositionSlider.IsEnabled = true;
                DurationText.Text = _player.Duration.ToString(@"hh\:mm\:ss\.fff");
            }
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
        private async Task SeekAsync(TimeSpan time)
        {
            PauseWatch();
            _watchStartTime = time;
            UpdateUI(WatchTime, TimeSpan.Zero);
            Stopwatch sw = Stopwatch.StartNew();
            await _player.SeekAsync(time); //using ffme Position do not change immediately after seek
            sw.Stop();
            Console.WriteLine($"SEEK Duration:{sw.Elapsed}");
            StartWatch();
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

        private void SeekTestBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_watch.IsRunning)
            {
                _testTimer.Tick -= TestTimer_Tick;
                _testTimer.Stop();
                Pause();
            }
            else
            {
                Play();
                _testTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                _testTimer.Tick += TestTimer_Tick;
                _testTimer.Start();
            }
        }

        private async void TestTimer_Tick(object sender, EventArgs e)
        {
            var sec = DateTime.Now.Second;
            if (sec % 2 == 0)
            {
                await SeekAsync(TimeSpan.FromSeconds(10));
            }
            else
            {
                await SeekAsync(TimeSpan.FromSeconds(30));
            }
        }
    }
}
