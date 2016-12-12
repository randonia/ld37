using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicroGameTea1AController : MicroGameController
{
    public override WorkstationData.WorkstationType GetDesire()
    {
        return WorkstationData.WorkstationType.tea_1;
    }

    public GameObject[] G_StartPositions;
    public GameObject G_StartPos;
    public GameObject[] G_EndPositions;
    public GameObject G_EndPos;
    public GameObject G_TeaCup;
    public GameObject G_Teabag;
    private const float kSpeed = 0.55f;
    private const float kBagSpeed = 0.45f;

    protected override void _Reinitialize()
    {
    }

    protected override void _ResetGame()
    {
        G_Teabag.SetActive(false);
    }

    protected override void _StartGame()
    {
        State = MicroState.Playing;
        G_Teabag.SetActive(true);
        int startIndex = (int)(UnityEngine.Random.Range(0f, 1f) * G_StartPositions.Length);
        int teacupIndex = (UnityEngine.Random.Range(0f, 1f) < 0.5f) ? 1 : 2;
        int endIndex = (int)(UnityEngine.Random.Range(0f, 1f) * G_EndPositions.Length);
        G_StartPos = G_StartPositions[startIndex];
        G_EndPos = G_EndPositions[endIndex];
        G_Teabag.transform.position = G_StartPos.transform.position;
        Vector3 cupSampleTransform = G_StartPositions[teacupIndex].transform.position;
        cupSampleTransform.y = G_TeaCup.transform.position.y;
        cupSampleTransform.z = G_TeaCup.transform.position.z;
        G_TeaCup.transform.position = cupSampleTransform;
    }

    // Use this for initialization
    private void Start()
    {
        Debug.Assert(G_StartPos != null);
        Debug.Assert(G_StartPositions != null);
        Debug.Assert(G_EndPos != null);
        Debug.Assert(G_EndPositions != null);
        Debug.Assert(G_TeaCup != null);
        Debug.Assert(G_Teabag != null);
        Debug.Assert(G_StartPositions.Length > 0);
        Debug.Assert(G_EndPositions.Length > 0);
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

    // Update is called once per frame
    private void TickPlaying()
    {
        Vector3 bagDirection = (G_EndPos.transform.position - G_StartPos.transform.position).normalized;
        Vector3 cupDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
        {
            cupDirection.x -= kSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            cupDirection.x += kSpeed;
        }
        G_TeaCup.transform.Translate(cupDirection);
        G_Teabag.transform.position += bagDirection * kBagSpeed;
        // Ending check
        float distanceSqr = (G_Teabag.transform.position - G_EndPos.transform.position).sqrMagnitude;
        float startDistanceSqr = (G_Teabag.transform.position - G_StartPos.transform.position).sqrMagnitude;
        float distanceToCupSqr = (G_TeaCup.transform.position - G_Teabag.transform.position).sqrMagnitude;
        Debug.Log(distanceToCupSqr.ToString());
        if (distanceToCupSqr < 5f)
        {
            Debug.Log("Victory");
            State = MicroState.Victory;
        }
        if (distanceSqr < 40.0f || startDistanceSqr > 4000)
        {
            Debug.Log("Failure?");
            State = MicroState.Lose;
        }
    }

    private void OnDrawGizmos()
    {
        foreach (GameObject start in G_StartPositions)
        {
            foreach (GameObject end in G_EndPositions)
            {
                Gizmos.DrawLine(start.transform.position, end.transform.position);
            }
        }
    }
}