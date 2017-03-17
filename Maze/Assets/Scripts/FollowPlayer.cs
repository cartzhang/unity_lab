using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;
    private Transform camera;
    // Use this for initialization
    void Start ()
    {
        camera = Camera.main.transform;
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        camera.transform.position = target.position;
        camera.transform.rotation = target.rotation;

    }
}
