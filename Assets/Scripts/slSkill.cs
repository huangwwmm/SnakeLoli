using UnityEngine;

public class slSkill : MonoBehaviour
{
	public SkillType MyType;

	protected slSnake m_Snake;
	protected hwmInputButtonSimple m_InputButton;
	protected hwmConstants.ButtonIndex m_ButtonIndex;

	public virtual void Active(slSnake snake, hwmConstants.ButtonIndex buttonIndex)
	{
		m_Snake = snake;
		m_ButtonIndex = buttonIndex;

		m_InputButton.EnableButton(m_ButtonIndex);
	}

	public virtual void Deactive()
	{
		m_InputButton.DisableButton();

		m_Snake = null;
	}

	public virtual void DoUpdate()
	{

	}

	protected void Awake()
	{
		m_InputButton = gameObject.AddComponent<hwmInputButtonSimple>();
	}

	protected void OnDestroy()
	{
		m_InputButton = null;
	}

	public enum SkillType
	{
		SpeedUp = 0,
		Gluttony,
		Stealth,
		Radar,
		RemainsFoodContamination,
		Count,
	}

	public class EventArgs
	{
		public bool CanKeepSkill;
		public float CostPower;
		public float Effect;
	}
}