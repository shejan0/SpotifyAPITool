using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpotifyAPIToolGUI
{
    /// <summary>
    /// Interaction logic for InteractExtendedStreamingHistory.xaml
    /// </summary>
    public partial class InteractExtendedStreamingHistory : Window
    {
        private List<StreamingHistoryItem> streamingHistories;
        private List<string> filenames;
        public InteractExtendedStreamingHistory(List<string> extended)
        {
            filenames = extended;
            InitializeComponent();
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            parseJSONs();
        }
        private async void parseJSONs()
        {
            streamingHistories = new();
            foreach (string filename in filenames)
            {
                string filecontent = File.ReadAllText(filename);
                List<StreamingHistoryItem> filehist = JsonConvert.DeserializeObject<List<StreamingHistoryItem>>(filecontent);
                foreach (StreamingHistoryItem hist in filehist)
                {
                    try
                    {
                        hist.parseObject();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(hist);
                    }
                }
                streamingHistories.AddRange(filehist);
            }
            streamingHistories = streamingHistories.OrderByDescending(hist => hist.TSDateTime).ToList();
            loadCountLabel.Content = $"Loaded {streamingHistories.Count} items from Extended History";
            startDate.Visibility = Visibility.Visible;
            startLabel.Visibility = Visibility.Visible;
            endDate.Visibility = Visibility.Visible;
            endLabel.Visibility = Visibility.Visible;
            findInDateButton.Visibility = Visibility.Visible;
            startDate.SelectedDate=streamingHistories.Last().TSDateTime;
            endDate.SelectedDate=streamingHistories.First().TSDateTime;
        }

        private void findInDateButton_Click(object sender, RoutedEventArgs e)
        {
           InRangeList.ItemsSource=streamingHistories.Where(x => x.TSDateTime >= startDate.SelectedDate && x.TSDateTime <= endDate.SelectedDate).ToList();
           InRangeList.Visibility=Visibility.Visible;
           getMostPlayed.Visibility=Visibility.Visible;
        }

        private void getMostPlayed_Click(object sender, RoutedEventArgs e)
        {
            List<StreamingHistoryItem> list = (List<StreamingHistoryItem>)InRangeList.ItemsSource;
            //var occurrences = list.Select(x => x.TrackUri).GroupBy(item => item).Select(group => new { Item = group.Key, Count = group.Count()}).OrderByDescending(x=>x.Count).ToList();
            var occurrences =
    list.GroupBy(x => x.TrackUri)
        .Select(g => new
        {
            TrackUri = g.Key,
            Count = g.Count(),
            Items = g.ToList()   // full objects preserved
        })
        .OrderByDescending(x => x.Count)
        .ToList();

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "TSV files (*.tsv)|*.tsv",
                DefaultExt = "tsv",
                AddExtension = true,
                FileName = $"mostplayed-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.tsv"
            };
            if (saveFileDialog.ShowDialog().Value)
            {
                using (var writer = new StreamWriter(saveFileDialog.FileName)
                {
                    AutoFlush = true
                })
                {
                    writer.WriteLine($"{startDate.SelectedDate}\t{endDate.SelectedDate}");
                    writer.WriteLine("ID\tOccurences\tName\tArtist");
                    foreach(var a in occurrences)
                    {
                        List<string> details = new()
                        {
                            a.TrackUri,
                            a.Count.ToString(),
                            a.Items.First().master_metadata_track_name,
                            a.Items.First().master_metadata_album_artist_name,
                        };
                        writer.WriteLine(string.Join('\t', details));
                    }
                }
            }
        }
    }

    public class StreamingHistoryItem
    {
        public string ts { get; set; }
        public string platform { get; set; }
        public int ms_played { get; set; }
        public string conn_country { get; set; }
        public string ip_addr { get; set; }
        public string master_metadata_track_name { get; set; }
        public string master_metadata_album_artist_name { get; set; }
        public string master_metadata_album_album_name { get; set; }
        public string spotify_track_uri { get; set; }
        public string episode_name { get; set; }
        public string episode_show_name { get; set; }
        public string spotify_episode_uri { get; set; }
        public string audiobook_title { get; set; }
        public string audiobook_uri { get; set; }
        public string audiobook_chapter_uri { get; set; }
        public string audiobook_chapter_title { get; set; }
        public string reason_start { get; set; }
        public string reason_end { get; set; }
        public bool shuffle { get; set; }
        public bool skipped { get; set; }
        public bool offline { get; set; }
        public long? offline_timestamp { get; set; }
        public bool incognito_mode { get; set; }
        public IPAddress? IPAddress { get; set; }
        public DateTime? TSDateTime { get; set; }
        public DateTime? OfflineTimeStamp { get; set; }
        public string TrackUri { get; set; }
        public void parseObject()
        {
            IPAddress br;
            if(IPAddress.TryParse(ip_addr, out br))
            {
                IPAddress = br;
            }
            DateTime ss;
            if(DateTime.TryParse(ts,out ss))
            {
                TSDateTime = ss;
            }

            OfflineTimeStamp = offline_timestamp.HasValue
                ? DateTimeOffset.FromUnixTimeMilliseconds(offline_timestamp.Value).DateTime
                : (DateTime?)null;

            TrackUri = string.IsNullOrEmpty(spotify_track_uri)
                ? ""
                : spotify_track_uri.Split(':').Last();
        }
        public override string ToString()
        {
            return $"{TSDateTime}: {master_metadata_track_name} by {master_metadata_album_artist_name}";
        }
    }
}
