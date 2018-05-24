using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class GameRunningCheck : MonoBehaviour
{

    public bool Running;

    private void Start()
    {
        InvokeRepeating("GameCheck", 0, 1);
    }

    public void GameCheck()
    {


        if (Process.GetProcessesByName("EliteDangerous64").Length > 0)
        {
            Running = true;
        }
        else
        {
            Running = false;
        }
    }

}