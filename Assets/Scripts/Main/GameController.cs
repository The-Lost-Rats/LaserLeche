using System.Collections.Generic;
using UnityEngine;

public enum GameState {
    UNKNOWN,
    PLAY
}

public class GameController : MonoBehaviour {
    public static GameController instance = null;

    [SerializeField] private SceneController sceneController;

    public GameState currGameState { get; private set; }

    public void Awake() {
        if ( null == instance ) {
            instance = this;
            DontDestroyOnLoad( this.gameObject );
        } else {
            Destroy( this.gameObject );
        }

        // Initialize the scene controller
        sceneController.Initialize();

        // Ensure that the base scene exists
        if (!sceneController.activeScenes.Contains(Scenes.Base))
        {
            // TODO Does this work?
            Debug.Break();
        }
        // This means that only the base scene exists, and we should create the main menu
        if (sceneController.activeScenes.Count == 1)
        {
            sceneController.LoadScene(Scenes.Play, false);
            currGameState = GameState.PLAY; // Directly assign because LoadScene takes a sec
        }
        else
        {
            currGameState = sceneController.GetCurrSceneGameState();
        }
    }

    public void ChangeState(GameState newGameState)
    {
        if (newGameState == currGameState)
        {
            Debug.Log($"Tried to change game state but already {currGameState}");
            return;
        }
        // TODO Create state machine chart showing states
        bool handled = true;
        string nextSceneName = null;
        switch (currGameState)
        {
            case GameState.PLAY:
                handled = false;
                break;
            default:
                handled = false;
                break;
        }
        if (!handled)
        {
            Debug.LogError($"Unknown game flow: {currGameState} -> {newGameState}");
            return;
        }

        if (nextSceneName != null)
        {
            sceneController.LoadScene(nextSceneName, false); // TODO Look into if we need to use async
        }
        Debug.Log($"Changing game state to {newGameState}");
        currGameState = newGameState;
    }
}
