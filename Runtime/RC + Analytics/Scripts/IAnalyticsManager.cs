using System.Collections.Generic;
namespace CustomAnalytics {
    public interface IAnalyticsManager {
        [System.Serializable]
        public class Parameter {
            public string Key;
            public string Value;
            public Parameter(string key, string value) {
                Key = key;
                Value = value;
            }
        }
        bool LogEvent(string eventName);
        bool LogEvent(string eventName, Parameter[] parameters);
    }
}
