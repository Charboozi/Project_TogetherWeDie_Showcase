using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

public class DayManager : NetworkBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Day Settings")]
    [Tooltip("Length of a full day in seconds, Example: 2 minutes = 1 day")]
    [SerializeField] private float dayLengthInSeconds = 120f;

    private NetworkVariable<float> currentTime = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public float CurrentTime => currentTime.Value;
    public float CurrentDay => CurrentTime / dayLengthInSeconds;
    public int CurrentDayInt => Mathf.FloorToInt(CurrentDay);

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dayChangeClip;

    public event Action<int> OnNewDayStarted;

    private int lastDayChecked = -1;

    private Dictionary<int, Action> scheduledOneTimeEvents = new();
    private List<RecurringDayEvent> recurringEvents = new();
    private List<TimedDayEvent> timedDayEvents = new();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!IsServer) return;

        float speedMultiplier = GameModeManager.Instance != null && GameModeManager.Instance.IsPvPMode ? 2.5f : 1f;
        currentTime.Value += Time.deltaTime * speedMultiplier;

        int day = Mathf.FloorToInt(CurrentDay);
        if (day != lastDayChecked)
        {
            lastDayChecked = day;
            OnNewDayStarted?.Invoke(day);
            InvokeScheduledEvents(day);

            // ✅ Server tells all clients to play the day change sound
            PlayDayChangeSoundClientRpc();
        }

        foreach (var evt in timedDayEvents)
        {
            if (!evt.triggered && CurrentDayInt == evt.targetDay && currentTime.Value >= evt.targetDay * dayLengthInSeconds + evt.timeIntoDay)
            {
                evt.callback?.Invoke();
                evt.triggered = true;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentTime.Value = dayLengthInSeconds;
            lastDayChecked = 0;
        }
    }

    private void OnEnable()
    {
        currentTime.OnValueChanged += OnTimeChanged;
    }

    private void OnDisable()
    {
        currentTime.OnValueChanged -= OnTimeChanged;
    }

    private void OnTimeChanged(float oldValue, float newValue)
    {
        if (IsServer) return; // 🛑 Host/server already handled this

        int day = Mathf.FloorToInt(newValue / dayLengthInSeconds);
        if (day != lastDayChecked)
        {
            lastDayChecked = day;
            OnNewDayStarted?.Invoke(day);
            Debug.Log($"🌞 (Client) New day: {day}");
        }
    }



    private void InvokeScheduledEvents(int day)
    {
        if (scheduledOneTimeEvents.TryGetValue(day, out var oneTimeCallback))
        {
            oneTimeCallback?.Invoke();
            scheduledOneTimeEvents.Remove(day);
        }

        foreach (var recurring in recurringEvents)
        {
            if (day >= recurring.startDay && (day - recurring.startDay) % recurring.interval == 0)
            {
                recurring.callback?.Invoke(day);
            }
        }
    }

    public void ScheduleEventForDay(int day, Action callback)
    {
        if (!scheduledOneTimeEvents.ContainsKey(day))
            scheduledOneTimeEvents[day] = callback;
        else
            scheduledOneTimeEvents[day] += callback;
    }
    public void ScheduleRecurringEvent(int startDay, int interval, Action<int> callback)
    {
        recurringEvents.Add(new RecurringDayEvent(startDay, interval, callback));
    }
    public void ScheduleEventForDayTime(int day, float secondsIntoDay, Action callback)
    {
        timedDayEvents.Add(new TimedDayEvent(day, secondsIntoDay, callback));
    }


    public float GetDayFraction(float days)
    {
        return days * dayLengthInSeconds;
    }

    public bool HasReachedDay(float targetDay)
    {
        return CurrentDay >= targetDay;
    }

    [ClientRpc]
    private void PlayDayChangeSoundClientRpc()
    {
        if (audioSource != null && dayChangeClip != null)
        {
            audioSource.PlayOneShot(dayChangeClip);
        }
    }

    private class RecurringDayEvent
    {
        public int startDay;
        public int interval;
        public Action<int> callback;

        public RecurringDayEvent(int startDay, int interval, Action<int> callback)
        {
            this.startDay = startDay;
            this.interval = interval;
            this.callback = callback;
        }
    }

    private class TimedDayEvent
    {
        public int targetDay;
        public float timeIntoDay;
        public Action callback;
        public bool triggered;

        public TimedDayEvent(int day, float seconds, Action callback)
        {
            targetDay = day;
            timeIntoDay = seconds;
            this.callback = callback;
            triggered = false;
        }
    }
}
