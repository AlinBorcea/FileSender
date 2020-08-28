using System;

namespace File_Sender
{
    class Program
    {
        static void Main()
        {
            UserInput userInput = new UserInput();
            
            if (userInput.Error != UserInputError.None)
            {
                userInput.PrintErrorMessage();
                return;
            }

            SocketHandler socketHandler = new SocketHandler(userInput);

            if (userInput.AppType == "1") // Server
            {
                FileManager.ClearDir(FileManager.Turtle);
                socketHandler.RunServer();
            }
            else // Client
            {
                FileManager.ClearDir(FileManager.Parrot);
                FileManager.EncryptDir(userInput.DirPath, FileManager.Parrot);
                socketHandler.RunClient();
            }

            if (socketHandler.WasSuccessful)
                Console.WriteLine("Program Was Successful!");
            else
                Console.WriteLine("Program Failed!");

            Console.Write("Press Enter To Continue...");
            Console.Read();
        }
    }
}
