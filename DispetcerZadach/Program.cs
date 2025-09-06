using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Запуск диспетчера задач...");
        Console.WriteLine("1 - Запустить сервер");
        Console.WriteLine("2 - Запустить клиент");
        Console.WriteLine("3 - Запустить оба (в разных окнах)");
        Console.Write("Выберите вариант: ");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                StartServer();
                break;
            case "2":
                StartClient();
                break;
            case "3":
                StartBothInSeparateWindows();
                break;
            default:
                Console.WriteLine("Неверный выбор");
                break;
        }
    }

    static void StartServer()
    {
        Console.WriteLine("Запуск сервера...");
        ProcessServer server = new ProcessServer();
        server.Start();
    }

    static void StartClient()
    {
        Console.WriteLine("Запуск клиента...");
        ProcessClient client = new ProcessClient();
        client.Run();
    }

    static void StartBothInSeparateWindows()
    {
        // Запускаем сервер в новом окне консоли
        ProcessStartInfo serverInfo = new ProcessStartInfo
        {
            FileName = Process.GetCurrentProcess().MainModule.FileName,
            Arguments = "server",
            UseShellExecute = true,
            CreateNoWindow = false
        };
        Process.Start(serverInfo);

        // Небольшая задержка перед запуском клиента
        System.Threading.Thread.Sleep(1000);

        // Запускаем клиент в новом окне консоли
        ProcessStartInfo clientInfo = new ProcessStartInfo
        {
            FileName = Process.GetCurrentProcess().MainModule.FileName,
            Arguments = "client",
            UseShellExecute = true,
            CreateNoWindow = false
        };
        Process.Start(clientInfo);
    }
}