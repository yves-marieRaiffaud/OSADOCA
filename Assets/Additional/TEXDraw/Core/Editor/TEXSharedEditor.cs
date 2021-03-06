using UnityEngine;
using System.IO;
using UnityEditor;
using TexDrawLib;
using System.Linq;

public class TEXSharedEditor
{
    
    static GUIContent g_FontIndex = new GUIContent("Font Index");
    
    public static int DoFontIndexSelection(int FontIndex)
    {
        return EditorGUILayout.IntPopup(g_FontIndex, FontIndex, TEXPreference.main.FontIDsGUI, TEXPreference.main.FontIndexs);
    }

    public static void DoFontIndexSelection(SerializedProperty font)
    {
        // -1 is math default
        EditorGUI.BeginChangeCheck();
        var val = DoFontIndexSelection(font.hasMultipleDifferentValues ? -2 : font.intValue);
        if (EditorGUI.EndChangeCheck() && val != -2)
            font.intValue = val;
    }

    public enum MatWarnType
    {
        Okay = 0,
        Null = 1,
        MultipleValues = 2,
        NotRegistered = 3,
        NotATexDrawMaterial = 4,
        RequireUV2Fill = 5,
        AmbigueShaderTags = 6,
    }

    public static MatWarnType GetMaterialIssue(Material mat, ITEXDraw texDraw)
    {
        var cases = MatWarnType.Okay;
        if (!mat)
        {
            cases = MatWarnType.Null;
        }
        else
        {
            var keyWord = mat.GetTag("TexMaterialType", false, "Null");
            if (keyWord == "Null")
                cases = MatWarnType.NotATexDrawMaterial;
            else if (keyWord == "Standard")
                cases = MatWarnType.Okay;
            else if (!TEXPreference.main.watchedMaterial.Contains(mat))
                cases = MatWarnType.NotRegistered;
            else if (keyWord != "RequireUV2" && keyWord != "RequireTMPFix")
                cases = MatWarnType.AmbigueShaderTags;
            else if (texDraw == null)
                cases = MatWarnType.Okay;
            else if (keyWord == "RequireUV2" && texDraw.autoFill == Filling.None)
                cases = MatWarnType.RequireUV2Fill;
        }
        return cases;
    }

    static float GetRotationAngle (TEXDraw tex)
    {
		var mtx = Quaternion.Inverse(tex.transform.rotation);
        if (tex.canvas)
            mtx = tex.canvas.transform.rotation * mtx;
		return Quaternion.Angle(mtx, Quaternion.identity);
    }

    static string[] matCaseLabels = new string[] { "Duplicate", "New Custom", "---", "FIX", "---", "Set AutoFill", "---", "Add TFollow", "Add TFollow", "Add TMPFix" };
    public static void DoMaterialGUI(SerializedProperty material, ITEXDraw texDraw)
    {
        var cases = MatWarnType.Okay;
        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(material, GUILayout.ExpandWidth(true));
        {
            var pref = TEXPreference.main;
            // 0 = Normal, 1 = Null, 2 = Multiple Value, 3 = Not registered

            if (material.hasMultipleDifferentValues)
                cases = MatWarnType.MultipleValues;
            else
            {
                var mat = (Material)material.objectReferenceValue;
                cases = GetMaterialIssue(mat, texDraw);
            }

            GUI.enabled = cases != MatWarnType.MultipleValues && cases != MatWarnType.NotATexDrawMaterial;
            if (GUILayout.Button(matCaseLabels[(int)cases], EditorStyles.miniButton, GUILayout.Width(80), GUILayout.Height(16)))
            {
                switch (cases)
                {
                    case MatWarnType.Okay:
                    case MatWarnType.Null:
                        var existMat = material.objectReferenceValue ?? pref.defaultMaterial;
                        var path = EditorUtility.SaveFilePanel("Save new Material into ...",
                        Path.GetDirectoryName(AssetDatabase.GetAssetPath(existMat)),
                        existMat.name + " - Copy",
                        "mat");
                        if (string.IsNullOrEmpty(path))
                            break;
                        path = path.Replace(Application.dataPath, "Assets");
                        var newMat = Object.Instantiate<Material>((Material)existMat);
                        material.objectReferenceValue = newMat;
                        AssetDatabase.CreateAsset(newMat, path);
                        ArrayUtility.Add(ref pref.watchedMaterial, newMat);
                        pref.RebuildMaterial();
                        AssetDatabase.SaveAssets();
                        cases = MatWarnType.Okay;
                        break;
                    case MatWarnType.NotRegistered:
                        if (AssetDatabase.Contains(material.objectReferenceValue))
                        {
                            ArrayUtility.Add(ref pref.watchedMaterial, (Material)material.objectReferenceValue);
                            pref.RebuildMaterial();
                            AssetDatabase.SaveAssets();
                            cases = MatWarnType.Okay;
                        }
                        break;
                    case MatWarnType.RequireUV2Fill:
                        texDraw.autoFill = Filling.LocalContinous;
                        break;
                }
            }
            GUI.enabled = true;

        }
        GUILayout.EndHorizontal();
        if (cases == MatWarnType.NotRegistered)
            EditorGUILayout.HelpBox("The material doesn't registered in Preference's Watched List", MessageType.Warning);
       else if (cases == MatWarnType.RequireUV2Fill)
            EditorGUILayout.HelpBox("Auto Fill must set other than \'None\' to make this material works properly", MessageType.Warning);
       else if (cases == MatWarnType.NotATexDrawMaterial)
            EditorGUILayout.HelpBox("Selected Material doesn't use TEXDraw shaders, or it's shader doesn't implement \"TexMaterialType\" Tag", MessageType.Warning);
       else if (cases == MatWarnType.AmbigueShaderTags)
            EditorGUILayout.HelpBox("Shader have Ambigue \"TexMaterialType\" Tag Value, possible values are \'Standard\' and \'RequireUV2\'", MessageType.Warning);
    }



