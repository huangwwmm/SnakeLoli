using UnityEngine;
using UnityEngine.UI;

public class slGluttonySkill : slSkill 
{
	public Properties MyProperties;
	public Text CDText;

	private float m_CD;

	public override void Active(slSnake snake, hwmConstants.ButtonIndex buttonIndex)
	{
		base.Active(snake, buttonIndex);

		m_CD = MyProperties.FirstCD;
	}

	public override void DoUpdate()
	{
		m_CD -= Time.deltaTime;
		bool canUse = m_CD <= 0;
		m_InputButton.GetUIButton().interactable = canUse;
		hwmInput.Button skillInput = hwmSystem.GetInstance().GetInput().GetButton(m_ButtonIndex);
		skillInput.SetEnable(canUse);
		CDText.text = canUse ? "" : Mathf.CeilToInt(m_CD).ToString();

		if (skillInput.GetState() == hwmInput.Button.State.Up)
		{
			m_Snake.UseGluttonySkill(MyProperties.Radius);
			m_CD = MyProperties.CD;
		}
	}

	[System.Serializable]
	public class Properties
	{
		public float FirstCD;
		public float CD;
		public int Radius;
	}
}