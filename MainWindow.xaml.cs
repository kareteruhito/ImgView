using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
