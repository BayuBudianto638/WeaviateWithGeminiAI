using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;

class Program
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly string weaviateUrl = "http://localhost:8080/v1";

    static async Task Main(string[] args)
    {
        string query = "Your context here?";
        string weaviateResponse = await QueryWeaviate(query);

        string geminiResponse = await GenerateResponseWithGemini(weaviateResponse);

        Console.WriteLine("Gemini AI Response:");
        Console.WriteLine(geminiResponse);
    }

    private static async Task<string> QueryWeaviate(string query)
    {
        var searchQuery = new
        {
            query = $@"
        {{
            Get {{
                Article(
                    nearText: {{
                        concepts: [""{query}""]
                    }}
                ) {{
                    title
                    content
                    _additional {{
                        distance
                    }}
                }}
            }}
        }}"
        };

        var json = System.Text.Json.JsonSerializer.Serialize(searchQuery);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{weaviateUrl}/graphql", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return responseJson;
    }

    private static async Task<string> GenerateResponseWithGemini(string context)
    {
        var geminiUrl = "Gemini-AI-Website"; 
        var geminiKey = "Gemini-AI-API"; 
        string _apiUrl = $"{geminiUrl}?key={geminiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = $"Based on the following information, answer the user's question:\n{context}\nQuestion: What is the title about?" }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 1,
                topK = 40,
                topP = 0.95,
                maxOutputTokens = 8192
            }
        };

        string json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = true });

        string jsonBody = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = true });
       
        string apiUrl = $"{geminiUrl}?key={geminiKey}";
        var client = new RestClient(apiUrl);
        var request = new RestRequest("", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(jsonBody);

        var response = await client.ExecuteAsync(request);


        return response.Content;
    }

    public class GeminiResponse
    {
        public List<Choice> choices { get; set; }

        public class Choice
        {
            public string text { get; set; }
        }
    }
}