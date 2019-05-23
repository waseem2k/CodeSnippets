using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
	//[HideInInspector]
	public Health health;
	public Image fill;
	//public Image frame;
	public Image background;
	//public Image backgroundTextured;
	public Text bossName;
	public BossUI[] bossFrame;
	[Space]
	public float fadeTime;

	public float fillTime;

	private bool showing;
	private int frameIndex;
	private int bossIndex;
	private bool fadeName;
	public static BossHealthUI instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
		DontDestroyOnLoad(gameObject);
	}
	private void Start()
	{
		FadeOnStart(0.01f);
	}

	private void FadeOnStart(float f_time)
	{
		foreach (var t in bossFrame)
		{
			t.FadeOnStart(f_time);
		}
		FadeOut(f_time);
	}

	public void UpdateTarget(Health _health, int _index)
	{
		health.OnDamaged -= UpdateHealth;
		health.OnHealed -= UpdateHealth;
		bossFrame[frameIndex].UpdateName(_index);
		health = _health;
		health.OnDamaged += UpdateHealth;
		health.OnHealed += UpdateHealth;
	}

	public void AssignTarget(GameObject obj, string _name, int frameTarget, int bossNumber = 0, bool _fadeName = false)
	{
		health = obj.GetComponent<Health>();
		bossName.text = _name;
		frameIndex = frameTarget;
		bossIndex = bossNumber;
		fadeName = _fadeName;
		if (health == null)
		{
			Debug.LogError("Assigned target does not have a Health script attached");
			return;
		}
		UpdateHealth(null);
		health.OnDamaged += UpdateHealth;
		health.OnHealed += UpdateHealth;
		FadeIn(fadeTime);
	}

	public void HideHealthBar()
	{
		health.OnDamaged -= UpdateHealth;
		health.OnHealed -= UpdateHealth;
		health = null;
		
		FadeOut(fadeTime);
	}

	private void Update()
	{
		if (health == null) return;

		if (health.CurrentHealth < 0 && showing)
		{
			HideHealthBar();
		}
		if (Player.instance.health.CurrentHealth < 0 && showing)
		{
			HideHealthBar();
		}
	}

	private void UpdateHealth(GameObject obj)
	{
		if(showing)
			fill.fillAmount = health.CurrentHealth / health.MaxHealth;
	}

	private void FadeIn(float f_time)
	{
		background.CrossFadeAlpha(1f, f_time, false);
		fill.CrossFadeAlpha(1f, f_time, false);
		bossFrame[frameIndex].FadeIn(bossIndex, f_time, fadeName);
		if(!fadeName) bossName.CrossFadeAlpha(1f, f_time, false);
		StartCoroutine(FillBar(fillTime));
	}

	private void FadeOut(float f_time)
	{
		showing = false;
		background.CrossFadeAlpha(0, f_time, false);
		fill.CrossFadeAlpha(0, f_time, false);
		bossFrame[frameIndex].FadeOut(f_time);
		if(!fadeName) bossName.CrossFadeAlpha(0, f_time, false);
		fill.fillAmount = 0;
	}

	private IEnumerator FillBar(float f_time)
	{
		var _fillTime = 1 / (f_time * 1000);

		while (fill.fillAmount < health.CurrentHealth / health.MaxHealth)
		{
			fill.fillAmount += Time.deltaTime;
			yield return new WaitForSeconds(_fillTime);
		}
		showing = true;
	}

	[Serializable]
	public class BossUI
	{
		public Image[] frames;
		public Image[] names;
		public Image outline;

		private int nameIndex;
		private bool fadeName;
		
		public void FadeIn(int index, float f_time, bool _fadeName)
		{
			nameIndex = index;
			fadeName = _fadeName;
			foreach (Image img in frames)
			{
				img.CrossFadeAlpha(1f, f_time, false);
			}
			if(fadeName) names[nameIndex].CrossFadeAlpha(1f, f_time, false);
			outline.CrossFadeAlpha(1f, f_time, false);
		}

		public void FadeOut(float f_time)
		{
			foreach (Image img in frames)
			{
				img.CrossFadeAlpha(0, f_time, false);
			}
			if(fadeName) names[nameIndex].CrossFadeAlpha(0, f_time, false);
			outline.CrossFadeAlpha(0, f_time, false);
		}

		public void FadeOnStart(float f_time)
		{
			foreach (Image img in frames)
			{
				img.CrossFadeAlpha(0, f_time, false);
			}
			foreach (Image img in names)
			{
				img.CrossFadeAlpha(0, f_time, false);
			}
			outline.CrossFadeAlpha(0, f_time, false);
		}

		public void UpdateName(int _index)
		{
			names[nameIndex].CrossFadeAlpha(0f, 0.2f, false);
			nameIndex = _index;
			names[nameIndex].CrossFadeAlpha(1f, 0.2f, false);
		}
	}
}
