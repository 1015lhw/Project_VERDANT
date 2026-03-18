using UnityEngine;

public class OpeningAudioDebugProbe : MonoBehaviour
{
    [Header("References")]
    [Tooltip("可选：不填则自动查找场景里的 OpeningSequenceManager。")]
    public OpeningSequenceManager openingSequenceManager;
    [Tooltip("可选：不填则优先取 OpeningSequenceManager.audioSource。")]
    public AudioSource targetAudioSource;
    [Tooltip("可选：用于观察强制对话是否打开/关闭。")]
    public DialogueManager dialogueManager;

    [Header("Logging")]
    public bool logOnStart = true;
    public bool logEveryClipChange = true;
    public bool logPlayStateTransitions = true;
    public bool logOpeningLockTransitions = true;
    public bool logDialogueTransitions = true;
    public bool logEverySecondWhilePlaying = true;
    [Min(0.1f)]
    public float playingHeartbeatInterval = 1f;
    public KeyCode dumpStateKey = KeyCode.F8;

    AudioClip lastClip;
    bool lastIsPlaying;
    bool lastOpeningLock;
    bool lastDialogueOpen;
    int lastSlideIndex = int.MinValue;
    float nextHeartbeatTime;

    void Awake()
    {
        ResolveReferences();
    }

    void Start()
    {
        ResolveReferences();

        if (logOnStart)
        {
            Debug.LogWarning("[OpeningAudioDebugProbe] Probe active. Watching opening audio state.", this);
            DumpState("Start");
        }
    }

    void Update()
    {
        ResolveReferences();

        if (Input.GetKeyDown(dumpStateKey))
        {
            DumpState($"Manual dump ({dumpStateKey})");
        }

        TrackClipChanges();
        TrackPlayState();
        TrackOpeningLock();
        TrackDialogueState();
        TrackSlideState();
        EmitHeartbeatWhilePlaying();
    }

    void ResolveReferences()
    {
        if (openingSequenceManager == null)
        {
            openingSequenceManager = FindFirstObjectByType<OpeningSequenceManager>();
        }

        if (targetAudioSource == null && openingSequenceManager != null)
        {
            targetAudioSource = openingSequenceManager.audioSource;
        }

        if (dialogueManager == null)
        {
            dialogueManager = DialogueManager.Instance;
            if (dialogueManager == null)
            {
                dialogueManager = FindFirstObjectByType<DialogueManager>();
            }
        }
    }

    void TrackClipChanges()
    {
        if (!logEveryClipChange || targetAudioSource == null)
        {
            return;
        }

        AudioClip currentClip = targetAudioSource.clip;
        if (currentClip == lastClip)
        {
            return;
        }

        Debug.Log($"[OpeningAudioDebugProbe] Clip changed: {DescribeClip(lastClip)} -> {DescribeClip(currentClip)} | time={Time.time:0.00}", this);
        lastClip = currentClip;
    }

    void TrackPlayState()
    {
        if (!logPlayStateTransitions || targetAudioSource == null)
        {
            return;
        }

        bool isPlaying = targetAudioSource.isPlaying;
        if (isPlaying == lastIsPlaying)
        {
            return;
        }

        string state = isPlaying ? "PLAYING" : "STOPPED";
        Debug.LogWarning($"[OpeningAudioDebugProbe] AudioSource state -> {state} | {DescribeAudioSourceState(targetAudioSource)} | time={Time.time:0.00}", this);

        lastIsPlaying = isPlaying;
        nextHeartbeatTime = Time.time + playingHeartbeatInterval;
    }

    void TrackOpeningLock()
    {
        if (!logOpeningLockTransitions)
        {
            return;
        }

        bool currentLock = OpeningLock.IsLocked;
        if (currentLock == lastOpeningLock)
        {
            return;
        }

        Debug.LogWarning($"[OpeningAudioDebugProbe] OpeningLock -> {currentLock} | time={Time.time:0.00}", this);
        lastOpeningLock = currentLock;
    }

