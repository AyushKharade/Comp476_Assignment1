using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : MonoBehaviour
{
    [Range(-1,1)]
    public int faceDirection=1;
    public float moveSpeed;
    bool tagged;

    

    //chaser reference
    public Transform ChaserRef;

    // Start is called before the first frame update
    void Start()
    {
        ChaserRef = GameObject.FindGameObjectWithTag("Chaser").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(!tagged)
            Flee();
        //transform.Translate(new Vector3(faceDirection*moveSpeed * Time.deltaTime, 0, 0));
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Chaser")
        {
            Debug.Log("Collided");
            collision.collider.GetComponent<Rigidbody>().AddForce(50 * Vector3.forward, ForceMode.Impulse);
        }
    }
}
