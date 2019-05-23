using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Machine : MonoBehaviour
{
	public PowerConduit powerSource;
	public float requiredPower = 50f;
	protected bool powerAvailable;
	public bool IsRunning { get { return powerSource.IsRunning; } } // If machine has enough power and a request for use has been called
	public bool debug;
	private void Update()
	{
		UpdateMachine();
	}

	protected void StartMachine() // Call when starting the machine, begins drawing power
	{
		// If resources available and conduit has power
		powerSource.RequestPower(requiredPower);
	}

	protected void StopMachine() // Call when machine is done and it no longer needs to use power
	{
		// Stop machine
		powerSource.ReleaseAllPower();
	}

	private void CheckResources() // Not yet implemented
	{
		// If resources are depleted, stop machine
	}

	protected abstract void UpdateMachine(); // Use this instead of Update
}
