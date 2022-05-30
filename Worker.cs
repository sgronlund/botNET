using SlackBotMessages;
using SlackBotMessages.Models;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;
namespace slackBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string _hook;
        private readonly HttpClient _httpClient;
        private readonly Message startMsg;
        private readonly string url;
        public Worker(ILogger<Worker> logger, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _logger = logger;
            _hook = config["slackHook"];
            _httpClient = new HttpClient();
            url = config.GetValue<string>("URI:Aland");
            startMsg = new Message
            {
                Username = "Dagens Lunch",
                Text = "Starting up ...",
                IconEmoji = Emoji.Hamburger
            };
        }

        private async Task<string> ParseHTMLBody()
        {
            _logger.LogInformation(url);
            try
            {
                HttpResponseMessage responseMessage = await _httpClient.GetAsync(url);
                responseMessage.EnsureSuccessStatusCode();
                _logger.LogInformation("Response return 200");
                var html = await responseMessage.Content.ReadAsStringAsync();
                var parser = new HtmlParser();
                var doc = await parser.ParseDocumentAsync(html);
                var namesOfRestaurants = doc.QuerySelectorAll(".restaurant-name"); //This fetches the titles of all restaurants
                var infoOfRestaurants = doc.QuerySelectorAll(".dishes-wrapper"); //This all of the dishes for the specific restaurant
                if (namesOfRestaurants.Count() == 0 || infoOfRestaurants.Count() == 0)
                {
                    _logger.LogError("QuerySelector failed, site has probably been updated");
                    return "";
                }
                else if (namesOfRestaurants.Count() != infoOfRestaurants.Count())
                {
                    _logger.LogError("Number of restaurants not matching divs with dishes, querySelector might be incorrect");
                    return "";
                }
                else
                {
                    for (int i = 0; i < namesOfRestaurants.Count(); ++i)
                    {
                        var name = namesOfRestaurants[i].TextContent; //This should be the title of the restuarant
                        var test = infoOfRestaurants[i].TextContent.Trim();
                        //price regex ([0-9.,€]+)
                        //dish info regex ([a-öA-ö (),-]+)
                        // could possibly be done more efficiently
                    }
                    return "success";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Response wasn't successful, exception message: {msg}", ex.Message);
                return "";
            }
        }

        private Message CreateInfoMessage(string parsedBody)
        {
            if (parsedBody.Length == 0)
            {
                throw new Exception();
            }
            var msg = new Message
            {
                Text = "parsedBody"
            };
            return msg;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var client = new SbmClient(_hook);
            var msg = await client.SendAsync(startMsg);
            _logger.LogInformation("Sent startup message");
            if (msg.Contains("Error when"))
            {
                var exceptionMsg = msg.Split(": ")[1];
                _logger.LogError("Error during startup, exception occured with message: {exMsg}", exceptionMsg);
            }
            else
            {
                var successMsg = new Message
                {
                    Text = "Succesfully started!"
                };
                await client.SendAsync(successMsg);
                _logger.LogInformation("Client now running since {stamp}", DateTimeOffset.Now);
                Task<string> taskBody = ParseHTMLBody();
                string body = await taskBody;
                var infoMsg = CreateInfoMessage(body);
                await client.SendAsync(infoMsg);
            }
        }
    }
}