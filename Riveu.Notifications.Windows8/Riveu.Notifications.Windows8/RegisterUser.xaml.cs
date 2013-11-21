using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class RegisterUser : UserControl
    {
        public RegisterUser()
        {
            this.InitializeComponent();
        }

        private void SettingsBackClicked(object sender, RoutedEventArgs e)
        {
            if (this.Parent.GetType() == typeof(Popup))
            {
                ((Popup)this.Parent).IsOpen = false;
            }
            SettingsPane.Show();
        }

        private async void createAccountButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Password;

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
            {
                try
                {
                    if (true)//check account
                    {
                        if (true)//create account
                        {
                            await new MessageDialog("Account has been successfully created. You may use this new account by updating the application settings.").ShowAsync();
                        }
                        else
                        {
                            await new MessageDialog("Unable to create account. Please try again.").ShowAsync();
                        }
                    }
                    else
                    {
                        await new MessageDialog("Username already in use. Please try a different one.").ShowAsync();
                    }
                }
                catch
                {
                    new MessageDialog("Unable to connect to service. Please verify internet connection and try again.").ShowAsync();
                }
            }
            else
            {
                await new MessageDialog("Username and Password are both required. Please ensure both are entered and try again.").ShowAsync();
            }
        }
    }
}
