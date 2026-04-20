using System;
using System.Globalization;
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
            Console.WriteLine("Hata: HUGGINGFACE_API_KEY çevre değişkeni bulunamadı!");
            return;
        }
        Console.Write("Enter your text here: ");
        var text = Console.ReadLine();

        var modelUrl = "https://api-inference.huggingface.co/models/cardiffnlp/twitter-roberta-base-sentiment";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var json = JsonSerializer.Serialize(new { inputs = text });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(modelUrl, content);
        var result = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(result);
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
        Console.WriteLine($"Label: {labelText}");
        Console.WriteLine($"Confidence Score: %{(score * 100).ToString("F2", CultureInfo.InvariantCulture)}");
    }
}