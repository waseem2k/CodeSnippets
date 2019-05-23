using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Example Machine setup
/// </summary>
public class MiningRig : Machine
{
	private void Start()
	{
		StartMachine();
	}

	protected override void UpdateMachine()
	{
		if (IsRunning) GenerateResources();
	}

	private void GenerateResources()
	{
		// Check if storage is connected (If a barrel or storage of some sort is connected)
		// Check if storage already has some sort of resource, and the resource is the same as what's being generated
		// Add resources to storage if it's empty or the resources match
		// If storage is full or resources don't match, display error in game and stop running (Stop using power)
	}

}
