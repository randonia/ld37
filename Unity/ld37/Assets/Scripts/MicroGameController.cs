using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MicroGameController : MonoBehaviour
{
    public GameObject G_GameController;
    protected GameController mGameController;

    public enum MicroState
    {
        Idle,
        Transitioning,
        Playing,
        Victory,
        Lose
    }

    protected abstract void _StartGame();

    protected abstract void _ResetGame();

    protected abstract void _Reinitialize();

    public void StartGame()
    {
        G_GameController = GameObject.Find("GameController");
        Debug.Assert(G_GameController != null);
        mGameController = G_GameController.GetComponent<GameController>();
        Debug.Assert(mGameController != null);
        Debug.Assert(GetDesire() != WorkstationData.WorkstationType.None);
        State = MicroState.Transitioning;
        iTween.MoveTo(gameObject, iTween.Hash("position", mGameController.G_MicroGameArena.transform, "time", 0.75f, "oncomplete", "StartGameCallback", "oncompletetarget", gameObject));
        _StartGame();
    }

    public void ResetGame()
    {
        iTween.MoveTo(gameObject, iTween.Hash("position", new Vector3(UnityEngine.Random.Range(-200, 200), 0, 150),
            "time", 1.5f,
            "delay", GetDelay(),
            "easetype", "easeoutcubic",
            "onstart", "Reinitialize",
            "onstarttarget", gameObject));
        State = MicroState.Idle;
    }

    public virtual float GetDelay()
    {
        return 0.3f;
    }

    public void Reinitialize()
    {
        mGameController.CameraToPos(GameController.CAMERA_GAMEPOS);
        _Reinitialize();
    }

    public abstract WorkstationData.WorkstationType GetDesire();

    protected WorkstationData.WorkstationType mDesire = WorkstationData.WorkstationType.None;
    public MicroState State = MicroState.Idle;
}