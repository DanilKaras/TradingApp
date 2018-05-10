using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using TradingApp.Domain.Interfaces;

namespace TradingApp.Core.Scheduler
{
    public class SendScheduledMessage : HostedService
    {
        private readonly ITelegram _messenger;

        public SendScheduledMessage(ITelegram messenger)
        {
            _messenger = messenger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _messenger.SendMessage("Shduled Message");
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }
}