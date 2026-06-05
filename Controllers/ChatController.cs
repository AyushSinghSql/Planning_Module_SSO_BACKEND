using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatClient _chatClient;

        private readonly IConfiguration? _configuration;
        public ChatController(
          ILogger<ChatController> logger,
          IChatClient chatClient,
          IConfiguration configuration
        )
        {
            _logger = logger;
            _chatClient = chatClient;
            _configuration = configuration;
        }

        [HttpPost(Name = "Chat")]
        public async Task<string> Chat([FromBody] string message)
        {
            try
            {
                // MCP Server Endpoint
                var endpoint = new Uri(
                    _configuration["AI:MCPServiceUri"]
                    ?? throw new InvalidOperationException("MCPServiceUri is not configured"));

                // MCP Transport
                var httpTransport = new HttpClientTransport(new HttpClientTransportOptions
                {
                    Endpoint = endpoint
                });

                // MCP Client
                var mcpClient = await McpClient.CreateAsync(httpTransport);

                // Available MCP Tools
                var tools = await mcpClient.ListToolsAsync();

                // Chat Messages
                var messages = new List<ChatMessage>
            {
                new(ChatRole.System, "You are a helpful assistant."),
                new(ChatRole.User, message)
            };

                bool toolExecuted = false;

                StringBuilder responseBuilder = new();

                // Stream response
                await foreach (var update in _chatClient.GetStreamingResponseAsync(
                    messages,
                    new ChatOptions
                    {
                        Tools = [.. tools]
                    }))
                {
                    // Detect actual tool execution
                    if (update.Contents != null &&
                        update.Contents.OfType<FunctionCallContent>().Any())
                    {
                        toolExecuted = true;
                    }

                    // Collect assistant text response
                    if (!string.IsNullOrWhiteSpace(update.Text))
                    {
                        responseBuilder.Append(update.Text);
                    }
                }

                // Return response only if tool was executed
                if (toolExecuted)
                {
                    return responseBuilder.ToString();
                }

                return "This falicility is not available.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        //        [HttpPost(Name = "Chat")]
        //        public async Task<string> Chat([FromBody] string message)
        //        {
        //            // Create MCP client connecting to our MCP server
        //            var endpoint = new Uri(
        //            _configuration["AI:MCPServiceUri"]
        //            ?? throw new InvalidOperationException("MCPServiceUri is not configured"));

        //            var httpTransport = new HttpClientTransport(new HttpClientTransportOptions
        //            {
        //                Endpoint = endpoint
        //            });
        //            var mcpClient = await McpClient.CreateAsync(httpTransport);
        //            // Get available tools from the MCP server
        //            var tools = await mcpClient.ListToolsAsync();

        //            // Set up the chat messages
        //            var messages = new List<ChatMessage> {
        //      new ChatMessage(ChatRole.System, "You are a helpful assistant.")
        //    };
        //            messages.Add(new(ChatRole.User, message));

        //            // Get streaming response and collect updates
        //            List<ChatResponseUpdate> updates = [];
        //            StringBuilder result = new StringBuilder();

        //            await foreach (var update in _chatClient.GetStreamingResponseAsync(
        //              messages,
        //              new() { Tools = [.. tools] }
        //            ))
        //            {
        //                result.Append(update);
        //                updates.Add(update);
        //            }

        //            // Add the assistant's responses to the message history
        //            messages.AddMessages(updates);
        //            return result.ToString();
        //        }
        //    }
    }
}
