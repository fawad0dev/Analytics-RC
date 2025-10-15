using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.RemoteConfig;
using Firebase.Extensions;
namespace CustomRC.Firebase.RC {
    public class FirebaseRemoteConfigManager : MonoBehaviour, IRemoteConfigManager {
        [SerializeField] private bool debugLogs = true;
        [SerializeField] private float cacheExpirationTime = 1f;
        private bool isInitialized = false;
        private Dictionary<string, object> cachedValues = new();
        private Dictionary<string, RCValue> rcValueLookup = new();
        public Dictionary<string, object> Values {
            get => cachedValues;
            set => cachedValues = value ?? new Dictionary<string, object>();
        }
        public event Action OnConfigFetched;
        public void Initialize() {
            if (isInitialized) {
                LogDebug("Firebase Remote Config already initialized");
                return;
            }
            LogDebug("Initializing Firebase Remote Config");
            try {
                SetDefaultValues();
                isInitialized = true;
                LogDebug("Firebase Remote Config initialized successfully");
                FetchRemoteConfig();
            } catch (Exception e) {
                LogError($"Failed to initialize Firebase Remote Config: {e.Message}");
            }
        }
        public void FetchRemoteConfig() {
            //if (!Application.isPlaying) return;
            LogDebug("Fetching remote config values...");
            FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromSeconds(cacheExpirationTime))
                .ContinueWithOnMainThread(task => {
                    if (task.IsCompleted) {
                        var info = FirebaseRemoteConfig.DefaultInstance.Info;
                        if (info.LastFetchStatus == LastFetchStatus.Success) {
                            FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                                .ContinueWithOnMainThread(activateTask => {
                                    if (activateTask.IsCompleted) {
                                        LogDebug("Remote config fetched and activated successfully");
                                        ProcessFetchedValues();
                                        OnConfigFetched?.Invoke();
                                    } else {
                                        LogError("Failed to activate remote config");
                                    }
                                });
                        } else {
                            LogError($"Remote config fetch failed: {info.LastFetchStatus}");
                            ProcessDefaultValues();
                        }
                    } else {
                        LogError($"Remote config fetch task failed: {task.Exception?.Message}");
                        ProcessDefaultValues();
                    }
                });
        }
        public T GetRemoteConfigValue<T>(string key, T defaultValue) {
            if (string.IsNullOrEmpty(key)) {
                LogError("Key cannot be null or empty");
                return defaultValue;
            }
            try {
                if (cachedValues.ContainsKey(key)) {
                    return ConvertValue<T>(cachedValues[key], defaultValue);
                }
                if (isInitialized) {
                    var configValue = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                    var convertedValue = ConvertFirebaseValue<T>(configValue, defaultValue);
                    cachedValues[key] = convertedValue;
                    LogDebug($"Retrieved config value for '{key}': {convertedValue}");
                    return convertedValue;
                }
            } catch (Exception e) {
                LogError($"Error getting remote config value for key '{key}': {e.Message}");
            }
            LogDebug($"Using default value for '{key}': {defaultValue}");
            return defaultValue;
        }

