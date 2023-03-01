using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using ClientDetails;
namespace SpotifyAPITool
{
    class Program
    {
        private static EmbedIOAuthServer _server;
        private static string ClientId = IDSecret.ClientId, ClientSecret = IDSecret.ClientSecret;
        private static string personalplaylistid = IDSecret.PersonalPlayListID, likedplaylistid = IDSecret.LikedPlayListID;
        private static async Task RunInstace(SpotifyClient client)
        {
            try
            {
                PrivateUser p = await client.UserProfile.Current();
                var playlist = await client.Playlists.GetItems(personalplaylistid, new PlaylistGetItemsRequest()
                {
                    Offset = 0
                }); //Shejan Personal Playlist
                int count = (int)playlist.Total;
                List<FullTrack> outplaylist = new List<FullTrack>();
                List<FullTrack> outliked = new List<FullTrack>();
                Console.WriteLine("Grabbing Personal list");
                using (var writer = new StreamWriter("personalplaylist.tsv")
                {
                    AutoFlush=true
                })
                {
                    for (int n = 0; n < count; n += 100)
                    {
                        playlist = await client.Playlists.GetItems(personalplaylistid, new PlaylistGetItemsRequest()
                        {
                            Offset = n
                        });
                        var tracks = playlist.Items;
                        //tracks.Offset = n;
                        foreach (var item in tracks)
                        {
                            var track = item.Track;
                            if (track is FullEpisode)
                            {
                                Console.WriteLine("BRUH");
                            }
                            else if (track is FullTrack)
                            {
                                FullTrack fulltrack = track as FullTrack;
                                writer.WriteLine(item.AddedAt + "\t" + fulltrack.Id + "\t" + fulltrack.Name + "\t" + string.Join('|', fulltrack.Artists.Select(x => x.Name)));
                                outplaylist.Add(fulltrack);
                            }
                            else
                            {
                                Console.WriteLine("Unknown type");
                            }
                        }

                    }
                }
                Console.WriteLine("Grabbing Liked list");
                playlist = await client.Playlists.GetItems(likedplaylistid, new PlaylistGetItemsRequest()
                {
                    Offset = 0
                }); //Shejan Personal Playlist
                count = (int)playlist.Total;
                using (var writer = new StreamWriter("likedplaylist.tsv")
                {
                    AutoFlush = true
                })
                {
                    for (int n = 0; n < count; n += 100)
                    {
                        playlist = await client.Playlists.GetItems(likedplaylistid, new PlaylistGetItemsRequest()
                        {
                            Offset = n
                        });
                        var tracks = playlist.Items;
                        //tracks.Offset = n;
                        foreach (var item in tracks)
                        {
                            var track = item.Track;
                            if (track is FullEpisode)
                            {
                                Console.WriteLine("BRUH");
                            }
                            else if (track is FullTrack)
                            {
                                FullTrack fulltrack = track as FullTrack;
                                writer.WriteLine(item.AddedAt + "\t" + fulltrack.Id + "\t" + fulltrack.Name + "\t" + string.Join('|', fulltrack.Artists.Select(x => x.Name)));
                                outliked.Add(fulltrack);
                            }
                            else
                            {
                                Console.WriteLine("Unknown type");
                            }
                        }

                    }
                }
                Console.WriteLine("Playlist Count: " + outplaylist.Count);
                Console.WriteLine("Liked Count: " + outliked.Count);
                List<FullTrack> diff = outliked.Where(x => !outplaylist.Select(y => y.Id).Contains(x.Id)).ToList();
                Console.WriteLine("Diff Count: " + diff.Count);
                FullPlaylist newplaylist = await client.Playlists.Create(p.Id, new PlaylistCreateRequest("diff")
                {
                    Public = false,
                });
                List<string> diffuris = new List<string>();
                using (var writer = new StreamWriter("diff.tsv")
                {
                    AutoFlush = true
                })
                {
                    foreach (var fulltrack in diff)
                    {
                        writer.WriteLine(fulltrack.Id + "\t" + fulltrack.Name + "\t" + string.Join('|', fulltrack.Artists.Select(x => x.Name)));
                        diffuris.Add(fulltrack.Uri);
                        if (diffuris.Count >= 100)
                        {
                            await client.Playlists.AddItems(newplaylist.Id, new PlaylistAddItemsRequest(diffuris));
                            diffuris = new List<string>();
                        }
                    }
                }
                if (diffuris.Count != 0)
                {
                    await client.Playlists.AddItems(newplaylist.Id, new PlaylistAddItemsRequest(diffuris));

                }
                Console.WriteLine("Saved diff");

                var saved = await client.Library.GetTracks(new LibraryTracksRequest()
                {
                    Limit=50
                });

                while (saved.Total != 0)
                {
                    Console.WriteLine("Deleting");
                    await client.Library.RemoveTracks(new LibraryRemoveTracksRequest(saved.Items.Select(x => x.Track.Id).ToList()));
                    saved = await client.Library.GetTracks(new LibraryTracksRequest()
                    {
                        Limit = 50
                    });

                }
                Console.WriteLine("Saving playlist again");
                //save from the end.
                List<string> songids = new List<string>();
                Console.WriteLine(DateTime.Now);
                for (int n = outplaylist.Count - 1; n >= 0; n--)
                //for(int n =0;n<outplaylist.Count;n++)
                {
                    if (!outplaylist[n].IsLocal)
                    {

                        string id = outplaylist[n].Id;
                        if (string.IsNullOrEmpty(id))
                        {
                            Console.Error.WriteLine("NULL FOUND IN OUTPLAYLIST: " + n);
                        }
                        else
                        {
                            Console.WriteLine("Saving " + id + ": " + outplaylist[n].Name);
                            songids.Add(id);
                            await client.Library.SaveTracks(new LibrarySaveTracksRequest(songids));
                            await Task.Delay(1000);
                            songids = new List<string>();
                        }
                        
                    }
                }
                if (songids.Count != 0)
                {
                    await client.Library.SaveTracks(new LibrarySaveTracksRequest(songids));
                }
                Console.WriteLine(DateTime.Now);
            }
            catch (Exception e)
            {
                if(e is APIPagingException)
                {
                    APIPagingException s = e as APIPagingException;
                    Console.Error.WriteLine(s.Message);
                    Console.Error.WriteLine(s.StackTrace);
                    Console.Error.WriteLine(s.HelpLink);
                    Console.Error.WriteLine(s.HResult);
                    Console.Error.WriteLine(s.Response);
                    Console.Error.WriteLine(s.RetryAfter);
                    throw; 
                }if(e is APITooManyRequestsException)
                {
                    APIException s = e as APIException;
                    Console.Error.WriteLine(s.Message);
                    Console.Error.WriteLine(s.StackTrace);
                    Console.Error.WriteLine(s.HelpLink);
                    Console.Error.WriteLine(s.HResult);
                    Console.Error.WriteLine(s.Source);
                    Console.Error.WriteLine(s.TargetSite);
                    throw; 
                }if(e is APIException)
                {
                    APIException s = e as APIException;
                    Console.Error.WriteLine(s.Message);
                    Console.Error.WriteLine(s.StackTrace);
                    Console.Error.WriteLine(s.HelpLink);
                    Console.Error.WriteLine(s.HResult);
                    throw; 
                }
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                throw;
            }

        }

