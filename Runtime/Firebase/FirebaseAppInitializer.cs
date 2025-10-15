using System;
using System.Collections;
using CustomAnalytics.Firebase.Analytics;
using CustomRC.Firebase.RC;
using Firebase;
using UnityEngine;
using UnityEngine.Events;
namespace CustomRC.Firebase {
    public class FirebaseAppInitializer : MonoBehaviour {
        [SerializeField] bool debugLogs;
        [SerializeField] FirebaseAnalyticsManager firebaseAnalyticsManager;
        [SerializeField] FirebaseRemoteConfigManager firebaseRemoteConfigManager;
        private bool hasInitialized;
        [SerializeField] UnityEvent onInitialized;

        IEnumerator Start() {
            Initialize();
            yield return new WaitUntil(() => hasInitialized);
            onInitialized?.Invoke();
        }
        void Initialize() {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                if (task.IsFaulted) {
                    return;
                }
                var status = task.Result;
                if (status == DependencyStatus.Available) {
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                    if (debugLogs) Debug.Log("[FirebaseAppInitializer] Firebase initialized.");
                    if (firebaseAnalyticsManager != null) {
                        firebaseAnalyticsManager.Initialize();
                        Debug.Log("[FirebaseAppInitializer] Firebase Analytics initialized.");
                    }
                    if (firebaseRemoteConfigManager != null) {
                        firebaseRemoteConfigManager.Initialize();
                        Debug.Log("[FirebaseAppInitializer] Firebase Remote Config initialized.");
                    }
                    hasInitialized = true;
                } else {
                    if (debugLogs) Debug.LogError($"[FirebaseAppInitializer] Dependency error: {status}");
                }
            });
        }
    }
}