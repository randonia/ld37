using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicroGameCoffee1AController : MicroGameController
{
    private const string TARGETNAME = "cup_empty";
    public GameObject G_CoffeePot;
    public GameObject G_Cup;
    public GameObject G_CoffeeSpew;
    private ParticleSystem mCoffeeSpew;
    public float mStartTime;
    public float range_X;
    public float potSpeed = 5.0f;

    protected override void _StartGame()
    {
        State = MicroState.Playing;
        mStartTime = Time.time;
        G_CoffeePot.SetActive(true);
    }

    protected override void _ResetGame()
    {
    }

    // Use this for initialization
    private void Start()
    {
        Debug.Assert(G_CoffeePot != null);
        Debug.Assert(G_Cup != null);
        Debug.Assert(G_CoffeeSpew != null);
        mCoffeeSpew = G_CoffeeSpew.GetComponent<ParticleSystem>();
        mDesire = WorkstationData.WorkstationType.coffee_1;
    }

    // Update is called once per frame
    private void Update()
    {
        switch (State)
        {
            case MicroState.Playing:
                tickPlaying();
                break;
            case MicroState.Victory:
                break;
            case MicroState.Lose:
                break;
        }
    }

    private void tickPlaying()
    {
        float potPosX = Mathf.Sin(mStartTime + Time.time * potSpeed) * range_X;
        Vector3 potPos = G_CoffeePot.transform.localPosition;
        potPos.x = potPosX;
        G_CoffeePot.transform.localPosition = potPos;

        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 targetDir = G_Cup.transform.position - G_CoffeeSpew.transform.position;
            targetDir.x = 0;
            targetDir.Normalize();
            Debug.DrawRay(G_CoffeeSpew.transform.position, targetDir * 10.0f, Color.red, 5.0f);
            RaycastHit hit;
            mCoffeeSpew.Play();
            if (Physics.Raycast(G_CoffeeSpew.transform.position, targetDir * 10.0f, out hit) && hit.collider.gameObject.name == TARGETNAME)
            {
                State = MicroState.Victory;
                ParticleSystem.MainModule main = mCoffeeSpew.main;
                main.startLifetime = 1.13f;
            }
            else
            {
                State = MicroState.Lose;
                ParticleSystem.MainModule main = mCoffeeSpew.main;
                main.startLifetime = 10;
            }
        }
        else
        {
            mCoffeeSpew.Stop();
        }
    }

    public override WorkstationData.WorkstationType GetDesire()
    {
        return mDesire;
    }

    protected override void _Reinitialize()
    {
        mCoffeeSpew.Stop();
        mCoffeeSpew.Clear();
        G_CoffeePot.SetActive(false);
    }
}