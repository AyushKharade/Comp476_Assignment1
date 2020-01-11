using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapAround : MonoBehaviour
{
    public Transform TeleportTo;
   
    private void OnTriggerEnter(Collider other)
    {
        other.transform.position = TeleportTo.position;
        // also turn them 180 degrees
    }
}
