using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MicroGameController : MonoBehaviour
{
    public enum MicroState
    {
        Playing,
        Victory,
        Lose
    }

    /// <summary>
    /// Resets the game
    /// </summary>
    public abstract void StartGame();

    public MicroState State;
}