using System.Collections.Generic;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour {
    static IAnalyticsManager AnalyticsServiceInstance { get; set; }
    [SerializeField] FirebaseAnalyticsManager firebaseAnalyticsManager;
    private static Queue<(string, IAnalyticsManager.Parameter[])> eventQueue = new();
    enum AnalyticsType { Firebase, }
    [SerializeField]
    private AnalyticsType analyticsType = AnalyticsType.Firebase;
    void Start() {
        InitializeAnalytics();
        HandlePendingAnalytics();
    }
    
    static void HandlePendingAnalytics() {
        while (eventQueue.Count > 0) {
            var (eventName, parameters) = eventQueue.Dequeue();
            if (parameters == null)
                AnalyticsServiceInstance.LogEvent(eventName);
            else
                AnalyticsServiceInstance.LogEvent(eventName, parameters);
        }
    }
    private void InitializeAnalytics() {
        switch (analyticsType) {
            case AnalyticsType.Firebase:
                AnalyticsServiceInstance = firebaseAnalyticsManager;
                break;
            default:
                Debug.LogWarning("Unknown analytics type. Firebase will be used by default.");
                AnalyticsServiceInstance = firebaseAnalyticsManager;
                break;
        }
    }
    public static void LogEvent(string eventName) {
        if (!AnalyticsServiceInstance.LogEvent(eventName)) {
            eventQueue.Enqueue((eventName, null));
        } else {
            HandlePendingAnalytics();
        }
    }
    public static void LogEvent(string eventName, IAnalyticsManager.Parameter[] parameters) {
        if (!AnalyticsServiceInstance.LogEvent(eventName, parameters)) {
            eventQueue.Enqueue((eventName, parameters));
        } else {
            HandlePendingAnalytics();
        }

    }
}
