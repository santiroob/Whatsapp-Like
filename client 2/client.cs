using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.100.33"), 1111);

                clientSocket.Connect(endPoint);
                Console.WriteLine("Conectado al servidor.");

                Thread receiveThread = new Thread(() => ReceiveData(clientSocket));
                receiveThread.Start();

                Thread sendThread = new Thread(() => SendData(clientSocket));
                sendThread.Start();

                receiveThread.Join();
                sendThread.Join();

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ReceiveData(Socket clientSocket)
        {
            try
            {
                byte[] recibir = new byte[100];
                while (true)
                {
                    int array_size = clientSocket.Receive(recibir);
                    if (array_size == 0)
                        break;

                    Array.Resize(ref recibir, array_size);
                    string data = Encoding.Default.GetString(recibir);
                    Console.WriteLine("Datos recibidos: {0}", data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la recepción de datos: {ex.Message}");
            }
        }

        static void SendData(Socket clientSocket)
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Digite el mensaje a enviar:");
                    string message = Console.ReadLine();

                    byte[] dataToSend = Encoding.Default.GetBytes(message);
                    clientSocket.Send(dataToSend);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el envío de datos: {ex.Message}");
            }
        }
    }
}
