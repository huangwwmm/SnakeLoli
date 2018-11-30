using System;
using System.Collections.Generic;
using UnityEngine;

public class slSnakeSystem
{
	private Transform m_PoolRoot;
	private Dictionary<string, int> m_SnakeNameToIndex;
	private Dictionary<string, slSnakeTweakableProperties> m_TweakablePropertiess;
	private List<slSnakeProperties> m_Propertiess;
	private List<NodePool<slSnake.HeadNode>> m_HeadPools;
	private List<NodePool<slSnake.ClothesNode>> m_ClothesNodePools;
	private List<NodePool<slSnake.BodyNode>> m_BodyPools;
	private hwmQuadtree<slSnake.QuadtreeElement> m_Quadtree;

	public void Initialize()
	{
		m_PoolRoot = new GameObject("Snakes").transform;
		m_SnakeNameToIndex = new Dictionary<string, int>();
		m_TweakablePropertiess = new Dictionary<string, slSnakeTweakableProperties>();
		m_Propertiess = new List<slSnakeProperties>();

		m_HeadPools = new List<NodePool<slSnake.HeadNode>>();
		m_ClothesNodePools = new List<NodePool<slSnake.ClothesNode>>();
		m_BodyPools = new List<NodePool<slSnake.BodyNode>>();

		m_Quadtree = new hwmQuadtree<slSnake.QuadtreeElement>();
		m_Quadtree.Initialize(CalculateQuadtreeDepth()
			, slConstants.SNAKE_QUADTREE_MAXELEMENT_PERNODE
			, slConstants.SNAKE_QUADTREE_MINELEMENT_PREPARENTNODE
			, new Vector2(slConstants.SNAKE_QUADTREE_LOOSESIZE, slConstants.SNAKE_QUADTREE_LOOSESIZE)
			, slWorld.GetInstance().GetMap().GetMapBox());
		m_Quadtree.AutoMergeAndSplitNode = false;
#if UNITY_EDITOR
		slQuadtreeGizmos.SnakeQuadtree = m_Quadtree;
#endif
	}

	public void Dispose()
	{
		for (int iPool = 0; iPool < m_BodyPools.Count; iPool++)
		{
			m_HeadPools[iPool].Dispose();
			m_ClothesNodePools[iPool].Dispose();
			m_BodyPools[iPool].Dispose();
		}
		m_HeadPools.Clear();
		m_HeadPools = null;
		m_ClothesNodePools.Clear();
		m_ClothesNodePools = null;
		m_BodyPools.Clear();
		m_BodyPools = null;

		m_Propertiess.Clear();
		m_Propertiess = null;
		m_TweakablePropertiess.Clear();
		m_TweakablePropertiess = null;
		m_SnakeNameToIndex.Clear();
		m_SnakeNameToIndex = null;
		UnityEngine.Object.Destroy(m_PoolRoot.gameObject);
	}

	public hwmQuadtree<slSnake.QuadtreeElement> GetQuadtree()
	{
		return m_Quadtree;
	}

