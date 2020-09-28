using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Mathf.Clamp(player.transform.position.x, -0.41163603f, player.transform.position.x), Mathf.Clamp(player.transform.position.y + 0.3f, player.transform.position.y + 0.5f, 0.522453f),transform.position.z);

    }
}
