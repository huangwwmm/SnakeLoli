using UnityEngine;

public class slSnakePresentation : MonoBehaviour
{
	public Properties MyProperties;

	[System.Serializable]
	public class Properties
	{
		public GameObject Head;
		public GameObject Clothes;
		public GameObject Body;
		public int BodySpriteMaxOrderInLayer;
	}
}