using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientNetworkManager : NetworkManager
{

	// Use this for initialization
	void Start ()
    {
        networkPort = 7777;
        StartClient();
    }

    // Update is called once per frame
    void Update ()
    {
		
	}
}
