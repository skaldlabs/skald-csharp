using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Skald;

/// <summary>
/// Custom JSON naming policy for snake_case conversion
/// </summary>
internal class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public static readonly SnakeCaseNamingPolicy Instance = new SnakeCaseNamingPolicy();

    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var builder = new StringBuilder();
        builder.Append(char.ToLowerInvariant(name[0]));

        for (int i = 1; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsUpper(c))
            {
                builder.Append('_');
                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}

/// <summary>
/// Custom JSON converter for enums that uses snake_case
/// </summary>
internal class SnakeCaseEnumConverter<TEnum> : System.Text.Json.Serialization.JsonConverter<TEnum> where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return default;

        // Convert snake_case to PascalCase for parsing
        var pascalCase = string.Join("", value.Split('_').Select(part =>
            char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant()));

        if (Enum.TryParse<TEnum>(pascalCase, true, out var result))
            return result;

        throw new JsonException($"Unable to convert \"{value}\" to {typeof(TEnum).Name}");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        var name = value.ToString();
        var snakeCase = SnakeCaseNamingPolicy.Instance.ConvertName(name);
        writer.WriteStringValue(snakeCase);
    }
}

/// <summary>
/// Client for interacting with the Skald API
/// </summary>
public class SkaldClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly bool _disposeHttpClient;

    /// <summary>
    /// Initialize a new Skald client with an API key
    /// </summary>
    /// <param name="apiKey">Your Skald API key</param>
    /// <param name="baseUrl">Base URL for the API (default: https://api.useskald.com)</param>
    public SkaldClient(string apiKey, string baseUrl = "https://api.useskald.com")
        : this(apiKey, new HttpClient(), baseUrl, disposeHttpClient: true)
    {
    }

    /// <summary>
    /// Initialize a new Skald client with a custom HttpClient
    /// </summary>
    /// <param name="apiKey">Your Skald API key</param>
    /// <param name="httpClient">Custom HttpClient instance</param>
    /// <param name="baseUrl">Base URL for the API (default: https://api.useskald.com)</param>
    /// <param name="disposeHttpClient">Whether to dispose the HttpClient when this client is disposed</param>
    public SkaldClient(string apiKey, HttpClient httpClient, string baseUrl = "https://api.useskald.com", bool disposeHttpClient = false)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
        _baseUrl = baseUrl.TrimEnd('/');

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    /// <summary>
    /// Create a new memo. The memo will be automatically processed (summarized, chunked, and indexed for search).
    /// </summary>
    /// <param name="memoData">The memo creation parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CreateMemoResponse indicating success</returns>
    /// <exception cref="SkaldException">Thrown when the API request fails</exception>
    public async Task<CreateMemoResponse> CreateMemoAsync(MemoData memoData, CancellationToken cancellationToken = default)
    {
        if (memoData == null)
            throw new ArgumentNullException(nameof(memoData));

        memoData.Metadata ??= new Dictionary<string, object>();

        var url = $"{_baseUrl}/api/v1/memo";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var jsonContent = JsonSerializer.Serialize(memoData, options);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new SkaldException($"Skald API error ({(int)response.StatusCode}): {errorText}");
        }

        return await response.Content.ReadFromJsonAsync<CreateMemoResponse>(cancellationToken)
            ?? throw new SkaldException("Failed to deserialize response");
    }

    /// <summary>
    /// Get a memo by UUID or reference ID.
    /// </summary>
    /// <param name="request">The get memo request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete memo details</returns>
    /// <exception cref="SkaldException">Thrown when the API request fails</exception>
    public async Task<Memo> GetMemoAsync(GetMemoRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.MemoId))
            throw new ArgumentException("Memo ID cannot be null or empty", nameof(request.MemoId));

        var url = $"{_baseUrl}/api/v1/memo/{Uri.EscapeDataString(request.MemoId)}";
        if (request.IdType == IdType.ReferenceId)
        {
            url += "?id_type=reference_id";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new SkaldException($"Skald API error ({(int)response.StatusCode}): {errorText}");
        }

        return await response.Content.ReadFromJsonAsync<Memo>(cancellationToken)
            ?? throw new SkaldException("Failed to deserialize response");
    }

    /// <summary>
    /// List all memos in the project with pagination.
    /// </summary>
    /// <param name="parameters">Optional pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of memos</returns>
    /// <exception cref="SkaldException">Thrown when the API request fails</exception>
    public async Task<ListMemosResponse> ListMemosAsync(ListMemosParams? parameters = null, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl}/api/v1/memo";
        var queryParams = new List<string>();

        if (parameters?.Page.HasValue == true)
            queryParams.Add($"page={parameters.Page.Value}");

        if (parameters?.PageSize.HasValue == true)
            queryParams.Add($"page_size={parameters.PageSize.Value}");

        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new SkaldException($"Skald API error ({(int)response.StatusCode}): {errorText}");
        }

        return await response.Content.ReadFromJsonAsync<ListMemosResponse>(cancellationToken)
            ?? throw new SkaldException("Failed to deserialize response");
    }

    /// <summary>
    /// Update an existing memo by UUID or reference ID. If content is updated,
    /// the memo will be reprocessed (summary, tags, chunks regenerated).
    /// </summary>
    /// <param name="request">The update memo request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>UpdateMemoResponse indicating success</returns>
    /// <exception cref="SkaldException">Thrown when the API request fails</exception>
    public async Task<UpdateMemoResponse> UpdateMemoAsync(
        UpdateMemoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.MemoId))
            throw new ArgumentException("Memo ID cannot be null or empty", nameof(request.MemoId));

        if (request.UpdateData == null)
            throw new ArgumentNullException(nameof(request.UpdateData));

        var url = $"{_baseUrl}/api/v1/memo/{Uri.EscapeDataString(request.MemoId)}";
        if (request.IdType == IdType.ReferenceId)
        {
            url += "?id_type=reference_id";
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var jsonContent = JsonSerializer.Serialize(request.UpdateData, options);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = content
        };

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new SkaldException($"Skald API error ({(int)response.StatusCode}): {errorText}");
        }

        return await response.Content.ReadFromJsonAsync<UpdateMemoResponse>(cancellationToken)
            ?? throw new SkaldException("Failed to deserialize response");
    }

    /// <summary>
    /// Delete a memo by UUID or reference ID. This permanently deletes the memo
    /// and all associated data (content, summary, tags, chunks).
    /// </summary>
    /// <param name="request">The delete memo request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>DeleteMemoResponse indicating success</returns>
    /// <exception cref="SkaldException">Thrown when the API request fails</exception>
    public async Task<DeleteMemoResponse> DeleteMemoAsync(DeleteMemoRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.MemoId))
            throw new ArgumentException("Memo ID cannot be null or empty", nameof(request.MemoId));

        var url = $"{_baseUrl}/api/v1/memo/{Uri.EscapeDataString(request.MemoId)}";
        if (request.IdType == IdType.ReferenceId)
        {
            url += "?id_type=reference_id";
        }

        var response = await _httpClient.DeleteAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new SkaldException($"Skald API error ({(int)response.StatusCode}): {errorText}");
        }

        if (response.Content.Headers.ContentLength == 0 ||
            response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return new DeleteMemoResponse { Ok = true };
        }

        return await response.Content.ReadFromJsonAsync<DeleteMemoResponse>(cancellationToken)
            ?? throw new SkaldException("Failed to deserialize response");
    }

    /// <summary>
    /// Search through memos using various search methods with optional filtering.
    /// </summary>
    /// <param name="searchRequest">The search parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results with memo details and relevance scores</returns>
    /// <exception cref="SkaldException">Thrown when the API request fails</exception>
    public async Task<SearchResponse> SearchAsync(SearchRequest searchRequest, CancellationToken cancellationToken = default)
    {
        if (searchRequest == null)
            throw new ArgumentNullException(nameof(searchRequest));

        var url = $"{_baseUrl}/api/v1/search";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var jsonContent = JsonSerializer.Serialize(searchRequest, options);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new SkaldException($"Skald API error ({(int)response.StatusCode}): {errorText}");
        }

        return await response.Content.ReadFromJsonAsync<SearchResponse>(cancellationToken)
            ?? throw new SkaldException("Failed to deserialize response");
    }

    /// <summary>
    /// Ask questions about your knowledge base using an AI agent with optional filtering (non-streaming).
    /// </summary>
    /// <param name="chatParams">The chat request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete chat response with answer, citations, and metadata</returns>
    /// <exception cref="SkaldException">Thrown when the API request fails</exception>
    public async Task<ChatResponse> ChatAsync(ChatRequest chatParams, CancellationToken cancellationToken = default)
    {
        if (chatParams == null)
            throw new ArgumentNullException(nameof(chatParams));

        if (string.IsNullOrWhiteSpace(chatParams.Query))
            throw new ArgumentException("Query cannot be null or empty", nameof(chatParams.Query));

        // Ensure stream is set to false
        var chatRequest = new ChatRequest
        {
            Query = chatParams.Query,
            Stream = false,
            ChatId = chatParams.ChatId,
            SystemPrompt = chatParams.SystemPrompt,
            Filters = chatParams.Filters,
            RagConfig = chatParams.RagConfig,
            ProjectId = chatParams.ProjectId
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var url = $"{_baseUrl}/api/v1/chat";
        var jsonContent = JsonSerializer.Serialize(chatRequest, options);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new SkaldException($"Skald API error ({(int)response.StatusCode}): {errorText}");
        }

        return await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken)
            ?? throw new SkaldException("Failed to deserialize response");
    }

    /// <summary>
    /// Ask questions about your knowledge base using an AI agent with streaming responses and optional filtering.
    /// Returns an async enumerable that yields tokens as they arrive.
    /// </summary>
    /// <param name="chatParams">The chat request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of chat stream events</returns>
    /// <exception cref="SkaldException">Thrown when the API request fails</exception>
    public async IAsyncEnumerable<ChatStreamEvent> StreamedChatAsync(
        ChatRequest chatParams,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (chatParams == null)
            throw new ArgumentNullException(nameof(chatParams));

        if (string.IsNullOrWhiteSpace(chatParams.Query))
            throw new ArgumentException("Query cannot be null or empty", nameof(chatParams.Query));

        // Ensure stream is set to true
        var chatRequest = new ChatRequest
        {
            Query = chatParams.Query,
            Stream = true,
            ChatId = chatParams.ChatId,
            SystemPrompt = chatParams.SystemPrompt,
            Filters = chatParams.Filters,
            RagConfig = chatParams.RagConfig,
            ProjectId = chatParams.ProjectId
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var jsonContent = JsonSerializer.Serialize(chatRequest, options);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var url = $"{_baseUrl}/api/v1/chat";
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new SkaldException($"Skald API error ({(int)response.StatusCode}): {errorText}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var buffer = new StringBuilder();

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6);
                var streamEvent = TryDeserialize<ChatStreamEvent>(data);
                if (streamEvent != null)
                {
                    yield return streamEvent;

                    if (streamEvent.Type == "done")
                        yield break;
                }
            }
            // Skip ping lines (": ping") and other lines
        }
    }

    /// <summary>
    /// Create a memo from a file (PDF, DOC, DOCX, PPTX). The file will be processed asynchronously.
    /// Use CheckMemoStatusAsync to monitor processing status.
    /// </summary>
    /// <param name="fileData">The file data and metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CreateMemoFromFileResponse with memo UUID</returns>
    /// <exception cref="SkaldException">Thrown when the API request fails</exception>
    public async Task<CreateMemoFromFileResponse> CreateMemoFromFileAsync(MemoFileData fileData, CancellationToken cancellationToken = default)
    {
        if (fileData == null)
            throw new ArgumentNullException(nameof(fileData));

        if (fileData.File == null || fileData.File.Length == 0)
            throw new ArgumentException("File cannot be null or empty", nameof(fileData.File));

        if (string.IsNullOrWhiteSpace(fileData.Filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(fileData.Filename));

        var url = $"{_baseUrl}/api/v1/memo";

        using var formData = new MultipartFormDataContent();

        // Add file
        var fileContent = new ByteArrayContent(fileData.File);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        formData.Add(fileContent, "file", fileData.Filename);

        // Add optional fields
        if (!string.IsNullOrWhiteSpace(fileData.Title))
            formData.Add(new StringContent(fileData.Title), "title");

        if (!string.IsNullOrWhiteSpace(fileData.ReferenceId))
            formData.Add(new StringContent(fileData.ReferenceId), "reference_id");

        if (fileData.Metadata != null && fileData.Metadata.Count > 0)
        {
            var metadataJson = JsonSerializer.Serialize(fileData.Metadata);
            formData.Add(new StringContent(metadataJson), "metadata");
        }

        if (fileData.Tags != null && fileData.Tags.Count > 0)
        {
            var tagsJson = JsonSerializer.Serialize(fileData.Tags);
            formData.Add(new StringContent(tagsJson), "tags");
        }

        if (!string.IsNullOrWhiteSpace(fileData.Source))
            formData.Add(new StringContent(fileData.Source), "source");

        var response = await _httpClient.PostAsync(url, formData, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new SkaldException($"Skald API error ({(int)response.StatusCode}): {errorText}");
        }

        return await response.Content.ReadFromJsonAsync<CreateMemoFromFileResponse>(cancellationToken)
            ?? throw new SkaldException("Failed to deserialize response");
    }

    /// <summary>
    /// Check the processing status of a memo (useful for file uploads).
    /// </summary>
    /// <param name="request">The check status request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>MemoStatusResponse with current status</returns>
    /// <exception cref="SkaldException">Thrown when the API request fails</exception>
    public async Task<MemoStatusResponse> CheckMemoStatusAsync(CheckMemoStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.MemoId))
            throw new ArgumentException("Memo ID cannot be null or empty", nameof(request.MemoId));

        var url = $"{_baseUrl}/api/v1/memo/{Uri.EscapeDataString(request.MemoId)}/status";
        if (request.IdType == IdType.ReferenceId)
        {
            url += "?id_type=reference_id";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new SkaldException($"Skald API error ({(int)response.StatusCode}): {errorText}");
        }

        return await response.Content.ReadFromJsonAsync<MemoStatusResponse>(cancellationToken)
            ?? throw new SkaldException("Failed to deserialize response");
    }

    /// <summary>
    /// Try to deserialize JSON, returning null on failure
    /// </summary>
    private static T? TryDeserialize<T>(string json) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Dispose the client and optionally the HttpClient
    /// </summary>
    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient?.Dispose();
        }
    }
}

/// <summary>
/// Exception thrown when Skald API requests fail
/// </summary>
public class SkaldException : Exception
{
    /// <summary>
    /// Initialize a new Skald exception
    /// </summary>
    /// <param name="message">Error message</param>
    public SkaldException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initialize a new Skald exception with an inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public SkaldException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
