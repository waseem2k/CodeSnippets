using System.Collections;
using UnityEngine;

public class WeaponPickup : EquipmentPickup
{
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
}
