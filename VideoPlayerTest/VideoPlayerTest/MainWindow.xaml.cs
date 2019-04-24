using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VideoPlayerTest.Players;

namespace VideoPlayerTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

            _player = new WpfVideoPlayer();
            PlayerHost.Content = _player;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_watch.IsRunning)
            {
                UpdateUI();
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

        private async void PositionSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(PositionSlider);

            TimeSpan time = TimeSpan.FromSeconds(PositionSlider.Maximum * pos.X / PositionSlider.ActualWidth);
            
            if (_watch.IsRunning)
            {
                PauseWatch();
                _watchStartTime = time;
                UpdateUI();
                await _player.SeekAsync(time);
                StartWatch();
            }
            else
            {
                _watchStartTime = time;
                _player.Position = _watchStartTime;
                UpdateUI();
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

        private void UpdateUI()
        {
            PositionSlider.Value = WatchTime.TotalSeconds;
            PositionText.Text = WatchTime.ToString(@"hh\:mm\:ss\.fff");
            OffsetText.Text = (WatchTime - _player.Position).ToString(@"hh\:mm\:ss\.fff");
        }

    }
}
