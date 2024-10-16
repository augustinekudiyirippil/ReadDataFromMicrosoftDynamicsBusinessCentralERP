 
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace BusinessCentralAPI
{
    class Program
    {

        //Below details will be availabie in the App registered  in Azure portal

        //private const string tenantId = " ";    // Azure AD Tenant ID
        //private const string clientId = " ";    // Azure AD App Client ID
        //private const string clientSecret = " ";  // Azure AD App Client Secret
        //private const string authority = $"https://login.microsoftonline.com/{tenantId}";
        //private const string scope = "https://api.businesscentral.dynamics.com/.default";
        //private const string environement = " ";  //Environment name from Business central ERP
        //private const string businesscentralid = " ";  //GUID from Business central ERP , you will get it from the URL




        private const string bcCompaniesApiUrl = $"https://api.businesscentral.dynamics.com/v2.0/{businesscentralid}/{environement}/api/v2.0/companies";

        
        static async Task Main(string[] args)
        {

            string bcContactApiUrl = "URL to read contacts ";
            string companyID;
            try
            {
                // Get the access token using Azure AD authentication
                string token = await GetAccessTokenAsync();

                // Stores the contacts details in below variable
                var companies = await GetDataFromBusinessCentralAsync(token, bcCompaniesApiUrl, "Companies");
                Console.WriteLine(companies);

                string jsonString = @""+companies;

                var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                // Extract the value array
                companies = jsonObject["value"];


                // Iterate through each company,  extract the ID and read  company details
                string companyId;
                foreach (var company in companies)
                {
                    companyId = company["id"].ToString();
                    bcContactApiUrl = $"https://api.businesscentral.dynamics.com/v2.0/{businesscentralid}/{environement}/api/v2.0/companies({companyId})/companyInformation";
                    var companyInfo = await GetDataFromBusinessCentralAsync(token, bcContactApiUrl, "Company Information");
                    Console.WriteLine(companyInfo);
                     
                }


                // Iterate through each company,  extract the ID and read  contact organisation / customers

                foreach (var company in companies)
                {
                    companyId = company["id"].ToString();
                    bcContactApiUrl = $"https://api.businesscentral.dynamics.com/v2.0/{businesscentralid}/{environement}/api/v2.0/companies({companyId})/customers";
                    var customers = await GetDataFromBusinessCentralAsync(token, bcContactApiUrl, "Company Information");
                    Console.WriteLine(customers);

                }


                // Iterate through each company,  extract the ID and read contacts from the company

                foreach (var company in companies)
                {
                    companyId = company["id"].ToString();
                    bcContactApiUrl = $"https://api.businesscentral.dynamics.com/v2.0/{businesscentralid}/{environement}/api/v2.0/companies({companyId})/contacts";
                    var companyContacts = await GetDataFromBusinessCentralAsync(token, bcContactApiUrl, "Company Contacts");
                    Console.WriteLine(companyContacts);
                    
                }

      
   



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
        private static async Task<dynamic> GetDataFromBusinessCentralAsync(string accessToken, string apiURL, string dataToRetreive)
        {
            using (var client = new HttpClient())
            {
                // Add Authorization  token
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

           
                var response = await client.GetAsync(apiURL);
                if (response.IsSuccessStatusCode)
                {
                    // Store contacts in Json
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                   
                    return JsonConvert.DeserializeObject(jsonResponse);
                }
                else
                {
                    throw new Exception($"Failed to retrieve {dataToRetreive}. Status code: {response.StatusCode}");
                }
            }
        }
    }
}
