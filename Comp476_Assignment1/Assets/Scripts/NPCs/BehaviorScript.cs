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
    public Transform SecondaryTarget;


    [Header("Additional Parameters")]
    public float align_Rotation_Speed;
    public bool allow_unFreezing;

    // if chaser doesnt catch someone for a while, increase speed
    public float chaseTimer = 0;

    void Start()
    {
        //speed randomizer:
        speed += Random.Range(-1.5f, 1.5f);

        //set animator's blend value to 0.8 so they are running.
        //GetComponent<Animator>().SetFloat("Blend",0.4f);
    }

    // Update is called once per frame
    void Update()
    {
        if (type + "" == "Chasing")
        {
            if (Target != null)
                Kinematic_SeekBehavior();
            else
                ChaserFindNewTarget();
        }
        else if (type + "" == "Fleeing" && !frozen)
        {
            RunnerBehavior();
        }
        //Kinematic_FleeBehavior();
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

    void Kinematic_WanderBehavior()
    {
        Vector3 currentRandomPoint = WanderCirclePoint();
        Vector3 moveDirection = (currentRandomPoint - transform.position).normalized;

        //GetComponent<Rigidbody>().velocity = (moveDirection * speed);
        transform.position += moveDirection * speed * Time.deltaTime;

        //align
        AlignOrientation(3);
    }

    float wanderCircleCenterOffset = 200.0f;
    float wanderCircleRadius = 100.0f;
    float maxWanderVariance = 0.0f;


    float distanceFromTarget;


    Vector3 WanderCirclePoint()
    {
        Vector3 wanderCircleCenter = transform.position + (Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized * wanderCircleCenterOffset);
        Vector3 wanderCirclePoint = wanderCircleRadius * (new Vector3(Mathf.Cos(Random.Range(maxWanderVariance, Mathf.PI - maxWanderVariance)),
                                                0.0f,
                                                Mathf.Sin(Random.Range(maxWanderVariance, Mathf.PI - maxWanderVariance))));

        return (wanderCirclePoint + wanderCircleCenter);
    }


    // runner behavior
    void RunnerBehavior()
    {
        // if target isnt chasing you, wander or unfreeze target if allowed  to, if chasing you flee
        if (Target == null)
        {
            Kinematic_WanderBehavior();
        }
        else if (Target.GetComponent<BehaviorScript>().type + "" == "Chasing" && Target.GetComponent<BehaviorScript>().Target.name == transform.name)
        {
            Kinematic_FleeBehavior();
        }
        else
        {
            Kinematic_WanderBehavior();
        }
    }

    void AlignOrientation(int id)   // if id==1, face towards, if id==2, face away, if id==3, face in forward direction
    {
        // get direction
        Quaternion lookDirection;
        Vector3 Dir;

        //get direction towards target:
        if (id == 1)
            Dir = (Target.position - transform.position).normalized;
        else if (id == 2)
            Dir = (transform.position - Target.position.normalized);
        else
            Dir = transform.forward;

        //set quaternion to this dir
        lookDirection = Quaternion.LookRotation(Dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, align_Rotation_Speed);

    }

    // Behaviors Above
    // ################################################################


    // Helper Methods
    public void ChaserFindNewTarget()
    {
        Collider[] temp = Physics.OverlapSphere(transform.position, 55f);
        //Debug.Log("Objects in overlap sphere array: "+temp.Length);
        //foreach (Collider c in temp)
        //{
        //    Debug.Log("Object:"+c.transform.name);
        //}
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
        GetComponent<Animator>().SetFloat("Blend",1);
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
    }
}
