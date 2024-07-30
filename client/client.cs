using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1 || !int.TryParse(args[0], out int listenPort))
            {
                Console.WriteLine("Uso: dotnet run <puerto>");
                return;
            }

            try
            {
                // Configurar el socket para escuchar
                Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Any, listenPort);

                listenSocket.Bind(listenEndPoint);
                listenSocket.Listen(10);

                Console.WriteLine($"Escuchando en el puerto {listenPort}...");

                // Iniciar el hilo para escuchar conexiones entrantes
                Thread listenThread = new Thread(() => ListenForClients(listenSocket));
                listenThread.Start();

                // Manejar la entrada del usuario para enviar mensajes
                while (true)
                {
                    Console.WriteLine("Digite el puerto de destino seguido del mensaje (formato: <puerto> <mensaje>):");
                    string input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                        continue;

                    string[] splitInput = input.Split(' ', 2);
                    if (splitInput.Length != 2 || !int.TryParse(splitInput[0], out int destinationPort))
                    {
                        Console.WriteLine("Formato incorrecto. Uso: <puerto> <mensaje>");
                        continue;
                    }

                    string message = splitInput[1];
                    SendMessage(destinationPort, message, listenPort);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ListenForClients(Socket listenSocket)
        {
            while (true)
            {
                try
                {
                    Socket clientSocket = listenSocket.Accept();
                    IPEndPoint remoteEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
                    if (remoteEndPoint != null)
                    {
                        int clientPort = remoteEndPoint.Port;
                        

                        Thread clientThread = new Thread(() => HandleClient(clientSocket, clientPort));
                        clientThread.Start();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en la aceptación de conexiones: {ex.Message}");
                }
            }
        }

        static void HandleClient(Socket clientSocket, int clientPort)
        {
            try
            {
                Thread receiveThread = new Thread(() => ReceiveData(clientSocket, clientPort));
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

        static void ReceiveData(Socket clientSocket, int clientPort)
        {
            try
            {
                byte[] buffer = new byte[1024];
                while (true)
                {
                    int receivedBytes = clientSocket.Receive(buffer);
                    if (receivedBytes == 0)
                        break;

                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    Console.WriteLine("{0}", receivedMessage);
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

                    byte[] dataToSend = Encoding.UTF8.GetBytes(message);
                    clientSocket.Send(dataToSend);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el envío de datos: {ex.Message}");
            }
        }

        static void SendMessage(int port, string message, int listenPort)
        {
            try
            {
                Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Loopback, port);

                sendSocket.Connect(sendEndPoint);

                byte[] data = Encoding.UTF8.GetBytes($"Mensaje desde el puerto {listenPort}: {message}");
                sendSocket.Send(data);

                sendSocket.Shutdown(SocketShutdown.Both);
                sendSocket.Close();

                Console.WriteLine("Mensaje enviado al puerto {0}: {1}", port, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el envío de datos: {ex.Message}");
            }
        }
    }
}