        public void RegisterRCValues(RCValue[] rcValues) {
            if (rcValues == null) return;
            rcValueLookup.Clear();
            foreach (var rcValue in rcValues) {
                if (!string.IsNullOrEmpty(rcValue.key)) {
                    rcValueLookup[rcValue.key] = rcValue;
                    LogDebug($"Registered RC Value: {rcValue.key} ({rcValue.valueType})");
                }
            }
            LogDebug($"Registered {rcValueLookup.Count} RC Values");
        }
        private void SetDefaultValues() {
            var defaults = new Dictionary<string, object>();
            foreach (var kvp in rcValueLookup) {
                var rcValue = kvp.Value;
                switch (rcValue.valueType) {
                    case RCType.Number:
                        defaults[rcValue.key] = rcValue.numberValue.defaultValue;
                        break;
                    case RCType.Bool:
                        defaults[rcValue.key] = rcValue.boolValue.defaultValue;
                        break;
                    case RCType.String:
                        defaults[rcValue.key] = rcValue.stringValue.defaultValue ?? "";
                        break;
                    case RCType.Json:
                        defaults[rcValue.key] = rcValue.jsonValue.defaultValue ?? "{}";
                        break;
                }
            }
            if (defaults.Count > 0) {
                FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
                    .ContinueWithOnMainThread(task => {
                        if (task.IsCompleted) {
                            LogDebug($"Set {defaults.Count} default values successfully");
                            foreach (var kvp in defaults) {
                                cachedValues[kvp.Key] = kvp.Value;
                            }
                        } else {
                            LogError("Failed to set default values");
                        }
                    });
            }
        }
        private void ProcessFetchedValues() {
            try {
                foreach (var kvp in rcValueLookup) {
                    var key = kvp.Key;
                    var rcValue = kvp.Value;
                    var configValue = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                    var fetchedValue = ConvertFirebaseValueToObject(configValue);
                    cachedValues[key] = fetchedValue;
                    rcValue.UpdateValue(fetchedValue);
                    LogDebug($"Updated RC Value '{key}': {fetchedValue}");
                }
                LogDebug($"Processed {rcValueLookup.Count} remote config values");
            } catch (Exception e) {
                LogError($"Error processing fetched values: {e.Message}");
            }
        }
        private void ProcessDefaultValues() {
            LogDebug("Using default values due to fetch failure");
            foreach (var kvp in rcValueLookup) {
                var rcValue = kvp.Value;
                object defaultValue = null;
                switch (rcValue.valueType) {
                    case RCType.Number:
                        defaultValue = rcValue.numberValue.defaultValue;
                        break;
                    case RCType.Bool:
                        defaultValue = rcValue.boolValue.defaultValue;
                        break;
                    case RCType.String:
                        defaultValue = rcValue.stringValue.defaultValue;
                        break;
                    case RCType.Json:
                        defaultValue = rcValue.jsonValue.defaultValue;
                        break;
                }
                if (defaultValue != null) {
                    cachedValues[kvp.Key] = defaultValue;
                    rcValue.UpdateValue(defaultValue);
                }
            }
        }
        private T ConvertFirebaseValue<T>(ConfigValue configValue, T defaultValue) {
            if (object.ReferenceEquals(configValue, null) || configValue.Source == ValueSource.StaticValue) {
                return defaultValue;
            }
            try {
                return ConvertValue<T>(ConvertFirebaseValueToObject(configValue), defaultValue);
            } catch {
                return defaultValue;
            }
        }
        private object ConvertFirebaseValueToObject(ConfigValue configValue) {
            if (object.ReferenceEquals(configValue, null)) return null;
            var stringValue = configValue.StringValue;
            if (bool.TryParse(stringValue, out bool boolResult)) {
                return boolResult;
            }
            if (long.TryParse(stringValue, out long longResult)) {
                return longResult;
            }
            if (double.TryParse(stringValue, out double doubleResult)) {
                return doubleResult;
            }
            return stringValue;
        }
        private T ConvertValue<T>(object value, T defaultValue) {
            if (value == null) return defaultValue;
            try {
                Type targetType = typeof(T);
                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    targetType = Nullable.GetUnderlyingType(targetType);
                }
                if (value.GetType() == targetType || value is T) {
                    return (T)value;
                }
                if (targetType == typeof(string)) {
                    return (T)(object)value.ToString();
                }
                if (targetType.IsPrimitive || targetType == typeof(decimal)) {
                    return (T)Convert.ChangeType(value, targetType);
                }
                if (targetType == typeof(bool)) {
                    if (value is string stringValue) {
                        if (bool.TryParse(stringValue, out bool boolResult)) {
                            return (T)(object)boolResult;
                        }
                        if (stringValue == "1") return (T)(object)true;
                        if (stringValue == "0") return (T)(object)false;
                    }
                    return (T)Convert.ChangeType(value, targetType);
                }
                return (T)Convert.ChangeType(value, targetType);
            } catch (Exception e) {
                LogError($"Failed to convert value '{value}' to type {typeof(T).Name}: {e.Message}");
                return defaultValue;
            }
        }
        private void LogDebug(string message) {
            if (debugLogs) {
                Debug.Log($"[FirebaseRemoteConfig] {message}");
            }
        }
        private void LogError(string message) {
            Debug.LogError($"[FirebaseRemoteConfig] {message}");
        }
        public RCValue GetRCValue(string key) {
            return rcValueLookup.TryGetValue(key, out var rcValue) ? rcValue : null;
        }
        public float GetNumberValue(string key) {
            var rcValue = GetRCValue(key);
            return rcValue?.valueType == RCType.Number ? rcValue.numberValue.Value : 0f;
        }
        public bool GetBoolValue(string key) {
            var rcValue = GetRCValue(key);
            return rcValue?.valueType == RCType.Bool ? rcValue.boolValue.Value : false;
        }
        public string GetStringValue(string key) {
            var rcValue = GetRCValue(key);
            return rcValue?.valueType == RCType.String ? rcValue.stringValue.Value : "";
        }
        public string GetJsonValue(string key) {
            var rcValue = GetRCValue(key);
            return rcValue?.valueType == RCType.Json ? rcValue.jsonValue.Value : "{}";
        }
    }
}