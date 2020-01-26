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
    
    // if chaser doesnt catch someone for a while, increase speed
    float chaseTimer = 0;

    void Start()
    {
        //speed randomizer:
        speed += Random.Range(-1.5f, 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        Kinematic_SeekBehavior();
    }


    // ################################################################
    // Behaviors Below

    void Kinematic_SeekBehavior()
    {
       
        /* If distance is small, character can directly go towards it
         * If its large, then character must face it before going towards it.
         * 
         */

        Vector3 Dir = (Target.position - transform.position).normalized;

        // find seek velocity (max speed * direction)
        Vector3 seekVelocity = Dir * speed;
        transform.position += seekVelocity * Time.deltaTime;


        //align orientation
        AlignOrientation();
    }

    // Orientation Function
    // look towards function
    void LookTarget(int id)
    {
        // if id==1; look at target, you are the chaser
        // if id==2; look away from target, you are the runner
        if (id == 1)
        {
            Vector3 towardsTarget_Dir = (Target.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(towardsTarget_Dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
        }
        else if (id == 2)
        {
            Vector3 towardsTarget_Dir = (transform.position - Target.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(towardsTarget_Dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
        }
    }

    void AlignOrientation()
    {
        // get direction
        Quaternion lookDirection;
        Vector3 Dir;
        float rotationSpeed = 5f;

        //get direction towards target:
        Dir = (Target.position - transform.position).normalized;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(Dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, rotationSpeed);

    }

    // Behaviors Above
    // ################################################################


    // Helper Methods
}
