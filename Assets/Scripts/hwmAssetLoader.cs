using UnityEngine;

/// <summary>
/// UNDONE support AssetBundle
/// </summary>
public class hwmAssetLoader
{
	public Object LoadAsset(AssetType assetType, string assetName, bool ignoreLog = false)
	{
		string assetPath = string.Format("{0}/{1}", assetType, assetName);
		Object obj = Resources.Load(assetPath);
		if (obj == null && !ignoreLog)
		{
			Debug.LogError(string.Format("Load AssetType:({0}) Name:({1}) Path:({2}) failed", assetType, assetName, assetPath));
		}
		return obj;
	}

	/// <summary>
	/// enum value need equal folder name
	/// </summary>
	public enum AssetType
	{
		UIRoot,
		Level,
	}
}