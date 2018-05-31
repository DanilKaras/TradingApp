using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using TradingApp.Domain.Interfaces;

namespace TradingApp.Core.BotTools
{
    public class FireScheduler : IFireScheduler
    {
        private readonly ILogger _logger;
        private readonly ISettings _settings;
        private readonly ITelegram _telegram;
        private readonly IForecaster _forecaster;
        public FireScheduler(ILoggerFactory logger, ISettings settings, ITelegram telegram, IForecaster forecaster)
        {
            _logger = logger.CreateLogger("BotTools");
            _settings = settings;
            _telegram = telegram;
            _forecaster = forecaster;
        }
        [AutomaticRetry(Attempts = 5)]    
        public void FireForecaster()
        {
            RecurringJob.AddOrUpdate("test-requrrent-job",
                () => Run(),
                "*/10 * * * *");
        }
        [AutomaticRetry(Attempts = 5)]    
        public void FireForecasterSecond()
        {
            RecurringJob.AddOrUpdate("test-requrrent-job2",
                () => Run(),
                "*/10 * * * *");
        }
        [AutomaticRetry(Attempts = 5)]
        public void StopForecaster()
        {
            RecurringJob.RemoveIfExists("test-requrrent-job");
        }
        
        [AutomaticRetry(Attempts = 5)]
        public void TriggerImmediately()
        {
            //_telegram.SendMessage("Triggering manually").Wait();
            RecurringJob.Trigger("test-requrrent-job");
        }
        
        public async Task Run()
        {
            var trend = new List<int> {0, 1};
            var width = new List<int> {0, 1};
            await _forecaster.MakeBotForecast(50, trend, width);
            await _telegram.SendMessage("Done with hangfire");
        }
        
        public async Task RunSecond()
        {
            var trend = new List<int> {0, 1, 2};
            var width = new List<int> {0, 1};
            await _forecaster.MakeBotForecast(50, trend, width);
            await _telegram.SendMessage("Done with hangfire second");
        }
    }
}