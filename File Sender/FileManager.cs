using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

public class FileManager
{
    public const string Parrot = "parrot";
    public const string Turtle = "turtle";


    public static void EncryptFile(string filePath, byte[] key, byte[] salt)
    {
        FileStream infs = new FileStream(filePath, FileMode.Open);
        FileStream outfs = File.Create($"{Parrot}/{Path.GetFileName(filePath)}");
        CryptoStream crypto = new CryptoStream(outfs, Aes.Create().CreateEncryptor(key, salt), CryptoStreamMode.Write);

        byte[] b = new byte[4];
        int read;

        while ((read = infs.Read(b, 0, b.Length)) > 0)
            crypto.Write(b, 0, read);

        crypto.Close();
        infs.Close();
        outfs.Close();
    }

    public static void DecryptFile(string dirPath, string fileName, byte[] key, byte[] salt)
    {
        FileStream infs = File.OpenRead($"{Turtle}/{fileName}");
        FileStream outfs = File.Create($"{dirPath}/{fileName}");
        CryptoStream crypto = new CryptoStream(outfs, Aes.Create().CreateDecryptor(key, salt), CryptoStreamMode.Read);

        byte[] b = new byte[4];
        int read;

        while ((read = crypto.Read(b, 0, b.Length)) > 0)
            outfs.Write(b, 0, read);

        crypto.Close();
        infs.Close();
        outfs.Close();
    }

    public static void ClearDir(string dirName)
    {
        try
        {
            string[] fileNames = Directory.GetFiles(dirName);
            foreach (string name in fileNames)
                File.Delete(name);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return;
        }
    }

}