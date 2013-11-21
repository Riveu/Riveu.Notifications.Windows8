using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class SendNotification : Page
    {
        public SendNotification()
        {
            this.InitializeComponent();
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NotificationService.NotificationServiceClient client = new NotificationService.NotificationServiceClient();
                string username = ApplicationData.Current.LocalSettings.Values["Username"] as String;
                string password = ApplicationData.Current.LocalSettings.Values["Password"] as String;
                if (!String.IsNullOrEmpty(messageTextBox.Text.Trim()))
                {
                    if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                    {
                        await client.SendNotificationAsync(username, password, messageTextBox.Text);
                        MessageDialog messageDialog = new MessageDialog("Message sent successfully.");
                        await messageDialog.ShowAsync();
                        messageTextBox.Text = String.Empty;
                    }
                    else
                    {
                        MessageDialog messageDialog = new MessageDialog("Please verify credentials are set in the settings.");
                        await messageDialog.ShowAsync();
                    }
                }
                else
                {
                    await new MessageDialog("Please ensure that you enter a message").ShowAsync();
                }
            }
            catch
            {
                MessageDialog messageDialog = new MessageDialog("Unable to send message. Please verify credentials are set in the settings and you have an internet connection.");
                messageDialog.ShowAsync();
            }
        }

        private void viewNotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