    //These bunch of Codes are actually coming from UnityEditor.UI thanks for it's open source code

    #region Alignment Control

    //The license below applies for this region only
    /*
		The MIT License (MIT)

		Copyright (c) 2014-2015, Unity Technologies

		Permission is hereby granted, free of charge, to any person obtaining a copy
		of this software and associated documentation files (the "Software"), to deal
		in the Software without restriction, including without limitation the rights
		to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
		copies of the Software, and to permit persons to whom the Software is
		furnished to do so, subject to the following conditions:

		The above copyright notice and this permission notice shall be included in
		all copies or substantial portions of the Software.

		THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
		IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
		FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
		AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
		LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
		OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
		THE SOFTWARE.
	*/

    private const int kAlignmentButtonWidth = 20;

    static int s_TextAlignmentHash = "DoTextAligmentControl".GetHashCode();

    static private class Styles
    {
        public static GUIStyle alignmentButtonLeft = new GUIStyle(EditorStyles.miniButtonLeft);
        public static GUIStyle alignmentButtonMid = new GUIStyle(EditorStyles.miniButtonMid);
        public static GUIStyle alignmentButtonRight = new GUIStyle(EditorStyles.miniButtonRight);

        public static GUIContent m_EncodingContent;

        public static GUIContent m_LeftAlignText;
        public static GUIContent m_CenterAlignText;
        public static GUIContent m_RightAlignText;
        public static GUIContent m_TopAlignText;
        public static GUIContent m_MiddleAlignText;
        public static GUIContent m_BottomAlignText;

        public static GUIContent m_LeftAlignTextActive;
        public static GUIContent m_CenterAlignTextActive;
        public static GUIContent m_RightAlignTextActive;
        public static GUIContent m_TopAlignTextActive;
        public static GUIContent m_MiddleAlignTextActive;
        public static GUIContent m_BottomAlignTextActive;

        static Styles()
        {
            m_EncodingContent = new GUIContent("Rich Text", "Use emoticons and colors");

            // Horizontal Aligment Icons
            m_LeftAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_horizontally_left", "Left Align");
            m_CenterAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_horizontally_center", "Center Align");
            m_RightAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_horizontally_right", "Right Align");
            m_LeftAlignTextActive = EditorGUIUtility.IconContent(@"GUISystem/align_horizontally_left_active", "Left Align");
            m_CenterAlignTextActive = EditorGUIUtility.IconContent(@"GUISystem/align_horizontally_center_active", "Center Align");
            m_RightAlignTextActive = EditorGUIUtility.IconContent(@"GUISystem/align_horizontally_right_active", "Right Align");

            // Vertical Aligment Icons
            m_TopAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_vertically_top", "Top Align");
            m_MiddleAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_vertically_center", "Middle Align");
            m_BottomAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_vertically_bottom", "Bottom Align");
            m_TopAlignTextActive = EditorGUIUtility.IconContent(@"GUISystem/align_vertically_top_active", "Top Align");
            m_MiddleAlignTextActive = EditorGUIUtility.IconContent(@"GUISystem/align_vertically_center_active", "Middle Align");
            m_BottomAlignTextActive = EditorGUIUtility.IconContent(@"GUISystem/align_vertically_bottom_active", "Bottom Align");

            FixAlignmentButtonStyles(alignmentButtonLeft, alignmentButtonMid, alignmentButtonRight);
        }

