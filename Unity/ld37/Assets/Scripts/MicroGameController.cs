using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MicroGameController : MonoBehaviour
{
    public enum MicroState
    {
        Idle,
        Transitioning,
        Playing,
        Victory,
        Lose
    }

    /// <summary>
    /// Resets the game
    /// </summary>
    public void StartGame()
    {
        Debug.Assert(mDesire != WorkstationData.WorkstationType.None);
        _StartGame();
    }

    protected abstract void _StartGame();

    public abstract void ResetGame();

    public abstract WorkstationData.WorkstationType GetDesire();

    protected WorkstationData.WorkstationType mDesire = WorkstationData.WorkstationType.None;
    public MicroState State = MicroState.Idle;
}