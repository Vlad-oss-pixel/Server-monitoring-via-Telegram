using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
public class ServerMonitor
{
    private TelegramBotClient bot;
    private string chatId;

    public ServerMonitor(string token, string chatId)
    {
        this.chatId = chatId;
        bot = new TelegramBotClient(token);
        bot.OnMessage += Bot_OnMessage;
        bot.StartReceiving();
    }

    public void Start()
    {
        Thread t = new Thread(new ThreadStart(Monitor));
        t.Start();
    }

    private void Monitor()
    {
        while (true)
        {
            Process[] processes = Process.GetProcesses();
            bool serverRunning = false;

            foreach (Process p in processes)
            {
                if (p.ProcessName.ToLower().Contains("server"))
                {
                    serverRunning = true;
                    break;
                }
            }

            if (!serverRunning)
            {
                SendMessage("Server is not running");
            }

            Thread.Sleep(30000);
        }
    }

    private void Bot_OnMessage(object sender, MessageEventArgs e)
    {
        if (e.Message.Chat.Id.ToString() == chatId)
        {
            string command = e.Message.Text;

            switch (command.ToLower())
            {
                case "/restart":
                    Process.Start("shutdown.exe", "-r -t 0");
                    break;
                case "/shutdown":
                    Process.Start("shutdown.exe", "-s -t 0");
                    break;
                default:
                    SendMessage("Invalid command");
                    break;
            }
        }
    }

    private void SendMessage(string message)
    {
        bot.SendTextMessageAsync(chatId, message, ParseMode.Default, false, false);
    }
    static void Main(string[] args)
    {
        string token = "TOKEN";
        string chatId = "CHAT_ID";
        ServerMonitor serverMonitor = new ServerMonitor(token, chatId);
        serverMonitor.Start();

        Console.ReadLine();
    } 
}

