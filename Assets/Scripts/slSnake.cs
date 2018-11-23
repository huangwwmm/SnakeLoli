using UnityEngine;

public class slSnake : hwmActor
{
	public Properties MyProperties;

	[System.Serializable]
	public class Properties
	{
		public float HeadColliderRadius;
		public float ClothesColliderRadius;
		public float BodyColliderRadius;

		public float ClothesToHeadDistance;
		public float BodyToClothesDistance;
		public float BodyToBodyDistance;
	}
}