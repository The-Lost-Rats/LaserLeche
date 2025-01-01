using UnityEngine;

public class PlaySceneController : ISceneController
{
    protected override GameState GetGameState()
    {
        return GameState.PLAY;
    }

    private void Start() { }

    protected override void SceneUpdate() { }
}
