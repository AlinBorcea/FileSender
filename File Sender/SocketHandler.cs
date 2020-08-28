using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketHandler
{
    private UserInput user;
    private IPHostEntry host;
    private IPAddress iPAddress;
    private IPEndPoint iPEndPoint;

    public bool WasSuccessful { get; set; }
    public SocketHandler(UserInput userInput)
    {
        user = userInput;
        try
        {
            host = Dns.GetHostEntry(user.IP);
            iPAddress = host.AddressList[0];
            iPEndPoint = new IPEndPoint(iPAddress, int.Parse(user.Port));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Environment.Exit(1);
        }
        WasSuccessful = false;
    }

    public void RunServer()
    {
        try
        {
            Socket socket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(iPEndPoint);
            socket.Listen(1);

            Socket handler = socket.Accept();
            Console.WriteLine("Connected to {0}", handler.RemoteEndPoint.ToString());

            FileStream stream;
            byte[] countBuffer = new byte[4];
            byte[] nameBuffer;
            byte[] contentBuffer;
            int fileCount, recvBytes;

            handler.Receive(countBuffer);
            fileCount = BitConverter.ToInt32(countBuffer);

            for (int i = 0; i < fileCount; i++)
            {
                handler.Receive(countBuffer);
                recvBytes = BitConverter.ToInt32(countBuffer);
                nameBuffer = new byte[recvBytes];
                handler.Receive(nameBuffer);

                stream = File.Create($"{FileManager.Turtle}/{Encoding.UTF8.GetString(nameBuffer)}");

                handler.Receive(countBuffer);
                recvBytes = BitConverter.ToInt32(countBuffer);
                contentBuffer = new byte[recvBytes];
                handler.Receive(contentBuffer);

                stream.Write(contentBuffer);
                stream.Close();
            }

            handler.Close();
            socket.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            WasSuccessful = false;
        }
        WasSuccessful = true;
    }

    public void RunClient()
    {
        Socket socket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            socket.Connect(iPEndPoint);
            Console.WriteLine("Connected to {0}", socket.RemoteEndPoint.ToString());

            string[] fileNames = Directory.GetFiles(FileManager.Parrot);
            byte[] countBuffer = BitConverter.GetBytes(fileNames.Length);
            byte[] nameBuffer;
            byte[] content;
            socket.Send(countBuffer);

            foreach (string name in fileNames)
            {
                nameBuffer = Encoding.UTF8.GetBytes(Path.GetFileName(name));
                countBuffer = BitConverter.GetBytes(nameBuffer.Length);
                
                socket.Send(countBuffer);
                socket.Send(nameBuffer);

                content = File.ReadAllBytes(name);
                countBuffer = BitConverter.GetBytes(content.Length);
                
                socket.Send(countBuffer);
                socket.Send(content);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            WasSuccessful = false;
        }

        socket.Close();
        WasSuccessful = true;
    }

}