using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

public class FileManager
{
    public const string Parrot = "parrot";
    public const string Turtle = "turtle";

    public static void EncryptDir(string src, string dirName, string password)
    {
        string[] fileNames = Directory.GetFiles(src);
        AesCryptoServiceProvider acsp = new AesCryptoServiceProvider();
        acsp.GenerateKey();
        acsp.GenerateIV();

        ICryptoTransform cryptoTransf = Aes.Create().CreateEncryptor(acsp.Key, acsp.IV);

        foreach (string name in fileNames)
        {
            FileStream trgtStrm = File.Create($"{dirName}/{Path.GetFileName(name)}");
            string srcstr = File.ReadAllText(name);

            CryptoStream crypto = new CryptoStream(trgtStrm, cryptoTransf, CryptoStreamMode.Write);
            StreamWriter swriter = new StreamWriter(crypto);

            foreach (char c in srcstr)
                swriter.Write(c);

            swriter.Close();
            crypto.Close();
            trgtStrm.Close();
        }
    }

    public static void DecryptDir(string dest, string dir, string password)
    {
        string[] fileNames = Directory.GetFiles(dir);
    }

    public static void ClearDir(string dirName)
    {
        if (dirName != Parrot && dirName != Turtle)
        {
            Console.WriteLine($"Directory must be {Turtle} or {Parrot}");
            Environment.Exit(1);
        }
        
        try
        {
            string[] fileNames = Directory.GetFiles(dirName);
            foreach (string name in fileNames)
                File.Delete(name);
        }
        catch (Exception e)
        {
            return;
        }
    }



}