using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    /*
     * Structure:
     * 
     * Bool for each behavior, public, enable / disable
     * One common script.
     * SetFunctions for setting a tagged npc
     * 
     
     */
    public Material Red_mat;
    public Material Green_mat;
    public Material Frozen_mat;


    [Header("State")]
    public bool tagged;
    public bool frozen;

    public enum npcType { Chasing, Fleeing};
    public npcType type = new npcType();

    [Header("Behavior")]
    public bool Kinematic_Flee;
    public bool Kinematic_Seek;

    // other private variables
    public float speed = 10f;           // then some random offset in Start()

    public Transform Target;

    void Start()
    {
        //speed randomizer:
        speed += Random.Range(-1.5f, 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (type+"" == "Chasing" && Target!=null)
        {
            //chasing behavior
            if (Kinematic_Seek)
                Kinematic_SeekBehavior();
        }
        else if(type+"" == "Fleeing" && !frozen && Target!=null)
        {
            //running behaviors
            // flee from target
            if (Kinematic_Flee)
                Kinematic_FleeBehavior();
        }


        if (Target == null)
            FindNewTarget();


        // to eliminate any forces acting on objects
        //GetComponent<Rigidbody>().velocity = Vector3.zero;
    }








    // behaviors:
    void Kinematic_FleeBehavior()
    {
        // get dir away from target
        // normalize
        // orient
        // move at max speed.

        Vector3 dir = (transform.position - Target.position).normalized;
        transform.Translate(dir*speed*Time.deltaTime);
    }


    void Kinematic_SeekBehavior()
    {
        Vector3 dir = (Target.position- transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);
    }

    //getter/setters
    public void SetFrozen()
    {
        frozen = true;
        GetComponent<Renderer>().material = Frozen_mat;
    }

    public void SetChaser()
    {
        tagged = true;
        type = npcType.Chasing;
        GetComponent<Renderer>().material = Red_mat;
        Debug.Log("Chaser set: "+transform.name);
    }

    public void ResetStates()
    {
        frozen = false;
        tagged = false;
    }


    void FindNewTarget()
    {
        Collider[] temp = Physics.OverlapSphere(transform.position, 55f);
        // find closest
        float shortestDistance = 1000;
        Collider shortestTarget = null;
        foreach (Collider c in temp)
        {
            if (Vector3.Distance(transform.position, c.transform.position) < shortestDistance 
                && c.transform.tag == "NPC" && c.GetComponent<NPC>().type+""=="Fleeing" && !c.GetComponent<NPC>().frozen)
            {
                //Debug.Log("Potential Target: "+c.transform.name);
                if (!c.gameObject.GetComponent<NPC>().frozen)
                {
                    shortestDistance = Vector3.Distance(transform.position, c.transform.position);
                    shortestTarget = c;
                }
            }
        }
        // we know shortest target now, seek that target
        if (shortestTarget != null)
            Target = shortestTarget.transform;
    }



    //set target for fleeing npcs
    public void SetFleeFromTarget(Transform target)
    {
        Target = target;
    }













    // collision
    private void OnCollisionEnter(Collision collision)
    {
        if (type + "" == "Chasing")
        {
            if (collision.collider.tag == "NPC")
            {
                //collision.collider.GetComponent<Rigidbody>().AddForce(50*Vector3.up,ForceMode.Impulse);
                collision.collider.GetComponent<NPC>().SetFrozen();
                Target = null;
            }
        }
    } 
}
