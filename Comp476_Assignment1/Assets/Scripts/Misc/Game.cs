using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    float TimeBetweenRounds = 4f;
    float Timer=0;

    public int RunnerCount = 5;

    bool firstGame=true;
    bool gameStarted;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;
        if (Timer > 3f && !gameStarted)
        {
            StartGame();
            gameStarted = true;
        }

        CheckRoundEnd();

        //Debug.DrawRay(transform.position, (Vector3.zero - transform.position), Color.green);
    }

    void StartGame()
    {
        // reset everything if its not the first time
        if (!firstGame)
        {
            //reset
            Debug.Log("Reseting all NPCS before next round.");

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<NPC>().ResetStates();
                transform.GetChild(i).GetComponent<NPC>().ResetSpeeds();
                transform.GetChild(i).GetComponent<NPC>().SetRunner();
            }
        }
        else
        {
            firstGame = false;
            Debug.Log("Started First round.");
        }


        //randomly make one npc tagged.
        int r = Random.Range(0, transform.childCount);
        GameObject target = transform.GetChild(r).gameObject;
        target.GetComponent<NPC>().SetChaser();

        // set everyone else's target to this
        for(int i=0;i<transform.childCount;i++)
        {
            if (transform.GetChild(i).transform.name != target.transform.name)
            {
                transform.GetChild(i).GetComponent<NPC>().SetFleeFromTarget(target.transform);
            }
        }
    }

    void CheckRoundEnd()
    {
        if (RunnerCount == 0)
        {
            gameStarted = false;
            Timer = 0;
            RunnerCount = 5;
            Debug.Log("End Of Round");
        }
    }
}
