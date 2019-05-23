using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
	public InteractableData Data;

	public abstract void Interact(CharacterInteraction target);

	public void ShowDescription()
	{
		DisplayInformation.ShowText(Data.Name, Data.Description);
	}

	public void HideDescription()
	{
		DisplayInformation.HideText();
	}
}

[System.Serializable]
public class InteractableData
{
	public string Name;
	[TextArea] public string Description;
}
