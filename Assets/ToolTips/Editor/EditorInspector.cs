using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Text;
using System;

/// <summary>
// Date:    9th September 2016
// Author:  Lukas Tomondy
// Info:    This class can show basic field types in Unity Editor Window, with most common attributes, as it is in Inspector
/// </summary>
public class EditorInspector
{
    public static void Show(object obj)
    {
        if (obj == null)
            return;

        FieldInfo[] fieldInfo = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        
        foreach (var field in fieldInfo)
        {
            ShowHeader(field);

            string fieldName = FirstLettersToUpper(AddSpacesToSentence(field.Name));
            GUIContent guiContent = GetContent(field, fieldName);

            // Draw fields
            if (field.FieldType == typeof(int))
            {
                int value = (int)field.GetValue(obj);
                int editorValue = EditorGUILayout.IntField(guiContent, value);
                field.SetValue(obj, editorValue);
            }
            if (field.FieldType == typeof(float))
            {
                float value = (float)field.GetValue(obj);
                float editorValue = EditorGUILayout.FloatField(guiContent, value);
                field.SetValue(obj, editorValue);
            }
            if (field.FieldType == typeof(Color))
            {
                Color value = (Color)field.GetValue(obj);
                Color editorValue = EditorGUILayout.ColorField(guiContent, value);
                field.SetValue(obj, editorValue);
            }
            if (field.FieldType == typeof(bool))
            {
                bool value = (bool)field.GetValue(obj);
                bool editorValue = EditorGUILayout.Toggle(guiContent, value);
                field.SetValue(obj, editorValue);
            }
            if (field.FieldType.IsEnum)
            {
                Enum value = (Enum)field.GetValue(obj);
                Enum editorValue = EditorGUILayout.EnumPopup(guiContent, value);
                field.SetValue(obj, editorValue);
            }
            if (field.GetValue(obj) is UnityEngine.Object)
            {
                UnityEngine.Object value = (UnityEngine.Object)field.GetValue(obj);
                UnityEngine.Object editorValue = EditorGUILayout.ObjectField(guiContent, value, value.GetType());
                field.SetValue(obj, editorValue);
            }
            if (field.FieldType == typeof(string))
            {
                string value = (string)field.GetValue(obj);
                string editorValue = EditorGUILayout.TextField(guiContent, value);
                field.SetValue(obj, editorValue);
            }
            if (field.FieldType == typeof(Vector3))
            {
                Vector3 value = (Vector3)field.GetValue(obj);
                Vector3 editorValue = EditorGUILayout.Vector3Field(guiContent, value);
                field.SetValue(obj, editorValue);
            }
        }
    }

    private static GUIContent GetContent(FieldInfo field, string fieldName)
    {
        object[] tooltipAttributes = field.GetCustomAttributes(typeof(TooltipAttribute), false);
        if (tooltipAttributes.Length == 0)
            return new GUIContent(fieldName);
        TooltipAttribute tooltip = (TooltipAttribute)tooltipAttributes[0];
        return new GUIContent(fieldName, tooltip.tooltip);
    }

    private static void ShowHeader(FieldInfo field)
    {
        object[] headerAttributes = field.GetCustomAttributes(typeof(HeaderAttribute), false);
        if (headerAttributes.Length > 0)
        {
            HeaderAttribute header = (HeaderAttribute)headerAttributes[0];
            GUILayout.Space(4);
            GUILayout.Label(header.header, EditorStyles.boldLabel);
        }
    }

    private static bool CanBeShown(FieldInfo field)
    {
        object[] hideInInspectorAttributes = field.GetCustomAttributes(typeof(HideInInspector), false);
        object[] nonSerializedAttributes = field.GetCustomAttributes(typeof(NonSerializedAttribute), false);
        object[] serializeFieldAttributes = field.GetCustomAttributes(typeof(SerializeField), false);
        if (field.Attributes == FieldAttributes.Private && serializeFieldAttributes.Length == 0)
            return false;
        return nonSerializedAttributes.Length == 0 && hideInInspectorAttributes.Length == 0;
    }

    private static string FirstLettersToUpper(string text)
    {
        return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
    }

    private static string AddSpacesToSentence(string text, bool preserveAcronyms = false)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
                if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                        i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
    }
}