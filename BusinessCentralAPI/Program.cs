 
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
        // Constants for Azure AD authentication
        //Below details will be availabie in the App registered  in Azure portal
        private const string tenantId = "your-tenant-id";    // Azure AD Tenant ID
        private const string clientId = "your-client-id";    // Azure AD App Client ID
        private const string clientSecret = "your-client-secret";  // Azure AD App Client Secret
        private const string authority = $"https://login.microsoftonline.com/{tenantId}";
        private const string scope = "https://api.businesscentral.dynamics.com/.default";

        // Business Central API URL (v2.0 for production environments)
        private const string businessCentralApiUrl = "https://api.businesscentral.dynamics.com/v2.0/{environment}/api/v2.0/contacts";

        static async Task Main(string[] args)
        {
            try
            {
                // Get the access token using Azure AD authentication
                string token = await GetAccessTokenAsync();

                // Make an HTTP request to fetch contacts from Business Central
                var contacts = await GetContactsFromBusinessCentralAsync(token);

                // Process or display the contacts
                Console.WriteLine(JsonConvert.SerializeObject(contacts, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Method to get access token using Microsoft.Identity.Client (MSAL)
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

        // Method to fetch contacts from Business Central API
        private static async Task<dynamic> GetContactsFromBusinessCentralAsync(string accessToken)
        {
            using (var client = new HttpClient())
            {
                // Add Authorization header with the token
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make GET request to the Business Central API
                var response = await client.GetAsync(businessCentralApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    // Read response content as a JSON string
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Deserialize JSON response to dynamic object
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
