using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class WorldButton : Interactable
{
	public UnityEvent myEvent;

	public override void Interact(CharacterInteraction target)
	{
		myEvent.Invoke();
	}
}