        static void FixAlignmentButtonStyles(params GUIStyle[] styles)
        {
            foreach (GUIStyle style in styles)
            {
                style.padding.left = 2;
                style.padding.right = 2;
            }
        }
    }

    static public void DoTextAligmentControl(Rect position, SerializedProperty alignment)
    {
        GUIContent alingmentContent = new GUIContent("Alignment");

        int id = EditorGUIUtility.GetControlID(s_TextAlignmentHash, FocusType.Passive, position);

        EditorGUIUtility.SetIconSize(new Vector2(15, 15));
        EditorGUI.BeginProperty(position, alingmentContent, alignment);
        {
            Rect controlArea = EditorGUI.PrefixLabel(position, id, alingmentContent);

            float width = kAlignmentButtonWidth * 3;
            float spacing = Mathf.Clamp(controlArea.width - width * 2, 2, 10);

            Rect horizontalAligment = new Rect(controlArea.x, controlArea.y, width, controlArea.height);
            Rect verticalAligment = new Rect(horizontalAligment.xMax + spacing, controlArea.y, width, controlArea.height);

            DoHorizontalAligmentControl(horizontalAligment, alignment);
            DoVerticalAligmentControl(verticalAligment, alignment);
        }
        EditorGUI.EndProperty();
        EditorGUIUtility.SetIconSize(Vector2.zero);
    }

    private static void DoHorizontalAligmentControl(Rect position, SerializedProperty alignment)
    {
        float horizontalAlignment = alignment.vector2Value.x;

        bool leftAlign = (horizontalAlignment == 0f);
        bool centerAlign = (horizontalAlignment == 0.5f);
        bool rightAlign = (horizontalAlignment == 1f);

        if (alignment.hasMultipleDifferentValues)
        {
            foreach (var obj in alignment.serializedObject.targetObjects)
            {
                var text = obj as ITEXDraw;
                horizontalAlignment = text.alignment.x;
                leftAlign = leftAlign || (horizontalAlignment == 0f);
                centerAlign = centerAlign || (horizontalAlignment == 0.5f);
                rightAlign = rightAlign || (horizontalAlignment == 1f);
            }
        }

        position.width = kAlignmentButtonWidth;

        EditorGUI.BeginChangeCheck();
        EditorToggle(position, leftAlign, leftAlign ? Styles.m_LeftAlignTextActive : Styles.m_LeftAlignText, Styles.alignmentButtonLeft);
        if (EditorGUI.EndChangeCheck())
        {
            SetHorizontalAlignment(alignment, 0f);
        }

        position.x += position.width;
        EditorGUI.BeginChangeCheck();
        EditorToggle(position, centerAlign, centerAlign ? Styles.m_CenterAlignTextActive : Styles.m_CenterAlignText, Styles.alignmentButtonMid);
        if (EditorGUI.EndChangeCheck())
        {
            SetHorizontalAlignment(alignment, 0.5f);
        }

        position.x += position.width;
        EditorGUI.BeginChangeCheck();
        EditorToggle(position, rightAlign, rightAlign ? Styles.m_RightAlignTextActive : Styles.m_RightAlignText, Styles.alignmentButtonRight);
        if (EditorGUI.EndChangeCheck())
        {
            SetHorizontalAlignment(alignment, 1f);
        }
    }

    private static void DoVerticalAligmentControl(Rect position, SerializedProperty alignment)
    {
        float verticalTextAligment = alignment.vector2Value.y;

        bool topAlign = (verticalTextAligment == 1f);
        bool middleAlign = (verticalTextAligment == 0.5f);
        bool bottomAlign = (verticalTextAligment == 0f);

        if (alignment.hasMultipleDifferentValues)
        {
            foreach (var obj in alignment.serializedObject.targetObjects)
            {
                var text = obj as ITEXDraw;
                verticalTextAligment = text.alignment.y;
                topAlign = topAlign || (verticalTextAligment == 1f);
                middleAlign = middleAlign || (verticalTextAligment == 0.5f);
                bottomAlign = bottomAlign || (verticalTextAligment == 0f);
            }
        }


        position.width = kAlignmentButtonWidth;

        // position.x += position.width;
        EditorGUI.BeginChangeCheck();
        EditorToggle(position, topAlign, topAlign ? Styles.m_TopAlignTextActive : Styles.m_TopAlignText, Styles.alignmentButtonLeft);
        if (EditorGUI.EndChangeCheck())
        {
            SetVerticalAlignment(alignment, 1f);
        }

        position.x += position.width;
        EditorGUI.BeginChangeCheck();
        EditorToggle(position, middleAlign, middleAlign ? Styles.m_MiddleAlignTextActive : Styles.m_MiddleAlignText, Styles.alignmentButtonMid);
        if (EditorGUI.EndChangeCheck())
        {
            SetVerticalAlignment(alignment, 0.5f);
        }

        position.x += position.width;
        EditorGUI.BeginChangeCheck();
        EditorToggle(position, bottomAlign, bottomAlign ? Styles.m_BottomAlignTextActive : Styles.m_BottomAlignText, Styles.alignmentButtonRight);
        if (EditorGUI.EndChangeCheck())
        {
            SetVerticalAlignment(alignment, 0f);
        }
    }

