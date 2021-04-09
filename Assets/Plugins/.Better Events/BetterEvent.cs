using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

[Serializable]
public struct BetterEvent
{
    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "GetDefaultBetterEvent", OnTitleBarGUI = "DrawInvokeButton")]
    public List<BetterEventEntry> Events;

    public void Invoke()
    {
        if (this.Events == null) return;
        for (int i = 0; i < this.Events.Count; i++)
        {
            this.Events[i].Invoke();
        }
    }

#if UNITY_EDITOR

    private BetterEventEntry GetDefaultBetterEvent()
    {
        return new BetterEventEntry(null);
    }

    private void DrawInvokeButton()
    {
        if (Sirenix.Utilities.Editor.SirenixEditorGUI.ToolbarButton("Invoke"))
        {
            this.Invoke();
        }
    }

#endif
}



[Serializable]
public struct BetterEvent<T0>
{
    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "GetDefaultBetterEvent")]
    public List<BetterEventEntry> Events;

    public void Invoke(T0 t0)
    {
        if (this.Events == null) return;
        for (int i = 0; i < this.Events.Count; i++) {
            this.Events[i].ParameterValues[0] = t0;
            this.Events[i].Invoke();
        }
    }

#if UNITY_EDITOR

    private BetterEventEntry GetDefaultBetterEvent()
    {
        return new BetterEventEntry(null);
    }
    

#endif
}


[Serializable]
public struct BetterEvent<T0, T1>
{
    [HideReferenceObjectPicker, ListDrawerSettings(CustomAddFunction = "GetDefaultBetterEvent")]
    public List<BetterEventEntry> Events;

    public void Invoke(T0 t0, T1 t1)
    {
        if (this.Events == null) return;
        for (int i = 0; i < this.Events.Count; i++) {
            Events[i].ParameterValues[0] = t0;
            Events[i].ParameterValues[1] = t1;
            Events[i].Invoke();
        }
    }

#if UNITY_EDITOR

    private BetterEventEntry GetDefaultBetterEvent()
    {
        return new BetterEventEntry(null);
    }
    

#endif
}


