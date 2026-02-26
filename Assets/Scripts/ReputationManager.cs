using System;
using UnityEngine;

public enum NpcId
{
    A,   //Marcus
    B    //Sierra
}

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance { get; private set; }

    [Header("Affection Range")]
    [SerializeField] private int minAffection = -100;
    [SerializeField] private int maxAffection = 100;

    [Header("Current Affection (Debug View)")]
    [SerializeField] private int affectionA = 0;
    [SerializeField] private int affectionB = 0;

    public event Action<NpcId, int, int, int, string> OnAffectionChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetAffection(NpcId npc)
    {
        return npc switch
        {
            NpcId.A => affectionA,
            NpcId.B => affectionB,
            _ => 0
        };
    }

    public void SetAffection(NpcId npc, int value, string reason = "Set")
    {
        int oldValue = GetAffection(npc);
        int newValue = Clamp(value);

        if (oldValue == newValue) return;

        Apply(npc, newValue);
        OnAffectionChanged?.Invoke(npc, oldValue, newValue, newValue - oldValue, reason);
    }

    public void AddAffection(NpcId npc, int delta, string reason = "Add")
    {
        int oldValue = GetAffection(npc);
        int newValue = Clamp(oldValue + delta);

        if (oldValue == newValue) return;

        Apply(npc, newValue);
        OnAffectionChanged?.Invoke(npc, oldValue, newValue, newValue - oldValue, reason);
    }

    private void Apply(NpcId npc, int newValue)
    {
        switch (npc)
        {
            case NpcId.A: affectionA = newValue; break;
            case NpcId.B: affectionB = newValue; break;
        }
    }

    private int Clamp(int v) => Mathf.Clamp(v, minAffection, maxAffection);

    [Serializable]
    public struct Snapshot
    {
        public int affectionA;
        public int affectionB;
        public int minAffection;
        public int maxAffection;
    }

    public Snapshot GetSnapshot()
    {
        return new Snapshot
        {
            affectionA = affectionA,
            affectionB = affectionB,
            minAffection = minAffection,
            maxAffection = maxAffection
        };
    }

    public void LoadSnapshot(Snapshot s)
    {
        minAffection = s.minAffection;
        maxAffection = s.maxAffection;

        SetAffection(NpcId.A, s.affectionA, "LoadSnapshot");
        SetAffection(NpcId.B, s.affectionB, "LoadSnapshot");
    }
}