    private static bool EditorToggle(Rect position, bool value, GUIContent content, GUIStyle style)
    {
        int hashCode = "AlignToggle".GetHashCode();
        int id = EditorGUIUtility.GetControlID(hashCode, FocusType.Passive, position);
        Event evt = Event.current;

        // Toggle selected toggle on space or return key
        if (EditorGUIUtility.keyboardControl == id && evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Space || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter))
        {
            value = !value;
            evt.Use();
            GUI.changed = true;
        }

        if (evt.type == EventType.KeyDown && Event.current.button == 0 && position.Contains(Event.current.mousePosition))
        {
            GUIUtility.keyboardControl = id;
            EditorGUIUtility.editingTextField = false;
            HandleUtility.Repaint();
        }

        bool returnValue = GUI.Toggle(position, id, value, content, style);

        return returnValue;
    }

    // We can't go through serialzied properties here since we're showing two controls for a single SerializzedProperty.
    private static void SetHorizontalAlignment(SerializedProperty alignment, float horizontalAlignment)
    {
        foreach (var obj in alignment.serializedObject.targetObjects)
        {
            var text = obj as ITEXDraw;
            Undo.RecordObject((Object)text, "Horizontal Alignment");
            text.alignment = new Vector2(horizontalAlignment, text.alignment.y);
            EditorUtility.SetDirty(obj);
        }
    }

    private static void SetVerticalAlignment(SerializedProperty alignment, float verticalAlignment)
    {
        foreach (var obj in alignment.serializedObject.targetObjects)
        {
            var text = obj as ITEXDraw;
            Undo.RecordObject((Object)text, "Vertical Alignment");
            text.alignment = new Vector2(text.alignment.x, verticalAlignment);
            EditorUtility.SetDirty(obj);
        }
    }

    #endregion
}

[CustomPropertyDrawer(typeof(ScaleOffset))]
public class ScaleOffsetDrawer : PropertyDrawer {
    
    public override void OnGUI (Rect position, SerializedProperty property,  GUIContent label) {
        var p_scale = property.FindPropertyRelative("scale");
        var p_offset = property.FindPropertyRelative("offset");
        EditorGUILayout.BeginHorizontal();
        {
            var l = EditorGUIUtility.labelWidth;
            var i = EditorGUI.indentLevel;
            EditorGUILayout.LabelField(label, GUILayout.Width(l));
            EditorGUIUtility.labelWidth = 40;
            EditorGUI.indentLevel = 0;
            EditorGUILayout.PropertyField(p_scale);
            EditorGUILayout.PropertyField(p_offset);            
            EditorGUIUtility.labelWidth = l;
            EditorGUI.indentLevel = i;
        }
        EditorGUILayout.EndHorizontal();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }
}

[CustomPropertyDrawer(typeof(FindReplace))]
public class FindReplaceDrawer : PropertyDrawer {
    
    public override void OnGUI (Rect position, SerializedProperty property,  GUIContent label) {
        var p_find = property.FindPropertyRelative("find");
        var p_replace = property.FindPropertyRelative("replace");
		var l = EditorGUIUtility.labelWidth;
        var i = EditorGUI.indentLevel;

        EditorGUILayout.BeginHorizontal();
        {
			EditorGUIUtility.labelWidth = 0;
	        
			//EditorGUILayout.LabelField(label, GUILayout.Width(l - 30));

			EditorGUI.indentLevel = 0;

            EditorGUILayout.PropertyField(p_find, GUIContent.none);
			EditorGUILayout.PropertyField(p_replace, GUIContent.none, GUILayout.Width(l));            
        }
        EditorGUILayout.EndHorizontal();

		EditorGUIUtility.labelWidth = l;
        EditorGUI.indentLevel = i - 1;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0;
    }
}