using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform Target;
    [Range(25,50)]
    public float zoomOffset;
    public float xOffset=30;
    public float yOffset=30;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Target != null)
        {
            Vector3 pos = Target.position;
            pos.y += zoomOffset;
            pos.x += xOffset;
            pos.z += yOffset;

            transform.position = pos;
            transform.LookAt(Target.position);

        }
    }
}
