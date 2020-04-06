using System.Drawing;

namespace EpidemicSimulator
{
    public interface IRenderer
    {
        void OnRenderUpdated(object _, Bitmap image);
    }
}
