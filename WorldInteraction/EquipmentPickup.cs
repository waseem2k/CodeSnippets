using System.Collections;
using UnityEngine;

public abstract class EquipmentPickup : Interactable
{
	public Item Item { get; set; }
	
	protected bool pickedUp;
	protected Collider col;
	private CharacterInteraction target;

	public override void Interact(CharacterInteraction tar)
	{
		target = tar;
		PickupItem();
	}

	private void PickupItem()
	{
		if (pickedUp || target == null) return;

		CharacterEquipment equip = target.GetComponent<CharacterEquipment>();
		if (equip != null && !equip.IsHoldingItem)
		{
			col = GetComponent<Collider>();
			Item = GetComponentInChildren<Item>();
			if (col != null && Item != null)
			{
				col.enabled = false;
				pickedUp = true;
				Item.SetEquipped(true);
				equip.GiveItem(this);
			}
		}
	}

	public void ResetComponents()
	{
		if(col != null) col.enabled = true;
		pickedUp = false;
	}

	public abstract void DropItem();
	public abstract void PlaceItem(); // No RB
	public abstract void UpdatePositions();
	public abstract void ResetObject();
}
