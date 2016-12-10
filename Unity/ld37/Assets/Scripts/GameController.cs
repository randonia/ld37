using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        End
    }

    public GameState mState = GameState.Menu;

    // Use this for initialization
    private void Start()
    {
        switch (mState)
        {
            case GameState.Menu:
                break;
            case GameState.Playing:
                break;
            case GameState.End:
                break;
            case GameState.Paused:
                break;
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}