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
	public const float FOOD_QUADTREE_LOOSESCALE = 1.2f;

	public static readonly Vector3 FOOD_DEACTIVE_POSITION = new Vector3(10000, 10000, 0);
	public static readonly Vector2 FOOD_MAP_EDGE = new Vector2(2, 2);

	public const int SNAKE_SPRITERENDERER_MAX_ORDERINLAYER = 32767;
	public const int SNAKE_SPRITERENDERER_MIN_ORDERINLAYER = -30000;

	public static readonly int SORTINGLAYER_MAP = SortingLayer.NameToID("Map");
	public static readonly int SORTINGLAYER_FOOD = SortingLayer.NameToID("Food");
	public static readonly int SORTINGLAYER_SNAKE = SortingLayer.NameToID("Snake");

	public const int SNAKE_INITIALIZEZ_NODE_MINCOUNT = 5;

	public const string SNAKE_PREfAB_NAME_STARTWITHS = "Snake_";
	public const string SNAKE_PRESENTATION_PREfAB_NAME_STARTWITHS = "SnakePresentation_";

	public enum Layer
	{
		Wall = 8,
		Food = 9,
		SnakeHead = 10,
		Snake = 11,
	}
}