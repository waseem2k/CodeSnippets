using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelSlot : Interactable
{
	public override void Interact(CharacterInteraction target)
	{
		if (target.CurrentHeldItem != null && target.CurrentHeldItem is BarrelPickup)
		{
			target.Equipment.PlaceItem(transform.position, transform.rotation, transform);
		}
	}
}