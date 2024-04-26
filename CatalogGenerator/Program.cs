using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
class Program
{
    static void Main(string[] args)
    {
        string directoryPath = Directory.GetCurrentDirectory();
        string catalogFilePath = Path.Combine(directoryPath, "catalog.txt");

        using (StreamWriter writer = new StreamWriter(catalogFilePath))
        {
            try
            {
                WriteCatalog(directoryPath, directoryPath, writer); // sends data to start the write process
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    // writes into the catalog.txt file
    static void WriteCatalog(string baseDirectory, string currentDirectory, StreamWriter writer)
    {
        string[] files = Directory.GetFiles(currentDirectory);
        foreach (string filePath in files)
        {
            if (filePath.EndsWith("catalog.txt", StringComparison.OrdinalIgnoreCase))
                continue; // skips catalog txt so it can write correctly.

            string relativePath = GetPath(baseDirectory, filePath).Replace("%20", " ");
            string fileName = Path.GetFileName(relativePath);
            string hash = HashSHA256(filePath);
            long fileSize = new FileInfo(filePath).Length;

            writer.WriteLine($"{relativePath} {hash} {fileSize} ");
        }

        string[] directories = Directory.GetDirectories(currentDirectory);
        foreach (string subDirectory in directories)
        {
            WriteCatalog(baseDirectory, subDirectory, writer);
        }
    }

    // this will get the path of the file relative to the base directory
    static string GetPath(string basePath, string fullPath)
    {
        if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            basePath += Path.DirectorySeparatorChar;

        Uri baseUri = new Uri(basePath);
        Uri fullUri = new Uri(fullPath);

        return baseUri.MakeRelativeUri(fullUri).ToString().Replace('/', Path.DirectorySeparatorChar);
    }


    // this will calculate the sha256 hash of the file
    static string HashSHA256(string filePath)
    {
        using (var sha256 = SHA256.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}