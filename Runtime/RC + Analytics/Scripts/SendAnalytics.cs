using System.Collections.Generic;
using CustomAnalytics;
using UnityEngine;

public class SendAnalytics : MonoBehaviour {
    [SerializeField] string eventName;
    [SerializeField] IAnalyticsManager.Parameter[] eventData;
    public void SendEvent() {
        AnalyticsManager.LogEvent(eventName, eventData);
    }
}