    void TrackDialogueState()
    {
        if (!logDialogueTransitions || dialogueManager == null)
        {
            return;
        }

        bool currentDialogueOpen = dialogueManager.IsOpen;
        if (currentDialogueOpen == lastDialogueOpen)
        {
            return;
        }

        Debug.LogWarning($"[OpeningAudioDebugProbe] DialogueManager.IsOpen -> {currentDialogueOpen} | time={Time.time:0.00}", this);
        lastDialogueOpen = currentDialogueOpen;
    }


    void TrackSlideState()
    {
        if (openingSequenceManager == null)
        {
            return;
        }

        int currentIndex = openingSequenceManager.CurrentSlideIndex;
        if (currentIndex == lastSlideIndex)
        {
            return;
        }

        lastSlideIndex = currentIndex;
        AudioClip voice = openingSequenceManager.CurrentSlideVoice;
        string slideInfo = currentIndex < 0
            ? "none"
            : $"index={currentIndex}, hasVoice={voice != null}, voice={DescribeClip(voice)}";

        Debug.LogWarning($"[OpeningAudioDebugProbe] Slide changed -> {slideInfo} | sequenceRunning={openingSequenceManager.IsSequenceRunning} | time={Time.time:0.00}", this);
    }

    void EmitHeartbeatWhilePlaying()
    {
        if (!logEverySecondWhilePlaying || targetAudioSource == null || !targetAudioSource.isPlaying)
        {
            return;
        }

        if (Time.time < nextHeartbeatTime)
        {
            return;
        }

        nextHeartbeatTime = Time.time + playingHeartbeatInterval;
        Debug.LogWarning($"[OpeningAudioDebugProbe] Heartbeat | {DescribeAudioSourceState(targetAudioSource)}", this);
    }

    void DumpState(string reason)
    {
        string managerState = openingSequenceManager == null ? "null" : "found";
        string dialogueState = dialogueManager == null ? "null" : dialogueManager.IsOpen.ToString();
        string audioState = targetAudioSource == null
            ? "AudioSource=null"
            : DescribeAudioSourceState(targetAudioSource);

        Debug.LogWarning($"[OpeningAudioDebugProbe] DumpState [{reason}] | OpeningManager={managerState} | OpeningLock={OpeningLock.IsLocked} | DialogueOpen={dialogueState} | {audioState}", this);

        if (openingSequenceManager != null)
        {
            Debug.LogWarning($"[OpeningAudioDebugProbe] Opening config | autoBeginOnSceneLoad={openingSequenceManager.autoBeginOnSceneLoad} | playForcedDialogueAfterOpening={openingSequenceManager.playForcedDialogueAfterOpening} | forcedDialogueInkJSON={(openingSequenceManager.forcedDialogueInkJSON != null ? openingSequenceManager.forcedDialogueInkJSON.name : "null")} | audioSourceLinked={(openingSequenceManager.audioSource != null)} | currentSlideIndex={openingSequenceManager.CurrentSlideIndex} | currentSlideVoice={DescribeClip(openingSequenceManager.CurrentSlideVoice)}", this);
        }
    }


    static string DescribeAudioSourceState(AudioSource source)
    {
        if (source == null)
        {
            return "AudioSource=null";
        }

        string playbackPosition = source.clip == null
            ? "position=n/a(no clip)"
            : $"position={source.time:0.00}s/{source.clip.length:0.00}s, samples={source.timeSamples}";

        return $"clip={DescribeClip(source.clip)}, playing={source.isPlaying}, enabled={source.enabled}, activeInHierarchy={source.gameObject.activeInHierarchy}, volume={source.volume:0.00}, mute={source.mute}, playOnAwake={source.playOnAwake}, {playbackPosition}";
    }

    static string DescribeClip(AudioClip clip)
    {
        if (clip == null)
        {
            return "null";
        }

        return $"{clip.name} ({clip.length:0.00}s, freq={clip.frequency}, samples={clip.samples})";
    }
}
