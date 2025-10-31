using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Skald;

/// <summary>
/// Data for creating a new memo
/// </summary>
public class MemoData
{
    /// <summary>
    /// The title of the memo (required, max 255 characters)
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// The full content/body of the memo (required)
    /// </summary>
    [JsonPropertyName("content")]
    public required string Content { get; set; }

    /// <summary>
    /// Optional custom JSON metadata (key-value pairs for additional context)
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Optional external reference ID (max 255 characters, used for linking Skald memos to IDs on your side)
    /// </summary>
    [JsonPropertyName("reference_id")]
    public string? ReferenceId { get; set; }

    /// <summary>
    /// Optional array of tags for categorization and filtering
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Optional source system name (max 255 characters, e.g., "notion", "confluence", "email")
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// Optional expiration date in ISO 8601 format
    /// </summary>
    [JsonPropertyName("expiration_date")]
    public string? ExpirationDate { get; set; }
}

/// <summary>
/// Response from creating a memo
/// </summary>
public class CreateMemoResponse
{
    /// <summary>
    /// Success status
    /// </summary>
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }
}

/// <summary>
/// Data for updating an existing memo
/// </summary>
public class UpdateMemoData
{
    /// <summary>
    /// Updated title
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Updated content (triggers reprocessing)
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Updated metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Updated client reference ID
    /// </summary>
    [JsonPropertyName("client_reference_id")]
    public string? ClientReferenceId { get; set; }

    /// <summary>
    /// Updated source
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// Updated expiration date
    /// </summary>
    [JsonPropertyName("expiration_date")]
    public string? ExpirationDate { get; set; }
}

/// <summary>
/// Response from updating a memo
/// </summary>
public class UpdateMemoResponse
{
    /// <summary>
    /// Success status
    /// </summary>
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }
}

/// <summary>
/// Type of identifier used for memo lookups
/// </summary>
public enum IdType
{
    /// <summary>
    /// Lookup by memo UUID
    /// </summary>
    MemoUuid,

    /// <summary>
    /// Lookup by client reference ID
    /// </summary>
    ReferenceId
}

/// <summary>
/// A tag associated with a memo
/// </summary>
public class MemoTag
{
    /// <summary>
    /// Tag UUID
    /// </summary>
    [JsonPropertyName("uuid")]
    public required string Uuid { get; set; }

    /// <summary>
    /// Tag text
    /// </summary>
    [JsonPropertyName("tag")]
    public required string Tag { get; set; }
}

/// <summary>
/// A content chunk from a memo
/// </summary>
public class MemoChunk
{
    /// <summary>
    /// Chunk UUID
    /// </summary>
    [JsonPropertyName("uuid")]
    public required string Uuid { get; set; }

    /// <summary>
    /// Content of the chunk
    /// </summary>
    [JsonPropertyName("chunk_content")]
    public required string ChunkContent { get; set; }

    /// <summary>
    /// Index of the chunk within the memo
    /// </summary>
    [JsonPropertyName("chunk_index")]
    public int ChunkIndex { get; set; }
}

/// <summary>
/// Complete memo with all details
/// </summary>
public class Memo
{
    /// <summary>
    /// Memo UUID
    /// </summary>
    [JsonPropertyName("uuid")]
    public required string Uuid { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [JsonPropertyName("updated_at")]
    public required string UpdatedAt { get; set; }

    /// <summary>
    /// Memo title
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// Full memo content
    /// </summary>
    [JsonPropertyName("content")]
    public required string Content { get; set; }

    /// <summary>
    /// AI-generated summary
    /// </summary>
    [JsonPropertyName("summary")]
    public required string Summary { get; set; }

    /// <summary>
    /// Content length in characters
    /// </summary>
    [JsonPropertyName("content_length")]
    public int ContentLength { get; set; }

    /// <summary>
    /// Custom metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public required Dictionary<string, object> Metadata { get; set; }

    /// <summary>
    /// Client reference ID
    /// </summary>
    [JsonPropertyName("client_reference_id")]
    public string? ClientReferenceId { get; set; }

    /// <summary>
    /// Source system
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// Memo type
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// Expiration date
    /// </summary>
    [JsonPropertyName("expiration_date")]
    public string? ExpirationDate { get; set; }

    /// <summary>
    /// Whether the memo is archived
    /// </summary>
    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    /// <summary>
    /// Whether the memo is pending processing
    /// </summary>
    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    /// <summary>
    /// Tags associated with the memo
    /// </summary>
    [JsonPropertyName("tags")]
    public List<MemoTag> Tags { get; set; } = new List<MemoTag>();

    /// <summary>
    /// Content chunks
    /// </summary>
    [JsonPropertyName("chunks")]
    public List<MemoChunk> Chunks { get; set; } = new List<MemoChunk>();
}

/// <summary>
/// Simplified memo information for list responses
/// </summary>
public class MemoListItem
{
    /// <summary>
    /// Memo UUID
    /// </summary>
    [JsonPropertyName("uuid")]
    public required string Uuid { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    [JsonPropertyName("created_at")]
    public required string CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    [JsonPropertyName("updated_at")]
    public required string UpdatedAt { get; set; }

    /// <summary>
    /// Memo title
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// AI-generated summary
    /// </summary>
    [JsonPropertyName("summary")]
    public required string Summary { get; set; }

    /// <summary>
    /// Content length in characters
    /// </summary>
    [JsonPropertyName("content_length")]
    public int ContentLength { get; set; }

    /// <summary>
    /// Custom metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public required Dictionary<string, object> Metadata { get; set; }

    /// <summary>
    /// Client reference ID
    /// </summary>
    [JsonPropertyName("client_reference_id")]
    public string? ClientReferenceId { get; set; }
}

/// <summary>
/// Paginated list of memos
/// </summary>
public class ListMemosResponse
{
    /// <summary>
    /// Total count of memos
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    /// URL for next page (null if no more pages)
    /// </summary>
    [JsonPropertyName("next")]
    public string? Next { get; set; }

