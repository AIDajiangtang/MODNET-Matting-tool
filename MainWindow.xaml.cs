using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Matting
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Drawing.Color mBgColor = System.Drawing.Color.Transparent;//背景颜色
        OpenCvSharp.Size mOrgSize;//原始图像大小
        MattingAlg mMattingAlg;//抠图AI算法

        OpenCvSharp.Mat mBackground;//背景
        OpenCvSharp.Mat mForeGround;//前景
        Dispatcher UI;
        public MainWindow()
        {
            InitializeComponent();
            this.mMattingAlg = new MattingAlg();
            this.UI = Dispatcher.CurrentDispatcher;
        }
        /// <summary>
        /// 选择文件
        /// </summary>
        private void File_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menu = sender as System.Windows.Controls.MenuItem;

            //加载本地图像
            if (menu.Header.ToString() == "图像文件")
            {
                this.imageST.Visibility = Visibility.Visible;
                this.videoST.Visibility = Visibility.Hidden;
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "*.*|*.bmp;*.jpg;*.jpeg;*.tiff;*.tiff;*.png";
                if (ofd.ShowDialog() != true) return;

                BitmapImage bitmap = new BitmapImage(new Uri(ofd.FileName));
                this.ShowOrgImage(bitmap);
                Thread thread = new Thread(() =>
                {
                    OpenCvSharp.Mat image = OpenCvSharp.Cv2.ImRead(ofd.FileName, OpenCvSharp.ImreadModes.Color);
                    if (this.mForeGround != null) this.mForeGround.Dispose();
                    this.mForeGround = this.Matting(image);
                    this.ShowMatting(this.mForeGround);//显示抠图
                });
                thread.Start();

            }
            //截图
            else if (menu.Header.ToString() == "截图")
            {
                this.imageST.Visibility = Visibility.Visible;
                this.videoST.Visibility = Visibility.Hidden;
                this.Hide();
                System.Threading.Thread.Sleep(200);

                ScreenCapturer.ScreenCapturerTool screenCapturer = new ScreenCapturer.ScreenCapturerTool();
                if (screenCapturer.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.ShowOrgImage(this.ConvertToBitmapSource(screenCapturer.Image));//显示图像 
                    this.Matting(screenCapturer.Image);//抠图
                }
                this.Show();
            }
            //剪切板
            else if (menu.Header.ToString() == "剪切板")
            {
                this.imageST.Visibility = Visibility.Visible;
                this.videoST.Visibility = Visibility.Hidden;
                System.Drawing.Bitmap bitmap = this.GetClipboardImage();
                this.ShowOrgImage(this.ConvertToBitmapSource(bitmap));//显示图像 
                this.Matting(bitmap);//抠图
            }
            //视频
            else if (menu.Header.ToString() == "视频")
            {
                this.imageST.Visibility = Visibility.Hidden;
                this.videoST.Visibility = Visibility.Visible;

                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "*.*|*.mp4;*.avi";
                if (ofd.ShowDialog() != true) return;
                // 加载视频文件
                this.ShowOrgVideo(ofd.FileName);
                Thread thread = new Thread(() =>
                {
                    // 创建VideoCapture对象，打开视频文件
                    using (OpenCvSharp.VideoCapture capture = new OpenCvSharp.VideoCapture(ofd.FileName))
                    {
                        this.Matting(capture);
                    }
                });
                thread.Start();

            }
        }
        /// <summary>
        /// 获取剪切板截图
        /// </summary>
        /// <returns></returns>
        private System.Drawing.Bitmap GetClipboardImage()
        {
            System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Windows.Forms.Clipboard.GetImage();
            if (bmp == null)
            {
                string[] bmpFilters = new string[] { ".bmp", ".jpg", ".jpeg", ".tiff", ".tif", ".png" };
                var files = System.Windows.Forms.Clipboard.GetFileDropList();

                string[] Filtersarr = new string[files.Count];
                files.CopyTo(Filtersarr, 0);
                Filtersarr = Filtersarr.Where(x => bmpFilters.Contains(System.IO.Path.GetExtension(x).ToLower())).ToArray();
                if (Filtersarr.Length > 0)
                {
                    var imagebyte = File.ReadAllBytes(Filtersarr[0]);
                    bmp = new System.Drawing.Bitmap(new MemoryStream(imagebyte));
                }
            }
            return bmp;
        }
        /// <summary>
        /// Image转BitMapImage
        /// </summary>
        private BitmapSource ConvertToBitmapSource(System.Drawing.Image image)
        {
            using (System.IO.MemoryStream stream = new MemoryStream())
            {
                // 将图像保存到内存流中
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                // 创建BitmapSource
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(stream.ToArray());
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
        /// <summary>
        /// 选择背景
        /// </summary>
        private void BackGround_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menu = sender as System.Windows.Controls.MenuItem;
            //纯色背景
            if (menu.Header.ToString() == "背景颜色")
            {
                ColorDialog colorDialog = new ColorDialog();
                System.Drawing.Color bgcolor = this.mBgColor;
                colorDialog.Color = bgcolor;
                if (colorDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                if (this.mBackground != null) this.mBackground.Dispose();

                this.mBackground = this.GenerateBgMat(colorDialog.Color);//生成背景Mat
                this.ShowBackground(this.mBackground);//显示背景
            }
            //背景图像
            else if (menu.Header.ToString() == "背景图像")
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "*.*|*.bmp;*.jpg;*.jpeg;*.tiff;*.tiff;*.png"; ;
                if (ofd.ShowDialog() != true) return;
                if (this.mBackground != null) this.mBackground.Dispose();
                this.mBackground = this.GenerateBgMat(ofd.FileName);//生成背景Mat
                this.ShowBackground(this.mBackground);//显示背景       
            }

        }
        /// <summary>
        /// 保存
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menu = sender as System.Windows.Controls.MenuItem;
            String exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (menu.Header.ToString() == "保存整体")//背景+前景
            {
                // 创建融合图像的目标Mat对象
                OpenCvSharp.Mat blendedImage = new OpenCvSharp.Mat(this.mForeGround.Size(), OpenCvSharp.MatType.CV_8UC4);
                for (int i = 0; i < mForeGround.Cols; i++)
                {
                    for (int j = 0; j < mForeGround.Rows; j++)
                    {
                        int index = j * mForeGround.Cols + i;
                        if (mForeGround.At<OpenCvSharp.Vec4b>(j, i)[3] > 128)
                        {
                            blendedImage.At<OpenCvSharp.Vec4b>(j, i)[0] = mForeGround.At<OpenCvSharp.Vec4b>(j, i)[0];
                            blendedImage.At<OpenCvSharp.Vec4b>(j, i)[1] = mForeGround.At<OpenCvSharp.Vec4b>(j, i)[1];
                            blendedImage.At<OpenCvSharp.Vec4b>(j, i)[2] = mForeGround.At<OpenCvSharp.Vec4b>(j, i)[2];
                            blendedImage.At<OpenCvSharp.Vec4b>(j, i)[3] = mForeGround.At<OpenCvSharp.Vec4b>(j, i)[3];
                        }
                        else
                        {
                            blendedImage.At<OpenCvSharp.Vec4b>(j, i)[0] = this.mBackground.At<OpenCvSharp.Vec4b>(j, i)[0];
                            blendedImage.At<OpenCvSharp.Vec4b>(j, i)[1] = mBackground.At<OpenCvSharp.Vec4b>(j, i)[1];
                            blendedImage.At<OpenCvSharp.Vec4b>(j, i)[2] = mBackground.At<OpenCvSharp.Vec4b>(j, i)[2];
                            blendedImage.At<OpenCvSharp.Vec4b>(j, i)[3] = 255;
                        }
                    }
                }
                // 选择保存参数
                OpenCvSharp.ImageEncodingParam[] parameters = new OpenCvSharp.ImageEncodingParam[]
                {
                    new OpenCvSharp.ImageEncodingParam(OpenCvSharp.ImwriteFlags.PngCompression, 9) // 设置PNG压缩级别
                };
              // 保存融合后的图像
                OpenCvSharp.Cv2.ImWrite(exePath + "\\full.png", blendedImage, parameters);
            }
            else if (menu.Header.ToString() == "保存前景")//前景only
            {
                // 选择保存参数
                OpenCvSharp.ImageEncodingParam[] parameters = new OpenCvSharp.ImageEncodingParam[]
                {
                    new OpenCvSharp.ImageEncodingParam(OpenCvSharp.ImwriteFlags.PngCompression, 9) // 设置PNG压缩级别
                };
             // 保存融合后的图像
                OpenCvSharp.Cv2.ImWrite(exePath + "\\foreground.png", this.mForeGround, parameters);
            }

        }

        /// <summary>
        /// 生成纯色背景图
        /// </summary>
        OpenCvSharp.Mat GenerateBgMat(System.Drawing.Color bgcolor)
        {
            OpenCvSharp.Mat bgMat = new OpenCvSharp.Mat(this.mOrgSize, OpenCvSharp.MatType.CV_8UC3);
            for (int i = 0; i < bgMat.Cols; i++)
            {
                for (int j = 0; j < bgMat.Rows; j++)
                {
                    int index = j * bgMat.Cols + i;
                    bgMat.At<OpenCvSharp.Vec3b>(j, i)[0] = bgcolor.B;
                    bgMat.At<OpenCvSharp.Vec3b>(j, i)[1] = bgcolor.G;
                    bgMat.At<OpenCvSharp.Vec3b>(j, i)[2] = bgcolor.R;
                }
            }

            return bgMat;
        }
        /// <summary>
        /// 生成背景图像
        /// </summary>
        OpenCvSharp.Mat GenerateBgMat(string file)
        {
            OpenCvSharp.Mat image = OpenCvSharp.Cv2.ImRead(file, OpenCvSharp.ImreadModes.Color);
            OpenCvSharp.Size newSize = this.mOrgSize;

            OpenCvSharp.Mat bgMat = new OpenCvSharp.Mat();
            // Resize the image
            OpenCvSharp.Cv2.Resize(image, bgMat, newSize, 0, 0, OpenCvSharp.InterpolationFlags.Linear);

            return bgMat;
        }
        /// <summary>
        /// 显示原始视频
        /// </summary>
        /// <param name="videopath"></param>
        void ShowOrgVideo(string videopath)
        {
            // 加载视频文件
            this.mOrgVideo.Source = new Uri(videopath);
            this.mOrgVideo.Play();
        }
        /// <summary>
        /// 显示原始图像
        /// </summary>
        void ShowOrgImage(BitmapSource img)
        {
            this.mImage.Source = img;//显示图像 
        }
        /// <summary>
        /// 对视频抠图
        /// </summary>
        void Matting(OpenCvSharp.VideoCapture video)
        {
            // 检查视频是否成功打开
            if (!video.IsOpened())
            {
                Console.WriteLine("无法打开视频文件.");
                return;
            }

            string exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // 设置视频文件保存路径和参数
            string videoFilePath = exePath + "\\path_to_save_video.mp4"; // 替换为你希望保存的视频文件路径
            OpenCvSharp.FourCC fourCC = OpenCvSharp.FourCC.MP4V;  // 视频编码器的FourCC码
            int fps = 2; // 视频的帧率
            // 创建VideoWriter对象
            using (OpenCvSharp.VideoWriter writer = new OpenCvSharp.VideoWriter(videoFilePath, fourCC, fps, new OpenCvSharp.Size(video.FrameWidth, video.FrameHeight)))
            {
                // 循环读取视频帧
                while (true)
                {
                    // 读取下一帧
                    OpenCvSharp.Mat frame = new OpenCvSharp.Mat();
                    if (!video.Read(frame))
                        break;
                    video.PosFrames += (int)video.Fps / fps;
                    OpenCvSharp.Mat matting = this.MattingVideo(frame);

                    // 写入当前帧到视频文件
                    writer.Write(matting);

                    // 释放当前帧的资源
                    matting.Dispose();

                    // 释放当前帧的资源
                    frame.Dispose();

                }
            }
            UI.Invoke(new Action(delegate
            {
                System.Windows.MessageBox.Show("Matting Video finished!");
                this.mMattingVideo.Source = new Uri(videoFilePath);
                this.mMattingVideo.Play();

            }));

        }
        /// <summary>
        /// 抠图
        /// </summary>
        void Matting(System.Drawing.Image img)
        {                // 将图像转换为Bitmap
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(img);
            // 获取图像的宽度和高度
            int width = bitmap.Width;
            int height = bitmap.Height;

            OpenCvSharp.Mat image = new OpenCvSharp.Mat(new OpenCvSharp.Size(width, height), OpenCvSharp.MatType.CV_8UC3);
            // 遍历图像的像素
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 获取像素的颜色
                    System.Drawing.Color color = bitmap.GetPixel(x, y);
                    image.At<OpenCvSharp.Vec3b>(y, x)[0] = color.B;
                    image.At<OpenCvSharp.Vec3b>(y, x)[1] = color.G;
                    image.At<OpenCvSharp.Vec3b>(y, x)[2] = color.R;
                }
            }
            // 释放资源
            bitmap.Dispose();

            Thread thread = new Thread(() =>
            {
                if (this.mForeGround != null) this.mForeGround.Dispose();
                this.mForeGround = this.Matting(image);
                this.ShowMatting(this.mForeGround);//显示抠图
            });
            thread.Start();
        }

        /// <summary>
        /// 抠图
        /// </summary>
        OpenCvSharp.Mat Matting(OpenCvSharp.Mat image)
        {

            this.mOrgSize = image.Size();
            // Define the new size for the resized image
            int min = Math.Min(image.Rows, image.Cols);
            float scale = 512.0f / min;
            int w = (int)(image.Cols * scale);
            int h = (int)(image.Rows * scale);
            int rw = w - w % 32;
            int rh = h - h % 32;
            OpenCvSharp.Size mnewSize = new OpenCvSharp.Size(rw, rh);

            // Create a new Mat object to store the resized image
            OpenCvSharp.Mat resizedImage = new OpenCvSharp.Mat();

            // Resize the image
            OpenCvSharp.Cv2.Resize(image, resizedImage, mnewSize, 0, 0, OpenCvSharp.InterpolationFlags.Linear);
            OpenCvSharp.Mat alpha = this.mMattingAlg.Seg(resizedImage);

            OpenCvSharp.Mat alpharesize = new OpenCvSharp.Mat();
            OpenCvSharp.Cv2.Resize(alpha, alpharesize, new OpenCvSharp.Size(image.Cols, image.Rows), 0, 0, OpenCvSharp.InterpolationFlags.Linear);

            alpha.Dispose();

            return this.GenerateForeGround(image, alpharesize);
        }
        /// <summary>
        /// 抠图
        /// </summary>
        OpenCvSharp.Mat MattingVideo(OpenCvSharp.Mat image)
        {

            this.mOrgSize = image.Size();
            // Define the new size for the resized image
            int min = Math.Min(image.Rows, image.Cols);
            float scale = 512.0f / min;
            int w = (int)(image.Cols * scale);
            int h = (int)(image.Rows * scale);
            int rw = w - w % 32;
            int rh = h - h % 32;
            OpenCvSharp.Size mnewSize = new OpenCvSharp.Size(rw, rh);

            // Create a new Mat object to store the resized image
            OpenCvSharp.Mat resizedImage = new OpenCvSharp.Mat();

            // Resize the image
            OpenCvSharp.Cv2.Resize(image, resizedImage, mnewSize, 0, 0, OpenCvSharp.InterpolationFlags.Linear);
            OpenCvSharp.Mat alpha = this.mMattingAlg.Seg(resizedImage);

            OpenCvSharp.Mat alpharesize = new OpenCvSharp.Mat();
            OpenCvSharp.Cv2.Resize(alpha, alpharesize, new OpenCvSharp.Size(image.Cols, image.Rows), 0, 0, OpenCvSharp.InterpolationFlags.Linear);

            alpha.Dispose();

            OpenCvSharp.Mat forground = new OpenCvSharp.Mat(image.Size(), OpenCvSharp.MatType.CV_8UC3);
            for (int y = 0; y < image.Rows; y++)
            {
                for (int x = 0; x < image.Cols; x++)
                {
                    float al = (byte)(255 * alpharesize.At<float>(y, x));
                    int ind = y * image.Cols + x;
                    if (al > 128)
                    {
                        forground.At<OpenCvSharp.Vec4b>(y, x)[0] = image.At<OpenCvSharp.Vec3b>(y, x)[0];  // Blue
                        forground.At<OpenCvSharp.Vec4b>(y, x)[1] = image.At<OpenCvSharp.Vec3b>(y, x)[1];  // Green
                        forground.At<OpenCvSharp.Vec4b>(y, x)[2] = image.At<OpenCvSharp.Vec3b>(y, x)[2]; // Red
                    }
                    else
                    {
                        forground.At<OpenCvSharp.Vec4b>(y, x)[0] = 0;  // Blue
                        forground.At<OpenCvSharp.Vec4b>(y, x)[1] = 0;  // Green
                        forground.At<OpenCvSharp.Vec4b>(y, x)[2] = 0; // Red
                    }

                }
            }

            return forground;

            return this.GenerateForeGround(image, alpharesize);
        }
        /// <summary>
        /// 生成前景图像
        /// </summary>
        /// <param name="orgImg">原始图像</param>
        /// <param name="Alpha">Alpha</param>
        /// <returns></returns>
        OpenCvSharp.Mat GenerateForeGround(OpenCvSharp.Mat orgImg, OpenCvSharp.Mat Alpha)
        {

            OpenCvSharp.Mat forground = new OpenCvSharp.Mat(orgImg.Size(), OpenCvSharp.MatType.CV_8UC4);
            for (int y = 0; y < orgImg.Rows; y++)
            {
                for (int x = 0; x < orgImg.Cols; x++)
                {
                    int ind = y * orgImg.Cols + x;
                    forground.At<OpenCvSharp.Vec4b>(y, x)[0] = orgImg.At<OpenCvSharp.Vec3b>(y, x)[0];  // Blue
                    forground.At<OpenCvSharp.Vec4b>(y, x)[1] = orgImg.At<OpenCvSharp.Vec3b>(y, x)[1];  // Green
                    forground.At<OpenCvSharp.Vec4b>(y, x)[2] = orgImg.At<OpenCvSharp.Vec3b>(y, x)[2];  // Red
                    forground.At<OpenCvSharp.Vec4b>(y, x)[3] = (byte)(255 * Alpha.At<float>(y, x));  // Alpha
                }
            }

            return forground;
        }
        /// <summary>
        /// 显示背景
        /// </summary>
        void ShowBackground(OpenCvSharp.Mat bg)
        {
            UI.Invoke(new Action(delegate
            {
                WriteableBitmap bp = new WriteableBitmap(bg.Cols, bg.Rows, 96, 96, PixelFormats.Bgra32, null);
                // 设置像素数据，将所有像素的透明度设置为半透明
                byte[] pixelData = new byte[bg.Cols * bg.Rows * 4];
                Array.Clear(pixelData, 0, pixelData.Length);
                for (int y = 0; y < bg.Rows; y++)
                {
                    for (int x = 0; x < bg.Cols; x++)
                    {
                        int ind = y * bg.Cols + x;
                        pixelData[4 * ind] = bg.At<OpenCvSharp.Vec3b>(y, x)[0];  // Blue
                        pixelData[4 * ind + 1] = bg.At<OpenCvSharp.Vec3b>(y, x)[1];  // Green
                        pixelData[4 * ind + 2] = bg.At<OpenCvSharp.Vec3b>(y, x)[2];  // Red
                        pixelData[4 * ind + 3] = 255;  // Alpha

                    }
                }

                bp.WritePixels(new Int32Rect(0, 0, bg.Cols, bg.Rows), pixelData, bg.Cols * 4, 0);
                // 创建一个BitmapImage对象，将WriteableBitmap作为源
                this.mBg.Source = bp;

            }));
        }
        /// <summary>
        /// 显示前景
        /// </summary>
        void ShowMatting(OpenCvSharp.Mat forground)
        {
            UI.Invoke(new Action(delegate
            {
                WriteableBitmap bp = new WriteableBitmap(forground.Cols, forground.Rows, 96, 96, PixelFormats.Bgra32, null);
                // 设置像素数据，将所有像素的透明度设置为半透明
                byte[] pixelData = new byte[forground.Cols * forground.Rows * 4];
                Array.Clear(pixelData, 0, pixelData.Length);
                for (int y = 0; y < forground.Rows; y++)
                {
                    for (int x = 0; x < forground.Cols; x++)
                    {
                        int ind = y * forground.Cols + x;
                        pixelData[4 * ind] = forground.At<OpenCvSharp.Vec4b>(y, x)[0];  // Blue
                        pixelData[4 * ind + 1] = forground.At<OpenCvSharp.Vec4b>(y, x)[1];  // Green
                        pixelData[4 * ind + 2] = forground.At<OpenCvSharp.Vec4b>(y, x)[2];  // Red
                        pixelData[4 * ind + 3] = forground.At<OpenCvSharp.Vec4b>(y, x)[3];    // Alpha
                    }
                }

                bp.WritePixels(new Int32Rect(0, 0, forground.Cols, forground.Rows), pixelData, forground.Cols * 4, 0);
                // 创建一个BitmapImage对象，将WriteableBitmap作为源
                this.mMatting.Source = bp;

            }));

        }
    }
}
