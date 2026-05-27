using System;
using System.Globalization;
using System.Linq; 
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var apiKey = Environment.GetEnvironmentVariable("HUGGINGFACE_API_KEY");

        if (string.IsNullOrEmpty(apiKey))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: HUGGINGFACE_API_KEY not found!");
            Console.ResetColor();
            return;
        }

        Console.Write("Enter your text here: ");
        var text = Console.ReadLine();

      
        if (string.IsNullOrWhiteSpace(text))  //empty space check
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warning: You entered an empty text.");
            Console.ResetColor();
            return;
        }

        var modelUrl = "https://api-inference.huggingface.co/models/cardiffnlp/twitter-roberta-base-sentiment";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var json = JsonSerializer.Serialize(new { inputs = text });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            
            var response = await client.PostAsync(modelUrl, content);  //api
            
            
            if (!response.IsSuccessStatusCode) //catch error
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ API Hatası: Sunucu {response.StatusCode} koduyla yanıt verdi.");
                Console.ResetColor();
                return;
            }

            var result = await response.Content.ReadAsStringAsync();

           
            var doc = JsonDocument.Parse(result); // json parsing
            var items = doc.RootElement[0];

            var topLabel = items
                .EnumerateArray()
                .OrderByDescending(e => e.GetProperty("score").GetDouble())
                .First();

            var label = topLabel.GetProperty("label").GetString();
            var score = topLabel.GetProperty("score").GetDouble();

            string labelText = label switch
            {
                "LABEL_0" => "Negative",
                "LABEL_1" => "Neutral",
                "LABEL_2" => "Positive",
                _ => "Unknown"
            };

            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("\n📝 Input Text:");
            Console.WriteLine($"{text}");

            Console.WriteLine("\n📊 Sentiment Analysis:");
            
            
            if (labelText == "Positive") Console.ForegroundColor = ConsoleColor.Green;  //color change depending on label
            else if (labelText == "Negative") Console.ForegroundColor = ConsoleColor.Red;
            else Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine($"Label: {labelText}");
            Console.WriteLine($"Confidence Score: %{(score * 100).ToString("F2", CultureInfo.InvariantCulture)}");
            Console.ResetColor();
        }
        catch (HttpRequestException)
        {
          
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n❌ Connection Error:Please check your internet connection.");
            Console.ResetColor();
        }
        catch (JsonException)
        {
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n⚠️ Data Parsing Error: An unexpected response was received from the API. The model may be loading, please try again in a few seconds.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
          
            Console.ForegroundColor = ConsoleColor.Red;  // unexpectible all errors
            Console.WriteLine($"\n❌ An unexpected error occurred: {ex.Message}");
            Console.ResetColor();
        }
    }
}
