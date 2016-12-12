using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggTimerController : MonoBehaviour
{
    public float duration = 5f;
    public float timeStarted;
    public GameObject G_Spinner;
    public bool Running;
    public bool Completed;
    private Quaternion mOriginalTransform;

    public void Start()
    {
        Debug.Assert(G_Spinner != null);
        mOriginalTransform = G_Spinner.transform.rotation;
    }

    public void StartTimer()
    {
        timeStarted = Time.time;
        Running = true;
        Completed = false;
    }

    public void ResetTimer()
    {
        Running = Completed = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Running)
        {
            float timeDelta = Time.time - timeStarted;
            float percentComplete = timeDelta / duration;
            float deltaAngle = 360f * percentComplete;
            G_Spinner.transform.rotation = mOriginalTransform;
            G_Spinner.transform.Rotate(Vector3.up, deltaAngle);
            if (percentComplete >= 1f)
            {
                Finish();
            }
        }
    }

    private void Finish()
    {
        Running = false;
        Completed = true;
    }
}