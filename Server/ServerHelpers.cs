using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IO;

public static class ServerHelpers
{

    const string DB_FILE_PATH = "database.txt";
    public static string Base64Encode(string plainText) 
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(string base64EncodedData) 
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public static string GetHash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public static string[] ReadDBToArray()
    {        
        // Check if the file exists
        if (!File.Exists(DB_FILE_PATH))
        {
            throw new FileNotFoundException($"File not found: {DB_FILE_PATH}");
        }

        // Read all lines from the file
        string[] lines = File.ReadAllLines(DB_FILE_PATH);

        return lines;
    }

    public static void WriteToDB(string newRoom)
    {
        // Check if the file exists
        if (!File.Exists(DB_FILE_PATH))
        {
            throw new FileNotFoundException($"File not found: {DB_FILE_PATH}");
        }

        using (StreamWriter writer = File.AppendText(DB_FILE_PATH))
        {
            writer.WriteLine(newRoom);
        }
        
    }
}
