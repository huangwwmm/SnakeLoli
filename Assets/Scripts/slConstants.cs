using UnityEngine;

public class slConstants 
{
	public const string SCENE_NAME_LOBBY = "Lobby";
	public const string SCENE_NAME_GAME = "Game";

	public const string DEFAULT_LEVEL_NAME = "Default";

	public const float WALL_THICK = 10.0f;

	public const int FOOD_QUADTREE_MAXDEPTH = 5;
	public const int FOOD_QUADTREE_MAXELEMENT_PERNODE = 100;
	public const int FOOD_QUADTREE_MAXELEMENT_PREPARENTNODE = 20;

	public static readonly Vector3 FOOD_DEACTIVE_POSITION = new Vector3(10000, 10000, 0);

	public enum Layer
	{
		Wall = 8,
		Food = 9,
	}
}