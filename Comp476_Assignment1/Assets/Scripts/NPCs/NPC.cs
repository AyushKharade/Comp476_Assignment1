﻿using System.Collections;
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


    // if chaser doesnt catch someone for a while, increase speed
    float chaseTimer = 0;

    void Start()
    {
        //speed randomizer:
        speed += Random.Range(-1.5f, 1.5f);

        //Kinematic_Flee = false;
        //Kinematic_Seek = false;
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

        CatchUp();
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
        Target = null;
    }

    public void SetRunner()
    {
        tagged = false;
        type = npcType.Fleeing;
        GetComponent<Renderer>().material = Green_mat;
        //Debug.Log("Runner set: " + transform.name);
    }

    public void ResetStates()
    {
        frozen = false;
        tagged = false;
    }

    public void ResetSpeeds()
    {
        speed = 10;
        speed += Random.Range(-1.5f, 1.5f);
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

        //Debug.Log("Chosen target: "+Target.transform.name); 
    }



    //set target for fleeing npcs
    public void SetFleeFromTarget(Transform target)
    {
        Target = target;
    }






    // chaser catch up, increase chaser's speed if they havent caught anyone in a while
    void CatchUp()
    {
        chaseTimer += Time.deltaTime;
        if (chaseTimer > 15f)
        {
            speed += 1;
            chaseTimer = 0;
        }
    }






    // collision
    private void OnCollisionEnter(Collision collision)
    {
        if (type + "" == "Chasing")
        {
            if (collision.collider.tag == "NPC" && !collision.collider.GetComponent<NPC>().frozen)
            {
                //collision.collider.GetComponent<Rigidbody>().AddForce(50*Vector3.up,ForceMode.Impulse);
                collision.collider.GetComponent<NPC>().SetFrozen();
                Target = null;

                //update counter in game.cs
                transform.parent.GetComponent<Game>().RunnerCount-=1;

                // get faster everytime you catch someone
                speed += 0.5f;

                chaseTimer = 0;
            }
        }
    } 
}
