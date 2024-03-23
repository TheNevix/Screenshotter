using Microsoft.Win32;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Screenshotter.UI
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //fix for some screenshots to not capture the whole screen
            double dpiX, dpiY;
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpiX = graphics.DpiX / 96.0;
                dpiY = graphics.DpiY / 96.0;
            }

            //Get screen dimensions
            double screenLeft = SystemParameters.VirtualScreenLeft * dpiX;
            double screenTop = SystemParameters.VirtualScreenTop * dpiY;
            double screenWidth = SystemParameters.VirtualScreenWidth * dpiX;
            double screenHeight = SystemParameters.VirtualScreenHeight * dpiY;

            using (Bitmap bmp = new Bitmap((int)screenWidth, (int)screenHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    //Temporarily hide window to avoid capturing it in the screenshot
                    Dispatcher.Invoke(() => Opacity = 0);

                    g.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, new System.Drawing.Size((int)screenWidth, (int)screenHeight));

                    //Restore the window visibility
                    Dispatcher.Invoke(() => Opacity = 1);

                    //Set up and show the SaveFileDialog using WPF's dialog
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "PNG Image|*.png",
                        Title = "Save Screenshot",
                        FileName = "Screenhot-" + DateTime.Now.ToString("ddMMyyyy-HHmmss") + ".png"
                    };

                    //Show the dialog and check if the user pressed OK
                    bool? result = Dispatcher.Invoke(() => saveFileDialog.ShowDialog());

                    if (result == true && !string.IsNullOrEmpty(saveFileDialog.FileName))
                    {
                        string filePath = saveFileDialog.FileName;

                        //Convert Bitmap to BitmapSource for WPF
                        BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            bmp.GetHbitmap(),
                            IntPtr.Zero,
                            System.Windows.Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());

                        //Save the captured screenshot
                        using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                            encoder.Save(fileStream);
                        }

                        // Inform the user
                        Dispatcher.Invoke(() => MessageBox.Show($"Your screenshot has been saved in the following path: {filePath}"));
                    }
                }
            }
        }
    }
}