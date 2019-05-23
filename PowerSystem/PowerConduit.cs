using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerConduit : MonoBehaviour
{
	public PowerGenerator powerSource;
	public List<PowerSocket> connectedSockets;
	private List<PowerGenerator> generators = new List<PowerGenerator>(); // List of available generators
	private List<GeneratorUsage> generatorsInUse = new List<GeneratorUsage>();

	#region Accessors

	public float AvailablePower
	{
		get
		{
			float power = 0;
			if (powerSource != null) power += powerSource.AvailablePower;
			power += generators.Sum(gen => gen.AvailablePower);
			return power;
		}
	}

	public float TotalPower
	{
		get
		{
			float power = 0;
			if (powerSource != null) power += powerSource.generatedPower;
			power += generators.Sum(gen => gen.generatedPower);
			return power;
		}
	}

	public float UsedPower
	{
		get
		{
			float power = 0;
			if (powerSource != null) power += powerSource.UsedPower;
			power += generators.Sum(gen => gen.UsedPower);
			return power;
		}
	}

	public bool IsRunning
	{
		get { return generatorsInUse.Count > 0 && UsedPower <= TotalPower; }
	}

	#endregion
	
	#region UpdateGenerators

	public void SendRefreshSignal()
	{
		foreach (PowerSocket ps in connectedSockets)
		{
			ps.SendRefreshSignal(this);
		}
	}

	public void ReceiveRefreshSignal(PowerSocket requestSource)
	{
		FindGenerators();
		foreach (PowerSocket ps in connectedSockets)
		{
			if (ps != requestSource)
			{
				ps.SendRefreshSignal(this);
			}
		}
	}


	public void FindGenerators() // Transmits a message with this source
	{
		generators = new List<PowerGenerator>();
		if (powerSource != null) generators.Add(powerSource);
		foreach (PowerSocket ps in connectedSockets)
		{
			ps.FindGenerators(this);
		}
		RefreshGenerators();
	}

	public void FindGenerators(PowerConduit source, PowerSocket requestSource) // Transmits a message with this source
	{
		if (source == this) return;
		if (powerSource != null)
		{
			source.AddGenerator(powerSource);
		}
		AddGenerator(source.powerSource);
		foreach (PowerSocket ps in connectedSockets)
		{
			if (ps != requestSource)
			{
				ps.FindGenerators(source);
			}
		}
		//FindGenerators();
	}

	private void AddGenerator(PowerGenerator gen)
	{
		if(gen != null && !generators.Contains(gen)) generators.Add(gen);
	}

	// On Cable disconnected
	public void RemoveGenerator(PowerGenerator gen, PowerSocket requestSource)
	{
		if(gen != null) gen.ReleasePower(this);
		generatorsInUse.RemoveAllUsage(gen);

		if (gen != null && generators.Contains(gen))
		{
			generators.Remove(gen);
		}

		foreach (PowerSocket ps in connectedSockets)
		{
			if(ps != requestSource) ps.RemoveGenerator(gen, this);
		}
	}

	private void RefreshGenerators()
	{
		List<GeneratorUsage> usage = generatorsInUse.Where(gu => !generators.Contains(gu.generator)).ToList();

		foreach (GeneratorUsage gu in usage)
		{
			gu.generator.ReleasePower(this);
			generatorsInUse.Remove(gu);
		}
	}

	#endregion

	public void RequestPower(float amount)
	{
		if (AvailablePower >= amount)
		{
			float amountRequested = amount;
			foreach (PowerGenerator gen in generators)
			{
				if (gen.AvailablePower > 0) // If generator has spare power
				{
					float sparePower = gen.AvailablePower; // Holding available power before it gets changed

					if (sparePower >= amountRequested) // If spare power is greater than or equal to the total amount we need
					{
						gen.UsePower(this, amountRequested); // If generator has enough power, we just use what we need
						AddGeneratorUsage(gen, amountRequested); // Add the generator being used to our list
						return;
					}

					// If the generator doesn't have enough power but has spare
					gen.UsePower(this, sparePower); // Use the spare amount
					AddGeneratorUsage(gen, sparePower); // Add the generator being used to our list

					amountRequested -= sparePower; // We minus the amount we got from this generator and continue the loop
				}
			}
		}
	}

	public void ReleaseAllPower()
	{
		foreach (GeneratorUsage gen in generatorsInUse)
		{
			gen.generator.ReleasePower(this);
		}
		generatorsInUse.Clear();
	}

	private void AddGeneratorUsage(PowerGenerator gen, float amount)
	{
		GeneratorUsage newGenUse = new GeneratorUsage(gen, amount);
		generatorsInUse.Add(newGenUse);
	}
}

public class GeneratorUsage
{
	public PowerGenerator generator;
	public float amountUsed;

	public GeneratorUsage(PowerGenerator gen, float amt)
	{
		generator = gen;
		amountUsed = amt;
	}
}