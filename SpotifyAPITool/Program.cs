using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using ClientDetails;
using Humanizer;
using SpotifyAPI.Web.Http;
using Swan.Logging;
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
                /*
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
                            await Task.Delay(250);
                            songids = new List<string>();
                        }
                        
                    }
                }
                if (songids.Count != 0)
                {
                    await client.Library.SaveTracks(new LibrarySaveTracksRequest(songids));
                }*/
                /*
                Console.WriteLine(DateTime.Now);
                foreach(string s in File.ReadLines("found.tsv"))
                {
                    string[] splits = s.Split('\t');
                    Console.WriteLine("Saving " + splits[1] + ": " + splits[2]+" - "+splits[3]);
                    List<string> songids = new List<string>() { splits[1] };
                    await client.Library.SaveTracks(new LibrarySaveTracksRequest(songids));
                    await Task.Delay(1000);
                }
                */
                int n = 0;

                List<SavedTrack> liked = new List<SavedTrack>();
                TimeSpan durationsongs = new TimeSpan();
                using (var writer = new StreamWriter($"saved-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.tsv")
                {
                    AutoFlush = true
                })
                {
                    writer.WriteLine("ID\tName\tArtists\tDurationMilliSeconds\tDurationHumanReadable\tAlbumName\tUri");
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
                        }catch(APIException ex)
                        {
                            Console.Error.WriteLine(ex.Message);
                            Console.Error.WriteLine(ex.StackTrace);
                            Console.Error.WriteLine(ex.Response.StatusCode);
                            Console.Error.WriteLine(ex.Response.Body);
                            if(ex.Response.StatusCode == System.Net.HttpStatusCode.BadGateway)
                            {
                                continue;
                            }
                            throw;
                        }
                        
                        liked.AddRange(saved.Items);
                        for(int a = n; a< liked.Count(); a++)
                        {
                            FullTrack track = liked[a].Track;
                            if (track.IsLocal)
                            {
                                continue;
                            }
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
                            durationsongs += TimeSpan.FromMilliseconds(track.DurationMs);
                            writer.WriteLine(string.Join('\t', trackdetails));
                        }
                        n += saved.Items.Count;
                        Console.WriteLine("Current size of saved in RAM: " + n);
                    }
                    string s = durationsongs.Humanize(precision: 10, maxUnit: Humanizer.Localisation.TimeUnit.Month);
                    writer.WriteLine(s);
                    Console.WriteLine(s);
                }

                /*Console.WriteLine("Creating playlist");
                FullPlaylist newplaylist = await client.Playlists.Create(p.Id, new PlaylistCreateRequest("Liked " + DateTime.Now.ToString())
                {
                    Public = false,
                });
                using (var writer = new StreamWriter($"saved-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.tsv")
                {
                    AutoFlush = true
                })
                {
                    foreach (SavedTrack strack in liked)
                    {
                        FullTrack track = strack.Track;
                        if (track.IsLocal)
                        {
                            continue;
                        }
                        List<string> trackdetails = new()
                        {
                            track.Id,
                            track.Name,
                            string.Join('|', track.Artists.Select(x => x.Name)),
                            (track.DurationMs*1000.0).ToString(),
                            track.Album.Name,
                            track.Uri,
                        };
                        durationsongs += TimeSpan.FromMilliseconds(track.DurationMs / 1000.0);
                        writer.WriteLine(string.Join('\t', trackdetails));
                        //writer.WriteLine(track.Id + "\t" + track.Name + "\t" + string.Join('|', track.Artists.Select(x => x.Name)));
                        
                    }
                }*/
                /*
                List<string> uri = new List<string>();
                for(int a = 0; a < n; a++)
                {
                    FullTrack track = liked[a].Track;
                    if (track.IsLocal)
                    {
                        continue;
                    }
                    uri.Add(track.Uri);
                    Console.WriteLine(a + " - " + track.Id + "-" + track.Name + "-" + string.Join(',', track.Artists.Select(x => x.Name)));
                    if (uri.Count == 100)
                    {
                        Console.WriteLine("Pushing " + uri.Count + " objects");
                        await client.Playlists.AddItems(newplaylist.Id, new PlaylistAddItemsRequest(uri));
                        uri.Clear();
                    }
                }
                if (uri.Count != 0)
                {
                    Console.WriteLine("Pushing " + uri.Count + " objects");
                    await client.Playlists.AddItems(newplaylist.Id, new PlaylistAddItemsRequest(uri));
                    uri.Clear();
                }
                */
                Environment.Exit(0);
                /*foreach (SavedTrack strack in liked)
                {
                    FullTrack track = strack.Track;
                    if (track.IsLocal)
                    {
                        continue;
                    }
                    Console.WriteLine(n+" - "+track.Id + "-" + track.Name + "-" + string.Join(',', track.Artists.Select(x => x.Name)));
                    List<string> uri = new List<string>() { track.Uri };
                    await client.Playlists.AddItems(newplaylist.Id, new PlaylistAddItemsRequest(uri));
                    await Task.Delay(100);
                }*/
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
                    Console.Error.WriteLine(s.Response.ToString());
                    Console.Error.WriteLine(s.Response.StatusCode);
                    Console.Error.WriteLine(s.Response.Body);
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


