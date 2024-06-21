using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task<string> LoginAsync(string baseUrl, string email, string password)
    {
        using (var client = new HttpClient())
        {
            var content = new StringContent(
                $"{{\"username\": \"{email}\", \"password\": \"{password}\"}}",
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync($"{baseUrl}/api/session", content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseString);
            return jsonResponse["id"].ToString(); // This is your token
        }
    }

    static async Task<JArray> GetSavedQueriesAsync(string baseUrl, string token)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-Metabase-Session", token);

            var response = await client.GetAsync($"{baseUrl}/api/card");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JArray.Parse(responseString); // This is the list of saved questions
        }
    }

    static async Task<JArray> RunQueryAsync(string baseUrl, string token, int queryId)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-Metabase-Session", token);

            var response = await client.PostAsync($"{baseUrl}/api/card/{queryId}/query/json", null);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JArray.Parse(responseString); // This is the query result
        }
    }

    static async Task Main(string[] args)
    {
        string baseUrl = "http://localhost:12345";
        string email = "your_email@gmail.com";
        string password = "your_password";

        // Login and get token
        string token = await LoginAsync(baseUrl, email, password);
        Console.WriteLine($"Token: {token}");

        // Get saved queries
        var queries = await GetSavedQueriesAsync(baseUrl, token);
        Console.WriteLine("Saved Queries:");
        foreach (var query in queries)
        {
            Console.WriteLine($"{query["id"]}: {query["name"]}");
        }

        // Run a specific query
        if (queries.Count > 0)
        {
            int queryId = (int)queries[0]["id"]; // Run the first query in the list
            var result = await RunQueryAsync(baseUrl, token, queryId);
            Console.WriteLine("Query Result:");
            Console.WriteLine(result);
        }
    }
}
