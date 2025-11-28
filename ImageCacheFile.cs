/*
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System;
using System.Text;

public class ImageCacheFile
{
    async public static Task<BitmapImage> CreateBitmapImageAsync(System.IO.Stream stream)
    {
        return await Task.Run(()=>
        {
            var bi = new BitmapImage();


            bi.BeginInit();
            bi.StreamSource = stream;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.CreateOptions = BitmapCreateOptions.None;
            bi.EndInit();
            bi.Freeze();

            return bi;
        });
    }
    public static string CreateCacheFilePath(string key)
    {
        string cacheDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImgCache");
        if (!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }
        var Hash = MD5.Create();
        byte[] fileBytes = Encoding.UTF8.GetBytes(key);
        byte[] hashBytes = Hash.ComputeHash(fileBytes);
        string hashString = BitConverter.ToString(hashBytes);
        string cacheImagePath = System.IO.Path.Combine(cacheDir, $"{hashString}.tiff");
        return cacheImagePath;
    }
    async public static Task<BitmapImage> ImageCacheGetAsync(string key)
    {
        string cacheImagePath = CreateCacheFilePath(key);

        if (!File.Exists(cacheImagePath)) return null;

        var stream = new FileStream(cacheImagePath, FileMode.Open, FileAccess.Read);
        var bi = await CreateBitmapImageAsync(stream);

        return bi;
    }    
    static public async Task ImageCacheSetAsync(string key, BitmapSource bi)
    {
        string cacheImagePath = CreateCacheFilePath(key);

        await using var stream = new FileStream(cacheImagePath, FileMode.CreateNew, FileAccess.Write);

        await Task.Run(()=>
        {
            var tiffEncoder = new TiffBitmapEncoder();
            tiffEncoder.Compression = TiffCompressOption.Rle;
            tiffEncoder.Frames.Add(BitmapFrame.Create(bi));
            tiffEncoder.Save(stream);
        });
    }
    static public void ImageCacheClear()
    {
        string cacheDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImgCache");
        if (!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }
        var files = Directory.EnumerateFiles(cacheDir, "*.tiff");
        foreach(string file in files)
        {
            System.IO.File.Delete(file);
        }
    }    
}//class
*/