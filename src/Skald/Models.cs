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

    /// <summary>
    /// UUID of the created memo
    /// </summary>
    [JsonPropertyName("memo_uuid")]
    public required string MemoUuid { get; set; }
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
/// Request for getting a memo
/// </summary>
public class GetMemoRequest
{
    /// <summary>
    /// The memo UUID or client reference ID
    /// </summary>
    public required string MemoId { get; set; }

    /// <summary>
    /// The type of identifier (default: MemoUuid)
    /// </summary>
    public IdType IdType { get; set; } = IdType.MemoUuid;
}

/// <summary>
/// Request for updating a memo
/// </summary>
public class UpdateMemoRequest
{
    /// <summary>
    /// The memo UUID or client reference ID
    /// </summary>
    public required string MemoId { get; set; }

    /// <summary>
    /// The fields to update
    /// </summary>
    public required UpdateMemoData UpdateData { get; set; }

    /// <summary>
    /// The type of identifier (default: MemoUuid)
    /// </summary>
    public IdType IdType { get; set; } = IdType.MemoUuid;
}

/// <summary>
/// Request for deleting a memo
/// </summary>
public class DeleteMemoRequest
{
    /// <summary>
    /// The memo UUID or client reference ID
    /// </summary>
    public required string MemoId { get; set; }

    /// <summary>
    /// The type of identifier (default: MemoUuid)
    /// </summary>
    public IdType IdType { get; set; } = IdType.MemoUuid;
}

/// <summary>
/// Response from deleting a memo
/// </summary>
public class DeleteMemoResponse
{
    /// <summary>
    /// Success status
    /// </summary>
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }
}

/// <summary>
/// Data for creating a memo from a file
/// </summary>
public class MemoFileData
{
    /// <summary>
    /// The file content as a byte array (required)
    /// </summary>
    public required byte[] File { get; set; }

    /// <summary>
    /// The filename with extension (required)
    /// </summary>
    public required string Filename { get; set; }

    /// <summary>
    /// Optional title for the memo (defaults to filename if not provided)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Optional external reference ID
    /// </summary>
    public string? ReferenceId { get; set; }

    /// <summary>
    /// Optional custom JSON metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Optional array of tags
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Optional source system name
    /// </summary>
    public string? Source { get; set; }
}

/// <summary>
/// Response from creating a memo from a file
/// </summary>
public class CreateMemoFromFileResponse
{
    /// <summary>
    /// Success status
    /// </summary>
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    /// <summary>
    /// UUID of the created memo
    /// </summary>
    [JsonPropertyName("memo_uuid")]
    public required string MemoUuid { get; set; }
}

/// <summary>
/// Memo processing status
/// </summary>
public enum MemoStatus
{
    /// <summary>
    /// Memo is currently being processed
    /// </summary>
    Processing,

    /// <summary>
    /// Memo has been successfully processed
    /// </summary>
    Processed,

    /// <summary>
    /// An error occurred during processing
    /// </summary>
    Error
}

/// <summary>
/// Request for checking memo status
/// </summary>
public class CheckMemoStatusRequest
{
    /// <summary>
    /// The memo UUID or client reference ID
    /// </summary>
    public required string MemoId { get; set; }

    /// <summary>
    /// The type of identifier (default: MemoUuid)
    /// </summary>
    public IdType IdType { get; set; } = IdType.MemoUuid;
}

/// <summary>
/// Response from checking memo status
/// </summary>
public class MemoStatusResponse
{
    /// <summary>
    /// Current processing status
    /// </summary>
    [JsonPropertyName("status")]
    [JsonConverter(typeof(SnakeCaseEnumConverter<MemoStatus>))]
    public MemoStatus Status { get; set; }

    /// <summary>
    /// Error reason if status is Error
    /// </summary>
    [JsonPropertyName("error_reason")]
    public string? ErrorReason { get; set; }
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
    [JsonPropertyName("memo_uuid")]
    public required string MemoUuid { get; set; }

    /// <summary>
    /// Chunk UUID
    /// </summary>
    [JsonPropertyName("chunk_uuid")]
    public required string ChunkUuid { get; set; }

    /// <summary>
    /// Memo title
    /// </summary>
    [JsonPropertyName("memo_title")]
    public required string MemoTitle { get; set; }

