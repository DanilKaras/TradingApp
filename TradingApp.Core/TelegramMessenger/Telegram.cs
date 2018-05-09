using System;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using TradingApp.Domain.Interfaces;
using TradingApp.Domain.Telegram;

namespace TradingApp.Core.TelegramMessenger
{
    public class Telegram : ITelegram
    {
        private readonly ISettings _settings;
        private string _urlString = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}";
        private string _channelId = "https://api.telegram.org/bot{0}/getUpdates";
        private readonly string _apiToken;
        private readonly string _chatId;

        public Telegram(ISettings settings)
        {
            _settings = settings;
            _apiToken = _settings.TelegramApi;
            _chatId = _settings.TelegramChatId;
        }

        public long GetChatId()
        {
            try
            {
                _channelId = string.Format(_channelId, _apiToken);
                var client = new RestClient(_channelId);
                var request = new RestRequest(Method.GET);
                request.AddHeader("cache-control", "no-cache");
                var response = client.Execute(request);
                var model = JsonConvert.DeserializeObject<TelegramResponse>(response.Content);
                var chatId = model.Result.First().ChannelPost.Chat.Id;
                return chatId;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                _urlString = string.Format(_urlString, _apiToken, _chatId, message);
                var client = new RestClient(_urlString);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                client.Execute(request);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        } 
    }
}