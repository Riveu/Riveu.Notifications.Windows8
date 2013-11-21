using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Riveu.Notifications.Windows8
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string username = String.Empty;
        string password = String.Empty;

        Rect _windowBounds;
        double _settingsWidth = 346;
        Popup _settingsPopup;
        DispatcherTimer timer = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            _windowBounds = Window.Current.Bounds;
            Window.Current.SizeChanged += Current_SizeChanged;
            
            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;
        }

        void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Clear();
            SettingsCommand settingsCommand = new SettingsCommand("settings", "Settings", (x) =>
            {
                _settingsPopup = new Popup();
                _settingsPopup.Closed += _settingsPopup_Closed;
                Window.Current.Activated += Current_Activated;
                _settingsPopup.IsLightDismissEnabled = true;
                _settingsPopup.Width = _settingsWidth;
                _settingsPopup.Height = _windowBounds.Height;
                SettingsUserControl settingsPane = new SettingsUserControl();
                settingsPane.Width = _settingsWidth;
                settingsPane.Height = _windowBounds.Height;
                _settingsPopup.Child = settingsPane;
                _settingsPopup.SetValue(Canvas.LeftProperty, _windowBounds.Width - _settingsWidth);
                _settingsPopup.SetValue(Canvas.TopProperty, 0);
                _settingsPopup.IsOpen = true;
            });
            SettingsCommand privacyPolicyCommand = new SettingsCommand("privacyPolicy", "Privacy Policy", (x) =>
            {
                _settingsPopup = new Popup();
                _settingsPopup.Closed += _settingsPopup_Closed;
                Window.Current.Activated += Current_Activated;
                _settingsPopup.IsLightDismissEnabled = true;
                _settingsPopup.Width = _settingsWidth;
                _settingsPopup.Height = _windowBounds.Height;
                PrivacyPolicy settingsPane = new PrivacyPolicy();
                settingsPane.Width = _settingsWidth;
                settingsPane.Height = _windowBounds.Height;
                _settingsPopup.Child = settingsPane;
                _settingsPopup.SetValue(Canvas.LeftProperty, _windowBounds.Width - _settingsWidth);
                _settingsPopup.SetValue(Canvas.TopProperty, 0);
                _settingsPopup.IsOpen = true;
            });
            SettingsCommand registerCommand = new SettingsCommand("register", "Register", (x) =>
            {
                _settingsPopup = new Popup();
                _settingsPopup.Closed += _settingsPopup_Closed;
                Window.Current.Activated += Current_Activated;
                _settingsPopup.IsLightDismissEnabled = true;
                _settingsPopup.Width = _settingsWidth;
                _settingsPopup.Height = _windowBounds.Height;
                RegisterUser settingsPane = new RegisterUser();
                settingsPane.Width = _settingsWidth;
                settingsPane.Height = _windowBounds.Height;
                _settingsPopup.Child = settingsPane;
                _settingsPopup.SetValue(Canvas.LeftProperty, _windowBounds.Width - _settingsWidth);
                _settingsPopup.SetValue(Canvas.TopProperty, 0);
                _settingsPopup.IsOpen = true;
            });
            args.Request.ApplicationCommands.Add(settingsCommand);
            args.Request.ApplicationCommands.Add(registerCommand);
            args.Request.ApplicationCommands.Add(privacyPolicyCommand);
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            _windowBounds = Window.Current.Bounds;
        }

        async void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                _settingsPopup.IsOpen = false;
                statusLabel.Text = "Retrieving Messages...";
                NotificationService.NotificationServiceClient client = new NotificationService.NotificationServiceClient();
                username = ApplicationData.Current.LocalSettings.Values["Username"] as String;
                password = ApplicationData.Current.LocalSettings.Values["Password"] as String;
                try
                {
                    if (await client.AuthenticateUserAsync(username, password))
                    {
                        var data = await client.GetNotificationsAsync(username);
                        listView.ItemsSource = data;
                        statusLabel.Text = String.Empty;
                        if (timer.IsEnabled)
                        {
                            timer.Stop();
                        }
                        try
                        {
                            int interval = Int32.Parse(ApplicationData.Current.LocalSettings.Values["RefreshRate"].ToString());
                            timer = new DispatcherTimer();
                            timer.Interval = new TimeSpan(0, 0, interval);
                            timer.Tick += timer_Tick;
                            timer.Start();
                        }
                        catch
                        {
                            try
                            {
                                MessageDialog dialog = new MessageDialog("Please set refresh rate in settings");
                                dialog.ShowAsync();
                            }
                            catch
                            {
                                //do nothing
                            }
                        }
                        if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values["EnablePush"]) == true)
                        {
                            CreatAndSendPushChannel();
                        }
                        else
                        {
                            UnregisterSubscriber();
                        }
                    }
                    else
                    {
                        statusLabel.Text = "Invalid Credentials";
                    }
                }
                catch
                {
                    new MessageDialog("Unable to connect to Riveu server. Please verify internet connection and re-launch application").ShowAsync();
                }
            }
        }

        void _settingsPopup_Closed(object sender, object e)
        {
            Window.Current.Activated -= Current_Activated;
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("EnablePush"))
            {
                MessageDialog messageBox = new MessageDialog("Push Notifications");
                messageBox = new MessageDialog("This application supports Push Notifications. Please go to the settings charm to configure credentials and push notification settings.");
                await messageBox.ShowAsync();
            }
            else if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Username") && ApplicationData.Current.LocalSettings.Values.ContainsKey("Password"))
            {
                statusLabel.Text = "Retrieving Messages...";
                NotificationService.NotificationServiceClient client = new NotificationService.NotificationServiceClient();
                username = ApplicationData.Current.LocalSettings.Values["Username"] as String;
                password = ApplicationData.Current.LocalSettings.Values["Password"] as String;
                //Authenticate
                try
                {
                    if (await client.AuthenticateUserAsync(username, password))
                    {
                        var data = await client.GetNotificationsAsync(username);
                        listView.ItemsSource = data;
                        statusLabel.Text = String.Empty;
                        if (timer.IsEnabled)
                        {
                            timer.Stop();
                        }
                        try
                        {
                            int interval = Int32.Parse(ApplicationData.Current.LocalSettings.Values["RefreshRate"].ToString());
                            timer = new DispatcherTimer();
                            timer.Interval = new TimeSpan(0, 0, interval);
                            timer.Tick += timer_Tick;
                            timer.Start();
                        }
                        catch
                        {
                            MessageDialog dialog = new MessageDialog("Please set refresh rate in settings");
                            dialog.ShowAsync();
                        }
                        if (Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values["EnablePush"]) == true)
                        {
                            CreatAndSendPushChannel();
                        }
                        else
                        {
                            UnregisterSubscriber();
                        }
                    }
                    else
                    {
                        statusLabel.Text = "Invalid Credentials";
                    }
                }
                catch
                {
                    new MessageDialog("Unable to connect to Riveu server. Please verify internet connection and re-launch application").ShowAsync();
                }
            }
            else
            {
                statusLabel.Text = "Please configure settings";
            }
        }

        async void timer_Tick(object sender, object e)
        {
            try
            {
                NotificationService.NotificationServiceClient client = new NotificationService.NotificationServiceClient();
                if (await client.AuthenticateUserAsync(username, password))
                {
                    var data = await client.GetNotificationsAsync(username);
                    if (data.Count != ((ObservableCollection<object>)listView.ItemsSource).Count)
                    {
                        listView.ItemsSource = data;
                    }

                }
            }
            catch
            {
                new MessageDialog("Unable to connect to Riveu server. Please verify internet connection and re-launch application").ShowAsync();
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void sendNotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SendNotification));
        }

        private async void CreatAndSendPushChannel()
        {
            PushNotificationChannel channel = null;
            try
            {
                channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                channel.PushNotificationReceived += channel_PushNotificationReceived;
                NotificationService.NotificationServiceClient client = new NotificationService.NotificationServiceClient();
                string deviceType = "Win8";
                var token = HardwareIdentification.GetPackageSpecificToken(null);
                var hardwareId = token.Id;
                var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);
                byte[] bytes = new byte[hardwareId.Length];
                dataReader.ReadBytes(bytes);
                string deviceId = BitConverter.ToString(bytes);
                await client.RegisterSubscriberAsync(username, password, channel.Uri, deviceType, deviceId);
                
            }
            catch(Exception ex)
            {
                //do nothing
            }
            
        }

        async void channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            
        }

        public async void UnregisterSubscriber()
        {
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;
            var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);
            byte[] bytes = new byte[hardwareId.Length];
            dataReader.ReadBytes(bytes);
            string deviceId = BitConverter.ToString(bytes);
            NotificationService.NotificationServiceClient client = new NotificationService.NotificationServiceClient();
            await client.UnregisterSubscriberAsync(username, password, "Win8", deviceId);
        }
    }
}
