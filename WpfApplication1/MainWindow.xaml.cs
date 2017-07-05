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
using Renderer.Core;
using System.Runtime.InteropServices;
using NVRCsharpDemo;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private HKVedioGenerator _hkVedioGenerator;
        private HKVedioGenerator _hkVedioGenerator2;

        public delegate void MyDebugInfo(string str);

        public MainWindow()
        {
            InitializeComponent();

            _hkVedioGenerator2 = new HKVedioGenerator();
            _hkVedioGenerator2.Address = "192.168.100.111";
            _hkVedioGenerator2.Port = 8000;
            _hkVedioGenerator2.UserName = "admin";
            _hkVedioGenerator2.Password = "Password0123";
            _hkVedioGenerator2.Channel = 1;
            _hkVedioGenerator2.OnFrameInvoked += _hkVedioGenerator_OnFrameInvoked2;


            _hkVedioGenerator = new HKVedioGenerator();
            _hkVedioGenerator.Address = "192.168.100.111";
            _hkVedioGenerator.Port = 8000;
            _hkVedioGenerator.UserName = "admin";
            _hkVedioGenerator.Password = "Password0123";
            _hkVedioGenerator.Channel = 3;
            _hkVedioGenerator.OnFrameInvoked += _hkVedioGenerator_OnFrameInvoked;
            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
        }

        private void _hkVedioGenerator_OnFrameInvoked2(object sender, ImageSource e)
        {
            T2.Source = e;
            T3.Source = e;
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _hkVedioGenerator.Stop();
            _hkVedioGenerator2.Stop();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void _hkVedioGenerator_OnFrameInvoked(object sender, ImageSource e)
        {
            T1.Source = e;
            T4.Source = e;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _hkVedioGenerator.Start();
            _hkVedioGenerator2.Start();
        }
    }
}
