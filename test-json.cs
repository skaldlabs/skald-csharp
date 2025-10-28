using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

public class MemoData
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

class Program
{
    static void Main()
    {
        var memo = new MemoData
        {
            Title = "Test Title",
            Content = "Test Content",
            Metadata = new Dictionary<string, object>()
        };

        var json = JsonSerializer.Serialize(memo);
        Console.WriteLine("Default serialization:");
        Console.WriteLine(json);
    }
}
