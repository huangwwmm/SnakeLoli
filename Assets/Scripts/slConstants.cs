using UnityEngine;

public class slConstants 
{
	public const string SCENE_NAME_LOBBY = "Lobby";
	public const string SCENE_NAME_GAME = "Game";

	public const string DEFAULT_LEVEL_NAME = "Default";

	public const float WALL_THICK = 10.0f;

	public const int FOOD_QUADTREE_MAXDEPTH_BOUNDESIZE = 4;
	public const int FOOD_QUADTREE_MAXELEMENT_PERNODE = 24;
	public const int FOOD_QUADTREE_MINELEMENT_PREPARENTNODE = 10;
	public const float FOOD_QUADTREE_LOOSESCALE = 1.2f;

	public static readonly Vector3 FOOD_DEACTIVE_POSITION = new Vector3(10000, 10000, 0);
	public const float FOOD_MAP_EDGE = 0.5f;

	public const string FOOD_PRESENTATION_PREFAB_ENDWITHS = "FoodPresentation";
	public const string FOOD_PROPERTIES_PREFAB_ENDWITHS = "FoodProperties";

	public const float FOOD_BEEAT_MOVE_TIME = 0.36f;
	public const int FOOD_SYSTEM_MAXCREATE_PREFRAME = 20;

	public const int SNAKE_SPRITERENDERER_MAX_ORDERINLAYER = 32760;
	public const int SNAKE_SPRITERENDERER_MIN_ORDERINLAYER = -32767;

	public static readonly int SORTINGLAYER_MAP = SortingLayer.NameToID("Map");
	public static readonly int SORTINGLAYER_FOOD = SortingLayer.NameToID("Food");
	public static readonly int SORTINGLAYER_SNAKE = SortingLayer.NameToID("Snake");

	public const int SNAKE_INITIALIZEZ_NODE_MINCOUNT = 5;

	public static readonly Vector3 SNAKE_BODYNODE_CREATE_POSITION = new Vector3(10000, 10000, 0);

	public const string SNAKE_PREfAB_NAME_STARTWITHS = "Snake_";
	public const string SNAKE_PRESENTATION_PREfAB_NAME_STARTWITHS = "SnakePresentation_";

	public const float SNAKE_SPAWN_SAFEAREA_MAP_EDGE = 10;
	public static readonly Vector2 SNAKE_SPAWN_SAFEAREA_SIZE = new Vector2(6, 12);
	public const float SNAKE_SPAWN_SAFEAREA_YAXIS_OFFSET = -1.6f;
	public const float SNAKE_NODE_TO_NODE_DISTANCE = 0.9f;

	public const float SNAKE_PREDICT_SIZE_X = 2.4f;
	public const float SNAKE_PREDICT_SIZE_Y = 6.0f;

	public const string DEFAULT_SNAKE_TWEAKABLE_PROPERTIES = "Default";

	public const float UPDATE_SNAKE_MOVEMENT_TIEM_INTERVAL = 0.1f;
	public const int UPDATE_RESPAWN_FRAME_INTERVAL = 3;
	public const int UPDATE_ALL_AI_FRAME = 5;

	#region maybe package to aiSetting
	public const float SNAKE_AIMOVEMENT_SAFEAREA_MAP_EDGE = 5;
	public const float SNAKE_DETECT_DISTANCE = SNAKE_NODE_TO_NODE_DISTANCE * 10.0f;
	public const float SNAKE_DETECT_ANGLE = 12.0f;
	public const int SNAKE_AI_CALCULATE_TIMES = (int)(360.0f / SNAKE_DETECT_ANGLE);
	public const float SNAKE_BEGIN_DETECT_RAND_MAXANGLE = 60.0f;
	public const int SNAKE_RANDMOVEMOENT_WHENNOTCHANGED_TIMES = 5;
	public const float SNAKE_RANDMOVEMOENT_WHENNOTCHANGED_PROBABILITY = 0.2f;
	#endregion

	public enum Layer
	{
		Wall = 8,
		Food = 9,
		SnakeHead = 10,
		Snake = 11,
		SnakePredict = 12,
	}
}