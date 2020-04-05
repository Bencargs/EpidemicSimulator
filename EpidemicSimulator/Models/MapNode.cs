using System.Drawing;

namespace EpidemicSimulator.Models
{
	public class MapNode
	{
		public int Id { get; set; }
		public Point Location { get; set; }

		public MapNode(int x, int y)
		{
			Location = new Point(x, y);
		}
	}
}
