using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicroGameWater1AController : MicroGameController
{
    public GameObject[] G_StartPositions;
    public GameObject G_BallAndCup;
    public GameObject G_Snowball;
    public GameObject G_WaterDrip;
    public GameObject G_BunsenBurner;

    private const float kMoveSpeed = 0.55f;
    private const float kSnowballTargetWidth = 0.5f;
    private const float kMeltingChangeScaleVelocity = 0.02f;
    private const float kBaseWaterDripRate = 0.5f;
    private const float kHeatedWaterDripRate = 80.0f;
    private Vector3 kInitialSnowballScale;
    private ParticleSystem mWaterDripParticleSystem;

    public override WorkstationData.WorkstationType GetDesire()
    {
        return WorkstationData.WorkstationType.water_1;
    }

    protected override void _Reinitialize()
    {
    }

    protected override void _ResetGame()
    {
    }

    protected override void _StartGame()
    {
        State = MicroState.Playing;
        G_Snowball.transform.localScale = kInitialSnowballScale;

        int startIndex = (int)(UnityEngine.Random.Range(0f, 1f) * G_StartPositions.Length);
        GameObject startPos = G_StartPositions[startIndex];

        Vector3 readonlygarbageunitydedgaem = G_BallAndCup.transform.position;
        G_BallAndCup.transform.position = new Vector3(startPos.transform.position.x, readonlygarbageunitydedgaem.y, readonlygarbageunitydedgaem.z);

        readonlygarbageunitydedgaem = G_BunsenBurner.transform.localPosition;
        G_BunsenBurner.transform.localPosition = new Vector3(0.0f, readonlygarbageunitydedgaem.y, readonlygarbageunitydedgaem.z);
    }

    private void Start()
    {
        Debug.Assert(G_StartPositions != null);
        Debug.Assert(G_StartPositions.Length > 0);
        Debug.Assert(G_BallAndCup != null);
        Debug.Assert(G_Snowball != null);
        Debug.Assert(G_WaterDrip != null);
        Debug.Assert(G_BunsenBurner != null);

        kInitialSnowballScale = G_Snowball.transform.localScale;
        mWaterDripParticleSystem = G_WaterDrip.GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        switch (State)
        {
            case MicroState.Playing:
                TickPlaying();
                break;
        }
    }

    private void TickPlaying()
    {
        Vector3 burnerDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.A))
        {
            burnerDirection.x -= kMoveSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            burnerDirection.x += kMoveSpeed;
        }

        G_BunsenBurner.transform.Translate(burnerDirection);

        float ballDistSqr = (G_Snowball.transform.position - G_BunsenBurner.transform.position).sqrMagnitude;
        Debug.Log(ballDistSqr);
        // Check if snowball is over burner
        //bool isBurnerUnderSnowball = System.Math.Abs(G_BunsenBurner.transform.position.x - G_Snowball.transform.position.x) < kSnowballTargetWidth;
        Debug.DrawLine(G_Snowball.transform.position, G_BunsenBurner.transform.position, Color.yellow);
        bool isBurnerUnderSnowball = ballDistSqr < 120f;

        ParticleSystem.EmissionModule emission = mWaterDripParticleSystem.emission;

        emission.rateOverTime = isBurnerUnderSnowball ? kHeatedWaterDripRate : kBaseWaterDripRate;
        G_Snowball.transform.localScale -= new Vector3(1, 1, 1) * (isBurnerUnderSnowball ? kMeltingChangeScaleVelocity : 0.0f);

        if (G_Snowball.transform.localScale.magnitude < 0.2f)
        {
            State = MicroState.Victory;
        }
    }
}