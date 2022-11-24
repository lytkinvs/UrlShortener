using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace UrlShortenerApi.Services
{
    public class TelegramBotService
    {
        private readonly UrlDatabase _database;
        private readonly TelegramBotClient _bot;
        private readonly QrCodeService _qrCodeService;
        private readonly IHostingEnvironment _environment;

        private const string TelegramId = "5095737015:AAE9UcfTZwewjVMISb6HKwXSXXRgV-LRRnL0U";
        private const string ServiceUrl = "http://91.105.198.215:8080/api/url/";

        public TelegramBotService(
            IHostingEnvironment environment,
            UrlDatabase database,
            QrCodeService qrCodeService
        )
        {
            _environment = environment;
            _database = database;
            _qrCodeService = qrCodeService;
            _bot = new TelegramBotClient(TelegramId);
            _bot.StartReceiving(UpdateHandler, ErrorHandler, new ReceiverOptions());
        }

        private async Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            await Task.FromResult(0);
        }

        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken arg3)
        {
            var message = update.Message ?? update.EditedMessage;
            if (message == null)
            {
                await Task.FromResult(0);
                return;
            }

            var strMessage = message.Text ?? string.Empty;
            var id = message.Chat.Id;

            var help = "Данный бот позволяет преобразовать ссылку URL \n" +
                       "в URL заданной длины и получить QR код. (url shortener) \n" +
                       "Использование: /url {адрес} \n" +
                       "Например: /url https://google.com";

            if (strMessage.Contains("/help"))
            {
                await client.SendTextMessageAsync(id, help);
                return;
            }

            if (strMessage.Contains("/url"))
            {
                var uri = strMessage.Replace("/url", string.Empty).Trim().ToLower();
                Uri uriResult;

                var result = Uri.TryCreate(uri, UriKind.Absolute, out uriResult)
                             && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (!result)
                {
                    await client.SendTextMessageAsync(id, "Ошибка - введите корректный URL ");
                    return;
                }

                using var sha1 = SHA1.Create();
                var hash = Convert.ToHexString(sha1.ComputeHash(Encoding.UTF8.GetBytes(uri)));


                var path = Path.Join(_environment.ContentRootPath, "images");
                var image = _qrCodeService.GenerateQRCore(path, ServiceUrl, hash);


                await client.SendPhotoAsync(id, new InputOnlineFile(File.OpenRead(image)));
                await client.SendTextMessageAsync(id, ServiceUrl + hash);

                if (_database.Map.ContainsKey(hash))
                {
                    return;
                }

                _database.Map.Add(hash, uri);

                File.Delete(image);
            }


            await client.SendTextMessageAsync(id, "/help - инструкция ");
        }
    }
}