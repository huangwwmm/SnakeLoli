using UnityEngine;
using UnityEngine.UI;

public class slStealthSkill : slSkill
{
	public Properties MyProperties;
	public Text CDText;

	private float m_CD;
	private bool m_Stealth;
	private float m_StealthRemainTime;

	public override void Active(slSnake snake, hwmConstants.ButtonIndex buttonIndex)
	{
		base.Active(snake, buttonIndex);

		m_CD = MyProperties.CD;
		m_Stealth = false;
		CDText.color = Color.white;
	}

	public override void DoUpdate()
	{
		if (m_Stealth)
		{
			m_StealthRemainTime -= Time.deltaTime;
			if (m_StealthRemainTime < 0)
			{
				EnableStealth(false);
				m_CD = MyProperties.CD;
				CDText.color = Color.white;
			}
			else
			{
				CDText.text = Mathf.CeilToInt(m_StealthRemainTime).ToString();
			}
		}
		else
		{
			m_CD -= Time.deltaTime;
			bool canUse = m_CD <= 0;
			m_InputButton.GetUIButton().interactable = canUse;
			hwmInput.Button skillInput = hwmSystem.GetInstance().GetInput().GetButton(m_ButtonIndex);
			skillInput.SetEnable(canUse);
			CDText.text = canUse || m_CD < 0
				? ""
				: Mathf.CeilToInt(m_CD).ToString();

			if (skillInput.GetState() == hwmInput.Button.State.Up)
			{
				m_StealthRemainTime = MyProperties.Duration;
				m_InputButton.GetUIButton().interactable = false;
				skillInput.SetEnable(false);
				CDText.color = Color.red;
				EnableStealth(true);
			}
		}
	}

	private void EnableStealth(bool enable)
	{
		m_Stealth = enable;
		m_Snake.EnableEatFood(!enable);
		m_Snake.EnableDamageLayer((int)slConstants.Layer.Snake, !enable);
		m_Snake.EnableDamageLayer((int)slConstants.Layer.SnakeHead, !enable);
	}

	[System.Serializable]
	public class Properties
	{
		public float CD;
		public float Duration;
	}
}