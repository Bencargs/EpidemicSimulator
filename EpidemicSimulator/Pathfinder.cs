using EpidemicSimulator.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EpidemicSimulator
{
    public class Pathfinder
    {
		private TrafficNode[] _trafficNodes;

		public Pathfinder(TrafficNode[] trafficNodes)
		{
			_trafficNodes = trafficNodes;
		}

		public Person[] GetCollisions(Person person, Person[] people)
		{
			return people.Where(p => HasCollided(person, p.Location)).ToArray();
		}

		public void Navigate(Person person)
		{
			if (HasCollided(person, person.Residence))
				person.Destination = GetPath(person.Location, person.WorkPlace);
			else if (HasCollided(person, person.WorkPlace))
				person.Destination = GetPath(person.Location, person.Residence);

			Move(person, person.Destination[0].Location);
			if (HasCollided(person, person.Destination[0].Location))
				person.Destination.RemoveAt(0);
		}

		public bool HasCollided(Person person, Point location)
		{
			// via bounding circle -
			var dx = Math.Pow(location.X - person.Location.X, 2);
			var dy = Math.Pow(location.Y - person.Location.Y, 2);
			var r = Math.Sqrt(dx + dy);
			return r < person.CollisionRadius;
		}

		private void Move(Person person, Point end)
		{
			// via Bresenham's algorithm -
			var start = person.Location;
			int dx = Math.Abs(end.X - start.X), sx = start.X < end.X ? 1 : -1;
			int dy = Math.Abs(end.Y - start.Y), sy = start.Y < end.Y ? 1 : -1;
			int err = (dx > dy ? dx : -dy) / 2, e2;

			e2 = err;
			if (e2 > -dx)
			{
				err -= dy;
				start.X += sx;
			}
			if (e2 < dy)
			{
				err += dx;
				start.Y += sy;
			}
			person.Location = start;
		}

		private List<MapNode> GetPath(Point source, Point destination)
		{
			var start = _trafficNodes.OrderBy(p => GetDistance(p.Location, source)).First();
			var end = _trafficNodes.OrderBy(p => GetDistance(p.Location, destination)).First();

			// breath first search of potential nodes
			var searched = new List<TrafficNode>();
			var potentials = new Queue<TrafficNode>(new[] { start });
			TrafficNode current = null;
			do
			{
				current = potentials.Dequeue();
				var children = current.Connections.Select(i => _trafficNodes[i]);
				foreach (var c in children)
				{
					if (searched.Contains(c))
						continue;

					c.Previous = current;
					searched.Add(c);
					potentials.Enqueue(c);
				}
			}
			while (potentials.Any() && current != end);

			// backwards iterate to find the shortest path
			var path = new List<MapNode>(new[] { current });
			while (current.Previous != null && !path.Contains(current.Previous))
			{
				path.Insert(0, current.Previous);
				current = current.Previous;
			}
			path.OrderBy(p => GetDistance(p.Location, destination));
			path.Add(new TrafficNode(destination.X, destination.Y));

			return path;
		}

		private double GetDistance(Point source, Point dest)
		{
			// euclidean distance -
			var xDistance = Math.Pow(dest.X - source.X, 2);
			var yDistance = Math.Pow(dest.Y - source.Y, 2);
			return Math.Sqrt(xDistance + yDistance);
		}
	}
}
