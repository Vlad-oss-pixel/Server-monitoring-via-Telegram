using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
                case "/start":
                    SendMessage("Welcome! This bot will help you monitor the server. \n/help - calling help");
                    break;
                case "/help":
                    SendMessage("Bot Commands:\n/restart - Reboot\n/shutdown - Shutdown\n/status - System status (CPU, RAM)");
                    break;
                case "/restart":
                    SendMessage("Restart..");
                    Process.Start("shutdown.exe", "-r -t 0");
                    break;
                case "/shutdown":
                    SendMessage("Turning it off..");
                    Process.Start("shutdown.exe", "-s -t 0");
                    break;
                case "/status":
                    SendMessage("System status");
                    GetSystemInfo();
                    break;
                default:
                    SendMessage("Invalid command");
                    break;
            }
        }
    }

    private void GetSystemInfo()
    {
        Process process = Process.GetCurrentProcess();
        TimeSpan cpuTime = process.TotalProcessorTime;

        // Calculate the CPU usage percentage
        TimeSpan elapsedTime = DateTime.UtcNow - process.StartTime;
        float cpuUsage = (float)cpuTime.TotalMilliseconds / (float)elapsedTime.TotalMilliseconds;
        cpuUsage *= 100;

        // Get the available memory
        float memUsage = (float)GC.GetTotalMemory(false) / (1024 * 1024);

        string message = string.Format("CPU Usage: {0:F}%\nMemory Available: {1:F} MB", cpuUsage, memUsage);
        SendMessage(message);
    }


    private void SendMessage(string message)
    {
        bot.SendTextMessageAsync(chatId, message, ParseMode.Default, false, false);
    }


}

class Program
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_HIDE = 0;
    static void Main(string[] args)
{
    string token = "YOUR_TOKEN";
    string chatId = "YOUR_CHATID";
    IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
    ShowWindow(hWnd, SW_HIDE);
    ServerMonitor serverMonitor = new ServerMonitor(token, chatId);
    serverMonitor.Start();
    while (true)
    {
        Thread.Sleep(1000);
    }
    }
}
