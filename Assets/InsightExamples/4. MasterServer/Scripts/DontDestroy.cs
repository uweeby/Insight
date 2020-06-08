using UnityEngine;

namespace Insight.Examples
{
	public class DontDestroy : MonoBehaviour
	{
		// Use this for initialization
		void Start()
		{
			DontDestroyOnLoad(this);
		}
	}
}
