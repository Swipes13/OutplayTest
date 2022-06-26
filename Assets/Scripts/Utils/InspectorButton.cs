using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class InspectorButtonAttribute : PropertyAttribute {
    public const float kDefaultButtonWidth = 80;
    public readonly string MethodName;

    private float _buttonWidth = kDefaultButtonWidth;
    public float ButtonWidth { get => _buttonWidth; set { _buttonWidth = value; } }
    public InspectorButtonAttribute(string methodName, float buttonWidth = kDefaultButtonWidth) {
        MethodName = methodName;
        ButtonWidth = buttonWidth;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
public class InspectorButtonPropertyDrawer : PropertyDrawer {
    private MethodInfo _eventMethodInfo = null;

    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label) {
        var buttonAtt = (InspectorButtonAttribute)attribute;
        var buttonRect = new Rect(position.x, position.y, buttonAtt.ButtonWidth, position.height);
        if (GUI.Button(buttonRect, label.text)) {
            var eventOwnerType = prop.serializedObject.targetObject.GetType();
            var eventName = buttonAtt.MethodName;

            if (_eventMethodInfo == null) _eventMethodInfo = eventOwnerType.GetMethod(eventName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (_eventMethodInfo != null) _eventMethodInfo.Invoke(prop.serializedObject.targetObject, null);
            else Debug.LogWarning($"InspectorButton: Unable to find method {eventName} in {eventOwnerType}");
        }
    }
}
#endif