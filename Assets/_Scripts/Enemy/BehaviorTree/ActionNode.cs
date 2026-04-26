
using System;

public class ActionNode : ITreeeNode
{
    private Action action;

    public ActionNode(Action action)
    {
        this.action = action;
    }

    public void Execute()
    {
        action?.Invoke();
    }
}
