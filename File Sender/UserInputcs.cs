using System;
using System.IO;
using System.Net;

public enum UserInputError
{
    None,
    IPErr,
    PortErr,
    DirPathErr,
    DirEmptyErr,
    AppTypeErr,
}

public class UserInput
{
    public string IP { get; set; }
    public string Port { get; set; }
    public string DirPath { get; set; }
    public string AppType { get; set; }
    public UserInputError Error { get; set; }

    public UserInput()
    {
        Console.Write("Sever IP address... ");
        IP= Console.ReadLine();

        Console.Write("Server Port... ");
        Port = Console.ReadLine();

        Console.Write("Target Directory... ");
        DirPath = Console.ReadLine();

        Console.WriteLine("1 -> Server");
        Console.WriteLine("2 -> Client");
        AppType = Console.ReadLine();

        Error = FindError();
    }

    public UserInputError FindError()
    {
        if (!IPAddress.TryParse(IP, out _))
            return UserInputError.IPErr;

        if (!int.TryParse(Port, out _))
            return UserInputError.PortErr;
        
        string[] files;
        try
        {
            files = Directory.GetFiles(DirPath);
        }
        catch (Exception e)
        {
            return UserInputError.DirPathErr;
        }

        if (files.Length < 1)
            return UserInputError.DirEmptyErr;
        
        if (AppType != "1" && AppType != "2")
            return UserInputError.AppTypeErr;

        return UserInputError.None;
    }

    public void PrintErrorMessage()
    {
        switch (Error)
        {
            case UserInputError.IPErr:
                Console.WriteLine("IP Is Invalid!");
                break;

            case UserInputError.PortErr:
                Console.WriteLine("Port Is Invalid!");
                break;

            case UserInputError.DirPathErr:
                Console.WriteLine("Could Not Find Given Directory!");
                break;

            case UserInputError.DirEmptyErr:
                Console.WriteLine("Directory Is Empty!");
                break;

            case UserInputError.AppTypeErr:
                Console.WriteLine("App Type Is Invalid!");
                break;

            default:
                Console.WriteLine("User Input Is Valid!");
                break;
        }
    }

}