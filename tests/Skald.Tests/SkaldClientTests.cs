using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Skald.Tests;

public class SkaldClientTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly SkaldClient _client;
    private const string ApiKey = "test-api-key";
    private const string BaseUrl = "https://api.test.com";

    public SkaldClientTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _client = new SkaldClient(ApiKey, _httpClient, BaseUrl, disposeHttpClient: false);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _httpClient?.Dispose();
    }

    [Fact]
    public void Constructor_WithApiKey_ShouldInitialize()
    {
        using var client = new SkaldClient("test-key");
        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithNullApiKey_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SkaldClient(null!));
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SkaldClient(""));
    }

    [Fact]
    public async Task CreateMemoAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var memoData = new MemoData
        {
            Title = "Test Memo",
            Content = "Test content",
            Metadata = new Dictionary<string, object> { { "type", "test" } },
            Tags = new List<string> { "test" },
            Source = "test-source"
        };

        var expectedResponse = new CreateMemoResponse { Ok = true, MemoUuid = "test-uuid-123" };
        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.CreateMemoAsync(memoData);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Ok);
        VerifyHttpRequest(HttpMethod.Post, $"{BaseUrl}/api/v1/memo");
    }

    [Fact]
    public async Task CreateMemoAsync_WithNullData_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.CreateMemoAsync(null!));
    }

    [Fact]
    public async Task CreateMemoAsync_WithoutMetadata_ShouldInitializeEmptyMetadata()
    {
        // Arrange
        var memoData = new MemoData
        {
            Title = "Test Memo",
            Content = "Test content"
        };

        var expectedResponse = new CreateMemoResponse { Ok = true, MemoUuid = "test-uuid-123" };
        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.CreateMemoAsync(memoData);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Ok);
        Assert.NotNull(memoData.Metadata);
    }

    [Fact]
    public async Task CreateMemoAsync_WithApiError_ShouldThrowSkaldException()
    {
        // Arrange
        var memoData = new MemoData
        {
            Title = "Test Memo",
            Content = "Test content"
        };

        SetupHttpResponse(HttpStatusCode.BadRequest, "Bad Request");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<SkaldException>(() => _client.CreateMemoAsync(memoData));
        Assert.Contains("400", exception.Message);
        Assert.Contains("Bad Request", exception.Message);
    }

    [Fact]
    public async Task GetMemoAsync_ByUuid_ShouldReturnMemo()
    {
        // Arrange
        var memoUuid = "550e8400-e29b-41d4-a716-446655440000";
        var expectedMemo = CreateSampleMemo(memoUuid);
        SetupHttpResponse(HttpStatusCode.OK, expectedMemo);

        // Act
        var result = await _client.GetMemoAsync(new GetMemoRequest { MemoId = memoUuid });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(memoUuid, result.Uuid);
        VerifyHttpRequest(HttpMethod.Get, $"{BaseUrl}/api/v1/memo/{memoUuid}");
    }

    [Fact]
    public async Task GetMemoAsync_ByReferenceId_ShouldReturnMemo()
    {
        // Arrange
        var referenceId = "external-id-123";
        var expectedMemo = CreateSampleMemo("some-uuid");
        SetupHttpResponse(HttpStatusCode.OK, expectedMemo);

        // Act
        var result = await _client.GetMemoAsync(new GetMemoRequest { MemoId = referenceId, IdType = IdType.ReferenceId });

        // Assert
        Assert.NotNull(result);
        VerifyHttpRequest(HttpMethod.Get, $"{BaseUrl}/api/v1/memo/{referenceId}?id_type=reference_id");
    }

    [Fact]
    public async Task GetMemoAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.GetMemoAsync(new GetMemoRequest { MemoId = "" }));
    }

    [Fact]
    public async Task ListMemosAsync_WithDefaultParams_ShouldReturnList()
    {
        // Arrange
        var expectedResponse = new ListMemosResponse
        {
            Count = 2,
            Next = null,
            Previous = null,
            Results = new List<MemoListItem>
            {
                CreateSampleMemoListItem("uuid1"),
                CreateSampleMemoListItem("uuid2")
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.ListMemosAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.Results.Count);
        VerifyHttpRequest(HttpMethod.Get, $"{BaseUrl}/api/v1/memo");
    }

    [Fact]
    public async Task ListMemosAsync_WithPagination_ShouldIncludeQueryParams()
    {
        // Arrange
        var parameters = new ListMemosParams { Page = 2, PageSize = 50 };
        var expectedResponse = new ListMemosResponse
        {
            Count = 100,
            Next = "next-url",
            Previous = "prev-url",
            Results = new List<MemoListItem>()
        };
        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.ListMemosAsync(parameters);

        // Assert
        Assert.NotNull(result);
        VerifyHttpRequest(HttpMethod.Get, $"{BaseUrl}/api/v1/memo?page=2&page_size=50");
    }

    [Fact]
    public async Task UpdateMemoAsync_ByUuid_ShouldReturnSuccess()
    {
        // Arrange
        var memoUuid = "550e8400-e29b-41d4-a716-446655440000";
        var updateData = new UpdateMemoData
        {
            Title = "Updated Title",
            Metadata = new Dictionary<string, object> { { "status", "reviewed" } }
        };
        var expectedResponse = new UpdateMemoResponse { Ok = true };
        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.UpdateMemoAsync(new UpdateMemoRequest { MemoId = memoUuid, UpdateData = updateData });

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Ok);
        VerifyHttpRequest(new HttpMethod("PATCH"), $"{BaseUrl}/api/v1/memo/{memoUuid}");
    }

    [Fact]
    public async Task UpdateMemoAsync_ByReferenceId_ShouldIncludeQueryParam()
    {
        // Arrange
        var referenceId = "external-id-123";
        var updateData = new UpdateMemoData { Title = "Updated Title" };
        var expectedResponse = new UpdateMemoResponse { Ok = true };
        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.UpdateMemoAsync(new UpdateMemoRequest { MemoId = referenceId, UpdateData = updateData, IdType = IdType.ReferenceId });

        // Assert
        Assert.NotNull(result);
        VerifyHttpRequest(new HttpMethod("PATCH"), $"{BaseUrl}/api/v1/memo/{referenceId}?id_type=reference_id");
    }

    [Fact]
    public async Task UpdateMemoAsync_WithNullData_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.UpdateMemoAsync(new UpdateMemoRequest { MemoId = "uuid", UpdateData = null! }));
    }

    [Fact]
    public async Task DeleteMemoAsync_ByUuid_ShouldSucceed()
    {
        // Arrange
        var memoUuid = "550e8400-e29b-41d4-a716-446655440000";
        var expectedResponse = new DeleteMemoResponse { Ok = true };
        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.DeleteMemoAsync(new DeleteMemoRequest { MemoId = memoUuid });

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Ok);
        VerifyHttpRequest(HttpMethod.Delete, $"{BaseUrl}/api/v1/memo/{memoUuid}");
    }

    [Fact]
    public async Task DeleteMemoAsync_ByReferenceId_ShouldIncludeQueryParam()
    {
        // Arrange
        var referenceId = "external-id-123";
        var expectedResponse = new DeleteMemoResponse { Ok = true };
        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.DeleteMemoAsync(new DeleteMemoRequest { MemoId = referenceId, IdType = IdType.ReferenceId });

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Ok);
        VerifyHttpRequest(HttpMethod.Delete, $"{BaseUrl}/api/v1/memo/{referenceId}?id_type=reference_id");
    }

    [Fact]
    public async Task SearchAsync_WithChunkSemanticSearch_ShouldReturnResults()
    {
        // Arrange
        var searchRequest = new SearchRequest
        {
            Query = "test query",
            SearchMethod = SearchMethod.ChunkSemanticSearch,
            Limit = 10
        };

        var expectedResponse = new SearchResponse
        {
            Results = new List<SearchResult>
            {
                new SearchResult
                {
                    Uuid = "test-uuid",
                    Title = "Test Memo",
                    Summary = "Test summary",
                    ContentSnippet = "Test snippet",
                    Distance = 0.5
                }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.SearchAsync(searchRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Results);
        Assert.Equal(0.5, result.Results[0].Distance);
        VerifyHttpRequest(HttpMethod.Post, $"{BaseUrl}/api/v1/search");
    }

    [Fact]
    public async Task SearchAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.SearchAsync(null!));
    }

    [Fact]
    public async Task ChatAsync_WithQuery_ShouldReturnResponse()
    {
        // Arrange
        var query = "What is this about?";
        var expectedResponse = new ChatResponse
        {
            Ok = true,
            Response = "This is the answer",
            IntermediateSteps = new List<object>()
        };
        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.ChatAsync(new ChatRequest { Query = query });

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Ok);
        Assert.Equal("This is the answer", result.Response);
        VerifyHttpRequest(HttpMethod.Post, $"{BaseUrl}/api/v1/chat");
    }

    [Fact]
    public async Task ChatAsync_WithEmptyQuery_ShouldThrowArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.ChatAsync(new ChatRequest { Query = "" }));
    }

    [Fact]
    public async Task ChatAsync_WithFilters_ShouldIncludeFilters()
    {
        // Arrange
        var query = "test query";
        var filters = new List<Filter>
        {
            new Filter
            {
                Field = "source",
                Operator = FilterOperator.Eq,
                Value = "notion",
                FilterType = FilterType.NativeField
            }
        };
        var expectedResponse = new ChatResponse
        {
            Ok = true,
            Response = "Filtered answer",
            IntermediateSteps = new List<object>()
        };
        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _client.ChatAsync(new ChatRequest { Query = query, Filters = filters });

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Filtered answer", result.Response);
    }

    [Fact]
    public async Task StreamedChatAsync_ShouldYieldEvents()
    {
        // Arrange
        var query = "Tell me something";
        var streamData = "data: {\"type\":\"token\",\"content\":\"Hello\"}\ndata: {\"type\":\"token\",\"content\":\" world\"}\ndata: {\"type\":\"done\"}\n";
        SetupStreamingHttpResponse(HttpStatusCode.OK, streamData);

        // Act
        var events = new List<ChatStreamEvent>();
        await foreach (var evt in _client.StreamedChatAsync(new ChatRequest { Query = query }))
        {
            events.Add(evt);
        }

        // Assert
        Assert.Equal(3, events.Count);
        Assert.Equal("token", events[0].Type);
        Assert.Equal("Hello", events[0].Content);
        Assert.Equal("token", events[1].Type);
        Assert.Equal(" world", events[1].Content);
        Assert.Equal("done", events[2].Type);
    }

    [Fact]
    public async Task StreamedChatAsync_ShouldSkipInvalidJson()
    {
        // Arrange
        var query = "test";
        var streamData = "data: {\"type\":\"token\",\"content\":\"valid\"}\ndata: invalid json\ndata: {\"type\":\"done\"}\n";
        SetupStreamingHttpResponse(HttpStatusCode.OK, streamData);

        // Act
        var events = new List<ChatStreamEvent>();
        await foreach (var evt in _client.StreamedChatAsync(new ChatRequest { Query = query }))
        {
            events.Add(evt);
        }

        // Assert
        Assert.Equal(2, events.Count);
        Assert.Equal("valid", events[0].Content);
        Assert.Equal("done", events[1].Type);
    }

    [Fact]
    public async Task StreamedChatAsync_ShouldSkipPingLines()
    {
        // Arrange
        var query = "test";
        var streamData = ": ping\ndata: {\"type\":\"token\",\"content\":\"test\"}\n: ping\ndata: {\"type\":\"done\"}\n";
        SetupStreamingHttpResponse(HttpStatusCode.OK, streamData);

        // Act
        var events = new List<ChatStreamEvent>();
        await foreach (var evt in _client.StreamedChatAsync(new ChatRequest { Query = query }))
        {
            events.Add(evt);
        }

        // Assert
        Assert.Equal(2, events.Count);
    }

    // Helper methods

    private void SetupHttpResponse<T>(HttpStatusCode statusCode, T content)
    {
        var json = JsonSerializer.Serialize(content);
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content, Encoding.UTF8, "text/plain")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void SetupStreamingHttpResponse(HttpStatusCode statusCode, string streamData)
    {
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(streamData, Encoding.UTF8, "text/event-stream")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void VerifyHttpRequest(HttpMethod method, string url)
    {
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri!.ToString().StartsWith(url)),
                ItExpr.IsAny<CancellationToken>());
    }

    private static Memo CreateSampleMemo(string uuid)
    {
        return new Memo
        {
            Uuid = uuid,
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-01T00:00:00Z",
            Title = "Test Memo",
            Content = "Test content",
            Summary = "Test summary",
            ContentLength = 100,
            Metadata = new Dictionary<string, object>(),
            ClientReferenceId = null,
            Source = null,
            Type = "memo",
            ExpirationDate = null,
            Archived = false,
            Pending = false,
            Tags = new List<MemoTag>(),
            Chunks = new List<MemoChunk>()
        };
    }

    private static MemoListItem CreateSampleMemoListItem(string uuid)
    {
        return new MemoListItem
        {
            Uuid = uuid,
            CreatedAt = "2024-01-01T00:00:00Z",
            UpdatedAt = "2024-01-01T00:00:00Z",
            Title = "Test Memo",
            Summary = "Test summary",
            ContentLength = 100,
            Metadata = new Dictionary<string, object>(),
            ClientReferenceId = null
        };
    }
}
