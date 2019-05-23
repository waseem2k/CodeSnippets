using Obi;
using UnityEngine;

public class PowerCable : EquipmentPickup
{
	[Header("Cable States")]
	public GameObject stiffCable;
	public GameObject obiCable;
	
	[Header("Power")]
	public PowerPlug primaryPlug;
	public PowerPlug secondaryPlug;
	private PowerPlug lastSelectedPlug;

	private bool primaryConnected;
	private bool secondaryConnected;
	public bool EndPickedUp { get; set; } // If any end is being held by the player	


	public void SetSelectedEnd(PowerPlug tar)
	{
		if (tar.primaryEnd && !secondaryPlug.IsConnected)
		{
			secondaryPlug.transform.parent = null;
		}
		else if (!tar.primaryEnd && !primaryPlug.IsConnected)
		{
			primaryPlug.transform.parent = null;
		}
		transform.SetParent(null);
		lastSelectedPlug = tar;
	}

	public void ReleaseSelectedEnd(PowerPlug tar)
	{
		if (tar.primaryEnd && !secondaryPlug.IsConnected) secondaryPlug.transform.SetParent(transform);
		else if (!tar.primaryEnd && !primaryPlug.IsConnected) primaryPlug.transform.SetParent(transform);
		lastSelectedPlug = tar;
	}

	public void UpdateState() // If true, the stiff cable becomes active
	{
		if (col == null) col = GetComponent<Collider>();

		secondaryConnected = secondaryPlug.IsConnected;
		primaryConnected = primaryPlug.IsConnected;

		if (EndPickedUp || primaryConnected || secondaryConnected)
		{
			stiffCable.SetActive(false);
			obiCable.SetActive(true);
			col.enabled = false;
		}
		else if (!primaryConnected && !secondaryConnected && !EndPickedUp)
		{
			stiffCable.SetActive(true);
			obiCable.SetActive(false);
			col.enabled = true;
			ResetState();
		}
	}

	private void ResetState()
	{
		transform.SetParent(null);
		transform.position = lastSelectedPlug.transform.position;
		primaryPlug.ResetPosition();
		secondaryPlug.ResetPosition();
	}

	#region Inherited

	public override void DropItem()
	{
		Item.SetEquipped(false);
		ResetComponents();
	}

	public override void PlaceItem()
	{
		// Do nothing
	}

	public override void UpdatePositions()
	{
		// Do nothing
	}

	public override void ResetObject()
	{
		// Do nothing
	}

	#endregion
}
