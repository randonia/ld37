using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicroGameCoffee1AController : MicroGameController
{
    public GameObject G_CoffeePot;
    public GameObject G_Cup;
    public float range_X;
    public float potSpeed = 5.0f;

    public override void StartGame()
    {
        throw new NotImplementedException();
    }

    // Use this for initialization
    private void Start()
    {
        Debug.Assert(G_CoffeePot != null);
        Debug.Assert(G_Cup != null);
    }

    // Update is called once per frame
    private void Update()
    {
        float potPosX = Mathf.Sin(Time.time * potSpeed) * range_X;
        Vector3 potPos = G_CoffeePot.transform.position;
        potPos.x = potPosX;
        G_CoffeePot.transform.position = potPos;
    }
}