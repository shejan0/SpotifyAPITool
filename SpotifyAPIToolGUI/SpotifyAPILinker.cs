using ClientDetails;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyAPIToolGUI
{
    internal class SpotifyAPILinker
    {
        private static EmbedIOAuthServer _server;
        private static string ClientId = IDSecret.ClientId, ClientSecret = IDSecret.ClientSecret;
        private static SpotifyClient client = null;
        private static string errorStr = null;
        public SpotifyAPILinker()
        {
            _server = new EmbedIOAuthServer(new Uri("http://127.0.0.1:42069/callback"), 42069);
        }
        public SpotifyClient Link()
        {
            _server.Start().Wait();
            _server.AuthorizationCodeReceived += OnAuthorizationCodeReceived;
            _server.ErrorReceived += OnErrorReceived;
            var request = new LoginRequest(_server.BaseUri, ClientId, LoginRequest.ResponseType.Code)
            {
                Scope = new List<string> { 
                    Scopes.AppRemoteControl,
                    Scopes.PlaylistModifyPrivate, 
                    Scopes.PlaylistModifyPublic,
                    Scopes.PlaylistReadCollaborative, 
                    Scopes.PlaylistReadPrivate, 
                    Scopes.Streaming, 
                    Scopes.UgcImageUpload, 
                    Scopes.UserFollowModify, 
                    Scopes.UserFollowRead, 
                    Scopes.UserLibraryModify, 
                    Scopes.UserLibraryRead, 
                    Scopes.UserModifyPlaybackState, 
                    Scopes.UserReadCurrentlyPlaying, 
                    Scopes.UserReadEmail, 
                    Scopes.UserReadPlaybackPosition, 
                    Scopes.UserReadPlaybackState, 
                    Scopes.UserReadPrivate, 
                    Scopes.UserReadRecentlyPlayed, 
                    Scopes.UserTopRead,

                }
            };
            BrowserUtil.Open(request.ToUri());
            while (client == null && String.IsNullOrEmpty(errorStr)) ;
            if (!String.IsNullOrEmpty(errorStr))
            {
                throw new Exception(errorStr);
            }
            return client;
        }
        private static async Task OnAuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        {
            await _server.Stop();

            var config = SpotifyClientConfig.CreateDefault();
            var tokenResponse = await new OAuthClient(config).RequestToken(
              new AuthorizationCodeTokenRequest(
                ClientId, ClientSecret, response.Code, new Uri("http://127.0.0.1:42069/callback")
              )
            );

            client = new SpotifyClient(tokenResponse.AccessToken);
        }

        private static async Task OnErrorReceived(object sender, string error, string state)
        {
            Console.WriteLine($"Aborting authorization, error received: {error}");
            errorStr = error;
        }
    }
}
