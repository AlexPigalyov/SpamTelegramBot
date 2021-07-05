using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Telegram.Bot;

namespace SpamTelegramBot
{
    class Program
    {
        private static TelegramBotClient bot;
        private static string dataFilePath = Path.Combine(Environment.CurrentDirectory, "Data.txt");
        private static string chatsFilePath = Path.Combine(Environment.CurrentDirectory, "Chats.txt");
        static void Main(string[] args)
        {
            if (!File.Exists(chatsFilePath))
            {
                File.Create(chatsFilePath);
            }

            var dataLines = File.ReadAllLines(dataFilePath);

            bot = new TelegramBotClient(dataLines[0].Trim());
            Console.WriteLine("Telegram Bot Initialized");

            bot.StartReceiving();
            bot.OnMessage += Bot_OnMessage;

            Task.Run(async() =>
            {
                string messageText = string.Empty;

                for(int i = 0; i < dataLines.Length; i++)
                {
                    if (i == 0) continue;

                    messageText += dataLines[i] + "\n";
                }

                while (true)
                {
                    var chats = File.ReadAllLines(chatsFilePath);

                    if(chats != null)
                    {
                        foreach(var chat in chats)
                        {
                            await bot.SendTextMessageAsync(long.Parse(chat), messageText);
                        }
                    }

                    await Task.Delay(1000 * 60 * 60);
                }
            });
            Console.ReadLine();
        }


        private static void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                //if message text contains bots nickname then
                if (e.Message.Text.Trim().Contains(bot.GetMeAsync().Result.FirstName.Trim()))
                {
                    var chats = File.ReadAllLines(chatsFilePath);
                    //if this chat not contains in list of already used chats then
                    if (!chats.Any(x => long.Parse(x) == e.Message.Chat.Id))
                    {
                        var chatsList = chats.ToList();
                        chatsList.Add(e.Message.Chat.Id.ToString());

                        File.WriteAllLines(chatsFilePath, chatsList);
                    }
                }
            }
        }
    }
}
