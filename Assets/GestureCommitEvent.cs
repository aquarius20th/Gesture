using System;
using System.Collections.Generic;

public class GestureCommitEvent : EventArgs
{
    public GesturePrefab prefab;

    public new static readonly GestureCommitEvent Empty = new GestureCommitEvent();
}
