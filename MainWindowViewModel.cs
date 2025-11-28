using System.Diagnostics;
using Reactive.Bindings;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Reactive.Bindings.Extensions;
using System.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Linq;
using Reactive.Bindings.ObjectExtensions;

using System.IO;
using System.Reactive.Linq;

namespace ImgView
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name) => PropertyChanged(this, new PropertyChangedEventArgs(name));

        private CompositeDisposable Disposable { get; } = new ();
        public ReactiveProperty<string> Titlebar { get; private set; }
        public ReactiveProperty<Visibility> PlaylistVisibility { get; private set; }

        public ReactiveProperty<string> PlaylistSelected {get; set;}
        public ReactiveProperty<int> PlaylistIndex { get; set; }
        public ReactiveCollection<PlaylistItem> PlaylistItems { get; set; } = new ();

        public ReactiveCommand<DragEventArgs> DropCommand { get; }
        public ReactiveCommand<MouseButtonEventArgs> MouseDownCommand { get; }
        public AsyncReactiveCommand<MouseButtonEventArgs> MouseDownCommandAsync { get; }
        public ReactiveProperty<BitmapSource> PictureView { get; private set; }

        public ReactiveCommand FullScreenCommand { get; }

        public ReactiveProperty<WindowStyle> WinStyle { get; private set; }
        public ReactiveProperty<WindowState> WinState  { get; private set; }

        public ReactiveCommand<SizeChangedEventArgs> WinSinzeChangedCommand { get; }
        public ReactiveCommand LoadCommand { get; }

        public ReactiveCommand StartCommand {get;}
        public AsyncReactiveCommand StartCommandAsync {get;}
        public ReactiveCommand RemoveCommand { get; }
        public ReactiveCommand ClearCommand { get; }
        public ReactiveCommand UpCommand { get; }
        public ReactiveCommand DownCommand { get; }
        public ReactiveCommand SaveCommand { get; }
        public ReactiveCommand OpenCommand { get; }
        public ReactiveCommand EditCommand {get;}

        private PicturesModel _pictureModel;
        
        public MainWindowViewModel()
        {
            PropertyChanged += (o, e) => {};

            PictureView = new ReactiveProperty<BitmapSource>()
                .AddTo(Disposable);

            Titlebar = new ReactiveProperty<string>("Image viewer")
                .AddTo(Disposable);

            PlaylistVisibility = new ReactiveProperty<Visibility>(Visibility.Visible)
                .AddTo(Disposable);
            
            PlaylistSelected = new ReactiveProperty<string>()
                .AddTo(Disposable);
            
            PlaylistSelected.Subscribe(
                async e => {
                    if (e == null) return;

                    //Debug.Print("Index:{0} Name:{1}", PlaylistIndex.Value, PlaylistItems[PlaylistIndex.Value].FullName);
                    var fullname = PlaylistItems[PlaylistIndex.Value].FullName;
                    PictureView.Value = await Task.Run(() => ImageCacheManager.GetCoverPage(fullname));                    
                }
            );

            PlaylistIndex = new ReactiveProperty<int>(-1)
                .AddTo(Disposable);
                        
            PlaylistItems = new ReactiveCollection<PlaylistItem>()
                .AddTo(Disposable);
            PlaylistItems
                .ObserveAddChanged()
                .Subscribe(e=>{
                    if (PlaylistItems.Any() == false) return;
                    PlaylistIndex.Value = PlaylistItems.Count - 1;
                    /*
                    var fullname = PlaylistItems[PlaylistIndex.Value].FullName;
                    if (Path.GetExtension(fullname).ToUpper() == ".ZIP")
                    {
                        Task.Run(()=>PicturesModel.LoadAheadImage(fullname));
                    }
                    */
                    //var i = PicturesModel.LoadAheadImage(fullname);
                    StartCommandAsync.Execute();
                });

            WinStyle = new ReactiveProperty<WindowStyle>(WindowStyle.SingleBorderWindow)
                .AddTo(Disposable);

            WinState = new ReactiveProperty<WindowState>(WindowState.Normal)
                .AddTo(Disposable);

            WinSinzeChangedCommand = new ReactiveCommand<SizeChangedEventArgs>()
                .AddTo(Disposable);
            
            LoadCommand = new ReactiveCommand()
                .WithSubscribe(
                    ()=>{
                    string[] args = Environment.GetCommandLineArgs();

                    List<string> files = new List<string>();

                    for(var i=0; i < args.Length; i++)
                    {
                        if (i == 0) continue;
                        files.Add(args[i]);
                    }

                    if (files.Count <= 0) return;

                    if (PlaylistIndex.Value == -1)
                    {
                        PlaylistIndex.Value = 0;
                    }
                    foreach(var file in files)
                    {
                        PlaylistItems.AddOnScheduler(new PlaylistItem(file));
                    }
                })
                .AddTo(Disposable);

            DropCommand = new ReactiveCommand<DragEventArgs>()
                .WithSubscribe<DragEventArgs>(
                    e =>{
                        if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;

                        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                        if (files.Length == 0) return;

                        //Debug.Print("DropFileFirst1:{0}", files[0]);

                        if (PlaylistIndex.Value == -1)
                        {
                            PlaylistIndex.Value = 0;
                        }
                        foreach(var file in files)
                        {
                            PlaylistItems.AddOnScheduler(new PlaylistItem(file));
                        }
                    })
                .AddTo(Disposable);
            
            MouseDownCommand = new ReactiveCommand<MouseButtonEventArgs>()
                .WithSubscribe<MouseButtonEventArgs>(
                    e =>{
                        if (_pictureModel == null) return;

                        bool isMove = false;
                        if (e.ChangedButton == MouseButton.Left)
                        {
                            //Debug.Print("次へ");
                            isMove = _pictureModel.MoveNext();
                        }
                        if (e.ChangedButton == MouseButton.Right)
                        {
                            //Debug.Print("前へ");
                            isMove = _pictureModel.MovePrevious();
                        }

                        if (isMove)
                        {
                            var sw = new Stopwatch();
                            sw.Start();
                            
                            var b = _pictureModel.CurrentImage();
                            PictureView.Value = b;
                            Titlebar.Value = _pictureModel.CurrentImageName;

                            sw.Stop();
                            Titlebar.Value = string.Format("{0}ロード時間:{1}msec",
                                _pictureModel.CurrentImageName,
                                sw.Elapsed.Milliseconds);
                        }
                    })
                .AddTo(Disposable);
            
            MouseDownCommandAsync = new AsyncReactiveCommand<MouseButtonEventArgs>()
                .WithSubscribe<MouseButtonEventArgs>(
                    async e =>{
                        if (_pictureModel == null) return;

                        bool isMove = false;
                        if (e.ChangedButton == MouseButton.Left)
                        {
                            //Debug.Print("次へ");
                            isMove = _pictureModel.MoveNext();
                        }
                        if (e.ChangedButton == MouseButton.Right)
                        {
                            //Debug.Print("前へ");
                            isMove = _pictureModel.MovePrevious();
                        }

                        if (isMove)
                        {
                            var sw = new Stopwatch();
                            sw.Start();
                            
                            var b = await _pictureModel.CurrentImageAsync();
                            PictureView.Value = b;
                            Titlebar.Value = _pictureModel.CurrentImageName;

                            sw.Stop();
                            Titlebar.Value = string.Format("{0}ロード時間:{1}msec",
                                _pictureModel.CurrentImageName,
                                sw.Elapsed.Milliseconds);
                        }
                    })
                .AddTo(Disposable);

            FullScreenCommand = new ReactiveCommand()
                .WithSubscribe (
                    () => {
                        if (WinStyle.Value == WindowStyle.SingleBorderWindow)
                        {
                            //PlaylistVisibility.Value = Visibility.Collapsed;
                            WinStyle.Value = WindowStyle.None;
                            WinState.Value = WindowState.Maximized;
                        }
                        else
                        {
                            //PlaylistVisibility.Value = Visibility.Visible;
                            WinStyle.Value = WindowStyle.SingleBorderWindow;
                            WinState.Value = WindowState.Normal;
                        }
                    })
                .AddTo(Disposable);

            StartCommand = new ReactiveCommand()
                .WithSubscribe (
                    () => {
                        if (PlaylistItems.Any() == false) return;
                        PlaylistVisibility.Value = Visibility.Collapsed;

                        List<string> files = new List<string>();
                        foreach(var e in PlaylistItems)
                        {
                            files.Add(e.FullName);
                        }
                        _pictureModel = new PicturesModel(files.ToArray(), PlaylistIndex.Value);

                        var sw = new Stopwatch();
                        sw.Start();

                        var b = _pictureModel.CurrentImage();
                        PictureView.Value = b;
                        sw.Stop();
                        //Titlebar.Value = _pictureModel.CurrentImageName;

                        Titlebar.Value = string.Format("{0}ロード時間:{1}msec",
                            _pictureModel.CurrentImageName,
                            sw.Elapsed.Milliseconds);
                    }
                )
                .AddTo(Disposable);
            
            StartCommandAsync = new AsyncReactiveCommand()
                .WithSubscribe (
                    async () => {
                        if (PlaylistItems.Any() == false) return;
                        PlaylistVisibility.Value = Visibility.Collapsed;

                        List<string> files = new List<string>();
                        foreach(var e in PlaylistItems)
                        {
                            files.Add(e.FullName);
                        }
                        _pictureModel = new PicturesModel(files.ToArray(), PlaylistIndex.Value);

                        var sw = new Stopwatch();
                        sw.Start();

                        var b = await _pictureModel.CurrentImageAsync();
                        PictureView.Value = b;
                        sw.Stop();
                        //Titlebar.Value = _pictureModel.CurrentImageName;

                        Titlebar.Value = string.Format("{0}ロード時間:{1}msec",
                            _pictureModel.CurrentImageName,
                            sw.Elapsed.Milliseconds);
                    }
                )
                .AddTo(Disposable);

            RemoveCommand = new ReactiveCommand()
                .WithSubscribe(()=>{
                    if (PlaylistItems.Any() == false) return;
                    if (PlaylistIndex.Value < 0) return;
                    PlaylistItems.RemoveAtOnScheduler(PlaylistIndex.Value);
                    PictureView.Value = null;
                })
                .AddTo(Disposable);
            
            ClearCommand = new ReactiveCommand()
                .WithSubscribe(()=>{
                    if (PlaylistItems.Any() == false) return;
                    PlaylistItems.ClearOnScheduler();
                    PictureView.Value = null;
                })
                .AddTo(Disposable);
            
            UpCommand = new ReactiveCommand()
                .WithSubscribe(()=>{
                    if (PlaylistItems.Count <= 1) return;
                    if (PlaylistIndex.Value <= 0) return;

                    PlaylistItems.MoveOnScheduler(PlaylistIndex.Value, PlaylistIndex.Value-1);
                    PlaylistIndex.Value -= 1;
                }).AddTo(Disposable);
            
            DownCommand = new ReactiveCommand()
                .WithSubscribe(()=>{
                    if (PlaylistItems.Count <= 1) return;
                    if (PlaylistIndex.Value >= (PlaylistItems.Count-1)) return;

                    PlaylistItems.MoveOnScheduler(PlaylistIndex.Value, PlaylistIndex.Value+1);
                    PlaylistIndex.Value += 1;
                }).AddTo(Disposable);
            
            SaveCommand = new ReactiveCommand()
                .WithSubscribe(()=>{
                    if (PlaylistItems.Any() == false) return;

                    var dlg = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = "Playlist",
                        DefaultExt = ".txt",
                        Filter = "テキスト文書(.txt)|*.txt",
                    };
                    var result = dlg.ShowDialog();
                    if (result == true)
                    {
                        string filename = dlg.FileName;
                        PlaylistItem.Save(PlaylistItems, filename);
                    }
                }).AddTo(Disposable);
            
            OpenCommand = new ReactiveCommand()
                .WithSubscribe(()=>{
                    if (PlaylistItems.Any() == true) PlaylistItems.ClearOnScheduler();
                    
                    var dlg = new Microsoft.Win32.OpenFileDialog
                    {
                        FileName = "Playlist",
                        DefaultExt = ".txt",
                        Title = "ファイルを開く",
                        Filter = "テキスト文書(.txt)|*.txt",
                    };
                    var result = dlg.ShowDialog();
                    if (result == true)
                    {
                        string filename = dlg.FileName;
                        
                        var items = PlaylistItem.Load(filename);

                        foreach(var item in items)
                        {
                            PlaylistItems.AddOnScheduler(item);
                        }
                        if (PlaylistItems.Any())
                        {
                            PlaylistIndex.Value = 0;
                        }
                    }
                }).AddTo(Disposable);
            
            EditCommand = new ReactiveCommand()
                .WithSubscribe (
                    () => {
                        if (PlaylistVisibility.Value == Visibility.Collapsed)
                        {
                            // プレイリスト編集モード
                            PlaylistVisibility.Value = Visibility.Visible;

                            _pictureModel = null;

                            if (PlaylistItems.Any()==false) return;
                            PlaylistSelected.ForceNotify();
                        }
                        else
                        {
                            // 閲覧モード
                            StartCommandAsync.Execute();
                        }
                    }
                )
                .AddTo(Disposable);
            
        }
        public void Dispose() => Disposable.Dispose();
    }
}