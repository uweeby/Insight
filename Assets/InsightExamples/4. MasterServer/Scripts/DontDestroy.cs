using UnityEngine;

namespace Insight.Examples.MasterServer
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
