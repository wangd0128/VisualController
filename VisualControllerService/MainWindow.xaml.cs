using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
using VisualControllerService.ViewModels;

namespace VisualControllerService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ProcessScreenShot shot;
        public MainWindow()
        {
            InitializeComponent();
            shot = new ProcessScreenShot(); 
            this.DataContext = new MainModel(shot);
        }
        Point startPoint;
        Point endPoint;
        bool IsMove = false;
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(img);
            endPoint = e.GetPosition(img);
            selectionRectangle.Width = 0;
            selectionRectangle.Height = 0;
            selectionRectangle.Visibility = Visibility.Visible;

        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsMove = true;
                endPoint = e.GetPosition(img);
                double x = Math.Min(startPoint.X, endPoint.X);
                double y = Math.Min(startPoint.Y, endPoint.Y);
                double width = Math.Abs(startPoint.X - endPoint.X);
                double height = Math.Abs(startPoint.Y - endPoint.Y);
                selectionRectangle.Margin = new Thickness(x, y, 0, 0);
                selectionRectangle.Width = width;
                selectionRectangle.Height = height;
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMove)
            {
                IsMove = false;
                selectionRectangle.Visibility = Visibility.Hidden;
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)img_grid.ActualWidth, (int)img_grid.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(img);

                CroppedBitmap croppedBitmap = new CroppedBitmap(renderTargetBitmap, new Int32Rect((int)selectionRectangle.Margin.Left, (int)selectionRectangle.Margin.Top, (int)selectionRectangle.Width, (int)selectionRectangle.Height));

                crop_img.Source = croppedBitmap;
            }
        }

        private void MouseClick(object sender, RoutedEventArgs e)
        {
            shot.MouseClick((int)(selectionRectangle.Margin.Left + selectionRectangle.Width / 2), (int)(selectionRectangle.Margin.Top + selectionRectangle.Height / 2));
        }
    }
}
