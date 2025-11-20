# Skald C# SDK

C# client library for the Skald API.

## Installation

```bash
dotnet add package Skald
```

## Requirements

- .NET 8.0, .NET 9.0

## Usage

### Initialize the client

```csharp
using Skald;

var client = new SkaldClient("your-api-key-here");
```

### Memo Management

#### Create a Memo

Create a new memo that will be automatically processed (summarized, tagged, chunked, and indexed for search):

```csharp
var result = await client.CreateMemoAsync(new MemoData
{
    Title = "Meeting Notes",
    Content = "Full content of the memo...",
    Metadata = new Dictionary<string, object>
    {
        { "type", "notes" },
        { "author", "John Doe" }
    },
    ReferenceId = "external-id-123",
    Tags = new List<string> { "meeting", "q1" },
    Source = "notion",
    ExpirationDate = "2024-12-31T23:59:59Z"
});

Console.WriteLine(result.MemoUuid); // "550e8400-e29b-41d4-a716-446655440000"
```

**Required Fields:**
- `Title` (string, max 255 chars) - The title of the memo
- `Content` (string) - The full content of the memo

**Optional Fields:**
- `Metadata` (Dictionary<string, object>) - Custom JSON metadata
- `ReferenceId` (string, max 255 chars) - An ID from your side that you can use to match Skald memo UUIDs with documents on your end
- `Tags` (List<string>) - Tags for categorization
- `Source` (string, max 255 chars) - An indication of the source of this content, useful when building integrations
- `ExpirationDate` (string) - ISO 8601 timestamp for automatic memo expiration

#### Get a Memo

Retrieve a memo by its UUID or your reference ID:

```csharp
// Get by UUID
var memo = await client.GetMemoAsync(new GetMemoRequest
{
    MemoId = "550e8400-e29b-41d4-a716-446655440000"
});

// Get by reference ID
var memo = await client.GetMemoAsync(new GetMemoRequest
{
    MemoId = "external-id-123",
    IdType = IdType.ReferenceId
});

Console.WriteLine(memo.Title);
Console.WriteLine(memo.Content);
Console.WriteLine(memo.Summary);
Console.WriteLine(string.Join(", ", memo.Tags.ConvertAll(t => t.Tag)));
```

The `GetMemoAsync()` method returns complete memo details including content, AI-generated summary, tags, and content chunks.

#### List Memos

List all memos with pagination:

```csharp
// Get first page with default page size (20)
var memos = await client.ListMemosAsync();

// Get specific page with custom page size
var memos = await client.ListMemosAsync(new ListMemosParams
{
    Page = 2,
    PageSize = 50
});

Console.WriteLine($"Total memos: {memos.Count}");
Console.WriteLine($"Results: {memos.Results.Count}");
Console.WriteLine($"Next page: {memos.Next}");
```

**Parameters:**
- `Page` (int, optional) - Page number (default: 1)
- `PageSize` (int, optional) - Results per page (default: 20, max: 100)

#### Update a Memo

Update an existing memo by UUID or reference ID:

```csharp
// Update by UUID
await client.UpdateMemoAsync(new UpdateMemoRequest
{
    MemoId = "550e8400-e29b-41d4-a716-446655440000",
    UpdateData = new UpdateMemoData
    {
        Title = "Updated Title",
        Metadata = new Dictionary<string, object> { { "status", "reviewed" } }
    }
});

// Update by reference ID and trigger reprocessing
await client.UpdateMemoAsync(new UpdateMemoRequest
{
    MemoId = "external-id-123",
    IdType = IdType.ReferenceId,
    UpdateData = new UpdateMemoData
    {
        Content = "New content that will be reprocessed"
    }
});
```

**Note:** When you update the `Content` field, the memo will be automatically reprocessed (summary, tags, and chunks regenerated).

**Updatable Fields:**
- `Title` (string)
- `Content` (string)
- `Metadata` (Dictionary<string, object>)
- `ClientReferenceId` (string)
- `Source` (string)
- `ExpirationDate` (string)

#### Delete a Memo

Permanently delete a memo and all associated data:

```csharp
// Delete by UUID
var result = await client.DeleteMemoAsync(new DeleteMemoRequest
{
    MemoId = "550e8400-e29b-41d4-a716-446655440000"
});
Console.WriteLine(result.Ok); // true

// Delete by reference ID
await client.DeleteMemoAsync(new DeleteMemoRequest
{
    MemoId = "external-id-123",
    IdType = IdType.ReferenceId
});
```

**Warning:** This operation permanently deletes the memo and all related data (content, summary, tags, chunks) and cannot be undone.

### Search Memos

Search through your memos using various search methods with optional filters:

