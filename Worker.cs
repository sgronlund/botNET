using SlackAPI;
namespace slackBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string _token;
        private readonly string _userName;
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _token = config["slackToken"];
            _userName = config["slackUser"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var slackClient = new SlackTaskClient(_token);
            var resp = await slackClient.PostMessageAsync("#general", "Starting up....");
            if (resp == null)
            {
                _logger.LogWarning("Startup message failed, failed to connect to workspace");
            }
            else
            {
                _logger.LogInformation("Bot succesfully started at {stamp}!", DateTimeOffset.Now);
                var user = await slackClient.GetUserListAsync();
                if (user != null)
                {
                    var members = user.members;
                    var me = members.FirstOrDefault(m => m.name == _userName);
                    if(me == null)
                    {
                        _logger.LogError("Couldn't find user: {0}", _userName);
                    } else
                    {
                        var userDM = await slackClient.JoinDirectMessageChannelAsync(me.real_name);
                        if(userDM == null)
                        {
                            _logger.LogError("Couldn't join DM");
                        } else
                        {
                            var userChannel = userDM.channel;
                            await slackClient.PostMessageAsync(userChannel.id, "Hello from the DMs!");
                        }

                    }
                }
            }
            
        }
    }
}