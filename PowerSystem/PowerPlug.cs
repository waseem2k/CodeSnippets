using UnityEngine;

public class PowerPlug : EquipmentPickup
{
	public PowerCable connectedCable;
	public bool primaryEnd;
	public bool IsConnected { get; set; }
	public Transform resetAnchor;

	[Header("Power")]
	public PowerPlug oppositePlug; // The other end of this cable
	private PowerSocket connectedSocket;

	#region UpdateGenerators

	public void FindGenerators(PowerConduit source, PowerPlug requestSource)
	{
		if (connectedSocket != null)
		{
			connectedSocket.FindGenerators(source, this);
		}
	}
	public void FindGenerators(PowerConduit source, PowerSocket requestSource)
	{
		oppositePlug.FindGenerators(source, this);
	}

	public void FindGenerators(PowerSocket requestSource)
	{
		oppositePlug.FindGenerators(this);
	}

	public void FindGenerators(PowerPlug requestSource)
	{
		if(connectedSocket != null) connectedSocket.FindGenerators(this);
	}

	public void SetConnectedSocket(PowerSocket source)
	{
		connectedSocket = source;
	}

	public void RemoveGenerator(PowerGenerator gen, PowerSocket requestSource)
	{
		oppositePlug.RemoveGenerator(gen, this);
	}

	public void RemoveGenerator(PowerGenerator gen, PowerPlug requestSource)
	{
		if(connectedSocket != null) connectedSocket.RemoveGenerator(gen, this);
	}

	public void SendRefreshSignal(PowerSocket requestSource)
	{
		oppositePlug.SendRefreshSignal(this);
	}

	public void SendRefreshSignal(PowerPlug requestSource)
	{
		if(connectedSocket != null) connectedSocket.SendRefreshSignal(this);
	}
	#endregion

	public override void Interact(CharacterInteraction tar)
	{
		base.Interact(tar);
		IsConnected = false;
		connectedCable.SetSelectedEnd(this);
	}

	public override void DropItem()
	{
		transform.SetParent(connectedCable.transform);
		ResetComponents();
		connectedCable.ReleaseSelectedEnd(this);
	}

	public override void PlaceItem() // Called when connected to a plug
	{
		Item.SetEquipped(false);
		IsConnected = true;
		//ResetComponents();
		if(col != null) col.enabled = false;
		connectedCable.ReleaseSelectedEnd(this);
	}

	public override void UpdatePositions()
	{
		connectedCable.EndPickedUp = pickedUp;
		connectedCable.UpdateState();
	}

	public override void ResetObject()
	{
		// Do nothing
	}

	public void ResetPosition()
	{
		transform.SetParent(resetAnchor);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
	}
}
