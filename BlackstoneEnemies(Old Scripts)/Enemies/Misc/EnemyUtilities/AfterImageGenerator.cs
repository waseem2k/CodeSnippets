using System.Collections;
using UnityEngine;

public class AfterImageGenerator : MonoBehaviour
{
	public float rate = 0.2f; // The rate at which to spawn after images, lower is faster
	public float length = 0.5f; // The time the image stays visible before destroying self
	[Range(0,1)] public float fadeAmount = 0.5f; // The amount the image is faded out
	 
	public Material material; // The material to apply to the after image

	private bool trailActive; // If the trail is to be shown or not

	public void SpawnTrail(bool _spawn)
	{
		if (_spawn) StartCoroutine(SpawnTrail());
		else trailActive = false;
	}

	private IEnumerator SpawnTrail()
	{
		trailActive = true;
		while (trailActive) // Disables self when trailActive is switched off
		{
			GameObject trailPart = new GameObject(); // Create a new object with a sprite renderer
			SpriteRenderer trailRenderer = trailPart.AddComponent<SpriteRenderer>(); // Add spriterender
			trailRenderer.sprite = GetComponent<SpriteRenderer>().sprite; // copy the current sprite from the main renderer to new renderer
			trailPart.transform.position = transform.position; // Set position
			trailPart.transform.rotation = transform.rotation; // Set rotation
			trailPart.transform.localScale = transform.localScale; // Set scale
			trailRenderer.material = material; // Apply material
			Color color = trailRenderer.color; // Get the current colour of the new renderer
			color.a -= fadeAmount; // reduce the alpha amount
			trailRenderer.color = color; // apply the new colour to the rendere
			Destroy(trailPart, length); // destroy after set duration

			yield return new WaitForSeconds(rate);
		}
	}

	private void OnValidate()
	{
		if (rate < 0) rate = 0;
		if (length < 0) length = 0;
	}
}
