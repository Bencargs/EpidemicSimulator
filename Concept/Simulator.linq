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
	
	//simulator.DrawPathNodes(person.Location, person.WorkPlace);
	//simulator.DrawNavigation(person, person.WorkPlace);
	//simulator.DrawNavigation2(person);
	
	var disease = new Disease
	{
		Infectivity = 25,
		SymptomaticDays = 100,
		FatalityRate = 1,
		RecoveryRate = 1
	};
	//var person1 = new Person { Location = new Point(100, 100), Residence = new Point (100, 100), WorkPlace = new Point(200, 100), State = State.Infected };
	//var person2 = new Person { Location = new Point(200, 100), Residence = new Point (200, 100), WorkPlace = new Point(100, 100) };
	//simulator.Simulate(new[] { person1, person2 }, disease);
	
	var population = simulator.Initialise();
	simulator.Simulate(population, disease);	
}

public class Simulator
{
	public void Simulate(Person[] people, Disease disease)
	{
		using (var original = new Bitmap(@"C:\Source\EpidemicSimulator\Map.png"))
		{
			var dc = new DumpContainer().Dump();
			while (!IsComplete(people))
			{
				using (var image = new Bitmap(original))
				using (var gfx = Graphics.FromImage(image))
				{
					dc.Content = image;
					foreach (var person in people)
					{
						DrawPerson(person, gfx);
						dc.Refresh();
						
						if (person.State == State.Dead)
							continue;

						if (person.State == State.Infected)
							EvaluateInfection(disease, person, people);

						Navigate(person);
					}
				}
			}
		}
	}
	
	public void DrawPerson(Person person, Graphics graphics)
	{
		var colour = GetColour(person);
		graphics.DrawEllipse(new Pen(new SolidBrush(Color.Black)), person.Location.X, person.Location.Y, 4, 4);
		graphics.FillEllipse(new SolidBrush(colour), person.Location.X, person.Location.Y, 4, 4);
	}
	
	public void EvaluateInfection(Disease disease, Person person, Person[] people)
	{
		foreach (var collision in GetCollisions(person, people))
		{
			if (collision.State == State.Suspeptible && Randomness.Percent() < disease.Infectivity)
				collision.State = State.Infected;
		}

		if (person.DaysInfected++ > disease.SymptomaticDays)
		{
			if (Randomness.Percent() < disease.FatalityRate)
				person.State = State.Dead;
			else if (Randomness.Percent() < disease.RecoveryRate)
				person.State = State.Recovered;
		}
	}

	public bool IsComplete(Person[] people)
	{
		var extinction = people.All(p => p.State == State.Dead);
		var eradication = !people.Any(p => p.State == State.Infected);
		return extinction || eradication;
	}
	
	public Color GetColour(Person person)
	{
		switch (person.State)
		{
			case State.Dead:
				return Color.Black;
			case State.Infected:
				return Color.Purple;
			case State.Recovered:
				return Color.Blue;
			default:
				return Color.Green;
		}
	}

	public void Navigate(Person person)
	{
		if (person.HasCollided(person.Residence))
			person.Destination = GetPath2(person.Location, person.WorkPlace);
		else if (person.HasCollided(person.WorkPlace))
			person.Destination = GetPath2(person.Location, person.Residence);

		Move2(person, person.Destination[0].Location);
		if (person.HasCollided(person.Destination[0].Location))
			person.Destination.RemoveAt(0);
	}
	
	public void DrawNavigation2(Person person)
	{
		using (var image = new Bitmap(@"C:\Source\EpidemicSimulator\Map.png"))
		{
			var dc = new DumpContainer(image).Dump();
			for (int i = 0; i < 1000; i++)
			{
				Navigate(person);
				image.SetPixel(person.Location.X, person.Location.Y, Color.Purple);
				dc.Refresh();
			}
		}
	}

	public Person[] GetCollisions(Person person, Person[] people)
	{
		return people.Where(p => person.HasCollided(p.Location)).ToArray();
	}
	
	public Person[] Initialise()
	{
		var suseptible =  Enumerable.Range(0, 1000).Select(x =>
		{
			var residence = GetRandomResidence();
			return new Person
			{
				Residence = residence,
				Location = residence,
				WorkPlace = GetRandomBusiness()
			};
		});
		
		var infected = Enumerable.Range(0, 10).Select(x =>
		{
			var residence = GetRandomResidence();
			return new Person
			{
				Residence = residence,
				Location = residence,
				WorkPlace = GetRandomBusiness(),
				State = State.Infected
			};
		});
		
		return suseptible.Concat(infected).ToArray();
	}
	
	//public Person CreatePerson()
	//{
	//	var residence = GetRandomResidence();
	//	var person = new Person
	//	{
	//		State = State.Suspeptible,
	//		Location = residence,
	//		Residence = residence,
	//		WorkPlace = GetRandomBusiness()
	//	};
	//	
	//	return person;
	//}
	
	public Point GetRandomBusiness()
	{
		var json = File.ReadAllText(@"C:\Source\EpidemicSimulator\Commercial.json");
		var nodes = Newtonsoft.Json.JsonConvert.DeserializeObject<MapNode[]>(json);
		var business = Randomness.Next(nodes);
		return business.Location;
	}

