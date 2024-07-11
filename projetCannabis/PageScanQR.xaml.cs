using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ZXing;
using ZXing.Common;
//using ZXing.Windows.Compatibility;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace projetCannabis
{
    /// <summary>
    /// Logique d'interaction pour PageScanQR.xaml
    /// </summary>
    public partial class PageScanQR : Page
    {
        public PageScanQR()
        {
            InitializeComponent();
        }


        private void BtnOnStartStopCaptureClick(object sender, RoutedEventArgs e)
        {
            
        }

    }
}


