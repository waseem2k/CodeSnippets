using UnityEngine;

public class PowerSocket : Interactable
{
	public Transform plugPivotPoint;
	private PowerPlug connectedPlug;
	public PowerConduit connectedConduit;
	
	public override void Interact(CharacterInteraction target)
	{
		// Connecting the cable
		if (target.CurrentHeldItem != null && target.CurrentHeldItem is PowerPlug)
		{
			target.Equipment.PlaceItem(plugPivotPoint.position, plugPivotPoint.rotation, plugPivotPoint);
			connectedPlug = (PowerPlug) target.CurrentHeldItem;
			if (connectedPlug != null)
			{
				connectedPlug.SetConnectedSocket(this);
				connectedPlug.SendRefreshSignal(this);
				//connectedPlug.FindGenerators(this);
			}
			connectedConduit.FindGenerators();
		}

		// Disconnecting the cable
		if (target.CurrentHeldItem == null && connectedPlug != null)
		{
			connectedPlug.ResetComponents();
			connectedPlug.Interact(target);
			connectedPlug.RemoveGenerator(connectedConduit.powerSource, this);
			connectedPlug.SetConnectedSocket(null);
			connectedPlug.SendRefreshSignal(this);
			//connectedPlug.FindGenerators(this);
			connectedPlug = null;
			connectedConduit.FindGenerators();
			connectedConduit.SendRefreshSignal();
		}
	}

	public void SendRefreshSignal(PowerConduit requestSource)
	{
		if(connectedPlug != null) connectedPlug.SendRefreshSignal(this);
	}
	public void SendRefreshSignal(PowerPlug requestSource)
	{
		connectedConduit.ReceiveRefreshSignal(this);
	}
	#region UpdateGenerators

	public void FindGenerators(PowerConduit source) // First call
	{
		if (connectedPlug != null)
		{
			connectedPlug.FindGenerators(source, this);
		}
	}

	public void FindGenerators(PowerConduit source, PowerPlug requestSource)
	{
		if (connectedConduit != source)
		{
			connectedConduit.FindGenerators(source, this);
		}
	}

	public void FindGenerators(PowerPlug requestSource)
	{
		//Debug.Log(gameObject.name + " Received request");
		connectedConduit.FindGenerators();
	}

	public void RemoveGenerator(PowerGenerator gen, PowerPlug requestSource)
	{
		connectedConduit.RemoveGenerator(gen, this);
	}

	public void RemoveGenerator(PowerGenerator gen, PowerConduit requestSource)
	{
		if(connectedPlug != null) connectedPlug.RemoveGenerator(gen, this);
	}

	#endregion
}
