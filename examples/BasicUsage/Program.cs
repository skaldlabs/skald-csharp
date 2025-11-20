using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Skald;

namespace BasicUsage;

class Program
{
    static async Task Main(string[] args)
    {
        // Get API key from environment variable
        var apiKey = Environment.GetEnvironmentVariable("SKALD_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Please set the SKALD_API_KEY environment variable");
            return;
        }

        // Initialize the client
        using var client = new SkaldClient(apiKey);

        try
        {
            // // Example 1: Create a memo
            Console.WriteLine("Creating a memo...");
            var createResponse = await client.CreateMemoAsync(new MemoData
            {
                Title = "Meeting Notes - Q1 Planning",
                Content = "Discussion about Q1 goals and objectives. Key points: increase revenue, expand team, launch new product.",
                Metadata = new Dictionary<string, object>
                {
                    { "type", "notes" },
                    { "author", "John Doe" },
                    { "department", "engineering" }
                },
                ReferenceId = "meeting-2024-q1-planning",
                Tags = new List<string> { "meeting", "q1", "planning" },
                Source = "notion"
            });
            Console.WriteLine($"Memo created successfully with UUID: {createResponse.MemoUuid}");

            // // Example 2: List memos
            Console.WriteLine("\nListing memos...");
            var memos = await client.ListMemosAsync(new ListMemosParams
            {
                Page = 1,
                PageSize = 10
            });
            Console.WriteLine($"Found {memos.Count} total memos");
            foreach (var memo in memos.Results)
            {
                Console.WriteLine($"  - {memo.Title} (UUID: {memo.Uuid})");
            }

            // Example 3: Get a memo by UUID
            if (memos.Results.Count > 0)
            {
                var firstMemoUuid = memos.Results[0].Uuid;
                Console.WriteLine($"\nGetting memo {firstMemoUuid}...");
                var memo = await client.GetMemoAsync(new GetMemoRequest { MemoId = firstMemoUuid });
                Console.WriteLine($"Title: {memo.Title}");
                Console.WriteLine($"Summary: {memo.Summary}");
                Console.WriteLine($"Tags: {string.Join(", ", memo.Tags.ConvertAll(t => t.Tag))}");
            }

            // Example 4: Search memos
            Console.WriteLine("\nSearching for memos about 'planning'...");
            var searchResults = await client.SearchAsync(new SearchRequest
            {
                Query = "planning goals",
                Limit = 5
            });
            Console.WriteLine($"Found {searchResults.Results.Count} results:");
            foreach (var result in searchResults.Results)
            {
                Console.WriteLine($"  - {result.MemoTitle} (Distance: {result.Distance})");
                Console.WriteLine($"    Snippet: {result.ContentSnippet.Substring(0, Math.Min(100, result.ContentSnippet.Length))}...");
            }

            // Example 5: Search with filters
            Console.WriteLine("\nSearching with filters...");
            var filteredSearch = await client.SearchAsync(new SearchRequest
            {
                Query = "goals",
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
                        Field = "tags",
                        Operator = FilterOperator.In,
                        Value = new[] { "meeting", "planning" },
                        FilterType = FilterType.NativeField
                    }
                }
            });
            Console.WriteLine($"Found {filteredSearch.Results.Count} filtered results");

            // Example 6: Chat (non-streaming)
            Console.WriteLine("\nAsking a question...");
            var chatResponse = await client.ChatAsync(new ChatRequest
            {
                Query = "What are the main goals for Q1?"
            });
            Console.WriteLine($"Answer: {chatResponse.Response}");
            Console.WriteLine($"Chat ID: {chatResponse.ChatId}");

            // Example 7: Chat with conversation continuity
            if (!string.IsNullOrEmpty(chatResponse.ChatId))
            {
                Console.WriteLine("\nContinuing the conversation...");
                var followUpResponse = await client.ChatAsync(new ChatRequest
                {
                    Query = "Can you elaborate on the first goal?",
                    ChatId = chatResponse.ChatId
                });
                Console.WriteLine($"Follow-up answer: {followUpResponse.Response}");
            }

            // Example 8: Chat with RAG configuration
            Console.WriteLine("\nAsking with custom RAG config...");
            var ragResponse = await client.ChatAsync(new ChatRequest
            {
                Query = "Summarize the planning meeting",
                RagConfig = new RAGConfig
                {
                    LlmProvider = LLMProvider.Anthropic,
                    QueryRewrite = new QueryRewriteConfig { Enabled = true },
                    VectorSearch = new VectorSearchConfig { TopK = 10, SimilarityThreshold = 0.7 },
                    Reranking = new RerankingConfig { Enabled = true, TopK = 5 },
                    References = new ReferencesConfig { Enabled = true }
                }
            });
            Console.WriteLine($"Answer: {ragResponse.Response}");
            if (ragResponse.References != null && ragResponse.References.Count > 0)
            {
                Console.WriteLine("References:");
                foreach (var reference in ragResponse.References)
                {
                    Console.WriteLine($"  [{reference.Key}] => Memo UUID: {reference.Value}");
                }
            }

            // Example 9: Chat with streaming
            Console.WriteLine("\nAsking a question with streaming...");
            await foreach (var evt in client.StreamedChatAsync(new ChatRequest
            {
                Query = "Summarize the key points from planning meetings"
            }))
            {
                if (evt.Type == "token" && evt.Content != null)
                {
                    Console.Write(evt.Content);
                }
                else if (evt.Type == "references" && evt.Content != null)
                {
                    Console.WriteLine($"\n[References: {evt.Content}]");
                }
                else if (evt.Type == "done")
                {
                    Console.WriteLine($"\nDone! (Chat ID: {evt.ChatId})");
                }
            }

            // Example 10: Create memo from file (if you have a PDF file)
            
            Console.WriteLine("\nCreating memo from PDF file...");
            var pdfBytes = await File.ReadAllBytesAsync("./localcurrency-snippet.pdf");
            var fileResponse = await client.CreateMemoFromFileAsync(new MemoFileData
            {
                File = pdfBytes,
                Filename = "document.pdf",
                Title = "Important Document",
                Tags = new List<string> { "document", "pdf" },
                Source = "file-upload"
            });
            Console.WriteLine($"File uploaded successfully with UUID: {fileResponse.MemoUuid}");

            // Check processing status
            Console.WriteLine("\nChecking memo status...");
            MemoStatus status;
            do
            {
                await Task.Delay(2000); // Wait 2 seconds
                var statusResponse = await client.CheckMemoStatusAsync(new CheckMemoStatusRequest
                {
                    MemoId = fileResponse.MemoUuid
                });
                status = statusResponse.Status;
                Console.WriteLine($"Status: {status}");

                if (status == MemoStatus.Error && statusResponse.ErrorReason != null)
                {
                    Console.WriteLine($"Error: {statusResponse.ErrorReason}");
                    break;
                }
            } while (status == MemoStatus.Processing);

            if (status == MemoStatus.Processed)
            {
                Console.WriteLine("File processed successfully!");
            }
            

            // // Example 11: Update a memo
            if (memos.Results.Count > 0)
            {
                var memoToUpdate = memos.Results[0].Uuid;
                Console.WriteLine($"\nUpdating memo {memoToUpdate}...");
                var updateResponse = await client.UpdateMemoAsync(new UpdateMemoRequest
                {
                    MemoId = memoToUpdate,
                    UpdateData = new UpdateMemoData
                    {
                        Title = "Updated: " + memos.Results[0].Title,
                        Metadata = new Dictionary<string, object>
                        {
                            { "status", "reviewed" },
                            { "last_updated_by", "API" }
                        }
                    }
                });
                Console.WriteLine($"Update successful: {updateResponse.Ok}");
            }

            // Example 12: Delete a memo (commented out to avoid deleting data)
        
            // if (memos.Results.Count > 0)
            // {
            //     var memoToDelete = memos.Results[0].Uuid;
            //     Console.WriteLine($"\nDeleting memo {memoToDelete}...");
            //     var deleteResponse = await client.DeleteMemoAsync(new DeleteMemoRequest
            //     {
            //         MemoId = memoToDelete
            //     });
            //     Console.WriteLine($"Delete successful: {deleteResponse.Ok}");
            // }
            

            Console.WriteLine("\nAll examples completed successfully!");
        }
        catch (SkaldException ex)
        {
            Console.WriteLine($"Skald API Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
