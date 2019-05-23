using UnityEngine;

public class Item : MonoBehaviour
{
	public int itemIndex;
	public Collider col;
	public float heightOffset;

	public void SetEquipped(bool state)
	{
		//if (col == null) col = GetComponent<Collider>();
		if (col != null)
		{
			col.isTrigger = state;
		}
	}
}
