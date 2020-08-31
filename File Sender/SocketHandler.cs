using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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

            Console.Write("One Time Password... ");
            string password = Console.ReadLine();

            FileStream fs;
            byte[] countBuffer = new byte[4];
            byte[] salt = new byte[16];
            byte[] nameBuffer;
            byte[] contentBuffer = new byte[1024];
            int recv;

            handler.Receive(salt);
            handler.Receive(countBuffer);

            Rfc2898DeriveBytes pass = new Rfc2898DeriveBytes(password, salt);
            byte[] key = pass.GetBytes(16);

            FileManager.ClearDir(FileManager.Turtle);
            for (int i = 0; i < BitConverter.ToInt32(countBuffer); i++)
            {
                handler.Receive(countBuffer);
                nameBuffer = new byte[BitConverter.ToInt32(countBuffer)];
                handler.Receive(nameBuffer);
                string str2 = Encoding.UTF8.GetString(nameBuffer);
                fs = File.Create($"{FileManager.Turtle}/{str2}");

                while ((recv = handler.Receive(contentBuffer)) > 0)
                    fs.Write(contentBuffer, 0, recv);

                fs.Close();
                string str = Encoding.UTF8.GetString(nameBuffer);
                FileManager.DecryptFile(user.DirPath, str, key, salt);
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

            // Encryption
            Console.Write("One Time Password... ");
            string password = Console.ReadLine();
            byte[] salt = new byte[16];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            Rfc2898DeriveBytes pass = new Rfc2898DeriveBytes(password, salt);
            byte[] key = pass.GetBytes(16);

            // File properties
            FileStream fs;
            string[] fileNames = Directory.GetFiles(user.DirPath);
            byte[] countBuffer = BitConverter.GetBytes(fileNames.Length);
            byte[] nameBuffer;
            byte[] contentBuffer = new byte[1024];
            int recv;

            socket.Send(salt);
            socket.Send(countBuffer);

            FileManager.ClearDir(FileManager.Parrot);
            foreach (string name in fileNames)
            {
                FileManager.EncryptFile(name, key, salt);
                fs = File.OpenRead($"{FileManager.Parrot}/{Path.GetFileName(name)}");
                nameBuffer = Encoding.UTF8.GetBytes(Path.GetFileName(name));
                socket.Send(BitConverter.GetBytes(nameBuffer.Length));
                socket.Send(nameBuffer);
                while ((recv = fs.Read(contentBuffer, 0, 1024)) > 0)
                    socket.Send(contentBuffer);

                fs.Close();
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