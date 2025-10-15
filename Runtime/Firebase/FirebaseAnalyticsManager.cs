using CustomAnalytics;
using Firebase;
using Firebase.Analytics;
using System.Collections.Generic;
using UnityEngine;
namespace CustomAnalytics.Firebase.Analytics {
    public class FirebaseAnalyticsManager : MonoBehaviour, IAnalyticsManager {
        private bool isInitialized = false;
        [SerializeField] bool debugLogs;
        public void Initialize() {
            Log("Initializing");
            isInitialized = true;
        }
        void Log(string message) {
            if (debugLogs) Debug.Log("[Firebase Analytics]" + message);
        }
        public bool LogEvent(string eventName) {
            if (!isInitialized) return false;
            FirebaseAnalytics.LogEvent(eventName);
            Log($"LogEvent: {eventName}");
            return true;
        }

        public bool LogEvent(string eventName, IAnalyticsManager.Parameter[] parameters) {
            if (!isInitialized) return false;
            Parameter[] firebaseParams = new Parameter[parameters.Length];
            /*foreach (var param in parameters)
            {
                firebaseParams[i] = param.Value switch {
                    string str => new Parameter(param.Key, str),
                    int intVal => new Parameter(param.Key, intVal),
                    long longVal => new Parameter(param.Key, longVal),
                    float floatVal => new Parameter(param.Key, floatVal),
                    double doubleVal => new Parameter(param.Key, doubleVal),
                    _ => new Parameter(param.Key, param.Value.ToString()),
                };
                i++;
            }*/
            for (int i = 0; i < parameters.Length; i++) {
                var param = parameters[i];
                firebaseParams[i] = new Parameter(param.Key, param.Value);
                Log($"Param Key: {param.Key}, Value: {param.Value}");
            }
            FirebaseAnalytics.LogEvent(eventName, firebaseParams);
            Log($"LogEvent: {eventName} with params");
            return true;

        }
    }
}