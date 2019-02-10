using Mirror;
using UnityEngine;

public class SimplePlayerController : NetworkBehaviour
{
    public float speed = 5;
	
	// Update is called once per frame
	void Update ()
    {
        // movement for local player
        if (!isLocalPlayer) return;

        if(Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.forward * -speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.right * -speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
    }
}
