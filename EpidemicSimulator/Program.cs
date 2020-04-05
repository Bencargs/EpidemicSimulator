using System;

namespace EpidemicSimulator
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var renderer = new GifRenderer())
            using (var simulator = new Simulator())
            {
                simulator.RenderUpdated += renderer.OnRenderUpdated;
                simulator.Initialise();
                simulator.Render();
            }
        }
    }
}
