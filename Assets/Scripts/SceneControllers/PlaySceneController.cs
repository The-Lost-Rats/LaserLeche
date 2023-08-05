using UnityEngine;

public class PlaySceneController : ISceneController
{
    override protected GameState GetGameState() { return GameState.PLAY; }

    private void Start()
    {
    }

    override protected void SceneUpdate()
    {
    }
}