    /// <summary>
    /// AI-generated summary of the memo
    /// </summary>
    [JsonPropertyName("memo_summary")]
    public required string MemoSummary { get; set; }

    /// <summary>
    /// Content snippet from the chunk
    /// </summary>
    [JsonPropertyName("content_snippet")]
    public required string ContentSnippet { get; set; }

    /// <summary>
    /// Full content of the matching chunk
    /// </summary>
    [JsonPropertyName("chunk_content")]
    public required string ChunkContent { get; set; }

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
/// LLM provider options
/// </summary>
public enum LLMProvider
{
    /// <summary>
    /// OpenAI
    /// </summary>
    Openai,

    /// <summary>
    /// Anthropic
    /// </summary>
    Anthropic,

    /// <summary>
    /// Groq
    /// </summary>
    Groq
}

/// <summary>
/// Configuration for query rewriting
/// </summary>
public class QueryRewriteConfig
{
    /// <summary>
    /// Whether query rewriting is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

/// <summary>
/// Configuration for vector search
/// </summary>
public class VectorSearchConfig
{
    /// <summary>
    /// Number of results to return (1-200)
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    /// <summary>
    /// Similarity threshold (0.0-1.0)
    /// </summary>
    [JsonPropertyName("similarity_threshold")]
    public double? SimilarityThreshold { get; set; }
}

/// <summary>
/// Configuration for reranking
/// </summary>
public class RerankingConfig
{
    /// <summary>
    /// Whether reranking is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Number of results after reranking
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }
}

/// <summary>
/// Configuration for references
/// </summary>
public class ReferencesConfig
{
    /// <summary>
    /// Whether references are enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

/// <summary>
/// RAG (Retrieval-Augmented Generation) configuration
/// </summary>
public class RAGConfig
{
    /// <summary>
    /// LLM provider to use
    /// </summary>
    [JsonPropertyName("llm_provider")]
    [JsonConverter(typeof(SnakeCaseEnumConverter<LLMProvider>))]
    public LLMProvider? LlmProvider { get; set; }

    /// <summary>
    /// Query rewrite configuration
    /// </summary>
    [JsonPropertyName("query_rewrite")]
    public QueryRewriteConfig? QueryRewrite { get; set; }

    /// <summary>
    /// Vector search configuration
    /// </summary>
    [JsonPropertyName("vector_search")]
    public VectorSearchConfig? VectorSearch { get; set; }

    /// <summary>
    /// Reranking configuration
    /// </summary>
    [JsonPropertyName("reranking")]
    public RerankingConfig? Reranking { get; set; }

    /// <summary>
    /// References configuration
    /// </summary>
    [JsonPropertyName("references")]
    public ReferencesConfig? References { get; set; }
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
    /// Optional chat ID to continue a conversation
    /// </summary>
    [JsonPropertyName("chat_id")]
    public string? ChatId { get; set; }

    /// <summary>
    /// Optional system prompt to guide the AI's behavior
    /// </summary>
    [JsonPropertyName("system_prompt")]
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Optional filters to focus chat context
    /// </summary>
    [JsonPropertyName("filters")]
    public List<Filter>? Filters { get; set; }

    /// <summary>
    /// Optional RAG configuration
    /// </summary>
    [JsonPropertyName("rag_config")]
    public RAGConfig? RagConfig { get; set; }

    /// <summary>
    /// Optional project ID (required with Token Authentication)
    /// </summary>
    [JsonPropertyName("project_id")]
    public string? ProjectId { get; set; }
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

    /// <summary>
    /// Chat ID for conversation continuity
    /// </summary>
    [JsonPropertyName("chat_id")]
    public string? ChatId { get; set; }

    /// <summary>
    /// References mapping citation numbers to memo information
    /// </summary>
    [JsonPropertyName("references")]
    public Dictionary<string, object>? References { get; set; }
}

/// <summary>
/// Event from streaming chat
/// </summary>
public class ChatStreamEvent
{
    /// <summary>
    /// Event type (token, references, or done)
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>
    /// Token content (for token events) or references content (for references events)
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Chat ID for conversation continuity (for done events)
    /// </summary>
    [JsonPropertyName("chat_id")]
    public string? ChatId { get; set; }
}
