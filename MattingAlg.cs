using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Matting
{
    /// <summary>
    /// 人像抠图算法
    /// </summary>
    class MattingAlg
    {
        InferenceSession mSession;
        public bool mReady = false;
        public MattingAlg()
        {
            Thread thread = new Thread(() =>
            {
                this.LoadONNXModel();
                this.mReady = true;
            });
            thread.Start();

        }

        void LoadONNXModel()
        {
            if (this.mSession != null)
                this.mSession.Dispose();

            string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string model_path = Path.Combine(exePath, "Model", "MODNET.onnx");
            if (!File.Exists(model_path))
            {
                MessageBox.Show(model_path + " not exist!");
                return;
            }

            this.mSession = new InferenceSession(model_path);
        }
        /// <summary>
        /// 分割人像
        /// </summary>
        /// <param name="image">输入RGB图像</param>
        /// <returns>alpha</returns>
        public OpenCvSharp.Mat Seg(OpenCvSharp.Mat image)
        {

            OpenCvSharp.Mat floatImage = new OpenCvSharp.Mat();
            image.ConvertTo(floatImage, OpenCvSharp.MatType.CV_32FC3);
            // 将像素值归一化到0到255的范围内
            float[] transformedImg = new float[3 * image.Cols * image.Rows];
            for (int i = 0; i < image.Cols; i++)
            {
                for (int j = 0; j < image.Rows; j++)
                {
                    int index = j * image.Cols + i;
                    transformedImg[index] = (floatImage.At<OpenCvSharp.Vec3f>(j, i)[2] / 255.0f - 0.5f) / 0.5f;
                    transformedImg[image.Cols * image.Rows + index] = (floatImage.At<OpenCvSharp.Vec3f>(j, i)[1] / 255.0f - 0.5f) / 0.5f;
                    transformedImg[2 * image.Cols * image.Rows + index] = (floatImage.At<OpenCvSharp.Vec3f>(j, i)[0] / 255.0f - 0.5f) / 0.5f;
                }
            }

            var tensor = new DenseTensor<float>(transformedImg, new[] { 1, 3, image.Rows, image.Cols });
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("img", tensor)
            };

            var results = this.mSession.Run(inputs);
            var alphaArray = results.First().AsTensor<float>().ToArray();
            OpenCvSharp.Mat alpha = new OpenCvSharp.Mat(new OpenCvSharp.Size(image.Cols, image.Rows), OpenCvSharp.MatType.CV_32FC1);
            for (int i = 0; i < image.Cols; i++)
            {
                for (int j = 0; j < image.Rows; j++)
                {
                    int index = j * image.Cols + i;
                    float a = alphaArray[index];
                    alpha.At<float>(j, i) = a;
                }
            }
            return alpha;

        }
    }
}
