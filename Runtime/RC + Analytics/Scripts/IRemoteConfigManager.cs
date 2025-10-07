using System;
using System.Collections.Generic;
using CustomAttributes;
using UnityEngine.Events;

public interface IRemoteConfigManager {
    Dictionary<string, object> Values { get; set; }
    void Initialize();
    void FetchRemoteConfig();
    T GetRemoteConfigValue<T>(string key, T defaultValue);
    void RegisterRCValues(RCValue[] rcValues);
    event Action OnConfigFetched;
    public RCValue GetRCValue(string key);
    public float GetNumberValue(string key);
    public bool GetBoolValue(string key);
    public string GetStringValue(string key);
    public string GetJsonValue(string key);
}

[Serializable]
public class ValueDetail<T> {
    public T Value;
    public T defaultValue;

    public ValueDetail() { }

    public ValueDetail(T defaultVal) {
        defaultValue = defaultVal;
        Value = defaultVal;
    }
}

public enum RCType {
    Number,
    Bool,
    String,
    Json
}

[Serializable]
public class RCValue {
    public string key;
    public RCType valueType;
    [ShowIf(nameof(valueType), RCType.Number)] public ValueDetail<float> numberValue;
    [ShowIf(nameof(valueType), RCType.Number)] public UnityEvent<float> onNumberValueFetched;
    [ShowIf(nameof(valueType), RCType.Bool)] public ValueDetail<bool> boolValue;
    [ShowIf(nameof(valueType), RCType.Bool)] public UnityEvent<bool> onBoolValueFetched;
    [ShowIf(nameof(valueType), RCType.String)] public ValueDetail<string> stringValue;
    [ShowIf(nameof(valueType), RCType.String)] public UnityEvent<string> onStringValueFetched;
    [ShowIf(nameof(valueType), RCType.Json)] public ValueDetail<string> jsonValue;
    [ShowIf(nameof(valueType), RCType.Json)] public UnityEvent<string> onJsonValueFetched;

    public RCValue() {
        numberValue = new ValueDetail<float>();
        boolValue = new ValueDetail<bool>();
        stringValue = new ValueDetail<string>();
        jsonValue = new ValueDetail<string>();

        onNumberValueFetched = new UnityEvent<float>();
        onBoolValueFetched = new UnityEvent<bool>();
        onStringValueFetched = new UnityEvent<string>();
        onJsonValueFetched = new UnityEvent<string>();
    }

    public void UpdateValue(object fetchedValue) {
        switch (valueType) {
            case RCType.Number:
                var numValue = ConvertToFloat(fetchedValue, numberValue.defaultValue);
                numberValue.Value = numValue;
                onNumberValueFetched?.Invoke(numValue);
                break;

            case RCType.Bool:
                var boolVal = ConvertToBool(fetchedValue, boolValue.defaultValue);
                boolValue.Value = boolVal;
                onBoolValueFetched?.Invoke(boolVal);
                break;

            case RCType.String:
                var stringVal = ConvertToString(fetchedValue, stringValue.defaultValue);
                stringValue.Value = stringVal;
                onStringValueFetched?.Invoke(stringVal);
                break;

            case RCType.Json:
                var jsonVal = ConvertToString(fetchedValue, jsonValue.defaultValue);
                jsonValue.Value = jsonVal;
                onJsonValueFetched?.Invoke(jsonVal);
                break;
        }
    }

    private float ConvertToFloat(object value, float defaultValue) {
        if (value == null) return defaultValue;

        if (value is float f) return f;
        if (value is double d) return (float)d;
        if (value is int i) return i;
        if (value is long l) return l;

        if (float.TryParse(value.ToString(), out float result))
            return result;

        return defaultValue;
    }

    private bool ConvertToBool(object value, bool defaultValue) {
        if (value == null) return defaultValue;

        if (value is bool b) return b;

        var stringValue = value.ToString().ToLower();
        if (stringValue == "true" || stringValue == "1") return true;
        if (stringValue == "false" || stringValue == "0") return false;

        if (bool.TryParse(stringValue, out bool result))
            return result;

        return defaultValue;
    }

    private string ConvertToString(object value, string defaultValue) {
        return value?.ToString() ?? defaultValue ?? "";
    }
}
