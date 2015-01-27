﻿using System;
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

using DataCloner.DataClasse;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;

using Microsoft.Win32;

namespace DataCloner.GUI
{
    public partial class MainWindow : Window
    {
        private const string FILE_ELEMENT = "File";
        private const string FILE_EXTENSION = ".dca";
        private const string FILTER = "Datacloner archive (.dca)|*.dca";

        private ServersMapsXml _serversMaps;
        private Cloner cloner = new Cloner();

        public MainWindow()
        {
            InitializeComponent();
            
            _serversMaps = Extensions.LoadXml<ServersMapsXml>("serversMaps.config");
            
            //Combobox source
            var listSources =  _serversMaps.Maps.Select(m => m.nameFrom).Distinct().ToList();
            listSources.Insert(0, FILE_ELEMENT);
            cbServerSource.ItemsSource = listSources;
        }

        private void cbServerSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbServerDestination.ItemsSource = _serversMaps.Maps.Where(m=>m.nameFrom == cbServerSource.SelectedValue.ToString())
                                                               .Select(m => m.nameTo).Distinct().ToList();

            //File selected
            if (cbServerSource.SelectedIndex == 0)
            {
                var dlg = new OpenFileDialog();
                dlg.DefaultExt = FILE_EXTENSION;
                dlg.Filter = FILTER;
                if (dlg.ShowDialog() == true)
                {
                    string fileName = dlg.FileName;
                }
            }
        }

        private void cbServerDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cloner.SaveToFile = false;
            
            if (cbServerDestination.SelectedValue.ToString() == FILE_ELEMENT)
            {
                var dlg = new SaveFileDialog();
                dlg.DefaultExt = FILE_EXTENSION;
                dlg.Filter = FILTER ;
                if (dlg.ShowDialog() == true)
                {
                    cloner.SaveToFile = true;
                    cloner.SavePath = dlg.FileName;
                }
            }
        }

        private void btnExec_Click(object sender, RoutedEventArgs e)
        {
            var selectedMap = _serversMaps.Maps.FirstOrDefault(m => m.nameFrom == cbServerSource.SelectedValue.ToString() &&
                                                                    m.nameTo == cbServerDestination.SelectedValue.ToString());
            cloner.ServerMap = selectedMap;
            cloner.Initialize();

            ////Basic test : 1 row
            var source = new RowIdentifier();
            source.ServerId = 1;
            source.Database = "sakila";
            source.Schema = "";
            source.Table = "actor";
            source.Columns.Add("actor_id", 1);

            //cloner.SqlTraveler(source, true, false);
        }
    }
}