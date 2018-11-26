using UnityEngine;

[System.Serializable]
public class slSnakeTweakableProperties : ScriptableObject 
{
	public float MaxTurnAngularSpeed;
	public int PowerToNode;
	public int NormalMoveNodeCount;
	public int SpeedUpMoveNodeCount;
	public int SpeedUpCostPower;
	public int SpeedUpEnterRequiredNode;
	public int SpeedUpKeepRequiredNode;
}