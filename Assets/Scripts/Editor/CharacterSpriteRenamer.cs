using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// 角色精灵自动切片与命名工具
/// 用于批量处理角色图集，按照规范命名切片
/// </summary>
public class CharacterSpriteRenamer : EditorWindow
{
    // 动作定义类
    private class ActionDefinition
    {
        public string Prefix;
        public int FrameCount;
        public bool IsSpecial;

        public ActionDefinition(string prefix, int frameCount, bool isSpecial = false)
        {
            Prefix = prefix;
            FrameCount = frameCount;
            IsSpecial = isSpecial;
        }
    }

    // 跳跃帧定义类
    private class JumpFrameDefinition
    {
        public int FrameIndex;
        public string ActionName;
        public int Index;

        public JumpFrameDefinition(int frameIndex, string actionName, int index)
        {
            FrameIndex = frameIndex;
            ActionName = actionName;
            Index = index;
        }
    }

    // 颜色对照表
    private static readonly Dictionary<string, string> s_colorMap = new Dictionary<string, string>
    {
        { "blue", "Blue" },
        { "green", "Green" },
        { "purple", "Purple" },
        { "red", "Red" }
    };

    // Sheet 1 动作定义 (char_xxx_1.png) - 基于 Animation Guide
    // 图集布局: 8列, 56x56像素
    private static readonly List<ActionDefinition> s_sheet1Actions = new List<ActionDefinition>
    {
        new ActionDefinition("Idle", 6),           // Row 1: 待机 - 6帧
        new ActionDefinition("Atk_Combo", 8),      // Row 2: 攻击 - 8帧 (5基础+2连击+1额外)
        new ActionDefinition("Run", 8),            // Row 3: 奔跑 - 8帧
        new ActionDefinition("Jump", 16, true),    // Row 4-5: 跳跃组 - 16帧 [特殊拆分] (跨2行)
        new ActionDefinition("Hit", 4),            // Row 6: 受击 - 4帧
        new ActionDefinition("Death", 12),         // Row 7-8: 死亡 - 12帧 (跨2行)
        new ActionDefinition("Cast", 8),           // Row 9: 施法 - 8帧
        new ActionDefinition("Crouch", 3),         // Row 10: 下蹲 - 3帧
        new ActionDefinition("Block", 3)           // Row 11: 盾防 - 3帧
    };

    // Sheet 2 动作定义 (char_xxx_2.png) - 基于 Animation Guide 2
    // 图集布局: 8列, 56x56像素
    private static readonly List<ActionDefinition> s_sheet2Actions = new List<ActionDefinition>
    {
        new ActionDefinition("Walk", 10),          // Row 1-2: 走路 - 10帧 (跨2行)
        new ActionDefinition("Slide", 8, true),    // Row 3: 滑铲组 - 8帧 [特殊拆分]
        new ActionDefinition("WallSlide", 4),      // Row 4: 贴墙滑 - 4帧
        new ActionDefinition("Atk_Heavy", 8),      // Row 5: 重击 - 8帧
        new ActionDefinition("Climb", 10)          // Row 6-7: 爬梯 - 10帧 (跨2行)
    };

    // 跳跃组特殊命名定义 - 基于 Animation Guide Row 4-5
    // 16帧 = 2准备 + 4上升 + 3 Jumping reload + 4下落 + 3落地
    private static readonly List<JumpFrameDefinition> s_jumpFrames = new List<JumpFrameDefinition>
    {
        new JumpFrameDefinition(0, "Jump_Prep", 0),     // 准备起跳 0
        new JumpFrameDefinition(1, "Jump_Prep", 1),     // 准备起跳 1
        new JumpFrameDefinition(2, "Jump_Up", 0),       // 上升 0
        new JumpFrameDefinition(3, "Jump_Up", 1),       // 上升 1
        new JumpFrameDefinition(4, "Jump_Up", 2),       // 上升 2
        new JumpFrameDefinition(5, "Jump_Up", 3),       // 上升 3
        new JumpFrameDefinition(6, "Jump_Reload", 0),   // Jumping reload 0
        new JumpFrameDefinition(7, "Jump_Reload", 1),   // Jumping reload 1
        new JumpFrameDefinition(8, "Jump_Reload", 2),   // Jumping reload 2
        new JumpFrameDefinition(9, "Fall", 0),          // 下落 0
        new JumpFrameDefinition(10, "Fall", 1),         // 下落 1
        new JumpFrameDefinition(11, "Fall", 2),         // 下落 2
        new JumpFrameDefinition(12, "Fall", 3),         // 下落 3
        new JumpFrameDefinition(13, "Land", 0),         // 落地 0
        new JumpFrameDefinition(14, "Land", 1),         // 落地 1
        new JumpFrameDefinition(15, "Land", 2)          // 落地 2
    };

