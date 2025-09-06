using System;
using System.Net.Sockets;
using System.Text;

public class ProcessClient
{
    private const string Host = "localhost";
    private const int Port = 8888;

    public void Run()
    {
        Console.Title = "Диспетчер задач - Клиент";

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== ДИСПЕТЧЕР ЗАДАЧ ===");
            Console.WriteLine("1. Показать список процессов");
            Console.WriteLine("2. Завершить процесс");
            Console.WriteLine("3. Запустить процесс");
            Console.WriteLine("4. Выход");
            Console.Write("Выберите действие: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    DisplayProcesses();
                    break;
                case "2":
                    KillProcess();
                    break;
                case "3":
                    StartProcess();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private string SendRequest(string request)
    {
        try
        {
            using (TcpClient client = new TcpClient(Host, Port))
            using (NetworkStream stream = client.GetStream())
            {
                byte[] data = Encoding.UTF8.GetBytes(request);
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
        }
        catch (Exception ex)
        {
            return $"ERROR|{ex.Message}";
        }
    }

    private void DisplayProcesses()
    {
        Console.Clear();
        Console.WriteLine("Загрузка списка процессов...");

        string response = SendRequest("GET_PROCESSES");

        if (response.StartsWith("ERROR"))
        {
            Console.WriteLine("Ошибка получения списка процессов: " + response.Substring(6));
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
            return;
        }

        Console.Clear();
        Console.WriteLine("Список процессов:");
        Console.WriteLine("№   Имя процесса".PadRight(30) + "ID".PadRight(10) + "Путь");
        Console.WriteLine(new string('-', 80));

        string[] processes = response.Split('\n');
        for (int i = 0; i < processes.Length; i++)
        {
            if (string.IsNullOrEmpty(processes[i])) continue;

            string[] parts = processes[i].Split('|');
            if (parts.Length >= 3)
            {
                Console.WriteLine($"{i + 1,-4} {parts[0].PadRight(30)} {parts[1].PadRight(10)} {parts[2]}");
            }
        }

        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    private void KillProcess()
    {
        Console.Clear();
        Console.WriteLine("Загрузка списка процессов...");

        string response = SendRequest("GET_PROCESSES");

        if (response.StartsWith("ERROR"))
        {
            Console.WriteLine("Ошибка получения списка процессов: " + response.Substring(6));
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
            return;
        }

        Console.Clear();
        Console.WriteLine("Список процессов:");
        Console.WriteLine("№   Имя процесса".PadRight(30) + "ID");
        Console.WriteLine(new string('-', 40));

        string[] processes = response.Split('\n');
        for (int i = 0; i < processes.Length; i++)
        {
            if (string.IsNullOrEmpty(processes[i])) continue;

            string[] parts = processes[i].Split('|');
            if (parts.Length >= 2)
            {
                Console.WriteLine($"{i + 1,-4} {parts[0].PadRight(30)} {parts[1]}");
            }
        }

        Console.Write("\nВведите номер процесса для завершения (0 для отмены): ");
        if (int.TryParse(Console.ReadLine(), out int processNumber) && processNumber > 0)
        {
            if (processNumber <= processes.Length)
            {
                string[] parts = processes[processNumber - 1].Split('|');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int pid))
                {
                    string killResponse = SendRequest($"KILL_PROCESS|{pid}");
                    Console.WriteLine(killResponse.Contains("SUCCESS") ?
                        "Процесс завершен успешно" :
                        "Ошибка завершения процесса: " + killResponse.Substring(6));
                }
                else
                {
                    Console.WriteLine("Неверный формат данных процесса");
                }
            }
            else
            {
                Console.WriteLine("Неверный номер процесса");
            }
        }
        else
        {
            Console.WriteLine("Отмена операции");
        }

        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    private void StartProcess()
    {
        Console.Clear();
        Console.Write("Введите путь к исполняемому файлу: ");
        string path = Console.ReadLine();

        if (!string.IsNullOrEmpty(path))
        {
            string response = SendRequest($"START_PROCESS|{path}");
            Console.WriteLine(response.Contains("SUCCESS") ?
                "Процесс запущен успешно" :
                "Ошибка запуска процесса: " + response.Substring(6));
        }
        else
        {
            Console.WriteLine("Путь не может быть пустым");
        }

        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }
}