    /// <summary>
    /// URL for previous page (null if on first page)
    /// </summary>
    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    /// <summary>
    /// List of memos on this page
    /// </summary>
    [JsonPropertyName("results")]
    public required List<MemoListItem> Results { get; set; }
}

/// <summary>
/// Parameters for listing memos
/// </summary>
public class ListMemosParams
{
    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// Results per page (default: 20, max: 100)
    /// </summary>
    public int? PageSize { get; set; }
}

/// <summary>
/// Filter operator for filtering search results
/// </summary>
public enum FilterOperator
{
    /// <summary>
    /// Equals (exact match)
    /// </summary>
    Eq,

    /// <summary>
    /// Not equals
    /// </summary>
    Neq,

    /// <summary>
    /// Contains substring (case-insensitive)
    /// </summary>
    Contains,

    /// <summary>
    /// Starts with prefix (case-insensitive)
    /// </summary>
    Startswith,

    /// <summary>
    /// Ends with suffix (case-insensitive)
    /// </summary>
    Endswith,

    /// <summary>
    /// Value is in array
    /// </summary>
    In,

    /// <summary>
    /// Value is not in array
    /// </summary>
    NotIn
}

/// <summary>
/// Type of field being filtered
/// </summary>
public enum FilterType
{
    /// <summary>
    /// Built-in memo field (title, source, client_reference_id, tags)
    /// </summary>
    NativeField,

    /// <summary>
    /// Custom metadata field
    /// </summary>
    CustomMetadata
}

/// <summary>
/// Filter for narrowing search results
/// </summary>
public class Filter
{
    /// <summary>
    /// Field name to filter on
    /// </summary>
    [JsonPropertyName("field")]
    public required string Field { get; set; }

    /// <summary>
    /// Comparison operator
    /// </summary>
    [JsonPropertyName("operator")]
    [JsonConverter(typeof(SnakeCaseEnumConverter<FilterOperator>))]
    public FilterOperator Operator { get; set; }

    /// <summary>
    /// Value(s) to compare against
    /// </summary>
    [JsonPropertyName("value")]
    public required object Value { get; set; }

    /// <summary>
    /// Type of field (native_field or custom_metadata)
    /// </summary>
    [JsonPropertyName("filter_type")]
    [JsonConverter(typeof(SnakeCaseEnumConverter<FilterType>))]
    public FilterType FilterType { get; set; }
}

/// <summary>
/// Search method to use
/// </summary>
public enum SearchMethod
{
    /// <summary>
    /// Semantic search on memo chunks
    /// </summary>
    ChunkSemanticSearch
}

/// <summary>
/// Request for searching memos
/// </summary>
public class SearchRequest
{
    /// <summary>
    /// Search query string (required)
    /// </summary>
    [JsonPropertyName("query")]
    public required string Query { get; set; }

    /// <summary>
    /// Search method to use (required)
    /// </summary>
    [JsonPropertyName("search_method")]
    [JsonConverter(typeof(SnakeCaseEnumConverter<SearchMethod>))]
    public SearchMethod SearchMethod { get; set; }

    /// <summary>
    /// Maximum results to return (1-50, default 10)
    /// </summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Optional filters to narrow results
    /// </summary>
    [JsonPropertyName("filters")]
    public List<Filter>? Filters { get; set; }
}

/// <summary>
/// A single search result
/// </summary>
public class SearchResult
{
    /// <summary>
    /// Memo UUID
    /// </summary>
    [JsonPropertyName("uuid")]
    public required string Uuid { get; set; }

    /// <summary>
    /// Memo title
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// AI-generated summary
    /// </summary>
    [JsonPropertyName("summary")]
    public required string Summary { get; set; }

    /// <summary>
    /// Content snippet
    /// </summary>
    [JsonPropertyName("content_snippet")]
    public required string ContentSnippet { get; set; }

    /// <summary>
    /// Distance score (0-2, lower is more relevant) - null for non-semantic searches
    /// </summary>
    [JsonPropertyName("distance")]
    public double? Distance { get; set; }
}

/// <summary>
/// Response from a search request
/// </summary>
public class SearchResponse
{
    /// <summary>
    /// List of search results
    /// </summary>
    [JsonPropertyName("results")]
    public required List<SearchResult> Results { get; set; }
}

/// <summary>
/// Request for chat
/// </summary>
public class ChatRequest
{
    /// <summary>
    /// Question to ask (required)
    /// </summary>
    [JsonPropertyName("query")]
    public required string Query { get; set; }

    /// <summary>
    /// Whether to stream the response
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    /// <summary>
    /// Optional filters to focus chat context
    /// </summary>
    [JsonPropertyName("filters")]
    public List<Filter>? Filters { get; set; }
}

/// <summary>
/// Response from a chat request
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// Success status
    /// </summary>
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    /// <summary>
    /// AI's answer with inline citations
    /// </summary>
    [JsonPropertyName("response")]
    public required string Response { get; set; }

    /// <summary>
    /// Steps taken by the agent
    /// </summary>
    [JsonPropertyName("intermediate_steps")]
    public required List<object> IntermediateSteps { get; set; }
}

/// <summary>
/// Event from streaming chat
/// </summary>
public class ChatStreamEvent
{
    /// <summary>
    /// Event type (token or done)
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// Token content (for token events)
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
