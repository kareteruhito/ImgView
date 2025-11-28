using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImgView;

/// <summary>
/// 画像の加工
/// </summary>
static public class ImageHelper
{
    /// <summary>
    /// DPIを96へ変換
    /// </summary>
    /// <param name="bi">変換対象画像</param>
    /// <returns>変換後の画像</returns>
    static public BitmapSource ConvertToDPI96(BitmapSource bi)
    {
        const double dpi = 96;
        if (bi.DpiX == dpi && bi.DpiY == dpi) return bi;

        int width = bi.PixelWidth;
        int height = bi.PixelHeight;
        int stride = width * 4;
        byte[] pixelData = new byte[stride * height];
        bi.CopyPixels(pixelData, stride, 0);

        var bii = BitmapImage.Create(
            width,
            height,
            dpi,
            dpi,
            PixelFormats.Bgra32,
            null,
            pixelData,
            stride);
        bii.Freeze();
        
        return bi;
    }
    /// <summary>
    /// 画像のピクセルフォーマットをRGBA32へ変換
    /// </summary>
    /// <param name="bi">変換対象画像</param>
    /// <returns>変換後画像</returns>
    static public BitmapSource ConvertToBgra32(BitmapSource bi)
    {

        if (bi.Format == PixelFormats.Bgra32)
        {
            return ImageHelper.ConvertToDPI96(bi);
        }

        bi = new FormatConvertedBitmap(
            bi,
            PixelFormats.Bgra32,
            null,
            0);
        bi.Freeze();

        return ImageHelper.ConvertToDPI96(bi);
    }
    /// <summary>
    /// async版DPIを96へ変換
    /// </summary>
    /// <param name="bi">変換対象画像</param>
    /// <returns>変換後の画像</returns>
    async static public Task<BitmapSource> ConvertToDPI96Async(BitmapSource bi)
    {
        double dpi = 96;
        if (bi.DpiX == dpi && bi.DpiY == dpi) return bi;
        var bi3 = await Task.Run(()=>
        {
            int width = bi.PixelWidth;
            int height = bi.PixelHeight;
            int stride = width * 4;
            byte[] pixelData = new byte[stride * height];
            bi.CopyPixels(pixelData, stride, 0);

            var bi2 = BitmapImage.Create(
                width,
                height,
                dpi,
                dpi,
                PixelFormats.Bgra32,
                null,
                pixelData,
                stride);
            bi2.Freeze();

            return bi2;
        });
        return bi3;
    }
    /// <summary>
    /// async版画像のピクセルフォーマットをRGBA32へ変換
    /// </summary>
    /// <param name="bi">変換対象画像</param>
    /// <returns>変換後画像</returns>
    async static public Task<BitmapSource> ConvertToBgra32Async(BitmapSource bi)
    {

        if (bi.Format == PixelFormats.Bgra32)
        {
            return await ConvertToDPI96Async(bi);
        }
        var bi3 = await Task.Run(()=>
        {
            var bi2 = new FormatConvertedBitmap(
                bi,
                PixelFormats.Bgra32,
                null,
                0);
            bi2.Freeze();
            return bi2;
        });

        return await ConvertToDPI96Async(bi3);
    }
    /// <summary>
    /// 見開き画像生成
    /// </summary>
    /// <param name="bi">左ページ</param>
    /// <param name="bi2">右ページ</param>
    /// <returns>見開き画像</returns>
    static public BitmapSource PlaceOnCanvasImage(BitmapSource bi, BitmapSource bi2=null)
    {
        int stride = bi.PixelWidth * 4;
        byte[] datas = new byte[stride * bi.PixelHeight];

        bi.CopyPixels(new Int32Rect(0, 0, bi.PixelWidth, bi.PixelHeight), datas, stride, 0);

        WriteableBitmap w;

        if (bi2==null)
        {
            // 画像１枚
            int width = (int)((bi.PixelHeight / 9.0) * 16.0);
            if (width < bi.PixelWidth) width = bi.PixelWidth;

            w  = new WriteableBitmap(width, bi.PixelHeight, bi.DpiX, bi.DpiY, PixelFormats.Bgra32, null);
            int x = (int)((width - bi.PixelWidth)/2);
            if (x < 0)
            {
                x = 0;
            }
            w.WritePixels(
                new Int32Rect(x, 0, bi.PixelWidth, bi.PixelHeight),
                datas,
                stride,
                0);
        }
        else
        {
            // 画像２枚
            int stride2 = bi2.PixelWidth * 4;
            byte[] datas2 = new byte[stride2 * bi2.PixelHeight];

            int height = (bi2.PixelHeight>bi.PixelHeight) ? bi2.PixelHeight : bi.PixelHeight;
            int width = bi2.PixelWidth + bi.PixelWidth;

            w  = new WriteableBitmap(width, height, bi.DpiX, bi.DpiY, PixelFormats.Bgra32, null);

            bi2.CopyPixels(new Int32Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight), datas2, stride2, 0);

            int x = 0;
            w.WritePixels(
                new Int32Rect(x, 0, bi2.PixelWidth, bi2.PixelHeight),
                datas2,
                stride2,
                0);
            w.WritePixels(
                new Int32Rect(x+bi2.PixelWidth, 0, bi.PixelWidth, bi.PixelHeight),
                datas,
                stride,
                0);
        }

