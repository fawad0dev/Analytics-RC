using UnityEngine;
using UnityEngine.Events;
public class RemoteConfigManager : MonoBehaviour {
    [SerializeField] private RCValue[] rCValues;
    [SerializeField] private FirebaseRemoteConfigManager firebaseRemoteConfigManager;
    enum RemoteConfigType { Firebase, }
    static IRemoteConfigManager RemoteConfigManagerInstance { get; set; }
    [SerializeField] private RemoteConfigType remoteConfigType;
    [SerializeField] UnityEvent<RCValue[]> onSelected;
    void Start() {
        InitializeRemoteConfig();
    }
    private void InitializeRemoteConfig() {
        switch (remoteConfigType) {
            case RemoteConfigType.Firebase:
                RemoteConfigManagerInstance = firebaseRemoteConfigManager;
                break;
            default:
                Debug.LogWarning("Unknown remote config type. Firebase will be used by default.");
                RemoteConfigManagerInstance = firebaseRemoteConfigManager;
                break;
        }
        if (RemoteConfigManagerInstance != null) {
            RemoteConfigManagerInstance.RegisterRCValues(rCValues);
            onSelected?.Invoke(rCValues);
        } else {
            Debug.LogError("RemoteConfigManagerInstance is not set.");
        }
    }
    public static RCValue GetRCValue(string key) {
        if (RemoteConfigManagerInstance == null) {
            Debug.LogError("RemoteConfigManagerInstance is not initialized.");
            return null;
        }
        return RemoteConfigManagerInstance.GetRCValue(key);
    }
    public static float GetNumberValue(string key) {
        if (RemoteConfigManagerInstance == null) {
            Debug.LogError("RemoteConfigManagerInstance is not initialized.");
            return default;
        }
        return RemoteConfigManagerInstance.GetNumberValue(key);
    }
    public static bool GetBoolValue(string key) {
        if (RemoteConfigManagerInstance == null) {
            Debug.LogError("RemoteConfigManagerInstance is not initialized.");
            return default;
        }
        return RemoteConfigManagerInstance.GetBoolValue(key);
    }
    public static string GetStringValue(string key) {
        if (RemoteConfigManagerInstance == null) {
            Debug.LogError("RemoteConfigManagerInstance is not initialized.");
            return default;
        }
        return RemoteConfigManagerInstance.GetStringValue(key);
    }
    public static string GetJsonValue(string key) {
        if (RemoteConfigManagerInstance == null) {
            Debug.LogError("RemoteConfigManagerInstance is not initialized.");
            return default;
        }
        return RemoteConfigManagerInstance.GetJsonValue(key);
    }
}
