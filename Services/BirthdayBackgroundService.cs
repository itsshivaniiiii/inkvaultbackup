namespace InkVault.Services
{
    public class BirthdayBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BirthdayBackgroundService> _logger;
        private Timer? _timer;

        public BirthdayBackgroundService(IServiceProvider serviceProvider, ILogger<BirthdayBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Birthday Background Service started");

            // Calculate time until next midnight
            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1);
            var timeUntilNextMidnight = nextMidnight - now;

            // First run at midnight
            _timer = new Timer(async _ => await CheckBirthdaysAsync(), null, timeUntilNextMidnight, TimeSpan.FromHours(24));

            await Task.CompletedTask;
        }

        private async Task CheckBirthdaysAsync()
        {
            try
            {
                _logger.LogInformation("Checking for birthdays...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var birthdayService = scope.ServiceProvider.GetRequiredService<IBirthdayService>();
                    await birthdayService.SendBirthdayEmailsAsync();
                }

                _logger.LogInformation("Birthday check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in birthday background service: {ex.Message}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Birthday Background Service is stopping");
            _timer?.Dispose();
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
