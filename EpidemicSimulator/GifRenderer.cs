using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EpidemicSimulator
{
    public class GifRenderer : IRenderer, IDisposable
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private readonly GifBitmapEncoder _encoder = new GifBitmapEncoder();

        public void OnRenderUpdated(object _, Bitmap image)
        {
            var bmp = image.GetHbitmap();
            var src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmp,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            _encoder.Frames.Add(BitmapFrame.Create(src));
            DeleteObject(bmp); // fixes a windows memory leak
        }

        public void Dispose()
        {
            var outputPath = @"C:\Source\EpidemicSimulator\EpidemicSimulator\Output\output.gif";
            File.Delete(outputPath);
            using (FileStream fs = new FileStream(outputPath, FileMode.Create))
            {
                _encoder.Save(fs);
            }
        }
    }
}