    // 滑铲组特殊命名定义 - 基于 Animation Guide 2 Row 3
    // 8帧 = 3 Sliding start + 3 Sliding loop + 2 Sliding end
    private static readonly List<JumpFrameDefinition> s_slideFrames = new List<JumpFrameDefinition>
    {
        new JumpFrameDefinition(0, "Slide_Start", 0),   // 滑铲开始 0
        new JumpFrameDefinition(1, "Slide_Start", 1),   // 滑铲开始 1
        new JumpFrameDefinition(2, "Slide_Start", 2),   // 滑铲开始 2
        new JumpFrameDefinition(3, "Slide_Loop", 0),    // 滑铲循环 0
        new JumpFrameDefinition(4, "Slide_Loop", 1),    // 滑铲循环 1
        new JumpFrameDefinition(5, "Slide_Loop", 2),    // 滑铲循环 2
        new JumpFrameDefinition(6, "Slide_End", 0),     // 滑铲结束 0
        new JumpFrameDefinition(7, "Slide_End", 1)      // 滑铲结束 1
    };

    // 切片尺寸设置 - 基于 Animation Guide: 56x56 size
    private int _spriteWidth = 56;
    private int _spriteHeight = 56;
    private int _columnsPerRow = 8;
    private Vector2 _pivot = new Vector2(0.5f, 0f);  // 底部中心对齐
    private int _pixelsPerUnit = 56;    // 每单位像素数

    private Vector2 _scrollPosition;
    private bool _showPreview = false;
    private string _previewText = "";

    [MenuItem("Tools/角色精灵命名工具")]
    public static void ShowWindow()
    {
        var window = GetWindow<CharacterSpriteRenamer>("角色精灵命名工具");
        window.minSize = new Vector2(450, 600);
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎮 角色精灵自动切片与命名工具", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "此工具会自动处理 Assets/Art Assets/Generic-Character/png 下的角色图集，\n" +
            "按照规范切片并命名精灵。支持蓝色、绿色、紫色、红色四种角色。",
            MessageType.Info);

        EditorGUILayout.Space(15);

