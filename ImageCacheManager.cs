using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImgView;

/// <summary>
/// 画像のキャッシュ管理クラス
/// </summary>
static public class ImageCacheManager
{
    static private Dictionary<string, BitmapSource> BitmapSourceCacheDictionay = new();

    /// <summary>
    /// キャッシュから画像をロード
    /// </summary>
    /// <param name="info">ロードする画像ファイルの情報</param>
    /// <returns>ロードした画像</returns>
    static private BitmapSource LoadCacheImage(FileInfo info)
    {
        // キャッシュにあるか
        var cacheKey = Path.Combine(info.Location, info.FileName);
        if (BitmapSourceCacheDictionay.ContainsKey(cacheKey))
        {
            return BitmapSourceCacheDictionay[cacheKey];
        }

        // キャッシュ無し
        BitmapImage bi = new();

        // ストリームを開く
        if (info.LocationType == "Zip")
        {
            // ZIP
            using var zip = System.IO.Compression.ZipFile.OpenRead(info.Location);
            using var fs = zip.GetEntry(info.FileName).Open();
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();

            ms.SetLength(0);
        }
        else
        {
            // Direcotry
            var path = Path.Join(info.Location, info.FileName);
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);

            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();

            ms.SetLength(0);
        }

        var bs = ImageHelper.ConvertToBgra32(bi);
        // ロック
        lock(BitmapSourceCacheDictionay)
        {
            // キャッシュに追加
            if (BitmapSourceCacheDictionay.ContainsKey(cacheKey) == false)
            {
                BitmapSourceCacheDictionay[cacheKey] = bs;
            }
        }
        return BitmapSourceCacheDictionay[cacheKey];
    }
    /// <summary>
    /// async版キャッシュから画像をロード
    /// </summary>
    /// <param name="info">ロードする画像ファイルの情報</param>
    /// <returns>ロードした画像</returns>
    async static private Task<BitmapSource> LoadCacheImageAsync(FileInfo info)
    {
        // キャッシュにあるか
        var cacheKey = Path.Combine(info.Location, info.FileName);
        
        if (BitmapSourceCacheDictionay.ContainsKey(cacheKey))
        {
            return BitmapSourceCacheDictionay[cacheKey];
        }
        

        // キャッシュ無し
        var bi = new BitmapImage();

        // ストリームを開く
        if (info.LocationType == "Zip")
        {
            // ZIP
            using var zip = System.IO.Compression.ZipFile.OpenRead(info.Location);
            using var fs = zip.GetEntry(info.FileName).Open();
            using var ms = new MemoryStream();
            await fs.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);
            bi = await Task.Run(()=>
            {
                var b = new BitmapImage();
                b.BeginInit();
                b.CacheOption = BitmapCacheOption.OnLoad;
                b.StreamSource = ms;
                b.EndInit();
                b.Freeze();
                return b;
            });
            ms.SetLength(0);
        }
        else
        {
            // Direcotry
            var path = Path.Join(info.Location, info.FileName);
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);

            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = ms;
            bi.EndInit();
            bi.Freeze();

            ms.SetLength(0);
        }

        var bs = await ImageHelper.ConvertToBgra32Async(bi);

        
        // ロック
        lock(BitmapSourceCacheDictionay)
        {
            // キャッシュに追加
            if (BitmapSourceCacheDictionay.ContainsKey(cacheKey) == false)
            {
                BitmapSourceCacheDictionay[cacheKey] = bs;
            }
        }
        return BitmapSourceCacheDictionay[cacheKey];
    }
    /// <summary>
    /// 画像をロード
    /// </summary>
    /// <param name="info">ロードする画像ファイルの情報</param>
    /// <returns>ロードした画像</returns>
    static public BitmapSource LoadImage(FileInfo info)
    {
        return LoadCacheImage(info);
    }
    /// <summary>
    /// async版キャッシュから画像をロード
    /// </summary>
    /// <param name="info">ロードする画像ファイルの情報</param>
    /// <returns>ロードした画像</returns>
    async static public Task<BitmapSource> LoadImageAsync(FileInfo info)
    {
        return await LoadCacheImageAsync(info);
    }
    static readonly string[] _pictureExtensions = new string[] { ".PNG", ".JPEG", ".JPG", ".BMP", ".WEBP"};
    /// <summary>
    /// 表紙画像の取得
    /// </summary>
    /// <param name="file">ファイルパス</param>
    /// <returns>表紙画像のBitmapSource</returns>
    public static BitmapSource GetCoverPage(string file)
    {
        var ext = Path.GetExtension(file).ToUpper();
        if (_pictureExtensions.Contains(ext) == true)
        {
            // 画像ファイル
            var Location = Path.GetDirectoryName(file);
            var filename = Path.GetFileName(file);
            return ImageCacheManager.LoadImage(new FileInfo{
                FileName = filename,
                Location = Location,
                LocationType = "Dir",
            });
        }
        if (ext == ".ZIP" || ext == ".EPUB")
        {
            // ZIPファイル
            var Location = file;
            using (var zip = System.IO.Compression.ZipFile.OpenRead(file))
            {
                var e = zip.Entries
                    .Where(x => _pictureExtensions.Contains(Path.GetExtension(x.FullName).ToUpper()))
                    .First();
                
                if (e != null)
                {
                    return ImageCacheManager.LoadImage(new FileInfo{
                        FileName = e.FullName,
                        Location = Location,
                        LocationType = "Zip",
                    });
                }                    
            }
        }
        return null;
    }
}//class