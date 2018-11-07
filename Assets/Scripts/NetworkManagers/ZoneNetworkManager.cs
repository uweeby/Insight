using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneNetworkManager : NetworkManager
{

	// Use this for initialization
	void Start ()
    {
        networkPort = 7777;
        StartServer();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
