using Microsoft.VisualBasic;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Formats.Asn1.AsnWriter;
using WpfLabel = System.Windows.Controls.Label;



namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private WpfLabel[] labels;
        private Brush[] targetColors = new Brush[4];
        private Brush[] predefinedColors = new Brush[] { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Yellow, Brushes.Orange, Brushes.White };
        private Dictionary<Brush, string> colorName = new Dictionary<Brush, string>
        {
            { Brushes.Red, "Red" },
            { Brushes.Green, "Green" },
            { Brushes.Blue, "Blue" },
            { Brushes.Yellow, "Yellow" },
            { Brushes.Orange, "Orange" },
            { Brushes.White, "White" }
        };

        private int score = 100;
        private int attempts = 1;
        private int remainingAttempts = 10;


        DispatcherTimer timer = new DispatcherTimer();
        TimeSpan elapsedTime;
        DateTime startTime;

        public MainWindow()
        {
            InitializeComponent();
            labels = new WpfLabel[] { label1, label2, label3, label4 };
            GenerateTargetColors();
            StartCountDown();
            StartGame();
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartCountDown();
            this.Title = $"Poging {attempts.ToString()}";
        }
        private void GenerateTargetColors()
        {
            //de kleurencode creeren
            Random rand = new Random();
            for (int i = 0; i < targetColors.Length; i++)
            {
                targetColors[i] = predefinedColors[rand.Next(0, 4)];
            }
        }
        private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //labelkleur veranderen
            WpfLabel clickedLabel = (WpfLabel)sender;
            int currentIndex = Array.IndexOf(predefinedColors, clickedLabel.Background);
            int nextIndex = (currentIndex + 1) % predefinedColors.Length;
            clickedLabel.Background = predefinedColors[nextIndex];
        }
        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            CheckCode();
            StartCountDown();
            attempts++;
            this.Title = $"Poging {attempts.ToString()}";
        }
        private void CheckCode()
        {
            bool match = true;
            int correctColors = 0;
            int correctPositions = 0;
            int totalPenalty = 0;

            string colorCode = string.Join(", ", targetColors.Select(color => colorName[color]));

            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i].Background == targetColors[i])
                {
                    correctPositions++;
                    totalPenalty += 0;
                }
                else if (targetColors.Contains(labels[i].Background))
                {
                    correctColors++;
                    match = false;
                    totalPenalty++;
                }
                else
                {
                    match = false;
                    totalPenalty = +2;
                }
            }
            score -= totalPenalty;
            AddAttemptToList(correctPositions, correctColors);

            if (!match)
            {
                remainingAttempts--;
                scoreLabel.Content = $"Score: {score}";
            }
            else if (match)
            {
                MessageBoxResult answer = MessageBox.Show($"Code is gekraakt in {attempts.ToString()} pogingen. Wil je nog eens?", $"WINNER", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (answer == MessageBoxResult.Yes)
                {
                    StartGame();
                }
                else
                {
                    Close();
                }
                return;
            }
            if (remainingAttempts <= 0)
            {
                timer.Stop();
                MessageBoxResult answer = MessageBox.Show($"Geen pogingen meer. De code was {colorCode}. Wil je nog eens?", $"FAILED", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (answer == MessageBoxResult.Yes)
                {
                    StartGame();
                }
                else
                {
                    Close();
                }
                return;
            }
        }
        private void AddAttemptToList(int correctPositions, int correctColors)
        {
            //controle gedaan in gekleurde vakjes achter historiek
            StackPanel attemptPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5)
            };

            foreach (var label in labels)
            {
                WpfLabel colorLabel = new WpfLabel
                {
                    Width = 30,
                    Height = 30,
                    Background = label.Background,
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    Margin = new Thickness(2)
                };
                attemptPanel.Children.Add(colorLabel);
            }
            for (int i = 0; i < correctPositions; i++)
            {
                WpfLabel feedbackLabel = new WpfLabel
                {
                    Width = 20,
                    Height = 20,
                    Background = Brushes.Red,
                    Margin = new Thickness(2),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(2)
                };
                attemptPanel.Children.Add(feedbackLabel);
            }
            for (int i = 0; i < correctColors; i++)
            {
                WpfLabel feedbackLabel = new WpfLabel
                {
                    Width = 20,
                    Height = 20,
                    Background = Brushes.White,
                    Margin = new Thickness(2),
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(2)
                };
                attemptPanel.Children.Add(feedbackLabel);
            }
            attemptsPanel.Children.Add(attemptPanel);
        }
        private void StartCountDown()
        {
            ///<para>Hier definieer ik dat de counter op 0 moet beginnen
            ///het gaat om de seconden. (TimeSpan)</para>
            startTime = DateTime.Now;
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();

        }
        private void StopCountDown()
        {
            ///<para> De timer stopt na 10 seconde als er niet op check code is geklikt.
            ///</para>
            timer.Stop();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedTime = DateTime.Now - startTime;
            timerLabel.Content = elapsedTime.ToString("ss");
            string colorCode = string.Join(", ", targetColors.Select(color => colorName[color]));

            if (elapsedTime.TotalSeconds > 10)
            {
                StopCountDown();
                attempts++;
                this.Title = $"Poging {attempts}";
                StartCountDown();
                if (attempts >= 10)
                {
                    timer.Stop();
                    MessageBoxResult answer = MessageBox.Show($" Geen pogingen meer. De code was  {colorCode}. Nog eens proberen?", "FAILED", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (answer == MessageBoxResult.Yes)
                    {
                        StartGame();
                    }
                    else
                    {
                        Close();
                    }
                    return;
                }
            }
        }
        private void ToggleDebug()
        {
#if DEBUG
            debugLabel.Visibility = debugLabel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            debugLabel.Content = $"Doelkleurencode: {string.Join(", ", targetColors.Select(color => colorName[color]))}";
#endif
        }
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F12 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ToggleDebug();
            }
        }
        public void ShowGeneratedCode()
        {
#if DEBUG
            debugLabel.Content = $" {string.Join(", ", targetColors.Select(color => colorName[color]))}";
#endif
        }
        private void StartGame()
        {
            string name = Interaction.InputBox("Wat is uw naam?", "Welkom", " ");
            while(!string.IsNullOrEmpty(name) || name == " " )
            {
                MessageBox.Show("Geef uw naam", "Foutieve invoer", MessageBoxButton.OK, MessageBoxImage.Warning);
                name = Interaction.InputBox("Wat is uw naam?", "Welkom", " ");
            }
            
            score = 100;
            attempts = 0;
            remainingAttempts = 10;

            attemptsPanel.Children.Clear();

            GenerateTargetColors();

            //scoreLabel.Content = "Score: 100";
            this.Title = $"Poging {attempts}";

            label1.Background = Brushes.White;
            label2.Background = Brushes.White;
            label3.Background = Brushes.White;
            label4.Background = Brushes.White;
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult answer = MessageBox.Show($"Wilt u het spel vroegtijdig beëindigen?", $"Poging {attempts}/10", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (answer == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void newGameMenu_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void highScoreMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("", "MasterMind highscores");
        }

        private void closeGameMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void amountOfAttemptsMenu_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
