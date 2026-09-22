using UnityEngine;

public class AdsCanvas : DamageReciver2
{
	[SerializeField]
	private string url;

	private bool hasOpened;

	public override void Damage(float dmg, int fromWhom = 0)
	{
		if (!hasOpened && url != string.Empty)
		{
			hasOpened = true;
			base.gameObject.SetActive(false);
			Application.OpenURL(url);
		}
	}

	public override void Heal(float val)
	{
	}
}
