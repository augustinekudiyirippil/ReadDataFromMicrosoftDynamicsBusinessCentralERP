 
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Identity.Client;

namespace BusinessCentralAPI
{
    class Program
    {
         
        //Below details will be availabie in the App registered  in Azure portal
        private const string tenantId = " ";    // Azure AD Tenant ID
        private const string clientId = " ";    // Azure AD App Client ID
        private const string clientSecret = " ";  // Azure AD App Client Secret
        private const string authority = $"https://login.microsoftonline.com/{tenantId}";
        private const string scope = "https://api.businesscentral.dynamics.com/.default";
        private const string environement = " ";  //Environment name from Business central ERP
        private const string businesscentralid = " ";  //GUID from Business central ERP , you will get it from the URL


        // Find the environment value from Business central
        

        private const string businessCentralApiUrl = $"https://api.businesscentral.dynamics.com/v2.0/{businesscentralid}/{environement}/api/v2.0/companies";
        static async Task Main(string[] args)
        {
            try
            {
                // Get the access token using Azure AD authentication
                string token = await GetAccessTokenAsync();

                // Stores the contacts details in below variable
                var contacts = await GetContactsFromBusinessCentralAsync(token);

                // To write the contacts details
                Console.WriteLine(JsonConvert.SerializeObject(contacts, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Get access token
        private static async Task<string> GetAccessTokenAsync()
        {
            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri(authority))
                .Build();

            string[] scopes = { scope };

            AuthenticationResult result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            return result.AccessToken;
        }

        // Read contacts from Business Central 
        private static async Task<dynamic> GetContactsFromBusinessCentralAsync(string accessToken)
        {
            using (var client = new HttpClient())
            {
                // Add Authorization  token
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

           
                var response = await client.GetAsync(businessCentralApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    // Store contacts in Json
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                   
                    return JsonConvert.DeserializeObject(jsonResponse);
                }
                else
                {
                    throw new Exception($"Failed to retrieve contacts. Status code: {response.StatusCode}");
                }
            }
        }
    }
}
