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
            FileStream fs;
            byte[] salt = new byte[16];
            byte[] countBuffer = new byte[4];
            byte[] nameBuffer = new byte[32];
            byte[] contentBuffer = new byte[1024];
            int recv;

            FileManager.ClearDir(FileManager.Turtle);
            FileManager.ClearDir(user.DirPath);

            Socket socket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(iPEndPoint);
            socket.Listen(1);

            byte[] b = new byte[1];
            Socket handler = socket.Accept();
            Console.WriteLine("Connected to {0}", handler.RemoteEndPoint.ToString());

            handler.Send(b);
            handler.Receive(salt);

            handler.Send(b);
            handler.Receive(countBuffer);

            int len = BitConverter.ToInt32(countBuffer);
            for (int i = 0; i < len; i++)
            {
                Array.Clear(countBuffer, 0, 4);
                Array.Clear(nameBuffer, 0, 32);

                handler.Send(b);
                handler.Receive(nameBuffer);

                string fileName = Encoding.ASCII.GetString(nameBuffer).Replace('\0', ' ');
                fs = File.Create(Path.Combine(FileManager.Turtle, fileName));

                handler.Send(b);
                handler.Receive(contentBuffer);
                fs.Write(contentBuffer);

                /*while ((recv = handler.Receive(contentBuffer)) > 0)
                {
                    fs.Write(contentBuffer, 0, recv);
                    //handler.Send(b);
                }*/

                fs.Close();
            }

            handler.Close();
            socket.Close();

            Console.Write("One Time Password... ");
            string password = Console.ReadLine();

            Rfc2898DeriveBytes pass = new Rfc2898DeriveBytes(password, salt);
            byte[] key = pass.GetBytes(16);

            foreach (string name in Directory.GetFiles(user.DirPath)) 
            {
                Console.WriteLine(name);
            }

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
            int len = fileNames.Length;
            byte[] countBuffer = BitConverter.GetBytes(fileNames.Length);
            byte[] nameBuffer = new byte[32];
            byte[] contentBuffer = new byte[1024];
            byte[] b = new byte[1];
            int recv;

            FileManager.ClearDir(FileManager.Parrot);
            foreach (string name in fileNames)
                FileManager.EncryptFile(name, key, salt);

            socket.Connect(iPEndPoint);
            Console.WriteLine("Connected to {0}", socket.RemoteEndPoint.ToString());

            socket.Receive(b);
            socket.Send(salt);

            socket.Receive(b);
            socket.Send(countBuffer);

            foreach (string name in Directory.GetFiles(FileManager.Parrot))
            {
                socket.Receive(b);
                socket.Send(Encoding.ASCII.GetBytes(Path.GetFileName(name)));

                fs = File.OpenRead(name);
                fs.Read(contentBuffer, 0, 1024);

                socket.Receive(b);
                socket.Send(contentBuffer);
                /*while ((recv = fs.Read(contentBuffer, 0, 1024)) > 0)
                {
                    socket.Send(contentBuffer);
                    //socket.Receive(b);
                }*/


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