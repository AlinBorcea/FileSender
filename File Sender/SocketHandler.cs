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
    }

    public bool RunServer()
    {
        try
        {
            Socket socket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(iPEndPoint);
            socket.Listen(1);

            Socket handler = socket.Accept();
            Console.WriteLine("Connected to {0}", handler.RemoteEndPoint.ToString());

            byte[] countBuffer = new byte[4];
            int fileCount;

            handler.Receive(countBuffer);
            fileCount = BitConverter.ToInt32(countBuffer);

            byte[] fileNameBuffer = new byte[32];
            for (int i = 0; i < fileCount; i++)
            {
                handler.Receive(fileNameBuffer);
                //string path = FileManager.GetFileName(Encoding.ASCII.GetString(fileNameBuffer));
                //File.Create(path);
            }

            handler.Close();
            socket.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return false;
        }
        return true;
    }

    public bool RunClient()
    {
        Socket socket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            socket.Connect(iPEndPoint);
            Console.WriteLine("Connected to {0}", socket.RemoteEndPoint.ToString());

            try
            {
                string[] fileNames = Directory.GetFiles(FileManager.Parrot);
                byte[] countBuffer = BitConverter.GetBytes(fileNames.Length);

                socket.Send(countBuffer);
                foreach (string name in fileNames)
                {
                    socket.Send(Encoding.ASCII.GetBytes(name));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return false;
        }

        socket.Close();
        return true;
    }

}