```csharp
// Basic semantic search
var results = await client.SearchAsync(new SearchRequest
{
    Query = "quarterly goals",
    Limit = 10
});

// Search with filters
var filtered = await client.SearchAsync(new SearchRequest
{
    Query = "python tutorial",
    Filters = new List<Filter>
    {
        new Filter
        {
            Field = "source",
            Operator = FilterOperator.Eq,
            Value = "notion",
            FilterType = FilterType.NativeField
        },
        new Filter
        {
            Field = "level",
            Operator = FilterOperator.Eq,
            Value = "beginner",
            FilterType = FilterType.CustomMetadata
        }
    }
});

Console.WriteLine($"Found {filtered.Results.Count} results");
foreach (var result in filtered.Results)
{
    Console.WriteLine($"- {result.MemoTitle} (distance: {result.Distance})");
    Console.WriteLine($"  Chunk: {result.ChunkContent.Substring(0, 100)}...");
}
```

#### Search Parameters

- `Query` (string, required) - The search query
- `Limit` (int, optional) - Maximum results to return (1-50, default 10)
- `Filters` (List<Filter>, optional) - Array of filter objects to narrow results

The search uses semantic search on memo chunks for detailed content search.

#### Search Response

```csharp
public class SearchResult
{
    public string MemoUuid { get; set; }       // Unique identifier for the memo
    public string ChunkUuid { get; set; }      // Unique identifier for the chunk
    public string MemoTitle { get; set; }      // Memo title
    public string MemoSummary { get; set; }    // Auto-generated memo summary
    public string ContentSnippet { get; set; } // Snippet of the chunk content
    public string ChunkContent { get; set; }   // Full content of the matching chunk
    public double? Distance { get; set; }      // Relevance score (0-2, lower is more relevant)
}
```

- `Distance` - A decimal from 0 to 2 determining how close the result was deemed to be to the query. The closer to 0 the more related the content is to the query.

### Chat with Your Knowledge Base

Ask questions about your memos using an AI agent. The agent retrieves relevant context and generates answers with inline citations.

#### Non-Streaming Chat

```csharp
var result = await client.ChatAsync(new ChatRequest
{
    Query = "What were the main points discussed in the Q1 meeting?"
});

Console.WriteLine(result.Response);
// "The main points discussed in the Q1 meeting were:
// 1. Revenue targets [[1]]
// 2. Hiring plans [[2]]
// 3. Product roadmap [[1]][[3]]"

Console.WriteLine(result.Ok); // true

// Access references if enabled
if (result.References != null)
{
    foreach (var reference in result.References)
    {
        Console.WriteLine($"Reference {reference.Key}: {reference.Value}");
    }
}
```

#### Streaming Chat

For real-time responses, use streaming chat:

```csharp
await foreach (var evt in client.StreamedChatAsync(new ChatRequest
{
    Query = "What are our quarterly goals?"
}))
{
    if (evt.Type == "token" && evt.Content != null)
    {
        // Write each token as it arrives
        Console.Write(evt.Content);
    }
    else if (evt.Type == "done")
    {
        Console.WriteLine($"\nDone! Chat ID: {evt.ChatId}");
    }
}
```

#### Chat Parameters

The `ChatRequest` object supports the following parameters:

- `Query` (string, required) - The question to ask
- `Stream` (bool, optional) - Whether to stream the response (not needed when using `StreamedChatAsync`)
- `ChatId` (string, optional) - Chat ID to continue a previous conversation
- `SystemPrompt` (string, optional) - Custom system prompt to guide the AI's behavior
- `Filters` (List<Filter>, optional) - Array of filter objects to focus chat context on specific sources
- `RagConfig` (RAGConfig, optional) - Advanced RAG configuration (LLM provider, query rewrite, vector search, reranking, references)
- `ProjectId` (string, optional) - Project ID (required with Token Authentication)

#### Chat Response

Non-streaming responses include:
- `Ok` (bool) - Success status
- `Response` (string) - The AI's answer with inline citations in format `[[N]]`
- `IntermediateSteps` (List<object>) - Steps taken by the agent (for debugging)
- `ChatId` (string) - Chat ID for conversation continuity
- `References` (Dictionary<string, object>) - References mapping citation numbers to memo information

Streaming responses yield events:
- `{ Type = "token", Content = "..." }` - Each text token as it's generated
- `{ Type = "references", Content = "..." }` - References information
- `{ Type = "done", ChatId = "..." }` - Indicates the stream has finished with chat ID

### File Upload

You can create memos from files (PDF, DOCX, TXT, etc.) which will be automatically processed:

```csharp
var pdfBytes = await File.ReadAllBytesAsync("document.pdf");
var result = await client.CreateMemoFromFileAsync(new MemoFileData
{
    File = pdfBytes,
    Filename = "document.pdf",
    Title = "Important Document",
    Tags = new List<string> { "document", "pdf" },
    Source = "file-upload",
    Metadata = new Dictionary<string, object>
    {
        { "department", "legal" }
    }
});

Console.WriteLine(result.MemoUuid);
```

