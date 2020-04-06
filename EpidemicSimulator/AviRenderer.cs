using SharpAvi.Codecs;
using SharpAvi.Output;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace EpidemicSimulator
{
    public class AviRenderer : IRenderer, IDisposable
    {
        private readonly AviWriter _writer;
        private IAviVideoStream _videoStream;

        public AviRenderer()
        {
            var filepath = @"C:\Source\EpidemicSimulator\EpidemicSimulator\Output\output.mpeg4";
            File.Delete(filepath);
            _writer = new AviWriter(filepath)
            {
                FramesPerSecond = 10,
                EmitIndex1 = true,
            };
        }

        private void CreateVideoStream(int width, int height)
        {
            var quality = 70;
            var screenWidth = width;
            var screenHeight = height;

            _videoStream = _writer.AddMotionJpegVideoStream(screenWidth, screenHeight, quality);
            _videoStream.Name = "Epidemic Simulator";
        }

        public void OnRenderUpdated(object _, Bitmap image)
        {
            if (_videoStream == null)
                CreateVideoStream(image.Width, image.Height);

            var buffer = new byte[image.Width * image.Height * 4];
            CopyToBuffer(image, buffer);

            var isKeyrame = _videoStream.FramesWritten % 24 == 0;
            _videoStream.WriteFrame(isKeyrame, buffer, 0, buffer.Length);
        }

        private byte[] CopyToBuffer(Bitmap image, byte[] buffer)
        {
            using (var graphics = Graphics.FromImage(image))
            {
                var bits = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
                image.UnlockBits(bits);
                return buffer;
            }
        }

        public void Dispose()
        {
            _writer.Close();
        }
    }
}
