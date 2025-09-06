using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ProcessServer
{
    private TcpListener _listener;
    private const int Port = 8888;
    private bool _isRunning = true;

    public void Start()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            Console.Title = "Диспетчер задач - Сервер";
            Console.WriteLine($"Сервер запущен на порту {Port}. Ожидание подключений...");
            Console.WriteLine("Для остановки сервера нажмите 'Q'");

            // Запускаем поток для обработки команд остановки
            Thread stopThread = new Thread(CheckForStopCommand);
            stopThread.IsBackground = true;
            stopThread.Start();

            // Основной цикл обработки подключений
            while (_isRunning)
            {
                if (_listener.Pending())
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                Thread.Sleep(100);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сервера: {ex.Message}");
        }
        finally
        {
            _listener?.Stop();
            Console.WriteLine("Сервер остановлен");
        }
    }

    private void CheckForStopCommand()
    {
        while (_isRunning)
        {
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
            {
                _isRunning = false;
                Console.WriteLine("\nОстановка сервера...");
            }
            Thread.Sleep(100);
        }
    }

    private void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;

        try
        {
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"Получен запрос: {request}");
                string response = ProcessRequest(request);

                byte[] data = Encoding.UTF8.GetBytes(response);
                stream.Write(data, 0, data.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки клиента: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    private string ProcessRequest(string request)
    {
        string[] parts = request.Split('|');
        string command = parts[0];

        try
        {
            switch (command)
            {
                case "GET_PROCESSES":
                    return GetProcessesList();

                case "KILL_PROCESS":
                    if (parts.Length > 1 && int.TryParse(parts[1], out int pid))
                        return KillProcess(pid) ? "SUCCESS|Процесс завершен" : "ERROR|Не удалось завершить процесс";
                    return "ERROR|Неверный ID процесса";

                case "START_PROCESS":
                    if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                        return StartProcess(parts[1]) ? "SUCCESS|Процесс запущен" : "ERROR|Не удалось запустить процесс";
                    return "ERROR|Не указан путь к файлу";

                default:
                    return "ERROR|Неизвестная команда";
            }
        }
        catch (Exception ex)
        {
            return $"ERROR|{ex.Message}";
        }
    }

    private string GetProcessesList()
    {
        StringBuilder result = new StringBuilder();
        Process[] processes = Process.GetProcesses();

        foreach (Process process in processes)
        {
            try
            {
                // Пытаемся получить информацию о процессе
                result.AppendLine($"{process.ProcessName}|{process.Id}|{process.MainModule?.FileName}");
            }
            catch
            {
                // Некоторые системные процессы могут быть недоступны
                result.AppendLine($"{process.ProcessName}|{process.Id}|N/A");
            }
        }

        return result.ToString();
    }

    private bool KillProcess(int pid)
    {
        try
        {
            Process process = Process.GetProcessById(pid);
            process.Kill();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool StartProcess(string path)
    {
        try
        {
            Process.Start(path);
            return true;
        }
        catch
        {
            return false;
        }
    }
}