#### Check Processing Status

After uploading a file, you can check its processing status:

```csharp
var statusResponse = await client.CheckMemoStatusAsync(new CheckMemoStatusRequest
{
    MemoId = result.MemoUuid
});

Console.WriteLine(statusResponse.Status); // Processing, Processed, or Error

if (statusResponse.Status == MemoStatus.Error)
{
    Console.WriteLine($"Error: {statusResponse.ErrorReason}");
}
```

### Filters

Filters allow you to narrow down results based on memo metadata. You can filter by native fields or custom metadata fields. Filters are supported in `SearchAsync()` and `ChatAsync()`.

#### Filter Structure

```csharp
public class Filter
{
    public string Field { get; set; }           // Field name to filter on
    public FilterOperator Operator { get; set; } // Comparison operator
    public object Value { get; set; }           // Value(s) to compare against
    public FilterType FilterType { get; set; }  // native_field or custom_metadata
}
```

#### Native Fields

Native fields are built-in memo properties:
- `title` - Memo title
- `source` - Source system (e.g., "notion", "confluence")
- `client_reference_id` - Your external reference ID
- `tags` - Memo tags (array)

#### Custom Metadata Fields

You can filter on any field from the `Metadata` object you provided when creating the memo.

#### Filter Operators

- **`Eq`** - Equals (exact match)
- **`Neq`** - Not equals
- **`Contains`** - Contains substring (case-insensitive)
- **`Startswith`** - Starts with prefix (case-insensitive)
- **`Endswith`** - Ends with suffix (case-insensitive)
- **`In`** - Value is in array (requires array value)
- **`NotIn`** - Value is not in array (requires array value)

#### Filter Examples

```csharp
// Filter by source
new Filter
{
    Field = "source",
    Operator = FilterOperator.Eq,
    Value = "notion",
    FilterType = FilterType.NativeField
}

// Filter by multiple tags
new Filter
{
    Field = "tags",
    Operator = FilterOperator.In,
    Value = new[] { "security", "compliance" },
    FilterType = FilterType.NativeField
}

// Filter by title containing text
new Filter
{
    Field = "title",
    Operator = FilterOperator.Contains,
    Value = "meeting",
    FilterType = FilterType.NativeField
}

// Filter by custom metadata field
new Filter
{
    Field = "department",
    Operator = FilterOperator.Eq,
    Value = "engineering",
    FilterType = FilterType.CustomMetadata
}

// Exclude specific sources
new Filter
{
    Field = "source",
    Operator = FilterOperator.NotIn,
    Value = new[] { "draft", "archive" },
    FilterType = FilterType.NativeField
}
```

#### Combining Multiple Filters

When you provide multiple filters, they are combined with AND logic (all filters must match):

```csharp
var results = await client.SearchAsync(new SearchRequest
{
    Query = "security best practices",
    Filters = new List<Filter>
    {
        new Filter
        {
            Field = "source",
            Operator = FilterOperator.Eq,
            Value = "security-docs",
            FilterType = FilterType.NativeField
        },
        new Filter
        {
            Field = "tags",
            Operator = FilterOperator.In,
            Value = new[] { "approved", "current" },
            FilterType = FilterType.NativeField
        },
        new Filter
        {
            Field = "status",
            Operator = FilterOperator.Neq,
            Value = "draft",
            FilterType = FilterType.CustomMetadata
        }
    }
});
```

#### Filters with Chat

Focus chat context on specific sources:

```csharp
var result = await client.ChatAsync(new ChatRequest
{
    Query = "What are our security practices?",
    Filters = new List<Filter>
    {
        new Filter
        {
            Field = "tags",
            Operator = FilterOperator.In,
            Value = new[] { "security", "compliance" },
            FilterType = FilterType.NativeField
        }
    }
});
```

### Error Handling

```csharp
try
{
    var result = await client.CreateMemoAsync(new MemoData
    {
        Title = "My Memo",
        Content = "Content here"
    });
    Console.WriteLine("Success");
}
catch (SkaldException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
}
```

### Using Custom HttpClient

If you need to customize the HTTP client (e.g., for proxy settings, custom timeout, or other configurations):

```csharp
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(60)
};

// Pass false for disposeHttpClient to prevent the SkaldClient from disposing your HttpClient
using var client = new SkaldClient("your-api-key", httpClient, disposeHttpClient: false);

// ... use the client

httpClient.Dispose(); // You are responsible for disposing the HttpClient
```

## Examples

See the [examples](examples/) directory for complete working examples.

## License

MIT

## Support

For issues and questions:
- GitHub Issues: https://github.com/skaldlabs/skald-csharp/issues
- Documentation: https://docs.useskald.com