using System.Collections;

public interface IGameState
{
    IEnumerator Load();
    void Enter();
    void Pause();
    void Resume();
    void Exit();
    IEnumerator Unload();
}
