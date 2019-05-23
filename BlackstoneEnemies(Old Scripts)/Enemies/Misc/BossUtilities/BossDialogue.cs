using System.Collections.Generic;
using UnityEngine;

public class BossDialogue : MonoBehaviour
{
	public List<Dialogue> dialogueList; // Called when initiating a boss fight
	public List<Dialogue> defeatDialogue; // Called when the boss is defeated
	public List<Dialogue> victoryDialogue; // Called when player is defeated

	public void InitiateDialogue()
	{
		if (dialogueList == null || dialogueList.Count < 1) return;
		DialogueManager.instance.DisplayDialogue(dialogueList.ToArray());
	}

	public void InitiateDefeatDialogue()
	{
		if (defeatDialogue == null || defeatDialogue.Count < 1) return;
		DialogueManager.instance.DisplayDialogue(defeatDialogue.ToArray());
	}

	public void InitiateVictoryDialogue()
	{
		if (victoryDialogue == null || victoryDialogue.Count < 1) return;
		DialogueManager.instance.DisplayDialogue(victoryDialogue.ToArray());
	}
}
