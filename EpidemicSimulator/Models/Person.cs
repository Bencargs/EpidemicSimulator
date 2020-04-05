using System.Collections.Generic;
using System.Drawing;

namespace EpidemicSimulator.Models
{
	public class Person
	{
		public int Id { get; set; }
		public State State { get; set; }
		public Point Location { get; set; }
		public List<MapNode> Destination { get; set; }
		public Point Residence { get; set; }
		public Point WorkPlace { get; set; }
		public int DaysInfected { get; set; }
		public int CollisionRadius { get; set; } = 4;
	}
}
