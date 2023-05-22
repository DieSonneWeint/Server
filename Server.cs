using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalServer
{
    internal class Server
    {
        TcpClient tcpClient;
        TcpListener listener;
        public void ServerStart()
        {
            listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();
            Console.WriteLine("Сервер запущен");
            CreatePto();
        }
        public void ServerStop()
        {
            Console.WriteLine("Сервер отключен");
            listener.Stop();
        }
        static void HandleClient(TcpClient clientSocket)
        {
            Console.WriteLine("Клиент подключен: {0}", clientSocket.Client.RemoteEndPoint);

            // Отправляем список логических устройств клиенту
            string[] devices = { "C:\\", "D:\\" };
            SendData(clientSocket, string.Join(",", devices));

            while (true)
            {
                // Принимаем данные от клиента
                string data = ReceiveData(clientSocket);

                if (data == "disconnect")
                {
                    // Клиент запрашивает отключение
                    break;
                }
                else if (Directory.Exists(data))
                {
                    // Клиент запрашивает список файлов и подкаталогов в указанном каталоге
                    string[] files = Directory.GetFiles(data);
                    string[] subdirs = Directory.GetDirectories(data);
                    SendData(clientSocket, string.Join(",", files) + "," + string.Join(",", subdirs));
                }
                else if (File.Exists(data))
                {
                    // Клиент запрашивает содержимое указанного файла
                    string content = File.ReadAllText(data);
                    SendData(clientSocket, content);
                }
                else
                {
                    // Клиент отправил некорректные данные
                    SendData(clientSocket, "error");
                }
            }

            Console.WriteLine("Клиент отключен: {0}", clientSocket.Client.RemoteEndPoint);
            clientSocket.Close();
        }
        static void SendData(TcpClient clientSocket, string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data + "\n");
            NetworkStream stream = clientSocket.GetStream();
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }

        static string ReceiveData(TcpClient clientSocket)
        {
            byte[] buffer = new byte[1024];
            NetworkStream stream = clientSocket.GetStream();
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
        }
        public void CreatePto()
        {
            while (true)
            {
                Console.WriteLine("Ожидание соединения с клиентом");
                // Ожидаем соединение с клиентом
                TcpClient clientSocket = listener.AcceptTcpClient();

                // Создаем отдельный поток для обработки клиента
                Thread clientThread = new Thread(() => HandleClient(clientSocket));
                clientThread.Start();
            }
        }      
    }
}
