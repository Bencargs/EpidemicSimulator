using System;
using System.Collections.Generic;

namespace EpidemicSimulator.Models
{
	public class TrafficNode : MapNode
	{
		public TrafficNode Previous { get; set; }
		public List<int> Connections { get; set; } = new List<int>();

		public TrafficNode(int x, int y)
			: base(x, y)
		{ }

		internal object OrderBy(Func<object, double> p)
		{
			throw new NotImplementedException();
		}
	}
}
