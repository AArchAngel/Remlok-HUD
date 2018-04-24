using System;
using UnityEngine;
using UnityEngine.UI;

public class TimeUpdate : MonoBehaviour
{
    void Update()
    {
        GetComponent<Text>().text = DateTime.Now.ToString("h:mm:ss tt");
    }
}
