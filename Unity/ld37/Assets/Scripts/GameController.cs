using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Playing,
        Microgame,
        Paused,
        End
    }

    public GameState mState = GameState.Menu;

    #region Workstation References

    // Forgive me father for I have sinned.
    public GameObject G_MicroGame_coffee_1;

    public MicroGameController mActiveGame;

    #endregion Workstation References

    // Use this for initialization
    private void Start()
    {
        Debug.Assert(G_MicroGame_coffee_1 != null);
    }

    // Update is called once per frame
    private void Update()
    {
        switch (mState)
        {
            case GameState.Menu:
                break;
            case GameState.Playing:
                break;
            case GameState.Microgame:
                tickMicrogame();
                break;
            case GameState.End:
                break;
            case GameState.Paused:
                break;
        }
    }

    private void tickMicrogame()
    {
        if (mActiveGame != null)
        {
            switch (mActiveGame.State)
            {
                case MicroGameController.MicroState.Idle:
                    break;
                case MicroGameController.MicroState.Lose:
                    Debug.Log("Lose!");
                    mActiveGame.ResetGame();
                    mState = GameState.Playing;
                    break;
                case MicroGameController.MicroState.Playing:
                    break;
                case MicroGameController.MicroState.Victory:
                    Debug.Log("Victory");
                    mActiveGame.ResetGame();
                    mState = GameState.Playing;
                    break;
            }
        }
    }

    public void SelectWorkstation(WorkstationData.WorkstationType type)
    {
        GameObject microGameObject = null;
        switch (type)
        {
            case WorkstationData.WorkstationType.coffee_1:
                microGameObject = G_MicroGame_coffee_1;
                mActiveGame = G_MicroGame_coffee_1.GetComponent<MicroGameController>();
                break;
        }

        Debug.Assert(microGameObject != null);
        Debug.Assert(mActiveGame != null);
        mState = GameState.Microgame;
        mActiveGame.StartGame();
    }
}