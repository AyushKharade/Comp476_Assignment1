using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerForTesting : MonoBehaviour
{
    public float speed=10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
    }


    void PlayerMovement()
    {
        if (Input.GetKey(KeyCode.W))
            transform.Translate(transform.forward * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.S))
            transform.Translate(transform.forward * -speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.A))
            transform.Rotate(new Vector3(0,15*-speed * Time.deltaTime,0));

        if (Input.GetKey(KeyCode.D))
            transform.Rotate(new Vector3(0, 15*speed * Time.deltaTime, 0));


    }
}