        w.Freeze();
        return w;
    }
    /// <summary>
    /// async版見開き画像生成
    /// </summary>
    /// <param name="bi">左ページ</param>
    /// <param name="bi2">右ページ</param>
    /// <returns>見開き画像</returns>

    async static public Task<BitmapSource> PlaceOnCanvasImageAsync(BitmapSource bi, BitmapSource bi2=null)
    {
        int stride = bi.PixelWidth * 4;
        byte[] datas = new byte[stride * bi.PixelHeight];

        await Task.Run(()=>
        {
            bi.CopyPixels(new Int32Rect(0, 0, bi.PixelWidth, bi.PixelHeight), datas, stride, 0);
        });

        var w2 = await Task.Run(()=>
        {
            WriteableBitmap w;

            if (bi2==null)
            {
                // 画像１枚
                int width = (int)((bi.PixelHeight / 9.0) * 16.0);
                if (width < bi.PixelWidth) width = bi.PixelWidth;

                w  = new WriteableBitmap(width, bi.PixelHeight, bi.DpiX, bi.DpiY, PixelFormats.Bgra32, null);
                int x = (int)((width - bi.PixelWidth)/2);
                if (x < 0)
                {
                    x = 0;
                }
                w.WritePixels(
                    new Int32Rect(x, 0, bi.PixelWidth, bi.PixelHeight),
                    datas,
                    stride,
                    0);
            }
            else
            {
                // 画像２枚
                int stride2 = bi2.PixelWidth * 4;
                byte[] datas2 = new byte[stride2 * bi2.PixelHeight];

                int height = (bi2.PixelHeight>bi.PixelHeight) ? bi2.PixelHeight : bi.PixelHeight;
                int width = bi2.PixelWidth + bi.PixelWidth;

                w  = new WriteableBitmap(width, height, bi.DpiX, bi.DpiY, PixelFormats.Bgra32, null);

                bi2.CopyPixels(new Int32Rect(0, 0, bi2.PixelWidth, bi2.PixelHeight), datas2, stride2, 0);

                int x = 0;
                w.WritePixels(
                    new Int32Rect(x, 0, bi2.PixelWidth, bi2.PixelHeight),
                    datas2,
                    stride2,
                    0);
                w.WritePixels(
                    new Int32Rect(x+bi2.PixelWidth, 0, bi.PixelWidth, bi.PixelHeight),
                    datas,
                    stride,
                    0);
            }

            w.Freeze();
            return w;
        });
        return w2;
    }

}//class