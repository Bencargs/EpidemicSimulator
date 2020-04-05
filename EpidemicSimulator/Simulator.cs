using EpidemicSimulator.Models;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace EpidemicSimulator
{
    public class Simulator : IDisposable
    {
		private Image _background;
		private Person[] _population;
		private MapNode[] _residence;
		private MapNode[] _businesses;
		private Pathfinder _pathfinder;
		private readonly Disease _disease = new Disease();

		public EventHandler<Bitmap> RenderUpdated;

		public void Initialise()
		{
			_residence = LoadFile<MapNode>("Residential");
			_businesses = LoadFile<MapNode>("Commercial");
			var trafficNodes = LoadFile<TrafficNode>("TrafficNodes");
			_background = new Bitmap(@"C:\Source\EpidemicSimulator\EpidemicSimulator\Resources\Map.png");

			_pathfinder = new Pathfinder(trafficNodes);
			_population = CreatePopulation();
		}

		public void Render()
		{
			while (!IsComplete(_population))
			{
				using (var image = new Bitmap(_background))
				using (var gfx = Graphics.FromImage(image))
				{
					foreach (var person in _population)
					{
						DrawPerson(person, gfx);

						if (person.State == State.Dead)
							continue;

						if (person.State == State.Infected)
							EvaluateInfection(_disease, person, _population);

						_pathfinder.Navigate(person);
					}
					RenderUpdated?.Invoke(this, image);
				}
			}
		}

		private bool IsComplete(Person[] people)
		{
			var extinction = people.All(p => p.State == State.Dead);
			var eradication = !people.Any(p => p.State == State.Infected);
			return extinction || eradication;
		}

		private Person[] CreatePopulation()
		{
			var suseptible = Enumerable.Range(0, 1000).Select(x =>
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

		private Point GetRandomBusiness()
		{
			var business = Randomness.Next(_businesses);
			return business.Location;
		}

		private Point GetRandomResidence()
		{
			var residence = Randomness.Next(_residence);
			return residence.Location;
		}

		private T[] LoadFile<T>(string filename)
		{
			var json = File.ReadAllText($@"C:\Source\EpidemicSimulator\EpidemicSimulator\Resources\{filename}.json");
			var nodes = Newtonsoft.Json.JsonConvert.DeserializeObject<T[]>(json);
			return nodes;
		}

		private void DrawPerson(Person person, Graphics graphics)
		{
			var colour = GetColour(person);
			graphics.DrawEllipse(new Pen(new SolidBrush(Color.Black)), person.Location.X, person.Location.Y, 4, 4);
			graphics.FillEllipse(new SolidBrush(colour), person.Location.X, person.Location.Y, 4, 4);
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

		private void EvaluateInfection(Disease disease, Person person, Person[] people)
		{
			foreach (var collision in _pathfinder.GetCollisions(person, people))
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

		public void Dispose()
		{
			_background?.Dispose();
		}
	}
}