        // 切片设置
        EditorGUILayout.LabelField("⚙️ 切片设置", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        _spriteWidth = EditorGUILayout.IntField("精灵宽度 (px)", _spriteWidth);
        _spriteHeight = EditorGUILayout.IntField("精灵高度 (px)", _spriteHeight);
        _columnsPerRow = EditorGUILayout.IntField("每行列数", _columnsPerRow);
        _pixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit", _pixelsPerUnit);
        _pivot = EditorGUILayout.Vector2Field("轴心点 (Pivot)", _pivot);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(15);

        // 预览按钮
        if (GUILayout.Button("👁 预览命名规则", GUILayout.Height(30)))
        {
            GeneratePreview();
            _showPreview = true;
        }

        if (_showPreview && !string.IsNullOrEmpty(_previewText))
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("命名预览:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(_previewText, GUILayout.Height(200));
        }

        EditorGUILayout.Space(15);

        // 执行按钮
        EditorGUILayout.LabelField("🚀 执行操作", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("处理单个颜色", GUILayout.Height(35)))
        {
            ShowColorSelectionMenu();
        }
        if (GUILayout.Button("处理所有颜色", GUILayout.Height(35)))
        {
            ProcessAllColors();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // 处理选中的图片
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("📂 处理选中的图集", GUILayout.Height(40)))
        {
            ProcessSelectedTextures();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(20);

        // 动作对照表
        DrawActionReference();

        EditorGUILayout.EndScrollView();
    }

    private void GeneratePreview()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Sheet 1 (char_xxx_1.png) 命名预览 ===");
        sb.AppendLine();

        foreach (var action in s_sheet1Actions)
        {
            if (action.IsSpecial && action.Prefix == "Jump")
            {
                sb.AppendLine($"Row {s_sheet1Actions.IndexOf(action) + 1} - 跳跃组:");
                foreach (var frame in s_jumpFrames)
                {
                    string name = frame.Index >= 0
                        ? $"Char_Blue_{frame.ActionName}_{frame.Index:D2}"
                        : $"Char_Blue_{frame.ActionName}";
                    sb.AppendLine($"  帧{frame.FrameIndex}: {name}");
                }
            }
            else
            {
                sb.AppendLine($"Row {s_sheet1Actions.IndexOf(action) + 1} - {action.Prefix}:");
                sb.AppendLine($"  Char_Blue_{action.Prefix}_00 ~ {action.FrameCount - 1:D2}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("=== Sheet 2 (char_xxx_2.png) 命名预览 ===");
        sb.AppendLine();

        foreach (var action in s_sheet2Actions)
        {
            if (action.IsSpecial && action.Prefix == "Slide")
            {
                sb.AppendLine($"Row {s_sheet2Actions.IndexOf(action) + 1} - 滑铲组:");
                foreach (var frame in s_slideFrames)
                {
                    sb.AppendLine($"  帧{frame.FrameIndex}: Char_Blue_{frame.ActionName}_{frame.Index:D2}");
                }
            }
            else
            {
                sb.AppendLine($"Row {s_sheet2Actions.IndexOf(action) + 1} - {action.Prefix}:");
                sb.AppendLine($"  Char_Blue_{action.Prefix}_00 ~ {action.FrameCount - 1:D2}");
            }
        }

        _previewText = sb.ToString();
    }

    private void ShowColorSelectionMenu()
    {
        GenericMenu menu = new GenericMenu();
        foreach (var color in s_colorMap)
        {
            string colorKey = color.Key;
            menu.AddItem(new GUIContent(color.Value), false, () => ProcessSingleColor(colorKey));
        }
        menu.ShowAsContext();
    }

    private void ProcessSingleColor(string colorFolder)
    {
        string basePath = "Assets/Art Assets/Generic-Character/png";
        string colorPath = $"{basePath}/{colorFolder}";

        if (!AssetDatabase.IsValidFolder(colorPath))
        {
            EditorUtility.DisplayDialog("错误", $"找不到文件夹: {colorPath}", "确定");
            return;
        }

        ProcessColorFolder(colorPath, colorFolder);
        EditorUtility.DisplayDialog("完成", $"{s_colorMap[colorFolder]} 角色精灵处理完成！", "确定");
    }

    private void ProcessAllColors()
    {
        string basePath = "Assets/Art Assets/Generic-Character/png";
        int processedCount = 0;

        foreach (var color in s_colorMap)
        {
            string colorPath = $"{basePath}/{color.Key}";
            if (AssetDatabase.IsValidFolder(colorPath))
            {
                ProcessColorFolder(colorPath, color.Key);
                processedCount++;
            }
        }

        EditorUtility.DisplayDialog("完成", $"已处理 {processedCount} 种颜色的角色精灵！", "确定");
    }

    private void ProcessColorFolder(string folderPath, string colorKey)
    {
        string colorName = s_colorMap[colorKey];

        // 查找并处理 Sheet 1
        string sheet1Path = $"{folderPath}/char_{colorKey}_1.png";
        if (File.Exists(sheet1Path.Replace("Assets/", Application.dataPath + "/")))
        {
            ProcessSpriteSheet(sheet1Path, colorName, s_sheet1Actions, 1);
        }

        // 查找并处理 Sheet 2
        string sheet2Path = $"{folderPath}/char_{colorKey}_2.png";
        if (File.Exists(sheet2Path.Replace("Assets/", Application.dataPath + "/")))
        {
            ProcessSpriteSheet(sheet2Path, colorName, s_sheet2Actions, 2);
        }

        AssetDatabase.Refresh();
    }

    private void ProcessSpriteSheet(string assetPath, string colorName, List<ActionDefinition> actions, int sheetNumber)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"无法找到图片: {assetPath}");
            return;
        }

        // 设置纹理导入参数
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = _pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        // 读取纹理尺寸
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (texture == null)
        {
            Debug.LogWarning($"无法加载纹理: {assetPath}");
            return;
        }

        int textureHeight = texture.height;

        // 使用新的 Sprite Editor Data Provider API
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        var spriteRects = new List<SpriteRect>();
        int currentRow = 0;

        foreach (var action in actions)
        {
            for (int frame = 0; frame < action.FrameCount; frame++)
            {
                int col = frame % _columnsPerRow;
                int rowOffset = frame / _columnsPerRow;

                // 计算精灵在图集中的位置 (Unity坐标系从左下角开始)
                int x = col * _spriteWidth;
                int y = textureHeight - ((currentRow + rowOffset + 1) * _spriteHeight);

                // 生成精灵名称
                string spriteName = GenerateSpriteName(colorName, action, frame, sheetNumber);

                var spriteRect = new SpriteRect
                {
                    name = spriteName,
                    rect = new Rect(x, y, _spriteWidth, _spriteHeight),
                    pivot = _pivot,
                    alignment = SpriteAlignment.Custom,
                    spriteID = GUID.Generate()
                };

                spriteRects.Add(spriteRect);
            }

            // 计算这个动作占用了多少行
            currentRow += Mathf.CeilToInt((float)action.FrameCount / _columnsPerRow);
        }

        // 应用切片数据
        dataProvider.SetSpriteRects(spriteRects.ToArray());
        dataProvider.Apply();

        // 保存并重新导入
        var assetImporterEditor = (AssetImporter)dataProvider.targetObject;
        assetImporterEditor.SaveAndReimport();

        Debug.Log($"✅ 已处理: {assetPath}, 生成 {spriteRects.Count} 个精灵切片");
    }

