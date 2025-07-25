using System.Collections;

public interface IDrawBehavior
{
    public IEnumerator InitialDrawPhase();
    public IEnumerator DrawPhase();
    public IEnumerator DiscardPhase();
}