using System;
using UnityEngine;

public class Health : MonoBehaviour
{
	public float MaxHealth = 100f;
	public float CurrentHealth; // { get; private set; }
	public bool regenerateHealth = true;
	public float healthRegenRate = 0.5f;
	public float uiUpdateRate = 5f; // The time to wait before updating UI
	private float updateTimer = 5f;
	public event Action OnDeath;
	public event Action<float> OnDamageTaken;
	

	

	// Called by anything that deals damage to the owner
	public void TakeDamage(float amount)
	{
		if (amount <= 0) return;

		CurrentHealth -= amount;
		if (OnDamageTaken != null) OnDamageTaken(amount);
		ValidateHealth();
		UpdateUI();
	}

	// Increase current health (Health Pickups etc)
	public void GiveHealth(float amount)
	{
		if (amount <= 0) return;

		CurrentHealth += amount;
		ValidateHealth();
		UpdateUI();
	}

	private void Start()
	{
		CurrentHealth = MaxHealth;
		UpdateUI();
	}
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.G))
		{
			TakeDamage(10);
		}
		PassiveHealthRegen();
	}

	// Slowly regenerate health over time
	private void PassiveHealthRegen()
	{
		if (regenerateHealth && CurrentHealth < MaxHealth)
		{
			CurrentHealth += healthRegenRate * Time.deltaTime;
			updateTimer -= Time.deltaTime;
			if (updateTimer < 0)
			{
				UpdateUI();
				updateTimer = uiUpdateRate;
			}

			if (CurrentHealth >= MaxHealth)
			{
				CurrentHealth = MaxHealth;
				UpdateUI();
			}
		}
		
		// Health regen logic
	}

	// Keeps the values clamped between zero and max
	private void ValidateHealth()
	{
		if (CurrentHealth < 0)
		{
			CurrentHealth = 0;
			DeathEvent();
		}
		if (CurrentHealth > MaxHealth)
			CurrentHealth = MaxHealth;
	}


	// Calls the death event so other scripts can access it
	private void DeathEvent()
	{
		if (OnDeath != null) OnDeath();

		// Handle death stuff here
	}

	// Updates the UI only when health changes or by timer
	public void UpdateUI()
	{
		UIManager.HealthUI.UpdateUI(CurrentHealth, MaxHealth);
	}
}
