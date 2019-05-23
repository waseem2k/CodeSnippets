using System.Collections;
using UnityEngine;

public class BarrelPickup : EquipmentPickup
{
	public Transform handlePivot; // The place where this item is held
	public Transform placementPivot; // The point that sits on the ground
	public LayerMask worldCollisionLayers; // The layers to check for collision when dropping barrel

	private ObjectOrientation orb;
	private bool needsMovingToHandle;

	public override void Interact(CharacterInteraction tar)
	{
		if (orb == null) orb = GetComponent<ObjectOrientation>();
		needsMovingToHandle = true;
		base.Interact(tar);
	}

	public override void DropItem()
	{
		Item.SetEquipped(false);
		ResetComponents();
		needsMovingToHandle = false;
	}

	public override void PlaceItem()
	{
		Item.SetEquipped(false);
		needsMovingToHandle = false;
		ResetComponents();
	}

	public override void UpdatePositions()
	{
		if (orb == null) return;

		if (needsMovingToHandle)
		{
			orb.MoveToHandle();
		}
		else
		{
			orb.MoveToGround();
		}
	}

	public override void ResetObject()
	{
		orb = GetComponent<ObjectOrientation>();
		orb.MoveToGround();
	}
}
