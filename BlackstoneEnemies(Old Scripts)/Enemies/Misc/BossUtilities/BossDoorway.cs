using UnityEngine;
using DG.Tweening;

public class BossDoorway : MonoBehaviour
{

	public float transitionTime = 1f;
	public bool startOpen;

	private Vector2 closePosition, openPosition;


	private void Awake()
	{
		closePosition = transform.position;
		openPosition = transform.Find("SecondPosition").position;

		if (startOpen)
		{
			Open();
		}
	}

	public void Open()
	{
		transform.DOMove(openPosition, transitionTime).SetEase(Ease.InOutQuad);
	}

	public void Close()
	{
		transform.DOMove(closePosition, transitionTime).SetEase(Ease.InOutQuad);
	}
}
