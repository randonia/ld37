using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BakeryController : MonoBehaviour
{
    public GameObject G_Clock;
    private EggTimerController mTimer;

    public bool Inventory;

    // Use this for initialization
    private void Start()
    {
        Debug.Assert(G_Clock != null);
        mTimer = G_Clock.GetComponent<EggTimerController>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (mTimer.Completed && G_Clock.activeSelf)
        {
            Inventory = true;
            mTimer.ResetTimer();
        }
    }

    public bool Interact()
    {
        if (Inventory)
        {
            return true;
        }
        else if (!mTimer.Running)
        {
            G_Clock.SetActive(true);
            mTimer.StartTimer();
        }
        return false;
    }

    public void TakeBread()
    {
        Inventory = false;
        G_Clock.SetActive(false);
    }
}