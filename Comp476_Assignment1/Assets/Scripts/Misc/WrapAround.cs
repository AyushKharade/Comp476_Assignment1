using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapAround : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        // multiple x and z values by -1: so it wraps around on opposite side
        if (other.collider.tag == "NPC")
        {
            //Debug.Log("Collided with PC");
            Vector3 pos = other.transform.position;
            pos.x *= -1;
            if (pos.x < 0)
                pos.x += 0.4f;
            else
                pos.x -= 0.8f;

            pos.z *= -1;
            if (pos.z < 0)
                pos.z += 0.4f;
            else
                pos.z -= 1f;

            other.transform.position = pos;
        }
    }
}
