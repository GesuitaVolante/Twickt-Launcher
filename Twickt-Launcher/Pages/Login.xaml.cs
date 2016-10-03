﻿// Copyright (c) 2016 Twickt / Ceschia Davide
//Application idea, code and time are given by Davide Ceschia / Twickt
//You may use them according to the GNU GPL v.3 Licence
//GITHUB Project: https://github.com/killpowa/Twickt-Launcher

/*Pagina di login*/
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Twickt_Launcher.Pages
{
    /// <summary>
    /// Logica di interazione per Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
            Window1.singleton.MenuToggleButton.IsEnabled = false;
            Window1.singleton.popupbox.IsEnabled = false;
            Window1.singleton.homeButton.IsEnabled = false;
            transition.SelectedIndex = 0;
            language.Text = Thread.CurrentThread.CurrentUICulture.Name;

        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static string sha256(string password)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;
            loading.Visibility = Visibility.Visible;
            sha256(password.Password);
            var client = new WebClient();
            var values = new NameValueCollection();
            values["username"] = username.Text;
            values["password"] = sha256(password.Password);
            values["appVersion"] = Properties.Settings.Default["version"].ToString();

            try
            {
                var response = await client.UploadValuesTaskAsync(config.loginWebService, values);
                var responseString = Encoding.Default.GetString(response);
                if (responseString.Contains("true"))
                {
                    /*var values1 = new NameValueCollection();
                    values["username"] = username.Text;
                    values["password"] = sha256(password.Password);
                    var response1 = await client.UploadValuesTaskAsync(config.loginWebService, values1);
                    var responseString1 = Encoding.Default.GetString(response);*/

                    var userdata = responseString.Split(';');

                    SessionData.username = userdata[2];
                    SessionData.email = userdata[3];
                    SessionData.isAdmin = userdata[4];
                    if (userdata[3] == "false")
                    {
                        try
                        {
                            var client1 = new WebClient();
                            var values1 = new System.Collections.Specialized.NameValueCollection();
                            var response1 = await client1.UploadValuesTaskAsync(config.launcherStatusWebService, values1);
                            var responseString1 = Encoding.Default.GetString(response1);
                            if (responseString1.Contains("enabled;true"))
                            {
                                MessageBox.Show("Attualmente solo i fucking admin possono accedere. Tu sei plebeo e riprovi piu' tardi");
                                Application.Current.Shutdown();
                            }
                        }
                        catch { }
                    }
                    Window1.singleton.MenuToggleButton.IsEnabled = true;
                    Window1.singleton.popupbox.IsEnabled = true;
                    Window1.singleton.homeButton.IsEnabled = true;
                    Window1.singleton.loggedinName.Text = "Logged in as " + username.Text;
                    transition.SelectedIndex = 2;
                    await Task.Delay(500);
                    //Properties.Settings.Default["Sessiondata"] = SessionData.username + ";" + SessionData.email + ";" + SessionData.isAdmin;
                    //Properties.Settings.Default.Save();
                    if (keepMeIn.IsChecked == true)
                    {
                        Properties.Settings.Default["RememberUsername"] = username.Text;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        Properties.Settings.Default["RememberUsername"] = "";
                        Properties.Settings.Default.Save();
                    }
                    if (Properties.Settings.Default["firstTimeHowTo"].ToString() == "true")
                    {
                        Window1.singleton.MainPage.Navigate(new Dialogs.HowTo());
                    }
                    else
                    {
                        if (Properties.Settings.Default["startingPage"].ToString() == "Home")
                        {
                            Window1.singleton.MainPage.Navigate(new Pages.Home());
                            Window1.singleton.NavigationMenu.SelectedIndex = 0;
                        }

                        else if (Properties.Settings.Default["startingPage"].ToString() == "Modpacks")
                        {
                            Window1.singleton.MainPage.Navigate(new Pages.Modpacks());
                            Window1.singleton.NavigationMenu.SelectedIndex = 2;
                        }
                    }
                }
                else
                {
                    if (responseString.Contains("notconfirmed"))
                    {
                        await DialogHost.Show(new Dialogs.OptionsUpdates("Account not confirmed yet. Check your email"), "RootDialog", ExtendedOpenedEventHandler);
                    }
                    else if (responseString.Contains("banned"))
                    {
                        await DialogHost.Show(new Dialogs.OptionsUpdates("Account Banned"), "RootDialog", ExtendedOpenedEventHandler);
                    }
                    else
                    {
                        await DialogHost.Show(new Dialogs.OptionsUpdates("Wrong username or password"), "RootDialog", ExtendedOpenedEventHandler);
                    }

                }
            }
            catch (TimeoutException timeout)
            {
                MessageBox.Show("Timeout del server. Probabilmente hai una connessione molto instabile");
            }
            catch(WebException interneterror)
            {
                MessageBox.Show("C'e' stato un errore con la rete.");
            }
            //alice encrypts a message for bob
            //var encrypted = PublicKeyBox.Create(MESSAGE, nonce, alice.PrivateKey, bob.PublicKey);
            //bob decrypt the message
            //var decrypted = PublicKeyBox.Open(encrypted, nonce, bob.PrivateKey, alice.PublicKey);
            loading.Visibility = Visibility.Hidden;
            button.IsEnabled = true;
        }

        private static async void ExtendedOpenedEventHandler(object sender, MaterialDesignThemes.Wpf.DialogOpenedEventArgs eventArgs)
        {
            try
            {
                await Task.Delay(1200);
                eventArgs.Session.Close();
            }
            catch
            {
                /*cancelled by user...tidy up and dont close as will have already closed */
            }
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Return))
            {
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }


        private static async void erroropenEvent(object sender, MaterialDesignThemes.Wpf.DialogOpenedEventArgs eventArgs)
        {
            try
            {
                await Task.Delay(2000);
                eventArgs.Session.Close();
            }
            catch (TaskCanceledException)
            {
                /*cancelled by user...tidy up and dont close as will have already closed */
            }
            catch
            {

            }
        }
        private static async void ExtendedOpenedEventHandlerLocal(object sender, MaterialDesignThemes.Wpf.DialogOpenedEventArgs eventArgs)
        {
            do
            {
                await Task.Delay(100);
            }
            while (Dialogs.Register.close == false);
            try
            {
                Dialogs.Register.close = false;
                eventArgs.Session.Close();
            }
            catch { }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(50);
            transition.SelectedIndex = 1;
            if(Properties.Settings.Default["RememberUsername"].ToString() != "")
            {
                username.Text = Properties.Settings.Default["RememberUsername"].ToString();
                password.Focus();
                keepMeIn.IsChecked = true;
            }

        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                await MaterialDesignThemes.Wpf.DialogHost.Show(new Dialogs.Register(), "RootDialog", ExtendedOpenedEventHandlerLocal);
            }
            catch (InvalidOperationException ex)
            {

            }
        }
    }
}
