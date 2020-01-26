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
    public bool Kinematic_Arrive;
    public bool Kinematic_Wander;

    // other public variables
    public float speed = 10f;           // then some random offset in Start()
    public float WanderRotation = 15f;

    public Transform Target;


    // if chaser doesnt catch someone for a while, increase speed
    float chaseTimer = 0;

    void Start()
    {
        

        //Kinematic_Flee = false;
        //Kinematic_Seek = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (type + "" == "Chasing" && Target != null)
        {
            //chasing behavior
            if (Kinematic_Seek)
                Kinematic_SeekBehavior();
            else if (Kinematic_Arrive)
                Kinematic_ArriveBehavior();
        }
        else if (type + "" == "Fleeing" && !frozen && Target != null)
        {
            //running behaviors
            // flee from target
            if (Kinematic_Flee)
                Kinematic_FleeBehavior();
            else if (Kinematic_Wander)
                Kinematic_WanderBehavior();
        }


        if (Target == null)
            FindNewTarget();


        // to eliminate any forces acting on objects
        //GetComponent<Rigidbody>().velocity = Vector3.zero;
        if(type+"" == "Chasing")
            CatchUp();
    }








    // behaviors:
    //Behaviors Below #########################################################################################################
    //Behaviors Below #########################################################################################################

    void Kinematic_FleeBehavior()
    {
        Vector3 dir = (transform.position - Target.position).normalized;
        transform.Translate(dir*speed*Time.deltaTime);

        //look away
        //LookTarget(2);
    }


    void Kinematic_SeekBehavior()
    {
        Vector3 dir = (Target.position - transform.position).normalized;

        if (Vector3.Distance(transform.position, Target.position) < 20)
            transform.Translate(dir * speed * Time.deltaTime);
        else
        {
            //make sure that you are facing the target.
            RaycastHit hit;
            Physics.Raycast(transform.position, transform.forward, out hit, 250f);

            if (hit.collider != null)
            {
                if (hit.collider.transform.name == Target.name)
                {
                    transform.Translate(dir * speed * Time.deltaTime);
                }
                else
                {
                    //Debug.Log("Raycast target: " + hit.collider.transform.name);
                }
            }
        }

        //orientation
        LookTarget(1);

        // drawing a raycast
        Debug.DrawRay(transform.position, (Target.position-transform.position), Color.green);
    }

    void Kinematic_ArriveBehavior()
    {
        // keep going at full speed untill distance is greater than d1
        // keep going slowly towards target until distance is d2 < distance < d1
        // if distance > d2 --> stop
        float d1 = 8, d2 = 1.5f;

        //orientation
        LookTarget(1);

        Vector3 dir = (Target.position - transform.position).normalized;
        //get distance
        float distance = Vector3.Distance(transform.position, Target.position);

        if (distance > d1)
            transform.Translate(dir * speed * Time.deltaTime);
        else if (distance > d2)
        {
            //slower
            //Debug.Log("Distance: " + distance);
            float newSpeed = speed * (distance / 10f);
            transform.Translate(dir * newSpeed * Time.deltaTime);
            //Debug.Log("New speed: "+newSpeed);
        }
        else
        {
            // dont move at all
        }

        // drawing a raycast
        Debug.DrawRay(transform.position, (Target.position - transform.position), Color.green);
    }

    void Kinematic_WanderBehavior()
    {
        // randomly change orientations by a fixed value multiplied by random(-1,+1)
        // translate on forward axis

        float r = Random.Range(-1f,1f);
        r *= WanderRotation;
        r *= 50;

        transform.Rotate(new Vector3(0,r*Time.deltaTime,0));
        //transform.Rotate(new Vector3(0,5,0));


        transform.Translate(transform.forward*speed*Time.deltaTime);

    }









    //Behaviors Above #########################################################################################################
    //Behaviors Above #########################################################################################################

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
            Vector3 towardsTarget_Dir = (transform.position-Target.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(towardsTarget_Dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
        }
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
    // also if target goes too far away, look for a new target.
    void CatchUp()
    {
        chaseTimer += Time.deltaTime;
        if (chaseTimer > 15f)
        {
            speed += 1;
            chaseTimer = 0;
        }

        /*
         * because of oscillating while fleeing, checking if target got far away wont work properly
         * 
         * 
        if (Vector3.Distance(Target.transform.position, transform.position) > 25f)
        {
            Debug.Log("Target Too Far ("+ Vector3.Distance(Target.transform.position, transform.position) + " units), finding new target.");
            Target = null;

        }
        */
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

                GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
    } 
}