    private string GenerateSpriteName(string colorName, ActionDefinition action, int frameIndex, int sheetNumber)
    {
        // 跳跃组特殊处理 (Sheet 1, Row 4)
        if (sheetNumber == 1 && action.IsSpecial && action.Prefix == "Jump")
        {
            var jumpFrame = s_jumpFrames.FirstOrDefault(j => j.FrameIndex == frameIndex);
            if (jumpFrame != null)
            {
                if (jumpFrame.Index >= 0)
                {
                    return $"Char_{colorName}_{jumpFrame.ActionName}_{jumpFrame.Index:D2}";
                }
                else
                {
                    return $"Char_{colorName}_{jumpFrame.ActionName}";
                }
            }
        }

        // 滑铲组特殊处理 (Sheet 2, Row 3)
        if (sheetNumber == 2 && action.IsSpecial && action.Prefix == "Slide")
        {
            var slideFrame = s_slideFrames.FirstOrDefault(j => j.FrameIndex == frameIndex);
            if (slideFrame != null)
            {
                return $"Char_{colorName}_{slideFrame.ActionName}_{slideFrame.Index:D2}";
            }
        }

        // 标准命名格式
        return $"Char_{colorName}_{action.Prefix}_{frameIndex:D2}";
    }

    private void ProcessSelectedTextures()
    {
        Object[] selectedObjects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "请先在 Project 窗口中选择要处理的图集文件", "确定");
            return;
        }

        foreach (Object obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string fileName = Path.GetFileNameWithoutExtension(assetPath).ToLower();

            // 解析文件名获取颜色和 sheet 编号
            // 格式: char_blue_1 或 char_red_2
            string[] parts = fileName.Split('_');
            if (parts.Length >= 3 && parts[0] == "char")
            {
                string colorKey = parts[1];
                int sheetNumber = 0;
                if (int.TryParse(parts[2], out sheetNumber))
                {
                    if (s_colorMap.ContainsKey(colorKey))
                    {
                        string colorName = s_colorMap[colorKey];
                        var actions = sheetNumber == 1 ? s_sheet1Actions : s_sheet2Actions;
                        ProcessSpriteSheet(assetPath, colorName, actions, sheetNumber);
                    }
                }
            }
        }

        EditorUtility.DisplayDialog("完成", "选中的图集处理完成！", "确定");
    }

    private void DrawActionReference()
    {
        EditorGUILayout.LabelField("📋 动作对照表", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Sheet 1 (基础动作):", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Row 1: Idle (待机) - 6帧");
        EditorGUILayout.LabelField("Row 2: Atk_Combo (攻击) - 8帧");
        EditorGUILayout.LabelField("Row 3: Run (奔跑) - 8帧");
        EditorGUILayout.LabelField("Row 4-5: Jump组 (跳跃) - 16帧 [特殊拆分]");
        EditorGUILayout.LabelField("Row 6: Hit (受击) - 4帧");
        EditorGUILayout.LabelField("Row 7-8: Death (死亡) - 12帧 [跨行]");
        EditorGUILayout.LabelField("Row 9: Cast (施法) - 8帧");
        EditorGUILayout.LabelField("Row 10: Crouch (下蹲) - 3帧");
        EditorGUILayout.LabelField("Row 11: Block (盾防) - 3帧");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Sheet 2 (进阶动作):", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Row 1-2: Walk (走路) - 10帧 [跨行]");
        EditorGUILayout.LabelField("Row 3: Slide组 (滑铲) - 8帧 [特殊: Start/Loop/End]");
        EditorGUILayout.LabelField("Row 4: WallSlide (贴墙滑) - 4帧");
        EditorGUILayout.LabelField("Row 5: Atk_Heavy (重击) - 8帧");
        EditorGUILayout.LabelField("Row 6-7: Climb (爬梯) - 10帧 [跨行]");
        EditorGUILayout.EndVertical();
    }
}
