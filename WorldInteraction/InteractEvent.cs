using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Used for setting up events similar to a UI Button
/// Can be called using a raycast
/// </summary>
public class InteractEvent : Interactable
{
	[Header("Events")]
	public UnityEvent Events;

	public override void Interact(CharacterInteraction target)
	{
		Events.Invoke();
	}
}