	public Point GetRandomResidence()
	{
		var json = File.ReadAllText(@"C:\Source\EpidemicSimulator\Residential.json");
		var nodes = Newtonsoft.Json.JsonConvert.DeserializeObject<MapNode[]>(json);
		var residence = Randomness.Next(nodes);
		return residence.Location;
	}

	public void DrawNavigation(Person person, Point destination)
	{
		using (var image = new Bitmap(@"C:\Source\EpidemicSimulator\Map.png"))
		{
			var dc = new DumpContainer(image).Dump();
		
			var path = GetPath2(person.Location, destination);	
			while (path.Any() && person.Location != path.First().Location)
			{			
				Move2(person, path[0].Location);
				if (person.HasCollided(path.First().Location))
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
	
	public void DrawPathNodes(Point source, Point destination)
	{
		var path = GetPath2(source, destination);
		
		using (var image = new Bitmap(@"C:\Source\EpidemicSimulator\Map.png"))
		using (var gfx = Graphics.FromImage(image))
		{
			var dc = new DumpContainer(image).Dump();
			foreach (var p in path)
			{
				gfx.DrawEllipse(new Pen(new SolidBrush(Color.Blue)), p.Location.X, p.Location.Y, 4, 4);
				dc.Refresh();
				Thread.Sleep(100);
			}
		}
	}

	public List<MapNode> GetPath2(Point source, Point destination)
	{
		var json = File.ReadAllText(@"C:\Source\EpidemicSimulator\TrafficNodes.json");
		var nodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MapNode>>(json);

		var start = nodes.OrderBy(p => GetDistance(p.Location, source)).First();
		var end = nodes.OrderBy(p => GetDistance(p.Location, destination)).First();

		// breath first search of potential nodes
		var searched = new List<MapNode>();
		var potentials = new Queue<MapNode>(new[] { start });
		MapNode current = null;
		do
		{
			var previous = current;
			current = potentials.Dequeue();
			if (searched.Contains(current))
				continue;
				
			current.Previous = previous;
			searched.Add(current);
			
			var connections = current.Connections.Select(i => nodes[i]);
			foreach (var next in connections)
				potentials.Enqueue(next);
		}
		while (potentials.Any() && current != end);

		// backwards iterate to find the shortest path
		var path = new List<MapNode>(new[] {current});
		while (current.Previous != null)
		{
			path.Insert(0, current.Previous);
			current = current.Previous;
		}
		path.OrderBy(p => GetDistance(p.Location, destination));
		path.Add(new MapNode(destination.X, destination.Y));
		
		return path;
	}

	public List<MapNode> GetPath(Point source, Point destination)
	{
		var json = File.ReadAllText(@"C:\Source\EpidemicSimulator\TrafficNodes.json");
		var nodes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MapNode>>(json);
		
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
						residential.Add(new MapNode(x, y) { Id = (residential.LastOrDefault()?.Id + 1) ?? 0 });
					}
					else if (pixel == Color.FromArgb(0, 0, 255))
					{
						commercial.Add(new MapNode(x, y) { Id = (commercial.LastOrDefault()?.Id + 1) ?? 0 });
					}
					else if (pixel == Color.FromArgb(255, 0, 0))
					{
						hospitals.Add(new MapNode(x, y) { Id = (hospitals.LastOrDefault()?.Id + 1) ?? 0 });
					}
					else if (pixel == Color.FromArgb(0, 255, 255))
					{
						shoppingCenters.Add(new MapNode(x, y) { Id = (shoppingCenters.LastOrDefault()?.Id + 1) ?? 0 });
					}
					else if (image.GetPixel(x, y) == Color.FromArgb(255, 0, 255))
					{
						trafficNodes.Add(new MapNode(x, y) { Id = (trafficNodes.LastOrDefault()?.Id + 1) ?? 0 });
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

public class Disease
{
	public int Infectivity = 25;
	public int SymptomaticDays = 100;
	public int FatalityRate = 1;
	public int RecoveryRate = 1;
}

public static class Randomness
{
	private static Random _rng = new Random();
	
	public static T Next<T>(this T[] source)
	{
		var index = _rng.Next(0, source.Length);
		return source[index];
	}
	
	public static int Percent()
	{
		return _rng.Next(0, 100);
	}
}

public class Person
{
	public int Id {get; set;}
	public State State {get; set;}
	public Point Location {get; set;}
	public List<MapNode> Destination {get; set;}
	public Point Residence {get; set;}
	public Point WorkPlace {get; set;}

	public int DaysInfected { get; set;}
	
	public int CollisionRadius {get; set;} = 4;
	
	public bool HasCollided(Point location)
	{
		var dx = Math.Pow(location.X - Location.X, 2);
		var dy = Math.Pow(location.Y - Location.Y, 2);
		var r = Math.Sqrt(dx + dy);
		return r < CollisionRadius;
	}
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
	public MapNode Previous {get; set; }
	public List<int> Connections {get; set;} = new List<int>();
	
	public MapNode(int x, int y)
	{
		Location = new Point(x, y);
	}
}