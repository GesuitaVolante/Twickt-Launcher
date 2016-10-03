﻿// Copyright (c) 2016 Twickt / Ceschia Davide
//Application idea, code and time are given by Davide Ceschia / Twickt
//You may use them according to the GNU GPL v.3 Licence
//GITHUB Project: https://github.com/killpowa/Twickt-Launcher

/*Caricamento dei pacchetti, attualmente inutile(credo)*/
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
    /// Logica di interazione per ModpackLoading.xaml
    /// </summary>
    public partial class ModpackLoading : UserControl
    {
        public static ModpackLoading singleton;
        public ModpackLoading(bool btn = false, string message = "Loading...")
        {
            InitializeComponent();
            singleton = this;
            if (btn == true)
                button.Visibility = Visibility.Visible;

            forgeProgress.Value = 0;
            label.Content = message;
        }
    }
}
