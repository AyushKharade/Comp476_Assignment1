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
    public bool Always_Flee;
    public bool Allow_Unfreezing;
    //public bool Kinematic_Wander;

    // other public variables
    public float speed = 10f;           // then some random offset in Start()
    public float WanderRotation = 15f;

    [Header("Target:")]
    public Transform Target;
    public Transform SecondaryTarget;
    //Vector3 WanderTarget;


    [Header("Additional Parameters")]
    public float align_Rotation_Speed;

    // if chaser doesnt catch someone for a while, increase speed
    float chaseTimer = 0;
    float wanderTimer = 0;
    float unfreezeCheckTimer = 0;
    float unfreezeCheckTime;
    public float wanderChangeTime = 0.2f;             // change orientation every 0.2 seconds

    void Start()
    {
        //speed randomizer:
        speed += Random.Range(-1.5f, 1.5f);

        //set animator's blend value to 0.8 so they are running.
        //GetComponent<Animator>().SetFloat("Blend",0.4f);
        unfreezeCheckTime= Random.Range(1f, 4f); 
    }

    // Update is called once per frame
    void Update()
    {
        if (type + "" == "Chasing")
        {
            if (Target != null)
                Kinematic_SeekBehavior();
            else if(transform.parent.GetComponent<Game>().RunnerCount>0)
                ChaserFindNewTarget();
        }
        else if (type + "" == "Fleeing" && !frozen)
        {
            if(Always_Flee && Target!=null)
                Kinematic_FleeBehavior();
            else
                RunnerBehavior();
        }
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
        Vector3 drawRay_Origin = new Vector3(transform.position.x, transform.position.y + 8, transform.position.z);
        Debug.DrawRay(drawRay_Origin, (Target.position - drawRay_Origin), Color.green);

        Debug.DrawRay(drawRay_Origin, transform.forward * 5f, Color.red);

        // every 5 seconds, find new target (so if anyone else is closer, chase them instead.
        chaseTimer += Time.deltaTime;
        if (chaseTimer > 5f && chaseTimer < 5.10f)
            ChaserFindNewTarget();
        if (chaseTimer > 10f)
        {
            speed += 1;
            chaseTimer = 0;
        }
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
        if (Vector3.Distance(Target.position, transform.position) < 10 || true)
        {
            transform.position += fleeVelocity * Time.deltaTime;
        }
        /*
        else
        {
            //make sure to check if you are facing your target.
            float angle = 270f;
            if (Vector3.Angle(transform.forward, transform.position - Target.position) > angle)
            {
                //allowed to move
                transform.position += fleeVelocity * Time.deltaTime;
            }
            // other wise move at half speed
            else
            {
                fleeVelocity = Dir * 0.05f * speed;
                transform.position += fleeVelocity * Time.deltaTime;
            }
        }
        */


        //align orientation
        AlignOrientation(2);

        
    }


    void Kinematic_WanderBehavior()
    {
        // randomly change orientations by a fixed value multiplied by random(-1,+1)
        // translate on forward axis


        wanderTimer += Time.deltaTime;
        if (wanderTimer > wanderChangeTime)
        {
            float r = Random.Range(-1f, 1f);
            r *= WanderRotation;
            r *= 50;

            transform.Rotate(new Vector3(0, r * Time.deltaTime, 0));
            wanderTimer = 0;
        }

        Vector3 wanderDirectionSpeed = speed * transform.forward;
        transform.position += wanderDirectionSpeed * Time.deltaTime;
    }
  

    // Arrive is used by fleeing characters who are able to unfreeze, thats why it pursues secondary target and not Target (which is chaser)
    void Kinematic_ArriveBehavior()
    {
        // define 2 radius of satisfaction r1, r2
        // if distance to target > r1, arrive at full speed
        // if distance <r1 & >r2, keep going slower depening on the distance.
        // if inside r2, stop completely.

        if (!SecondaryTarget.GetComponent<BehaviorScript>().frozen)                  // if someone else unfreezes a target.
            SecondaryTarget = null;
        else
        {
            float radius1 = 10f;
            float radius2 = 2f;

            Vector3 arriveVelocity;

            if (SecondaryTarget != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, SecondaryTarget.position);
                Vector3 Dir = (SecondaryTarget.position - transform.position).normalized;

                if (distanceToTarget > radius1)
                {
                    arriveVelocity = Dir * speed;
                }
                else if (distanceToTarget < radius1 && distanceToTarget > radius2)
                {
                    // go slower depending on the distance.
                    float speedFactor = (distanceToTarget / 10f);
                    arriveVelocity = Dir * speed * speedFactor;
                }
                else
                {
                    arriveVelocity = Vector3.zero;
                }

                transform.position += arriveVelocity * Time.deltaTime;
                AlignOrientation(4);

                //draw ray
                Vector3 drawRay_Origin = new Vector3(transform.position.x, transform.position.y + 8, transform.position.z);
                Debug.DrawRay(drawRay_Origin, (SecondaryTarget.position - drawRay_Origin), Color.blue);
            }
            else
            {
                Debug.Log("Secondary Target is NULL, called Arrive wrongly.");
            }
        }
    }
    

    // runner behavior
    void RunnerBehavior()
    {
        // if target isnt chasing you, wander or unfreeze target if allowed  to, if chasing you flee
        if (Target == null)
        {
            Kinematic_WanderBehavior();
        }
        else
        {
            // see if chaser's target is you, if yes, flee else wander
            Transform obj = Target.GetComponent<BehaviorScript>().Target;

            if (obj == null)
            {
                Kinematic_WanderBehavior();
            }
            else if (obj.name == transform.name || Always_Flee)
            {
                Kinematic_FleeBehavior();
            }
            else if (Allow_Unfreezing && SecondaryTarget == null)
            {
                /*
                unfreezeCheckTimer += Time.deltaTime;
                if (unfreezeCheckTimer > unfreezeCheckTime)
                {
                    //Debug.Log("Called function to find a unfreeze target");
                    unfreezeCheckTimer = 0;
                    FindUnfreezeTarget();
                    if (SecondaryTarget != null)
                        Debug.Log("Found a target to unfreeze");
                }
                else
                {
                    Kinematic_WanderBehavior();
                }*/
                FindUnfreezeTarget();
                if (SecondaryTarget == null)
                    Kinematic_WanderBehavior();
                else
                    Kinematic_ArriveBehavior();
            }
            else if (SecondaryTarget != null)
            {
                Kinematic_ArriveBehavior();
            }
            else
            {
                Kinematic_WanderBehavior();
            }
        }
    }


    void FindUnfreezeTarget()
    {
        Collider[] temp = Physics.OverlapSphere(transform.position, 100f);
        // find closest
        Collider UnfreezeTarget = null;
        foreach (Collider c in temp)
        {
            if (c.transform.tag == "NPC" && c.GetComponent<BehaviorScript>().type + "" == "Fleeing" && c.GetComponent<BehaviorScript>().frozen)
            {
                UnfreezeTarget = c;
                SecondaryTarget = c.transform;
                //Debug.Log("Secondary Target Assigned " + SecondaryTarget.transform.name + "! for Runner: " + transform.name);
                break;
            }
        }
        //SecondaryTarget = null;
    }

    void AlignOrientation(int id)   // if id==1, face towards, if id==2, face away, if id==3, face in forward direction, if id==4, face secondary target
    {
        // get direction
        Quaternion lookDirection;
        Vector3 Dir;

        //get direction towards target:
        if (id == 1)
            Dir = (Target.position - transform.position).normalized;
        else if (id == 2)
            Dir = (transform.position - Target.position).normalized;
        else if (id == 3)
            Dir = transform.forward;
        else
            Dir = (SecondaryTarget.position-transform.position).normalized;
        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(Dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, align_Rotation_Speed);

    }

    // Behaviors Above
    // ################################################################


    // Helper Methods
    public void ChaserFindNewTarget()
    {
        Collider[] temp = Physics.OverlapSphere(transform.position, 100f);
        // find closest
        float shortestDistance = 1000;
        Collider shortestTarget = null;
        foreach (Collider c in temp)
        {
            if (Vector3.Distance(transform.position, c.transform.position) < shortestDistance
                && c.transform.tag == "NPC" && c.GetComponent<BehaviorScript>().type + "" == "Fleeing" && !c.GetComponent<BehaviorScript>().frozen)
            {
                //Debug.Log("Potential Target: "+c.transform.name);
                if (!c.gameObject.GetComponent<BehaviorScript>().frozen)
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



    // freeze tag methods
    public void SetChaser()
    {
        tagged = true;
        type = npcType.Chasing;
        //GetComponent<Renderer>().material = Red_mat;
        transform.GetChild(2).GetComponent<Renderer>().material = Red_mat;
        Debug.Log("Chaser set: " + transform.name);
        Target = null;

        //give a speed boost
        speed += 3;

        //set anim
        //GetComponent<Animator>().SetFloat("Blend",1);
    }

    public void SetFleeFromTarget(Transform target)
    {
        Target = target;
    }

    public void SetFrozen()
    {
        frozen = true;
        //GetComponent<Renderer>().material = Frozen_mat;
        transform.GetChild(2).GetComponent<Renderer>().material = Frozen_mat;
    }

    //reinit
    public void ResetStates()
    {
        frozen = false;
        tagged = false;
        type = npcType.Fleeing;
        transform.GetChild(2).GetComponent<Renderer>().material = Green_mat;

        GetComponent<Animator>().SetFloat("Blend", 0);
    }

    public void ResetSpeeds()
    {
        speed = 10;
        speed += Random.Range(-1.5f, 1.5f);
    }





    // collision
    private void OnCollisionEnter(Collision collision)
    {
        if (type + "" == "Chasing")
        {
            if (collision.collider.tag == "NPC" && !collision.collider.GetComponent<BehaviorScript>().frozen)
            {
                //collision.collider.GetComponent<Rigidbody>().AddForce(50*Vector3.up,ForceMode.Impulse);
                collision.collider.GetComponent<BehaviorScript>().SetFrozen();
                Target = null;

                //update counter in game.cs
                transform.parent.GetComponent<Game>().RunnerCount -= 1;

                // get faster everytime you catch someone
                speed += 0.5f;

                chaseTimer = 0;

                GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
        else if (type + "" == "Fleeing" && Allow_Unfreezing)
        {
            if (collision.collider.tag == "NPC" && collision.collider.GetComponent<BehaviorScript>().frozen && collision.collider.name==SecondaryTarget.name)
            {
                collision.collider.GetComponent<BehaviorScript>().ResetStates();
                transform.parent.GetComponent<Game>().RunnerCount += 1;
                SecondaryTarget = null;
            }
        }
    }
}