	public slSnakeTweakableProperties GetTweakableProperties(string propertiesName)
	{
		slSnakeTweakableProperties properties;
		if (!m_TweakablePropertiess.TryGetValue(propertiesName, out properties))
		{
			properties = UnityEngine.Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game
				, slConstants.SNAKE_TWEAKABLE_PROPERTIES_PREfAB_NAME_STARTWITHS + propertiesName)) as slSnakeTweakableProperties;
			m_TweakablePropertiess.Add(propertiesName, properties);
		}
		return properties;
	}

	public slSnakeProperties GetProperties(string snakeName)
	{
		return m_Propertiess[SnakeNameToIndex(snakeName)];
	}

	public NodePool<slSnake.HeadNode> GetHeadPool(string snakeName)
	{
		return m_HeadPools[SnakeNameToIndex(snakeName)];
	}

	public NodePool<slSnake.ClothesNode> GetClothesPool(string snakeName)
	{
		return m_ClothesNodePools[SnakeNameToIndex(snakeName)];
	}

	public NodePool<slSnake.BodyNode> GetBodyPool(string snakeName)
	{
		return m_BodyPools[SnakeNameToIndex(snakeName)];
	}

	private int SnakeNameToIndex(string snakeName)
	{
		int index;
		if (!m_SnakeNameToIndex.TryGetValue(snakeName, out index))
		{
			Debug.LogError("Not loaded snake: " + snakeName);
		}
		return index;
	}

	public void LoadSnake(string snakeName)
	{
		int index;
		if (!m_SnakeNameToIndex.TryGetValue(snakeName, out index))
		{
			index = m_Propertiess.Count;
			m_SnakeNameToIndex.Add(snakeName, index);

			m_Propertiess.Add(UnityEngine.Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader().LoadAsset(hwmAssetLoader.AssetType.Game
				, slConstants.SNAKE_PROPERTIES_PREfAB_NAME_STARTWITHS + snakeName)) as slSnakeProperties);

			slSnakePresentationProperties snakePresentationProperties = slWorld.GetInstance().NeedPresentation()
				? UnityEngine.Object.Instantiate(hwmSystem.GetInstance().GetAssetLoader()
						.LoadAsset(hwmAssetLoader.AssetType.Game, slConstants.SNAKE_PRESENTATION_PROPERTIES_PREfAB_NAME_STARTWITHS + snakeName) as GameObject)
					.GetComponent<slSnakePresentationProperties>()
				: null;
			snakePresentationProperties.transform.SetParent(m_PoolRoot, false);
			snakePresentationProperties.gameObject.SetActive(false);

			NodePool<slSnake.HeadNode> headNodePool = new NodePool<slSnake.HeadNode>(snakePresentationProperties, m_PoolRoot, m_Quadtree);
			headNodePool.Initialize(1);
			m_HeadPools.Add(headNodePool);

			NodePool<slSnake.ClothesNode> clothesNodePool = new NodePool<slSnake.ClothesNode>(snakePresentationProperties, m_PoolRoot, m_Quadtree);
			clothesNodePool.Initialize(1);
			m_ClothesNodePools.Add(clothesNodePool);

			NodePool<slSnake.BodyNode> bodyNodePool = new NodePool<slSnake.BodyNode>(snakePresentationProperties, m_PoolRoot, m_Quadtree);
			bodyNodePool.Initialize(slConstants.SNAKE_POOL_BODYNODE_CACHECOUNT_PRESNAKE);
			m_BodyPools.Add(bodyNodePool);
		}
		else
		{
			m_HeadPools[index].Create(1);
			m_ClothesNodePools[index].Create(1);
			m_BodyPools[index].Create(slConstants.SNAKE_POOL_BODYNODE_CACHECOUNT_PRESNAKE);
		}
	}

	private int CalculateQuadtreeDepth()
	{
		int depth = 1;
		Vector2 size = slWorld.GetInstance().GetMap().GetMapBox().GetSize();
		while (true)
		{
			if (size.x < slConstants.SNAKE_QUADTREE_MAXDEPTH_BOXSIZE
				|| size.y < slConstants.SNAKE_QUADTREE_MAXDEPTH_BOXSIZE)
			{
				break;
			}
			size *= 0.5f;
			depth++;
		}
		return depth;
	}

	public class NodePool<T> : hwmPool<T> where T : slSnake.BodyNode, new()
	{
		private Transform m_Root;
		private slSnakePresentationProperties m_Presentation;
		private NodeType m_NodeType;
		private hwmQuadtree<slSnake.QuadtreeElement> m_Quadtree;

		public NodePool(slSnakePresentationProperties presentation, Transform root, hwmQuadtree<slSnake.QuadtreeElement> quadtree)
		{
			m_Presentation = presentation;
			m_Root = root;
			m_Quadtree = quadtree;

			Type nodeType = typeof(T);
			m_NodeType = nodeType == typeof(slSnake.HeadNode)
				? NodeType.Head
				: nodeType == typeof(slSnake.ClothesNode)
					? NodeType.Clothes
					: NodeType.Body;
		}

		protected override T HandleCreateItem()
		{
			T node = new T();

			if (m_Presentation != null)
			{
				switch (m_NodeType)
				{
					case NodeType.Head:
						node.Node = UnityEngine.Object.Instantiate(m_Presentation.Head);
						(node as slSnake.HeadNode).Sprite = node.Node.GetComponentsInChildren<SpriteRenderer>();
						break;
					case NodeType.Clothes:
						node.Node = UnityEngine.Object.Instantiate(m_Presentation.Clothes);
						node.Sprite = node.Node.GetComponent<SpriteRenderer>();
						break;
					case NodeType.Body:
						node.Node = UnityEngine.Object.Instantiate(m_Presentation.Body);
						node.Sprite = node.Node.GetComponent<SpriteRenderer>();
						break;
				}
			}
			else
			{
				node.Node = new GameObject();
			}

			node.Node.SetActive(false);
			node.Node.name = m_NodeType.ToString();
			node.Node.transform.SetParent(m_Root, false);

			node.Collider = node.Node.AddComponent<CircleCollider2D>();
			node.Collider.isTrigger = m_NodeType == NodeType.Head;
			node.OwnerQuadtree = m_Quadtree;

			node.Layer = (int)m_NodeType;
			if (m_NodeType == NodeType.Head)
			{
				slSnake.HeadNode headNode = node as slSnake.HeadNode;
				headNode.Trigger = headNode.Node.AddComponent<slSnakeHeadTrigger>();
				headNode.Rigidbody = headNode.Node.AddComponent<Rigidbody2D>();
				headNode.Rigidbody.isKinematic = true;

				headNode.Predict = new GameObject("Predict");
				headNode.Predict.SetActive(false);
				headNode.Predict.transform.SetParent(m_Root, false);
				headNode.Predict.layer = (int)slConstants.Layer.SnakePredict;
				headNode.PredictCollider = headNode.Predict.AddComponent<BoxCollider2D>();

				headNode.PredictNode = new slSnake.PredictNode();
				headNode.PredictNode.OwnerQuadtree = m_Quadtree;
			}

			return node;
		}
	}

	private enum NodeType
	{
		Head,
		Clothes,
		Body
	}
}