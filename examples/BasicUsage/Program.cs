using System;
using System.Collections.Generic;
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
            // Example 1: Create a memo
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
            Console.WriteLine($"Memo created: {createResponse.Ok}");

            // Example 2: List memos
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

            // Example 3: Get a memo by reference ID
            if (memos.Results.Count > 0)
            {
                var firstMemoUuid = memos.Results[0].Uuid;
                Console.WriteLine($"\nGetting memo {firstMemoUuid}...");
                var memo = await client.GetMemoAsync(firstMemoUuid);
                Console.WriteLine($"Title: {memo.Title}");
                Console.WriteLine($"Summary: {memo.Summary}");
                Console.WriteLine($"Tags: {string.Join(", ", memo.Tags.ConvertAll(t => t.Tag))}");
            }

            // Example 4: Search memos
            Console.WriteLine("\nSearching for memos about 'planning'...");
            var searchResults = await client.SearchAsync(new SearchRequest
            {
                Query = "planning goals",
                SearchMethod = SearchMethod.ChunkVectorSearch,
                Limit = 5
            });
            Console.WriteLine($"Found {searchResults.Results.Count} results:");
            foreach (var result in searchResults.Results)
            {
                Console.WriteLine($"  - {result.Title} (Distance: {result.Distance})");
                Console.WriteLine($"    Snippet: {result.ContentSnippet.Substring(0, Math.Min(100, result.ContentSnippet.Length))}...");
            }

            // Example 5: Search with filters
            Console.WriteLine("\nSearching with filters...");
            var filteredSearch = await client.SearchAsync(new SearchRequest
            {
                Query = "goals",
                SearchMethod = SearchMethod.TitleContains,
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
            var chatResponse = await client.ChatAsync("What are the main goals for Q1?");
            Console.WriteLine($"Answer: {chatResponse.Response}");

            // Example 7: Chat with streaming
            Console.WriteLine("\nAsking a question with streaming...");
            await foreach (var evt in client.StreamedChatAsync("Summarize the key points from planning meetings"))
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

            // Example 8: Generate document
            Console.WriteLine("\nGenerating a document...");
            var doc = await client.GenerateDocAsync(
                prompt: "Create a summary document of Q1 planning discussions",
                rules: "Use formal business language. Include sections for: Executive Summary, Key Goals, Action Items"
            );
            Console.WriteLine("Generated document:");
            Console.WriteLine(doc.Response);

            // Example 9: Generate document with streaming
            Console.WriteLine("\nGenerating a document with streaming...");
            await foreach (var evt in client.StreamedGenerateDocAsync(
                prompt: "Create a brief outline of the Q1 goals",
                rules: "Use bullet points"))
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

            // Example 10: Update a memo
            if (memos.Results.Count > 0)
            {
                var memoToUpdate = memos.Results[0].Uuid;
                Console.WriteLine($"\nUpdating memo {memoToUpdate}...");
                var updateResponse = await client.UpdateMemoAsync(memoToUpdate, new UpdateMemoData
                {
                    Title = "Updated: " + memos.Results[0].Title,
                    Metadata = new Dictionary<string, object>
                    {
                        { "status", "reviewed" },
                        { "last_updated_by", "API" }
                    }
                });
                Console.WriteLine($"Update successful: {updateResponse.Ok}");
            }

            // Example 11: Delete a memo (commented out to avoid deleting data)
            /*
            if (memos.Results.Count > 0)
            {
                var memoToDelete = memos.Results[0].Uuid;
                Console.WriteLine($"\nDeleting memo {memoToDelete}...");
                await client.DeleteMemoAsync(memoToDelete);
                Console.WriteLine("Delete successful");
            }
            */

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
