using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using ClientDetails;
using SpotifyAPI.Web.Http;
using Swan.Logging;
using Microsoft.Win32;

namespace SpotifyAPIToolGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpotifyClient client=null;
        private List<string> ExtendedStreamingHistory=null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LinkSpotifyAPI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SpotifyAPILinker linker = new();
                client = linker.Link();
                LinkSpotifyAPI.Content = "Connected to Spotify API";
                LinkSpotifyAPI.IsEnabled = false;
                continueButton.IsEnabled = true;
            }catch(Exception ex)
            {
                errorLabel.Content= ex.Message;
            }

        }

        private void OpenExtendedFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new()
            {
                InitialDirectory = Environment.CurrentDirectory,
                Multiselect = false,
                Title="Open Extended Streaming History Folder",
            };
            if (folderDialog.ShowDialog().Value)
            {
                string foldername = folderDialog.FolderName;
                ExtendedStreamingHistory = Directory.EnumerateFiles(foldername, "Streaming_History_Audio_*.json").ToList();
                ((TextBlock)OpenExtendedFolder.Content).Text = $"Currently selected: \"{foldername}\", with {ExtendedStreamingHistory.Count} matching items, CLICK AGAIN TO RESELECT";
                continueButton.IsEnabled = true;
            }
            
        }

        private void continueButton_Click(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                InteractSpotifyAPI interactspo = new(client);
                interactspo.Show();
            }
            if (ExtendedStreamingHistory != null && ExtendedStreamingHistory.Count !=0)
            {
                InteractExtendedStreamingHistory interactext = new(ExtendedStreamingHistory,client);
                interactext.Show();
            }
            this.Close();
        }
    }
}