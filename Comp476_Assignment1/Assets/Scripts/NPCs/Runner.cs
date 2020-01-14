using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : MonoBehaviour
{
    [Range(-1,1)]
    public int faceDirection=1;
    public float moveSpeed=5;
    bool tagged;

    

    //chaser reference
    public Transform ChaserRef;

    // Start is called before the first frame update
    void Start()
    {
        //ChaserRef = GameObject.FindGameObjectWithTag("Chaser").transform;
        // randomized speed multipler
        float r = Random.Range(-1f, 1f);
        moveSpeed += r;
    }

    // Update is called once per frame
    void Update()
    {
        //if(!tagged)
         //   Flee();
        transform.Translate(transform.forward*moveSpeed * Time.deltaTime);
    }


    void Flee()
    {
        // keep moving away from the chaser.
        // get a opposite direction.
        Vector3 dir = (transform.position - ChaserRef.position).normalized;

        // keep moving at max speed
        transform.Translate(dir * moveSpeed * Time.deltaTime);
    }



    void ChangeDirection()
    {
        faceDirection *= -1;
    }


    public bool IsTagged()
    {
        return tagged;
    }

    public void SetTagged()
    {
        tagged=true;
    }
}
