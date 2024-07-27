using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Crear un socket para escuchar conexiones entrantes
                Socket listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.100.33"), 1111);

                // Enlazar el socket al punto final y comenzar a escuchar
                listen.Bind(endPoint);
                listen.Listen(10);

                Console.WriteLine("Esperando conexiones...");

                while (true)
                {
                    // Aceptar una conexión entrante
                    Socket conexion = listen.Accept();
                    Console.WriteLine("Conexión aceptada desde {0}", conexion.RemoteEndPoint);

                    // Crear un nuevo hilo para manejar la conexión
                    Thread clientThread = new Thread(() => HandleClient(conexion));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void HandleClient(Socket clientSocket)
        {
            try
            {
                Thread receiveThread = new Thread(() => ReceiveData(clientSocket));
                receiveThread.Start();

                Thread sendThread = new Thread(() => SendData(clientSocket));
                sendThread.Start();

                receiveThread.Join();
                sendThread.Join();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la conexión con el cliente: {ex.Message}");
            }
            finally
            {
                // Cerrar el socket del cliente
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
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
