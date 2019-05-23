using UnityEngine;

public class Oxygen : MonoBehaviour
{
	public float MaxOxygen = 100f;
	public float CurrentOxygen;// { get; private set; }
	public bool regenerateOxygen = true;
	public float oxygenRegenRate = 0.5f;

	[Header("Health ")] 
	public float healthTickRate = 0.5f;
	private Health health;

	[Header("UI")]
	public float uiUpdateRate = 5f;
	private float updateTimer = 5f;

	public void DecreaseOxygen(float amount)
	{
		if (amount <= 0) return;

		CurrentOxygen -= amount;
		ValidateOxygen();
		UpdateUI();
	}

	public void IncreaseOxygen(float amount)
	{
		if (amount <= 0) return;

		CurrentOxygen += amount;
		ValidateOxygen();
		UpdateUI();
	}

	private void Start()
	{
		health = GetComponent<Health>();
		CurrentOxygen = MaxOxygen;
		UpdateUI();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.H))
		{
			DecreaseOxygen(10);
		}
		PassiveOxygenRegen();
		
		CheckOxygenLevel();
	}

	private void PassiveOxygenRegen()
	{
		if (regenerateOxygen && CurrentOxygen < MaxOxygen)
		{
			CurrentOxygen += oxygenRegenRate * Time.deltaTime;
			updateTimer -= Time.deltaTime;
			if (updateTimer < 0)
			{
				UpdateUI();
			}
			if (CurrentOxygen >= MaxOxygen)
			{
				CurrentOxygen = MaxOxygen;
				UpdateUI();
			}
		}
	}

	private void DecreaseHealth()
	{
		health.TakeDamage(healthTickRate * Time.deltaTime);
	}

	private void UpdateUI()
	{
		UIManager.OxygenUI.UpdateUI(CurrentOxygen, MaxOxygen);
	}

	private void ValidateOxygen()
	{
		if (CurrentOxygen < 0) // If less then 10%
		{
			CurrentOxygen = 0;
			// 0 oxygen event
		}
		if (CurrentOxygen > MaxOxygen)
			CurrentOxygen = MaxOxygen;
	}

	private void CheckOxygenLevel()
	{
		if (CurrentOxygen < Mathf.Lerp(0, MaxOxygen, 0.1f))
		{
			DecreaseHealth();
		}
	}
}
