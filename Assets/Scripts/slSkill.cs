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

		m_InputButton = gameObject.AddComponent<hwmInputButtonSimple>();
		m_InputButton.Index = buttonIndex;
	}

	public virtual void Deactive()
	{
		Destroy(m_InputButton);
		m_InputButton = null;

		m_Snake = null;
	}

	public virtual void DoUpdate()
	{

	}

	public enum SkillType
	{
		SpeedUp = 0,
		Gluttony,
		Stealth,
		Radar,
		Count,
	}

	public class EventArgs
	{
		public bool CanKeepSkill;
		public int CostPower;
		public float Effect;
	}
}