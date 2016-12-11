using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerController : MonoBehaviour
{
    public GameObject G_GameController;
    public GameObject G_CashRegister;
    private GameController mGameController;
    public WorkstationData.WorkstationType Desire;
    private int mCurrentPosition = -1;

    // Use this for initialization
    private void Start()
    {
        Debug.Assert(G_GameController != null);
        Debug.Assert(G_CashRegister != null);
        mGameController = G_GameController.GetComponent<GameController>();

        // Pick a desire at random
        HashSet<WorkstationData.WorkstationType> allDesires = mGameController.GetAvailableDesires();

        int selection = (int)(Random.Range(0f, 1f) * allDesires.Count);
        int count = 0;
        foreach (WorkstationData.WorkstationType type in allDesires)
        {
            count++;
            if (count > selection)
            {
                Desire = type;
                break;
            }
        }
    }

    /// <summary>
    /// Moves this character into queue position
    /// </summary>
    /// <param name="number"></param>
    public void MoveToQueuePosition(int number)
    {
        mCurrentPosition = number;
        Vector3[] path = iTweenPath.GetPath("Customer Line");
        Debug.DrawLine(transform.position, path[number], Color.yellow, 10f);
        iTween.MoveTo(gameObject, iTween.Hash("position", path[number], "speed", 3.0f, "easetype", "linear", "oncomplete", "OnQueueMoveComplete"));
        iTween.LookTo(gameObject, iTween.Hash("looktarget", path[number], "time", 0.2f));
    }

    public void OnQueueMoveComplete()
    {
        Debug.Log("Movecomplete for " + gameObject.name + ", " + mCurrentPosition);
        if (mCurrentPosition == 0)
        {
            iTween.LookTo(gameObject, iTween.Hash("looktarget", G_CashRegister.transform, "time", 0.2f, "axis", "y"));
        }
    }

    public void MoveToWaitingZone(int number)
    {
        // Awwww yea GAMEJAM
        mCurrentPosition = number;
        Vector3[] path = iTweenPath.GetPath("Customer Waiting Zone");
        Debug.DrawLine(transform.position, path[number], Color.red, 10f);
        iTween.MoveTo(gameObject, iTween.Hash("position", path[number], "speed", 3.0f, "easetype", "linear", "oncomplete", "OnWaitingZoneMoveComplete"));
        iTween.LookTo(gameObject, iTween.Hash("looktarget", path[number], "time", 0.2f));
    }

    private void OnWaitingZoneMoveComplete()
    {
        iTween.LookTo(gameObject, iTween.Hash("looktarget", mGameController.G_WaitingZoneLookTarget.transform, "time", 0.2f, "axis", "y"));
    }

    // Update is called once per frame
    private void Update()
    {
    }
}