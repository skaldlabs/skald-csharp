# Skald C# SDK Examples

This directory contains example applications demonstrating how to use the Skald C# SDK.

## Prerequisites

- .NET 8.0 or higher
- A Skald API key (get one at [https://useskald.com](https://useskald.com))
- A Skald instance with some memos created

## Running the Examples

### BasicUsage

The BasicUsage example demonstrates all the major features of the SDK:

1. Creating memos
2. Listing memos with pagination
3. Getting memo details
4. Searching memos (with and without filters)
5. Chat (both streaming and non-streaming)
6. Document generation (both streaming and non-streaming)
7. Updating memos
8. Deleting memos

To run the example:

```bash
# Set your API key
export SKALD_API_KEY="your-api-key-here"

# Navigate to the example directory
cd examples/BasicUsage

# Run the example
dotnet run
```

## Example Code Snippets

### Creating a Memo

```csharp
using var client = new SkaldClient("your-api-key");

var result = await client.CreateMemoAsync(new MemoData
{
    Title = "Meeting Notes",
    Content = "Full content of the memo...",
    Metadata = new Dictionary<string, object>
    {
        { "type", "notes" },
        { "author", "John Doe" }
    },
    Tags = new List<string> { "meeting", "q1" },
    Source = "notion"
});
```

### Searching Memos

```csharp
var results = await client.SearchAsync(new SearchRequest
{
    Query = "quarterly goals",
    SearchMethod = SearchMethod.ChunkVectorSearch,
    Limit = 10,
    Filters = new List<Filter>
    {
        new Filter
        {
            Field = "source",
            Operator = FilterOperator.Eq,
            Value = "notion",
            FilterType = FilterType.NativeField
        }
    }
});
```

### Streaming Chat

```csharp
await foreach (var evt in client.StreamedChatAsync("What are our quarterly goals?"))
{
    if (evt.Type == "token" && evt.Content != null)
    {
        Console.Write(evt.Content);
    }
    else if (evt.Type == "done")
    {
        Console.WriteLine("\nDone!");
    }
}
```

### Document Generation

```csharp
var doc = await client.GenerateDocAsync(
    prompt: "Create a product requirements document",
    rules: "Use formal business language. Include sections for: Overview, Requirements, Timeline"
);

Console.WriteLine(doc.Response);
```

## Notes

- Make sure you have memos in your Skald instance before running the examples
- The examples use the default base URL (https://api.useskald.com). If you're using a different instance, you can pass a custom base URL to the SkaldClient constructor
- Some operations (like delete) are commented out in the examples to avoid accidentally deleting your data
