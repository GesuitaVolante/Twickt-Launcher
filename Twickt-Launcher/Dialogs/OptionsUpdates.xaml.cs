﻿// Copyright (c) 2016 Twickt / Ceschia Davide
//Application idea, code and time are given by Davide Ceschia / Twickt
//You may use them according to the GNU GPL v.3 Licence
//GITHUB Project: https://github.com/killpowa/Twickt-Launcher

/*Finestra per l'aggiornamento del launcher*/
using System;
using System.Collections.Generic;
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

namespace Twickt_Launcher.Dialogs
{
    /// <summary>
    /// Logica di interazione per OptionsUpdates.xaml
    /// </summary>
    public partial class OptionsUpdates : UserControl
    {
        public OptionsUpdates(string message, int width = 248, int height = 57, bool button = false, string buttontext = "Close")
        {
            InitializeComponent();
            textBlock.Text = message;
            this.Width = width;
            this.Height = height;
            if (button == false)
            {
                btn.Visibility = Visibility.Hidden;
                textBlock.Height = height - 5;
                textBlock.Width = width - 20;
            }
            else
            {
                btn.Visibility = Visibility.Visible;
                textBlock.Height = height - 33;
                textBlock.Width = width - 20;
                btn.Content = buttontext;
            }
        }

    }
}
