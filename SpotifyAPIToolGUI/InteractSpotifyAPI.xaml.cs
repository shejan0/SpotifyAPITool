using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using Humanizer;
using Microsoft.Win32;
namespace SpotifyAPIToolGUI
{
    /// <summary>
    /// Interaction logic for InteractSpotifyAPI.xaml
    /// </summary>
    public partial class InteractSpotifyAPI : Window
    {
        private SpotifyClient client;
        private List<FullTrack> likedTracks;

        public InteractSpotifyAPI(SpotifyClient cl)
        {
            InitializeComponent();
            client = cl;
        }
        private void pullLiked_Click(object sender, RoutedEventArgs e)
        {
            pullLiked.IsEnabled = false;
            pullLikedList();
        }
        private async void pullLikedList()
        {
            int n = 0;
            likedTracks = new();
            List<SavedTrack> liked = new();
            try
            {
                var saved = await client.Library.GetTracks(new LibraryTracksRequest()
                {
                    Limit = 50
                });
                while (n < saved.Total)
                {
                    try
                    {
                        saved = await client.Library.GetTracks(new LibraryTracksRequest()
                        {
                            Limit = 50,
                            Offset = n
                        });

                    }
                    catch (APIException ex)
                    {
                        Console.Error.WriteLine(ex.Message);
                        Console.Error.WriteLine(ex.StackTrace);
                        Console.Error.WriteLine(ex.Response.StatusCode);
                        Console.Error.WriteLine(ex.Response.Body);
                        if (ex.Response.StatusCode == System.Net.HttpStatusCode.BadGateway)
                        {
                            continue;
                        }
                        throw;
                    }
                    liked.AddRange(saved.Items);
                    for (int a = n; a < liked.Count(); a++)
                    {
                        FullTrack track = liked[a].Track;
                        if (track.IsLocal)
                        {
                            continue;
                        }
                        likedTracks.Add(track);
                    }
                    n += saved.Items.Count;
                    pullingLabel.Content = "Current size of saved in RAM: " + n;
                }
            }
            catch(APITooManyRequestsException ex)
            {
                MessageBox.Show($"The API Key currently has a wait of {ex.RetryAfter}");
                throw;
            }
            pullLiked.IsEnabled = true;
            writeTSVButton.Visibility = Visibility.Visible;
            showLength.Visibility= Visibility.Visible;
            createPlayListButton.Visibility = Visibility.Visible;
        }

        private void writeTSVButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "TSV files (*.tsv)|*.tsv",
                DefaultExt = "tsv",
                AddExtension = true,
                FileName = $"saved-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.tsv"
            };
            if (saveFileDialog.ShowDialog().Value)
            {
                using (var writer = new StreamWriter(saveFileDialog.FileName)
                {
                    AutoFlush = true
                })
                {
                    writer.WriteLine("ID\tName\tArtists\tDurationMilliSeconds\tDurationHumanReadable\tAlbumName\tUri");
                    foreach (FullTrack track in likedTracks)
                    {
                        List<string> trackdetails = new()
                        {
                            track.Id,
                            track.Name,
                            string.Join('|', track.Artists.Select(x => x.Name)),
                            (track.DurationMs).ToString(),
                            TimeSpan.FromMilliseconds(track.DurationMs).Humanize(precision:3,minUnit:Humanizer.Localisation.TimeUnit.Second),
                            track.Album.Name,
                            track.Uri,
                        };
                        writer.WriteLine(string.Join('\t', trackdetails));
                    }
                }
            }

        }

        private void showLength_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan durationsongs = new TimeSpan();
            foreach (FullTrack track in likedTracks)
            {
                durationsongs += TimeSpan.FromMilliseconds(track.DurationMs);
            }
            MessageBox.Show(durationsongs.Humanize(precision: 10, maxUnit: Humanizer.Localisation.TimeUnit.Month), "Calculated Duration", MessageBoxButton.OK);
        }

        private void createPlayList_Click(object sender, RoutedEventArgs e)
        {
            createPlayListButton.IsEnabled = false;
            createPlayListButton.Content = "Creating";
            createPlayList();   
        }
        private async void createPlayList()
        {
            PrivateUser p = await client.UserProfile.Current();
            string playlistname = "Liked " + DateTime.Now.ToString();
            int parts = -1;

            try
            {
                FullPlaylist newplaylist = await client.Playlists.Create(new PlaylistCreateRequest(playlistname)
                {
                    Public = false,
                });
                for (int n = 0; n < likedTracks.Count; n += 50)
                {
                    if (n % 10000 == 0)
                    {
                        newplaylist = await client.Playlists.Create(new PlaylistCreateRequest($"{playlistname}-part {parts}")
                        {
                            Public = false,
                        });
                        parts--;
                    }
                    var saved = likedTracks.Skip(n).Take(50).ToList();
                    await client.Playlists.AddPlaylistItems(newplaylist.Id, new PlaylistAddItemsRequest(saved.Select(x => x.Uri).ToList()));
                }
            }
            catch(APITooManyRequestsException ex)
            {
                MessageBox.Show($"The API Key currently has a wait of {ex.RetryAfter}");
                throw;
            }
            
            
            createPlayListButton.IsEnabled = true;
            createPlayListButton.Content = "Recreate playlist";
        }
    }
}
