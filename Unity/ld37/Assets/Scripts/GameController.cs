using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Playing,
        Microgame,
        Paused,
        RoundEnd,
        GameEnd
    }

    public GameState mState = GameState.Menu;

    #region Workstation References

    // Forgive me father for I have sinned.
    public GameObject G_MicroGame_coffee_1;

    public GameObject G_Workstation_1;
    public GameObject G_Workstation_2;
    public GameObject G_Workstation_3;
    public GameObject G_Workstation_4;
    public GameObject G_Workstation_5;
    public GameObject G_Workstation_6;

    public MicroGameController mActiveGame;

    #endregion Workstation References

    #region UI Elements because the GameLogic totally should dictate what the UI does

    public GameObject UI_TimerText;

    #endregion UI Elements because the GameLogic totally should dictate what the UI does

    #region Round-based controls

    public float RoundTimeRemaining { get { return (mState == GameState.Playing) ? (mRoundStartTime + kRoundDuration) - Time.time : 0; } }
    private int mRoundNumber = 0;
    private float mRoundStartTime = -1;
    private const float kRoundDuration = 5;

    public string RoundTime { get { return string.Format("Round {0}\n{1}:{2:D2}", mRoundNumber, (int)(RoundTimeRemaining / 60), Mathf.RoundToInt(RoundTimeRemaining % 60)); } }

    #endregion Round-based controls

    // Use this for initialization
    private void Start()
    {
        Debug.Assert(G_MicroGame_coffee_1 != null);
        Debug.Assert(G_Workstation_1 != null);
        Debug.Assert(G_Workstation_2 != null);
        Debug.Assert(G_Workstation_3 != null);
        Debug.Assert(G_Workstation_4 != null);
        Debug.Assert(G_Workstation_5 != null);
        Debug.Assert(G_Workstation_6 != null);
        Debug.Assert(UI_TimerText != null);

        // Move this into user-clicked-start territory
        StartRound();
    }

    public void StartRound()
    {
        mState = GameState.Playing;
        mRoundNumber++;
        mRoundStartTime = Time.time;
    }

    // Update is called once per frame
    private void Update()
    {
        if (RoundTimeRemaining < 0)
        {
            // End the round - do the cleanup
            mState = GameState.RoundEnd;
            if (mActiveGame != null)
            {
                mActiveGame.ResetGame();
                mActiveGame = null;
            }
        }

        switch (mState)
        {
            case GameState.Menu:
                break;
            case GameState.Playing:
                break;
            case GameState.Microgame:
                tickMicrogame();
                break;
            case GameState.RoundEnd:
                break;
            case GameState.GameEnd:
                break;
            case GameState.Paused:
                break;
        }
        // Update the UI
        UI_TimerText.GetComponent<Text>().text = RoundTime;
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
                    mActiveGame = null;
                    break;
                case MicroGameController.MicroState.Playing:
                    break;
                case MicroGameController.MicroState.Victory:
                    Debug.Log("Victory");
                    mActiveGame.ResetGame();
                    mState = GameState.Playing;
                    mActiveGame = null;
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