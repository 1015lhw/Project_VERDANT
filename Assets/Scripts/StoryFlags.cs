using System.Collections.Generic;
using UnityEngine;

// In-memory flag store. Resets on every Play / game start
// (RuntimeInitializeOnLoadMethod fires at the beginning of each play session).
// To make a flag persist across game restarts, use PlayerPrefs directly instead.
public static class StoryFlags
{
    private static readonly Dictionary<string, bool> flags = new Dictionary<string, bool>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ClearOnPlay()
    {
        flags.Clear();
    }

    public static bool Get(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return flags.TryGetValue(name.Trim(), out bool v) && v;
    }

    public static void Set(string name, bool value = true)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        flags[name.Trim()] = value;
    }
}
