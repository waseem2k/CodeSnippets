using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Check if required resources are available
public class PowerGenerator : MonoBehaviour
{
	public float generatedPower = 50f; // This generator can generate this much power

	public float UsedPower // Gets the amount of power being used by each individual
	{
		get
		{
			float powerUsed = 0;
			foreach (ConduitUsage user in powerUsers)
			{
				powerUsed += user.amountUsed;
			}
			return powerUsed;
		}
	}

	public float AvailablePower { get { return generatedPower - UsedPower; } }

	public bool IsRunning = true; // { get; private set; } // Should be private
	private List<ConduitUsage> powerUsers = new List<ConduitUsage>();

	[Header("UI")]
	public TextMesh textMesh;

	private void Start()
	{
		UpdateUI();
	}

	public void UsePower(PowerConduit conduit, float amount)
	{
		ConduitUsage newUser = new ConduitUsage(conduit, amount);
		powerUsers.Add(newUser);
		UpdateUI();
	}

	public void ReleasePower(PowerConduit conduit)
	{
		ConduitUsage cu = FindConduit(conduit);
		if (cu != null) powerUsers.Remove(cu);
		UpdateUI();
	}

	private ConduitUsage FindConduit(PowerConduit con)
	{
		foreach (ConduitUsage cu in powerUsers)
		{
			if (cu.conduit == con) return cu;
		}
		return null;
	}

	private void UpdateUI()
	{
		textMesh.text = UsedPower + " / " + generatedPower;
	}
}

public class ConduitUsage
{
	public PowerConduit conduit;
	public float amountUsed;

	public ConduitUsage(PowerConduit con, float amt)
	{
		conduit = con;
		amountUsed = amt;
	}
}