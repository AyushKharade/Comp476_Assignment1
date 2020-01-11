using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chaser : MonoBehaviour
{
    [Range(-1, 1)]
    public int faceDirection = 1;
    public float moveSpeed;
    


    //chaser reference
    public Transform TargetRef;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetRef == null)
            FindNewTarget();
        else
            Seek();
        
    }

    void FindNewTarget()
    {
        Collider []temp=Physics.OverlapSphere(transform.position,5f);
        // find closest
        float shortestDistance = 100;
        Collider shortestTarget=null;
        foreach (Collider c in temp)
        {
            if (Vector3.Distance(transform.position, c.transform.position) < shortestDistance)
            {
                shortestDistance = Vector3.Distance(transform.position, c.transform.position);
                shortestTarget = c;
            }
        }
        // we know shortest target now, seek that target
        TargetRef = shortestTarget.transform;
    }

    void Seek()
    {
        // keep moving away from the chaser.
        // get a facing direction.
        Vector3 dir = (TargetRef.position - transform.position).normalized;

        // keep moving at max speed
        transform.Translate(dir * moveSpeed * Time.deltaTime);
    }
}
