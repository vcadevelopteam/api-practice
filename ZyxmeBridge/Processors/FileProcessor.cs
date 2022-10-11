using Microsoft.AspNetCore.Http;
using Serilog.Core;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ZyxMeBridge.Processors
{
    public class FileProcessor
    {
        public static async Task<string> SaveByteArrayToLocalStorage(Logger Logger, byte[] ByteArray, string FileName, string FileType)
        {
            try
            {
                if (ByteArray != null)
                {
                    if (ByteArray.Count() > 0)
                    {
                        Logger.ForContext("Context", "Upload Byte Array").Debug("Uploading byte array: {FileName}", $"{FileName}.{FileType}");

                        string FilePath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{Path.DirectorySeparatorChar}storage", $"{FileName}.{FileType}");

                        if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{Path.DirectorySeparatorChar}storage")))
                        {
                            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{Path.DirectorySeparatorChar}storage"));
                        }

                        await File.WriteAllBytesAsync(FilePath, ByteArray);

                        Logger.ForContext("Context", "Upload Byte Array").Debug("Uploaded byte array: {FilePath}", FilePath);

                        return FilePath;
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "Upload Byte Array").Error(Exception, "Exception found:");
            }

            return null;
        }

        public static async Task<string> SaveFormFileToLocalStorage(Logger Logger, IFormFile FormFile)
        {
            try
            {
                if (FormFile != null)
                {
                    if (FormFile.Length > 0)
                    {
                        Logger.ForContext("Context", "Upload Form File").Debug("Uploading form file: {FileName}", FormFile.FileName);

                        string FilePath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{Path.DirectorySeparatorChar}storage", Path.GetFileName(FormFile.FileName));

                        if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{Path.DirectorySeparatorChar}storage")))
                        {
                            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{Path.DirectorySeparatorChar}storage"));
                        }

                        using FileStream FileStream = new FileStream(FilePath, FileMode.Create);

                        await FormFile.CopyToAsync(FileStream);

                        Logger.ForContext("Context", "Upload Form File").Debug("Uploaded form file: {FilePath}", FilePath);

                        return FilePath;
                    }
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "Upload Form File").Error(Exception, "Exception found:");
            }

            return null;
        }

        public static string SaveImageToLocalStorage(Logger Logger, string Link, string FileName)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Link))
                {
                    Logger.ForContext("Context", "Upload Image Link").Debug("Uploading link: {FileName}", $"{FileName}.jpeg");

                    string FilePath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{Path.DirectorySeparatorChar}storage", $"{FileName}.jpeg");

                    if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{Path.DirectorySeparatorChar}storage")))
                    {
                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{Path.DirectorySeparatorChar}storage"));
                    }

                    using WebClient ImageClient = new WebClient();

                    byte[] ImageByteArray = ImageClient.DownloadData(Link);

                    using MemoryStream MemoryStream = new MemoryStream(ImageByteArray);

                    using System.Drawing.Image Image = System.Drawing.Image.FromStream(MemoryStream);

                    Image.Save(FilePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                    Logger.ForContext("Context", "Upload Image Link").Debug("Uploaded link: {FilePath}", FilePath);

                    return FilePath;
                }
            }
            catch (Exception Exception)
            {
                Logger.ForContext("Context", "Upload Image Link").Error(Exception, "Exception found:");
            }

            return null;
        }
    }
}