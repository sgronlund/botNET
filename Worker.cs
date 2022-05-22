using SlackBotMessages;
using SlackBotMessages.Models;
using System.Net.Http;
namespace slackBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string _hook;
        private readonly HttpClient _httpClient;
        private readonly Message startMsg;
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _hook = config["slackHook"];
            _httpClient = new HttpClient();
            startMsg = new Message
            {
                Username = "Dagens Lunch",
                Text = "Starting up ...",
                IconEmoji = Emoji.Hamburger
            };
        }

        private async Task ParseHTMLBody()
        {
            HttpResponseMessage responseMessage = await _httpClient.GetAsync("https://www.aland.com/lunch");
            try
            {
                responseMessage.EnsureSuccessStatusCode();
                var html = await responseMessage.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Response wasn't successful, exception message: {msg}", ex.Message);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var client = new SbmClient(_hook);
            var msg = await client.SendAsync(startMsg);
            _logger.LogInformation("Sent startup message");
            if(msg.Contains("Error when"))
            {
                var exceptionMsg = msg.Split(": ")[1];
                _logger.LogError("Error during startup, exception occured with message: {exMsg}",exceptionMsg);
            } else {
                var successMsg = new Message
                {
                    Text = "Succesfully started!"
                };
                await client.SendAsync(successMsg);
                _logger.LogInformation("Client now running since {stamp}", DateTimeOffset.Now);
                var body = ParseHTMLBody();
            }
        }
    }
}