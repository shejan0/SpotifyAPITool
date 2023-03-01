using System;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace JSONScrubber
{
    class Program
    {
        static void Main(string[] args)
        {
            //List<AddedToCollection> addedList = JsonConvert.DeserializeObject<List<AddedToCollection>>(File.ReadAllText("AddedToCollection.json"));
            List<StreamingHistory> streamingHistories = new List<StreamingHistory>();
            foreach(string s in Directory.EnumerateFiles(Directory.GetCurrentDirectory()).Where(f => f.Contains("StreamingHistory")))
            {
                List<StreamingHistory> filehist = JsonConvert.DeserializeObject<List<StreamingHistory>>(File.ReadAllText(s));
                foreach(StreamingHistory hist in filehist)
                {
                    hist.parseObject();
                }
                streamingHistories.AddRange(filehist);
            }
            Console.WriteLine(streamingHistories.Count);
            streamingHistories.OrderBy(hist => hist.EndTime);
            Console.WriteLine(streamingHistories.Last().EndTime);
            streamingHistories=streamingHistories.Where(x => x.EndTime <= DateTime.Parse("2023-02-03T18:05:14.000Z") && x.EndTime > DateTime.Parse("2022-12-12")).ToList();
            Console.WriteLine(streamingHistories.Count);
            List<EndSong> endSongs = new List<EndSong>();
            

            foreach (string s in Directory.EnumerateFiles(Directory.GetCurrentDirectory()).Where(f => f.Contains("endsong")))
            {
                List<EndSong> filehist = JsonConvert.DeserializeObject<List<EndSong>>(File.ReadAllText(s));
                foreach (EndSong hist in filehist)
                {
                    hist.parseObject();
                }
                endSongs.AddRange(filehist);
            }
            endSongs=endSongs.OrderBy(x => x.TSDateTime).ToList();
            //Console.WriteLine(endSongs.Last().TSDateTime);
            //Console.WriteLine(DateTime.Parse("2022-12-12"));
            endSongs = endSongs.Where(x => !string.IsNullOrEmpty(x.TrackUri)).ToList();
            // onsole.WriteLine(endSongs.Count);

            //endSongs = endSongs.Where(x => x.TSDateTime <= DateTime.Parse("2023-02-03T18:05:14.000Z") /*&&  x.TSDateTime > DateTime.Parse("2022-12-12")*/).ToList();
            //Console.WriteLine(endSongs.Count);
            string[] diff = File.ReadAllLines("diff.tsv");
            SortedDictionary<DateTime, string> sortedUris = new SortedDictionary<DateTime, string>();
            using(var missing = new StreamWriter("missing.tsv"))
            {
                foreach (string s in diff)
                {
                    string[] vals = s.Split('\t');
                    //string uri=vals[0],song = vals[1], artists = vals[2];
                    List<StreamingHistory> playbacks = streamingHistories.Where(hist => hist.trackName == vals[1] && vals[2].Contains(hist.artistName)).ToList();
                    if (playbacks.Count != 0)
                    {
                        Console.WriteLine(playbacks.Count);
                        Console.WriteLine(playbacks[0].EndTime);
                        Console.WriteLine(vals[1]);
                        Console.WriteLine(vals[2]);
                        if (!sortedUris.ContainsKey(playbacks[0].EndTime))
                        {
                            //    Console.Error.WriteLine("Same key");
                            sortedUris.Add(playbacks[0].EndTime, vals[0]);
                        }
                    }
                    else
                    {
                        missing.WriteLine(string.Join("\t", vals[0], vals[1], vals[2]));
                    }
                }
            }
            using (var writer = new StreamWriter("found.tsv"))
            {
                foreach (KeyValuePair<DateTime, string> pair in sortedUris)
                {
                    Console.WriteLine(pair.Key);
                    Console.WriteLine(pair.Value);
                    writer.WriteLine(pair.Key + "\t" + diff.Where(x => x.Contains(pair.Value)).First());
                }
            }
                

        }
    }
    class AddedToCollection
    {
        public string timestamp_utc;
        public string context_time;
        public DateTime TimeStamp;
        public string message_item_uri;
        public string Uri;
        public string message_set;
        public void parseObject()
        {
            TimeStamp = DateTime.Parse(timestamp_utc);
            Uri = message_item_uri.Substring(14);
        }
    }
    class StreamingHistory
    {
        public string endTime;
        public string artistName;
        public string trackName;
        public long msPlayed;
        public DateTime EndTime;
        public void parseObject()
        {
            EndTime = DateTime.Parse(endTime);
        }
    }
    class EndSong
    {
        public string ts,
            username, platform,
            ms_played,
            conn_country,
            ip_addr_decrypted,
            user_agent_decrypted,
            master_metadata_track_name,
            master_metadata_album_artist_name,
            master_metadata_album_album_name,
            spotify_track_uri,
            episode_name,
            episode_show_name,
            spotify_episode_uri,
            reason_start, reason_end,
            shuffle,
            skipped,
            offline, offline_timestamp,
            icognito_mode;
       public IPAddress IPAddress;
        public DateTime TSDateTime, OfflineTimeStamp;
        public string TrackUri;
        public void parseObject()
        {
            IPAddress = IPAddress.Parse(ip_addr_decrypted);
            TSDateTime = DateTime.Parse(ts);
            OfflineTimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(offline_timestamp)).DateTime;
            if (string.IsNullOrEmpty(spotify_track_uri))
            {
                TrackUri = "";
            }
            else
            {
                string[] splits = spotify_track_uri.Split(":");
                TrackUri = splits.Last();
            }
            

        }
    }
}
