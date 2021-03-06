﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Riveu.Notifications.Windows8
{
    public sealed partial class SettingsUserControl : UserControl
    {
        public SettingsUserControl()
        {
            this.InitializeComponent();
            this.Loaded += SettingsUserControl_Loaded;
        }

        void SettingsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("EnablePush"))
            {
                enablePushNotification.IsOn = Convert.ToBoolean(ApplicationData.Current.LocalSettings.Values["EnablePush"]);
            }
            else
            {
                enablePushNotification.IsOn = false;
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Username"))
            {
                usernameTextbox.Text = ApplicationData.Current.LocalSettings.Values["Username"].ToString();
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Password"))
            {
                passwordTextbox.Password = ApplicationData.Current.LocalSettings.Values["Password"].ToString();
            }

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("RefreshRate"))
            {
                refreshRateTextbox.Text = ApplicationData.Current.LocalSettings.Values["RefreshRate"].ToString();
            }

        }

        private void SettingsBackClicked(object sender, RoutedEventArgs e)
        {
            if (this.Parent.GetType() == typeof(Popup))
            {
                ((Popup)this.Parent).IsOpen = false;
            }
            SettingsPane.Show();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog messageBox;
            try
            {
                
                if (await new NotificationService.NotificationServiceClient().AuthenticateUserAsync(usernameTextbox.Text, passwordTextbox.Password))
                {
                    messageBox = new MessageDialog("Settings Saved");
                    if (String.IsNullOrEmpty(usernameTextbox.Text) || String.IsNullOrEmpty(passwordTextbox.Password))
                    {
                        messageBox = new MessageDialog("Username and Password are required");
                    }
                    int dummyVariable = 0;
                    if (!Int32.TryParse(refreshRateTextbox.Text, out dummyVariable))
                    {
                        messageBox = new MessageDialog("Refresh rate must be a number.");
                    }
                    else if((dummyVariable <1 ||  dummyVariable > 59))
                    {
                        messageBox = new MessageDialog("Refresh rate must be a number between 1 and 59");
                    }
                    if (messageBox.Content == "Settings Saved")
                    {
                        ApplicationData.Current.LocalSettings.Values["EnablePush"] = enablePushNotification.IsOn;
                        ApplicationData.Current.LocalSettings.Values["Username"] = usernameTextbox.Text;
                        ApplicationData.Current.LocalSettings.Values["Password"] = passwordTextbox.Password;
                        ApplicationData.Current.LocalSettings.Values["RefreshRate"] = refreshRateTextbox.Text;
                    }
                }
                else
                {
                    messageBox = new MessageDialog("Invalid Credentials. Please verify and try again.");
                }
            }
            catch
            {
                messageBox = new MessageDialog("An error occured while trying to validate the credentials. Please verify internet connection and try again.");
            }
            await messageBox.ShowAsync();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://riveu.com/"));
        }
    }
}
