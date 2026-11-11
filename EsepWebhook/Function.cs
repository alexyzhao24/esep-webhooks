using Amazon.Lambda.Core;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogLine($"Received: {input}");
        var json = JObject.Parse(input.ToString() ?? "{}");
        var issueUrl = (string?)json["issue"]?["html_url"];

        if (string.IsNullOrEmpty(issueUrl))
        {
            return "No issue URL found";
        }

        string payload = JsonConvert.SerializeObject(
            new { text = $"Issue Created: {issueUrl}" }
        );

        using var client = new HttpClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            Environment.GetEnvironmentVariable("SLACK_URL"))
        {
            Content = new StringContent(
                payload,
                Encoding.UTF8,
                "application/json")
        };

        using var response = client.Send(request);
        using var reader = new StreamReader(
            response.Content.ReadAsStream());
        return reader.ReadToEnd();
    }
}
