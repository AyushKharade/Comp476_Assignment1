using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    float TimeBetweenRounds = 4f;
    float Timer=0;
    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartGame()
    {
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
}
