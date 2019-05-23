using System;
using System.Collections;
using UnityEngine;

public class BossCollapsingFloor : MonoBehaviour
{
	[SerializeField] private CollapseType type;
	[SerializeField] private float gravityScale = 1f;
	[SerializeField] private float destroyAfter = 1f;
	[SerializeField] private float waitBetweenDrops = 0.1f;
	[Space]
	[SerializeField] private GameObject mainCollider;
	[Space]
	[SerializeField] private GameObject[] floors;
	
	private enum CollapseType { MiddleOut, LeftOut, RightOut, Together, Random}

	public void CollapseFloors()
	{
		Destroy(mainCollider);

		switch (type)
		{
			case CollapseType.MiddleOut:
				StartCoroutine(MiddleOut());
				break;
			case CollapseType.LeftOut:
				StartCoroutine(LeftOut());
				break;
			case CollapseType.RightOut:
				StartCoroutine(RightOut());
				break;
			case CollapseType.Together:
				AllTogether();
				break;
			case CollapseType.Random:
				StartCoroutine(RandomDrop());
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private IEnumerator LeftOut()
	{
		foreach (var t in floors)
		{
			var rb = t.GetComponent<Rigidbody2D>();
			rb.bodyType = RigidbodyType2D.Dynamic;
			rb.gravityScale = gravityScale;
			rb.constraints = RigidbodyConstraints2D.FreezeRotation;

			var joint = t.GetComponent<Joint2D>();
			Destroy(joint);
			Destroy(t, destroyAfter);

			yield return new WaitForSeconds(waitBetweenDrops);
		}
		Destroy(gameObject, destroyAfter);
	}

	private IEnumerator RightOut()
	{
		for (var i = floors.Length-1; i > -1; i--)
		{
			var rb = floors[i].GetComponent<Rigidbody2D>();
			rb.bodyType = RigidbodyType2D.Dynamic;
			rb.gravityScale = gravityScale;
			rb.constraints = RigidbodyConstraints2D.FreezeRotation;

			var joint = floors[i].GetComponent<Joint2D>();
			Destroy(joint);
			Destroy(floors[i], destroyAfter);

			yield return new WaitForSeconds(waitBetweenDrops);
		}
		Destroy(gameObject, destroyAfter);
	}

	private void AllTogether()
	{
		foreach (var t in floors)
		{
			var rb = t.GetComponent<Rigidbody2D>();
			rb.bodyType = RigidbodyType2D.Dynamic;
			rb.gravityScale = gravityScale;
			rb.constraints = RigidbodyConstraints2D.FreezeRotation;

			var joint = t.GetComponent<Joint2D>();
			Destroy(joint);
		}

		Destroy(gameObject, destroyAfter);
	}

	private IEnumerator MiddleOut()
	{
		var halfwayPoint = floors.Length / 2;
		var leftPoint = halfwayPoint;
		var rightPoint = halfwayPoint;

		for (var i = 0; i < halfwayPoint + 1; i++)
		{
			// Left Side
			if (leftPoint > -1)
			{
				var rbLeft = floors[leftPoint].GetComponent<Rigidbody2D>();

				rbLeft.bodyType = RigidbodyType2D.Dynamic;
				rbLeft.gravityScale = gravityScale;
				rbLeft.constraints = RigidbodyConstraints2D.FreezeRotation;

				var jointL = floors[leftPoint].GetComponent<Joint2D>();
				Destroy(jointL);
				Destroy(floors[leftPoint], destroyAfter);
				leftPoint--;
			}

			// Right Side
			if (rightPoint < floors.Length)
			{
				var rbRight = floors[rightPoint].GetComponent<Rigidbody2D>();

				rbRight.bodyType = RigidbodyType2D.Dynamic;
				rbRight.gravityScale = gravityScale;
				rbRight.constraints = RigidbodyConstraints2D.FreezeRotation;

				var jointR = floors[rightPoint].GetComponent<Joint2D>();
				Destroy(jointR);
				Destroy(floors[rightPoint], destroyAfter);
				rightPoint++;
			}

			yield return new WaitForSeconds(waitBetweenDrops);
		}
		Destroy(gameObject, destroyAfter);
	}

	private IEnumerator RandomDrop()
	{
		floors = ExtensionMethods.ShuffleObjects(floors);

		foreach (var t in floors)
		{
			var rb =  t.GetComponent<Rigidbody2D>();
			rb.bodyType = RigidbodyType2D.Dynamic;
			rb.gravityScale = gravityScale;
			rb.constraints = RigidbodyConstraints2D.FreezeRotation;

			var joint = t.GetComponent<Joint2D>();
			Destroy(joint);
			Destroy(t.gameObject, destroyAfter);

			yield return new WaitForSeconds(waitBetweenDrops);
		}
		Destroy(gameObject, destroyAfter);
	}
}
