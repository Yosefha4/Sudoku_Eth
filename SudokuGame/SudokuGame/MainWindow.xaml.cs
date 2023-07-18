using SudokuGame.Pages;
using SudokuGame.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using SudokuGame.ViewModels;
using SudokuGame.Utils;
using SudokuGame.UserControls;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text.Json;
using System.Net;

namespace SudokuGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsButtonVisible { get; set; } = true;
        SoundPlayer soundPlayer = null;
        // private GamePage gamePage;

        public MainWindow()
        {
            InitializeComponent();
            string soundFilePath = System.IO.Path.Combine("..", "..", "Sounds", "background-sound.wav");
            soundPlayer = new SoundPlayer(soundFilePath);
            //  Task.Run(() => soundPlayer.PlayLooping());

        }


        public void BackMainWinodw()
        {
            Task.Run(() => soundPlayer.PlayLooping());
            spMainButtons.Visibility = Visibility.Visible;
            this.Title = "Sudoku ETH";
            if (contentFrame.Content is IDisposable disposablePage)
            {
                disposablePage.Dispose();
            }
            contentFrame.Content = null;
        }


        private dynamic CreateControlToPopup(Type controlType, Thickness margin, double fontSize = 15, Visibility visible = Visibility.Visible)
        {
            dynamic control = Activator.CreateInstance(controlType);
            control.Margin = margin;
            control.FontSize = fontSize;
            control.Foreground = Brushes.Azure;
            control.Visibility = visible;
            return control;
        }


        private async void RunGame()
        {

            var stackPanelMain = new StackPanel()
            {
                Width = 430,
                Height = 140,
            };

            var textBlock = (TextBlock)CreateControlToPopup(typeof(TextBlock), new Thickness(0, 10, 0, 10));
            textBlock.Text = "Account wallet";
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            var textBox = (TextBox)CreateControlToPopup(typeof(TextBox), new Thickness(10, 0, 10, 20));
            string key = "Account";
            textBox.Text = ConfigurationManager.AppSettings[key];

            stackPanelMain.Children.Add(textBlock);
            stackPanelMain.Children.Add(textBox);
            var textBlockError = (TextBlock)CreateControlToPopup(typeof(TextBlock), new Thickness(10, 0, 0, 3), 12, Visibility.Collapsed);

            stackPanelMain.Children.Add(textBlockError);

            var stackPanelSecond = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };


            Action<string> displayMessageError = (string textMessage) =>
            {
                textBox.Margin = new Thickness(10, 0, 10, 10);
                textBlockError.Visibility = Visibility.Visible;
                textBlockError.Text = "*" + textMessage;
            };

            Action clickOk = async () =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // Validate the text box value
                    string walletAddress = textBox.Text.Trim();

                    string pattern = @"^(0x)?[0-9a-fA-F]{40}$";

                    if (Regex.IsMatch(walletAddress, pattern))
                    {


                        // save account in program
                        Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                        config.AppSettings.Settings[key].Value = walletAddress;
                        config.Save(ConfigurationSaveMode.Modified);
                        ConfigurationManager.RefreshSection("appSettings");
                        Mouse.OverrideCursor = Cursors.Wait;
                        stackPanelMain.IsEnabled = false;


                        var requestData = new
                        {
                            account = walletAddress,
                            amount = config.AppSettings.Settings["AmountStart"].Value
                        };
                        string json = System.Text.Json.JsonSerializer.Serialize(requestData, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Optional: Use camel case for property names
                        });

                        // Send request to the server
                        HttpClient httpClient = new HttpClient();

                        HttpResponseMessage response = await httpClient.PostAsync("https://sodukusmartcontractsce.onrender.com/mint", new StringContent(json, Encoding.UTF8, "application/json"));

                        // Process the response
                        if (response.IsSuccessStatusCode)
                        {


                            Application.Current.Dispatcher.Invoke(() =>
                            {

                                // await Task.Delay(500);
                                DialogHost.CloseDialogCommand.Execute(true, null);
                                soundPlayer.Stop();
                                contentFrame.Navigate(GamePage.Instance);
                                IsButtonVisible = false;
                                //  Mouse.OverrideCursor = null;
                            });
                            stackPanelMain.IsEnabled = true;
                            spMainButtons.Visibility = Visibility.Collapsed;
                        }

                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            string content = await response.Content.ReadAsStringAsync();
                            displayMessageError(content);

                        }
                        else
                        {

                            displayMessageError("Login error try later");
                            // Handle status code 403 - Only the manager can call this function

                        }

                        Mouse.OverrideCursor = null;
                        stackPanelMain.IsEnabled = true;

                    }
                    else
                    {
                        displayMessageError("Please enter a valid ETH wallet address.");
                    }
                }

                else
                {
                    displayMessageError("Please enter your account.");
                }
            };

            var okButton = new Button
            {
                Content = "OK",
                Command = new RelayCommand(clickOk)
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Command = new RelayCommand(() =>
                {
                    DialogHost.CloseDialogCommand.Execute(true, null);
                })
            };

            stackPanelSecond.Children.Add(okButton);
            stackPanelSecond.Children.Add(cancelButton);
            stackPanelMain.Children.Add(stackPanelSecond);

            CustomMaterialDesignPopup cusMat = new CustomMaterialDesignPopup(stackPanelMain);
            await DialogHost.Show(cusMat);

        }


        private void Main_Menu_Options(object sender, RoutedEventArgs e)
        {

            // var viewModel = new CustomMaterialDesignPopupViewModel();

            switch ((sender as Button).Name)
            {
                case "buPlayGame":
                    {
                        RunGame();
                        break;
                    }
                case "buHall":
                    break;
                case "buSettings":
                    break;
                case "buExit":
                    Application.Current.Shutdown();
                    break;
            }

        }


        private void GameWindow_Closed(object sender, EventArgs e)
        {

            //gameWindow.Dispose(); // Dispose the game window to clean up resources
            this.Visibility = Visibility.Visible;
            this.spMainButtons.Visibility = Visibility.Visible;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GamePage.Instance.ExitGame();
        }
    }

}
