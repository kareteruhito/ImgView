using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Windows.Media.Animation;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Reflection.Metadata;

namespace ImgView
{
    public class PicturesModel
    {
        static readonly string[] _pictureExtensions = new string[] { ".PNG", ".JPEG", ".JPG", ".BMP", ".WEBP"};

        List<FileInfo> _files;
        int _index;
        int _index2;
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
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="files">PlayListのファイルの一覧</param>
        /// <param name="index">PlayListの選択位置、開始位置</param>
        public PicturesModel(string[] files, int index = -1)
        {
            _files = new List<FileInfo>();


            int playListIndex = 0;  // PlayListのインデックス
            int imagesIndex = 0;  // 画像のインデックス
            _index = -1;
            _index2 = -1;
            foreach(var file in files)
            {
                var ext = Path.GetExtension(file).ToUpper();
                if (_pictureExtensions.Contains(ext) == true)
                {
                    // 画像ファイル
                    var Location = Path.GetDirectoryName(file);
                    var filename = Path.GetFileName(file);
                    _files.Add(new FileInfo{
                        FileName = filename,
                        Location = Location,
                        LocationType = "Dir",
                    });
                    if (playListIndex==index)
                    {
                        _index = imagesIndex;
                    }
                    playListIndex++;
                    imagesIndex++;
                    continue;
                }
                if (ext == ".ZIP" || ext == ".EPUB")
                {
                    // ZIPファイル
                    var Location = file;
                    using (var zip = System.IO.Compression.ZipFile.OpenRead(file))
                    {
                        var entries = zip.Entries
                            .Where(x => _pictureExtensions.Contains(Path.GetExtension(x.FullName).ToUpper()) );

                        if (entries.Any() == true)                        
                        {
                            if (playListIndex==index)
                            {
                                _index = imagesIndex;
                            }
                            playListIndex++;
                            foreach(var e in entries)
                            {
                                var filename = e.FullName;
                                _files.Add(new FileInfo{
                                    FileName = filename,
                                    Location = Location,
                                    LocationType = "Zip",
                                });
                                imagesIndex++;
                            }
                        }
                    }
                    continue;
                }
            }
        }





        static private BitmapSource PlaceOnCanvasImage(BitmapSource bi, BitmapSource bi2=null)
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

        async static private Task<BitmapSource> PlaceOnCanvasImageAsync(BitmapSource bi, BitmapSource bi2=null)
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

        public string CurrentImageName{ get; private set; } = "";
        public BitmapSource CurrentImage()
        {
            if (_index == -1) return null;

            if (_index == 0 || _index == (_files.Count-1) || _files.Count == 1 || _files[_index].Location != _files[_index+1].Location || _files[_index].Location != _files[_index-1].Location)
            {
                CurrentImageName = _files[_index].FileName + " ";
                _index2 = -1;
                var bi = ImageCacheManager.LoadImage(_files[_index]);
                return PlaceOnCanvasImage(bi);
            }
            else
            {
                var ri = ImageCacheManager.LoadImage(_files[_index]);
                if (ri.PixelWidth > ri.PixelHeight)
                {
                    _index2 = -1;
                    // 横長
                    return PlaceOnCanvasImage(ri);
                }

                _index2 = _index + 1;
                var ri2 = ImageCacheManager.LoadImage(_files[_index2]);
                if (ri2.PixelWidth > ri2.PixelHeight)
                {
                    _index2 = -1;
                    // 横長
                    return PlaceOnCanvasImage(ri);
                }

                CurrentImageName = _files[_index2].FileName + "|" + _files[_index].FileName + " ";

                return PlaceOnCanvasImage(ri, ri2);
            }
        }
        async public Task<BitmapSource> CurrentImageAsync()
        {
            if (_index == -1) return null;

            if (_index == 0 || _index == (_files.Count-1) || _files.Count == 1 || _files[_index].Location != _files[_index+1].Location || _files[_index].Location != _files[_index-1].Location)
            {
                CurrentImageName = _files[_index].FileName + " ";
                _index2 = -1;
                var bi = await ImageCacheManager.LoadImageAsync(_files[_index]);
                return await PlaceOnCanvasImageAsync(bi);
            }
            else
            {
                var ri = await ImageCacheManager.LoadImageAsync(_files[_index]);
                if (ri.PixelWidth > ri.PixelHeight)
                {
                    _index2 = -1;
                    // 横長
                    return await PlaceOnCanvasImageAsync(ri);
                }

                _index2 = _index + 1;
                var ri2 = await ImageCacheManager.LoadImageAsync(_files[_index2]);
                if (ri2.PixelWidth > ri2.PixelHeight)
                {
                    _index2 = -1;
                    // 横長
                    return await PlaceOnCanvasImageAsync(ri);
                }

                CurrentImageName = _files[_index2].FileName + "|" + _files[_index].FileName + " ";

                return await PlaceOnCanvasImageAsync(ri, ri2);
            }
        }
        public bool MoveNext()
        {
            if (_index == -1) return false;
            if (_files.Count == (_index+1)) return false;
            if (_files.Count == (_index2+1)) return false;

            if (_index2 != -1)
            {
                _index = _index2+1;
                _index2 = -1;
            }
            else
            {
                _index++;
            }



            return true;
        }
        public bool MovePrevious()
        {
            BitmapSource bs;

            if (_index == -1) return false; // 画像なし
            if (_index == 0) return false;  // 先頭のためこれ以上戻れない
            
            int fileCounter = 0;
            if (_files[_index].Location != _files[_index-1].Location)
            {
                var i = _index-1;
                var location = _files[i].Location;
                while(i >= 0 && _files[i].Location == location)
                {
                    fileCounter++;
                    i--;
                }
            }

            _index2 = -1;
            _index--;   // 1ページ戻る

            if (fileCounter > 0 && fileCounter%2 == 0)
            {
                // 偶数
                return true;
            }

            if (_index == 0) return true;  // 先頭のためこれ以上戻れない

            bs = ImageCacheManager.LoadImage(_files[_index]);
            if (bs.PixelWidth > bs.PixelHeight) return true;    // 横長

            if (_files[_index].Location != _files[_index-1].Location) return true;  // ロケーション(Zip or Dir)の先頭

            _index2 = _index;
            _index--;   // 2ページ戻る

            return true;
        }
    }
}