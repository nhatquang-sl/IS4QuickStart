using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace IS4ClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IdentityClient");
            Task.WaitAll(StartAppAsync());
        }

        static async Task StartAppAsync()
        {
            var client = new HttpClient();

            // discover endpoints from metadata
            var disco = await client.GetDiscoveryDocumentAsync("http://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token
            var tokenResponse = await GetTokenAsync(client, disco);

            // call api
            await CallApi(client, tokenResponse);
        }

        // request token
        static async Task<TokenResponse> GetTokenAsync(HttpClient client, DiscoveryResponse disco)
        {
            // without username and password
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });

            //var tokenResponse = await client.RequestPasswordTokenAsync(
            //    new PasswordTokenRequest
            //    {
            //        Address = disco.TokenEndpoint,

            //        ClientId = "client",
            //        ClientSecret = "secret",
            //        Scope = "api1",

            //        UserName = "alice",
            //        Password = "password"
            //    });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return null;
            }

            Console.WriteLine(tokenResponse.Json);
            return tokenResponse;
        }

        // call api
        static async Task CallApi(HttpClient client, TokenResponse tokenResponse)
        {
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("http://localhost:5001/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }
    }
}
