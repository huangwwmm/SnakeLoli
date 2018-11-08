using UnityEngine;

public class snDebug
{
	/// <summary>
	/// Q: why not use <see cref="Debug.Assert"/>
	/// A: <see cref="Debug.Assert"/> will not break
	/// </summary>
	/// <param name="valid">Assert if valid equal false</param>
	/// <param name="message">String or object to be converted to string representation for display</param>
	/// <param name="context">Object to which the message applies</param>
	/// <returns>valid</returns>
	public static bool Assert(bool valid, string message, Object context = null)
	{
		if (!valid)
		{
#if UNITY_EDITOR
			UnityEditor.EditorUtility.DisplayDialog("Assert Failed", message, "OK");
#endif
			Debug.LogAssertion(message, context);
			Debug.Break();
			snSystem.GetInstance().GetLogRecord().FlushStreamWriter();
		}
		return valid;
	}
}