using System.Windows;

namespace ImgView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        /*
        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Debug.Print("W:{0} H:{1}", (int)e.NewSize.Width, (int)e.NewSize.Height);
            Image1.Height = (int)e.NewSize.Height;
            Image1.Width = (int)e.NewSize.Width;
        }
        */
    }
}
