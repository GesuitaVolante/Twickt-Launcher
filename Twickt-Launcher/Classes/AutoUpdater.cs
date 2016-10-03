﻿// Copyright (c) 2016 Twickt / Ceschia Davide
//Application idea, code and time are given by Davide Ceschia / Twickt
//You may use them according to the GNU GPL v.3 Licence
//GITHUB Project: https://github.com/killpowa/Twickt-Launcher

/*Questa classe gestisce il sistema di autoaggiornamento del launcher*/
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Twickt_Launcher.Classes
{
    class AutoUpdater
    {

        public static Stopwatch sw = new Stopwatch();
        public long second = 0;
        public static string documents = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Twickt\\";
        public static WebClient webClient = new WebClient();

        public static async Task<string> CheckVersion()
        {
            string update = "";
            Windows.DebugOutputConsole.singleton.Write(Pages.SplashScreen.singleton.manager.GetString("checkingForUpdates"));
            string data = "";
            var client = new WebClient();
            var values = new System.Collections.Specialized.NameValueCollection();

            try
            {
                var response = await client.UploadValuesTaskAsync(config.updatessWebService, values);

                var responseString = Encoding.Default.GetString(response);

                if (!responseString.Contains("0results"))
                {
                    data = responseString;
                }
                else
                {
                    MessageBox.Show("Error getting updates. Shutting down");
                    Application.Current.Shutdown();
                }

            }
            catch
            {
                MessageBox.Show("Error updating");
            }

            try
            {
                dynamic json = JsonConvert.DeserializeObject(data);
                var version = (string)json["LatestVersion"];
                var versionurl = (string)json["LatestVersionURL"];
                var minimumversion = (string)json["MinimumVersion"];
                var minimumversionurl = (string)json["MinimumVersionUrl"];
                SessionData.latestVersion = version;
                update = versionurl;
                if (String.Compare(Properties.Settings.Default["version"].ToString(), minimumversion) < 0)
                {
                    var test = new Dialogs.UpdateAvailable(Properties.Settings.Default["version"].ToString(), version, Pages.SplashScreen.singleton.manager.GetString("youMustUpdate"));
                    var result = await MaterialDesignThemes.Wpf.DialogHost.Show(test, "RootDialog");
                    if (result.ToString() == "yes")
                    {
                        Pages.SplashScreen.singleton.firstLabel.Visibility = Visibility.Visible;
                        Pages.SplashScreen.singleton.secondLabel.Visibility = Visibility.Visible;
                        Pages.SplashScreen.singleton.progressbar.Visibility = Visibility.Visible;
                        Pages.SplashScreen.singleton.progressbar.IsIndeterminate = false;
                        Pages.SplashScreen.singleton.mainContent.Visibility = Visibility.Visible;
                        Pages.SplashScreen.singleton.mbToDownload.Visibility = Visibility.Visible;
                        Pages.SplashScreen.singleton.kbps.Visibility = Visibility.Visible;
                        Pages.SplashScreen.singleton.load.Visibility = Visibility.Hidden;
                        Window1.versionok = false;
                        Pages.SplashScreen.singleton.firstLabel.Content = Pages.SplashScreen.singleton.manager.GetString("updatingLauncherCompulsory");
                        Pages.SplashScreen.singleton.secondLabel.Content = Pages.SplashScreen.singleton.manager.GetString("updating") + " " + Properties.Settings.Default["version"].ToString() + " -> " + version;
                        Pages.SplashScreen.singleton.mainContent.Content = Pages.SplashScreen.singleton.manager.GetString("updating") + "...";
                        Windows.DebugOutputConsole.singleton.Write(Pages.SplashScreen.singleton.manager.GetString("compulsoryUpdateFound"));
                        return update;
                    }
                    else
                    {
                        MessageBox.Show("Questo aggiornamento e' necessario per avviare il launcher. Se non aggiorni non potrai avviare il launcher.");
                        Application.Current.Shutdown();
                    }
                }
                else if (String.Compare(Properties.Settings.Default["version"].ToString(), version) < 0 || Properties.Settings.Default["version"].ToString() == "")
                {
                    Pages.SplashScreen.singleton.firstLabel.Visibility = Visibility.Visible;
                    Pages.SplashScreen.singleton.secondLabel.Visibility = Visibility.Visible;
                    Pages.SplashScreen.singleton.progressbar.Visibility = Visibility.Visible;
                    Pages.SplashScreen.singleton.progressbar.IsIndeterminate = false;
                    Pages.SplashScreen.singleton.mainContent.Visibility = Visibility.Visible;
                    Pages.SplashScreen.singleton.mbToDownload.Visibility = Visibility.Visible;
                    Pages.SplashScreen.singleton.kbps.Visibility = Visibility.Visible;
                    Pages.SplashScreen.singleton.load.Visibility = Visibility.Hidden;
                    var test = new Dialogs.UpdateAvailable(Properties.Settings.Default["version"].ToString(), version);
                    var result = await MaterialDesignThemes.Wpf.DialogHost.Show(test, "RootDialog");
                    if (result.ToString() == "yes")
                    {
                        Window1.versionok = false;
                        Pages.SplashScreen.singleton.firstLabel.Content = Pages.SplashScreen.singleton.manager.GetString("updatingLauncher");
                        Pages.SplashScreen.singleton.secondLabel.Content = Pages.SplashScreen.singleton.manager.GetString("updating") + " " + Properties.Settings.Default["version"].ToString() + " -> " + version;
                        Pages.SplashScreen.singleton.mainContent.Content = Pages.SplashScreen.singleton.manager.GetString("updating") + "...";
                        Windows.DebugOutputConsole.singleton.Write(Pages.SplashScreen.singleton.manager.GetString("updateFound"));
                    }
                    else
                    {

                    }
                }
                else
                {
                    if (Pages.Options.optionsrequest == true)
                    {
                        await DialogHost.Show(new Dialogs.OptionsUpdates(Pages.SplashScreen.singleton.manager.GetString("noUpdatesAvailable"), 248, 40), "RootDialog", ExtendedOpenedEventHandler);
                        Windows.DebugOutputConsole.singleton.Write(Pages.SplashScreen.singleton.manager.GetString("noUpdatesAvailable"));
                    }
                    else
                    {
                        Windows.DebugOutputConsole.singleton.Write(Pages.SplashScreen.singleton.manager.GetString("noUpdatesAvailable"));
                    }
                }
            }
            catch (JsonReaderException)
            {
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR"); ;
            }
            return update;
        }


        public async static Task Download(string url)
        {
            if (Window1.versionok == false)
            {
                string filename = "";
                Uri uri = new Uri(url);
                filename = System.IO.Path.GetFileName(uri.LocalPath);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                sw.Start();
                try
                {
                    await webClient.DownloadFileTaskAsync(new Uri(url), Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + filename);
                    Properties.Settings.Default["version"] = SessionData.latestVersion;
                    Properties.Settings.Default.Save();
                }
                catch(UriFormatException)
                {
                    MessageBox.Show("Could not verify version");
                }
                catch(Exception e)
                {
                    File.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + filename);
                    MessageBox.Show(e.Message);
                    return;
                }
                Properties.Settings.Default["firstTimeHowTo"] = "true";
                Properties.Settings.Default["justUpdated"] = "true";
                Properties.Settings.Default.Save();
                ProcessStartInfo Info = new ProcessStartInfo();
                Info.Arguments = "/C choice /C Y /N /D Y /T 3 & Del " + "\"" +
                       System.Reflection.Assembly.GetExecutingAssembly().Location + "\"";
                Info.WindowStyle = ProcessWindowStyle.Hidden;
                Info.CreateNoWindow = true;
                Info.FileName = "cmd.exe";
                Process.Start(Info);
                Process.Start(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + filename);
                Application.Current.Shutdown();
            }
        }


        public static void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            /*MainWindow.singleton.splash.progressbar.Value = e.ProgressPercentage;

            //Form1.singleton.progressBar.Value = e.ProgressPercentage;
            MainWindow.singleton.splash.kbps.Content = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            MainWindow.singleton.splash.todownload.Content = string.Format("{0} MB / {1} MB",
            (e.BytesReceived / 1024d / 1024d).ToString("0"),
            (e.TotalBytesToReceive / 1024d / 1024d).ToString("0"));*/
            /*
            speedmb.Text = string.Format("{0} MB/s", (e.BytesReceived / 1024d / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            seconds = Convert.ToInt32((((sw.ElapsedMilliseconds / 1000) / (e.BytesReceived / 1024d)) * ((e.TotalBytesToReceive / 1024d) - (e.BytesReceived / 1024d))));
            timeremaining.Text = string.Format("{0} s", seconds.ToString("0"));
            //timeremaining = string.Format("{0} s", seconds.ToString("0"));*/

            Pages.SplashScreen.singleton.progressbar.Value = e.ProgressPercentage;
            Pages.SplashScreen.singleton.kbps.Content = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            Pages.SplashScreen.singleton.mbToDownload.Content = string.Format("{0} MB / {1} MB",
            (e.BytesReceived / 1024d / 1024d).ToString("0"),
            (e.TotalBytesToReceive / 1024d / 1024d).ToString("0"));

        }

        private static async void Completed(object sender, AsyncCompletedEventArgs e)
        {
            sw.Reset();
            if (e.Cancelled == true)
            {
                webClient.Dispose();
                return;
            }
            else
            {
                MessageBox.Show(Pages.SplashScreen.singleton.manager.GetString("openingNewVersion"));
                //await DialogHost.Show(new Dialogs.OptionsUpdates("In a few moments we will open the new version"), "RootDialog", ExtendedOpenedEventHandler);
            }

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
        public static void DownloadStop()
        {
            webClient.CancelAsync();
        }
    }
}
