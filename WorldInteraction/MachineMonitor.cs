using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineMonitor : Interactable
{
	public Machine machine;

    public override void Interact(CharacterInteraction target)
	{
		OpenMonitorConsole();
	}

	private void OpenMonitorConsole()
	{
		Debug.Log("Opening monitor console");
		// Wtf Unity
	}
}
