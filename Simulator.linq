<Query Kind="Program">
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>System.Drawing</Namespace>
</Query>

void Main()
{
	var simulator = new Simulator();
	//simulator.GenerateRouteMap();
	//simulator.GenerateTrafficNodes();
	//simulator.GenerateLandmarkNodes();

	var person = new Person { Location = new Point(10, 10), Destination = new Point(212, 150) };
	simulator.Test(person);
}

public class Simulator
{
	public void Test(Person person)
	{
		using (var image = new Bitmap(@"C:\Source\EpidemicSimulator\Map.png"))
		{
			var dc = new DumpContainer(image).Dump();
		
			var path = GetPath(person.Location, person.Destination);	
			while (path.Any() && person.Location != path.First().Location)
			{			
				Move2(person, path[0].Location);
				if (person.Location == path.First().Location)
					path.RemoveAt(0);

				image.SetPixel(person.Location.X, person.Location.Y, Color.Purple);
				dc.Refresh();
			}
		}
	}

	public void Move2(Person person, Point end)
	{
		// Via Bresenham's algorithm
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

	private void Move(Person person, Point destination)
	{
		if (person.Location.X < destination.X)
		{
			person.Location = new Point(person.Location.X + 1, person.Location.Y);
		}
		else if (person.Location.X > destination.X)
		{
			person.Location = new Point(person.Location.X - 1, person.Location.Y);
		}
		if (person.Location.Y < destination.Y)
		{
			person.Location = new Point(person.Location.X, person.Location.Y + 1);
		}
		else if (person.Location.Y > destination.Y)
		{
			person.Location = new Point(person.Location.X, person.Location.Y - 1);
		}
	}

	public List<MapNode> GetPath(Point source, Point destination)
	{
		var json = File.ReadAllText(@"C:\Source\EpidemicSimulator\TrafficNodes.json");
		var nodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MapNode>>(json);
		
		// Temp pending solving pathfinding
		return new List<MapNode>
		{
			nodes[119],
			nodes[202],
			nodes[180],
			nodes[134],
			nodes[72],
			nodes[73],
			nodes[175],
			nodes[194],
			nodes[217],
			nodes[219],
			nodes[220],
			nodes[227],
			nodes[228],
			nodes[269],
		};
		
		var start = nodes
		.Where(x => x.Connections.Any())
		.OrderBy(n => GetDistance(n.Location, source)).First();
		
		var end = nodes
		.Where(x => x.Connections.Any())
		.OrderBy(n => GetDistance(n.Location, destination)).First();
		
		nodes.Remove(start);
		var path = new List<MapNode>(new[] { start });
		
		while (path.Last() != end && nodes.Any())
		{
			var next = nodes
			.Where(x => x.Connections.Any())
			.OrderBy(n => GetDistance(path.Last().Location, end.Location)).First();
			
			path.Add(next);
			nodes.Remove(next);
		}
		return path;
	}
	
	public double GetDistance(Point source, Point dest)
	{
		//// Manhattan Distance for performance sake
		//var xDistance = Math.Abs(source.X - dest.X);
		//var yDistance = Math.Abs(source.Y - dest.Y);
		//return xDistance + yDistance;
		
		// Euclidean distance for correctness
		var xDistance = Math.Pow(dest.X - source.X, 2);
		var yDistance = Math.Pow(dest.Y - source.Y, 2);
		return Math.Sqrt(xDistance + yDistance);
	}
	
	public void GenerateLandmarkNodes()
	{
		var residential = new List<MapNode>();
		var commercial = new List<MapNode>();
		var hospitals = new List<MapNode>();
		var shoppingCenters = new List<MapNode>();
		var trafficNodes = new List<MapNode>();

		using (var image = new Bitmap(@"C:\Source\EpidemicSimulator\LandmarkNodes.png"))
		{
			var id = 0;
			for (int x = 0; x < image.Width; x++)
				for (int y = 0; y < image.Height; y++)
				{
					var pixel = image.GetPixel(x, y);
					if (pixel == Color.FromArgb(0, 255, 0))
					{
						residential.Add(new MapNode(x, y) { Id = id++ });
					}
					else if (pixel == Color.FromArgb(0, 0, 255))
					{
						commercial.Add(new MapNode(x, y) { Id = id++ });
					}
					else if (pixel == Color.FromArgb(255, 0, 0))
					{
						hospitals.Add(new MapNode(x, y) { Id = id++ });
					}
					else if (pixel == Color.FromArgb(0, 255, 255))
					{
						shoppingCenters.Add(new MapNode(x, y) { Id = id++} );
					}
					else if (image.GetPixel(x, y) == Color.FromArgb(255, 0, 255))
					{
						trafficNodes.Add(new MapNode(x, y) { Id = id++ });
					}
				}
			
			SaveJson(residential, "Residential");
			SaveJson(commercial, "Commercial");
			SaveJson(hospitals, "Hospitals");
			SaveJson(shoppingCenters, "ShoppingCenters");
			SaveJson(trafficNodes, "TrafficNodes");
		}
	}
	
	private MapNode[] LoadJson(string filename)
	{
		var json = File.ReadAllText(@$"C:\Source\EpidemicSimulator\{filename}.json");
		var nodes = Newtonsoft.Json.JsonConvert.DeserializeObject<MapNode[]>(json);
		return nodes;
	}
	
	private void SaveJson(List<MapNode> source, string filename)
	{
		var json = Newtonsoft.Json.JsonConvert.SerializeObject(source);
		File.WriteAllText(@$"C:\Source\EpidemicSimulator\{filename}.json", json);
	}
	
	public void GenerateRouteMap()
	{
		using (var image = new Bitmap(@"C:\Source\EpidemicSimulator\Map.png"))
		using (var output = new Bitmap(image.Width, image.Height))
		{
			for (int x = 0; x < image.Width; x++)
				for (int y = 0; y < image.Height; y++)
				{
					var pixel = image.GetPixel(x, y);
					if (pixel == Color.FromArgb(255, 255, 255))
					{	//Road
						output.SetPixel(x, y, Color.Blue);
					}
					else if (pixel.R >= 250 && (pixel.G >= 230 && pixel.G <= 245) && (pixel.B >= 150 && pixel.B <= 180))
					{	//Freeway
						output.SetPixel(x, y, Color.Yellow);
					}
				}
			output.Dump();
		}
	}
}

//public class Point
//{
//	public int X {get; set;}
//	public int Y {get; set;}
//	
//	public Point(int x, int y)
//	{
//		X = x;
//		Y = y;
//	}
//}

public class Person
{
	public int Id {get; set;}
	public State State {get; set;}
	public Point Location {get; set;}
	public Point Destination {get; set;}
}

public enum State
{
	Suspeptible,
	Infected,
	Recovered,
	Dead
}

public class MapNode
{
	public int Id {get; set;}
	public Point Location {get; set; }
	public List<int> Connections {get; set;} = new List<int>();
	
	public MapNode(int x, int y)
	{
		Location = new Point(x, y);
	}
}