using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorScript : MonoBehaviour
{
    /*
     * Structure:
     * 
     * Bool for each behavior, public, enable / disable
     * One common script.
     * SetFunctions for setting a tagged npc
     * 
     
     */

    //variables
    [Header("Flag Materials")]
    public Material Red_mat;
    public Material Green_mat;
    public Material Frozen_mat;


    [Header("State")]
    public bool tagged;
    public bool frozen;

    public enum npcType { Chasing, Fleeing };
    public npcType type = new npcType();

    [Header("Behavior: Chasing")]
    public bool Kinematic_Seek;
    public bool Kinematic_Arrive;


    [Header("Behavior: Fleeing")]
    public bool Kinematic_Flee;
    public bool Kinematic_Wander;

    // other public variables
    public float speed = 10f;           // then some random offset in Start()
    public float WanderRotation = 15f;

    [Header("Target:")]
    public Transform Target;


    [Header("Additional Parameters")]
    public float align_Rotation_Speed;

    // if chaser doesnt catch someone for a while, increase speed
    float chaseTimer = 0;

    void Start()
    {
        //speed randomizer:
        speed += Random.Range(-1.5f, 1.5f);

        //set animator's blend value to 0.8 so they are running.
        //GetComponent<Animator>().SetFloat("Blend",0.8f);
    }

    // Update is called once per frame
    void Update()
    {
        if (type + "" == "Chasing")
            Kinematic_SeekBehavior();
        else if (type + "" == "Fleeing")
            Kinematic_FleeBehavior();
    }


    // ################################################################
    // Behaviors Below

        // chase behaviors

    void Kinematic_SeekBehavior()
    {
       
        /* If distance is small, character can directly go towards it
         * If its large, then character must face it before going towards it.
         */
        Vector3 Dir = (Target.position - transform.position).normalized;            // Find Normalized Direction
        Vector3 seekVelocity = Dir * speed;                                         // find seek velocity (max speed * direction)


        // Check Distance, if its larger, then check if you are facing the target.
        if (Vector3.Distance(Target.position, transform.position) < 10)
        {
            transform.position += seekVelocity * Time.deltaTime;
        }
        else
        {
            //make sure to check if you are facing your target.
            float angle = 10f;
            if (Vector3.Angle(transform.forward, (Target.position - transform.position)) < angle)
            {
                //allowed to move
                transform.position += seekVelocity * Time.deltaTime;
            }
            // other wise move at half speed
            else
            {
                seekVelocity = Dir * 0.5f * speed;
                transform.position += seekVelocity * Time.deltaTime;
            }
        }



        //align orientation
        AlignOrientation(1);


        //draw raycast for details
        Vector3 drawRay_Origin = new Vector3(transform.position.x, transform.position.y+8, transform.position.z);
        Debug.DrawRay(drawRay_Origin, (Target.position-drawRay_Origin), Color.green);

        Debug.DrawRay(drawRay_Origin, transform.forward*5f, Color.red);
    }



    // flee behaviors

    void Kinematic_FleeBehavior()
    {
        /*
         * If chaser is not actively chasing you, wander or unfreeze targets
         * if chaser is chasing you, flee
         */

        //get direction away from target, flee, face away 
        
        Vector3 Dir = (transform.position- Target.position).normalized;            // Find Normalized Direction
        Vector3 fleeVelocity = Dir * speed;                                         // find seek velocity (max speed * direction)

        // Check Distance, if its larger, then check if you are facing the target.
        if (Vector3.Distance(Target.position, transform.position) < 10)
        {
            transform.position += fleeVelocity * Time.deltaTime;
        }
        else
        {
            //make sure to check if you are facing your target.
            float angle = 10f;
            if (Vector3.Angle(transform.forward, (transform.position - Target.position)) < angle)
            {
                //allowed to move
                transform.position += fleeVelocity * Time.deltaTime;
            }
            // other wise move at half speed
            else
            {
                fleeVelocity = Dir * 0.5f * speed;
                transform.position += fleeVelocity * Time.deltaTime;
            }
        }


        //align orientation
        AlignOrientation(2);
    }


    void AlignOrientation(int id)   // if id==1, face towards, if id==2, face away
    {
        // get direction
        Quaternion lookDirection;
        Vector3 Dir;

        //get direction towards target:
        if (id == 1)
            Dir = (Target.position - transform.position).normalized;
        else
            Dir = (transform.position - Target.position.normalized);

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(Dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, align_Rotation_Speed);

    }

    // Behaviors Above
    // ################################################################


    // Helper Methods
}