        static async Task Main(string[] args)
        {
            _server = new EmbedIOAuthServer(new Uri("http://localhost:42069/callback"), 42069);
            await _server.Start();
            _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
            _server.ErrorReceived += OnErrorReceived;
            var request = new LoginRequest(_server.BaseUri, ClientId, LoginRequest.ResponseType.Code)
            {
                Scope = new List<string> { Scopes.AppRemoteControl, Scopes.PlaylistModifyPrivate, Scopes.PlaylistModifyPublic, Scopes.PlaylistReadCollaborative, Scopes.PlaylistReadPrivate, Scopes.Streaming, Scopes.UgcImageUpload, Scopes.UserFollowModify, Scopes.UserFollowRead, Scopes.UserLibraryModify, Scopes.UserLibraryRead, Scopes.UserModifyPlaybackState, Scopes.UserReadCurrentlyPlaying, Scopes.UserReadEmail, Scopes.UserReadPlaybackPosition, Scopes.UserReadPlaybackState, Scopes.UserReadPrivate, Scopes.UserReadRecentlyPlayed, Scopes.UserTopRead,Scopes.Streaming }
            };
            
            BrowserUtil.Open(request.ToUri());
            while (Console.ReadLine() != "stop")
            {
                await Task.Delay(1000);
            }
        }
        private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        {
            await _server.Stop();

            var config = SpotifyClientConfig.CreateDefault();
            var tokenResponse = await new OAuthClient(config).RequestToken(
              new AuthorizationCodeTokenRequest(
                ClientId, ClientSecret, response.Code, new Uri("http://localhost:42069/callback")
              )
            );

            var spotify = new SpotifyClient(tokenResponse.AccessToken);
            try
            {
                await RunInstace(spotify);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }

        private static async Task OnErrorReceived(object sender, string error, string state)
        {
            Console.WriteLine($"Aborting authorization, error received: {error}");
            await _server.Stop();
        }
    }
}


