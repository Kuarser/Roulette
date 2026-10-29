#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FontGenerator : EditorWindow
{
    private const byte AlphaThreshold = 8;

    private Texture2D texture;
    private DefaultAsset imageFolder;
    private string characters = "0123456789";
    private string fontName = string.Empty;
    private string outputDirectory = "Assets/MyFonts/Generated";
    private string detectionMessage = string.Empty;
    private string folderMessage = string.Empty;
    private Vector2 scrollPosition;

    private int glyphWidth = 24;
    private int glyphHeight = 33;
    private int startX;
    private int startY;
    private int horizontalSpacing = 1;
    private int verticalSpacing;
    private int columns = 10;
    private int advance = 25;
    private int lineHeight = 33;
    private int folderCharacterSpacing = 1;

    [MenuItem("Tools/Fonts/Font Generator")]
    private static void Open()
    {
        FontGenerator window = GetWindow<FontGenerator>("Font Generator");
        window.minSize = new Vector2(380f, 500f);
    }

    private void OnEnable()
    {
        if (texture == null && Selection.activeObject is Texture2D selectedTexture)
        {
            SetTexture(selectedTexture);
        }
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.LabelField("Atlas Texture Source", EditorStyles.boldLabel);

        Texture2D selectedTexture = (Texture2D)EditorGUILayout.ObjectField(
            "Texture",
            texture,
            typeof(Texture2D),
            false);

        if (selectedTexture != texture)
        {
            SetTexture(selectedTexture);
        }

        using (new EditorGUI.DisabledScope(texture == null))
        {
            if (GUILayout.Button("Auto Detect Characters And Grid Layout"))
            {
                AutoDetect();
            }
        }

        if (string.IsNullOrEmpty(detectionMessage) == false)
        {
            EditorGUILayout.HelpBox(detectionMessage, MessageType.Info);
        }

        characters = EditorGUILayout.TextField("Characters", characters);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Layout", EditorStyles.boldLabel);
        glyphWidth = EditorGUILayout.IntField("Glyph Width", glyphWidth);
        glyphHeight = EditorGUILayout.IntField("Glyph Height", glyphHeight);
        startX = EditorGUILayout.IntField("Start X", startX);
        startY = EditorGUILayout.IntField("Start Y", startY);
        horizontalSpacing = EditorGUILayout.IntField("Horizontal Spacing", horizontalSpacing);
        verticalSpacing = EditorGUILayout.IntField("Vertical Spacing", verticalSpacing);
        columns = EditorGUILayout.IntField("Columns", columns);
        advance = EditorGUILayout.IntField("Advance", advance);
        lineHeight = EditorGUILayout.IntField("Line Height", lineHeight);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Single Image Folder Source", EditorStyles.boldLabel);

        DefaultAsset selectedFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Image Folder",
            imageFolder,
            typeof(DefaultAsset),
            false);

        if (selectedFolder != imageFolder)
        {
            SetImageFolder(selectedFolder);
        }

        folderCharacterSpacing = EditorGUILayout.IntField(
            "Character Spacing",
            folderCharacterSpacing);

        if (string.IsNullOrEmpty(folderMessage) == false)
        {
            EditorGUILayout.HelpBox(folderMessage, MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
        fontName = EditorGUILayout.TextField("Font Name", fontName);
        outputDirectory = EditorGUILayout.TextField("Output Directory", outputDirectory);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(texture == null))
        {
            if (GUILayout.Button("Generate From Atlas Texture", GUILayout.Height(30f)))
            {
                Generate();
            }
        }

        using (new EditorGUI.DisabledScope(imageFolder == null))
        {
            if (GUILayout.Button("Generate From Image Folder", GUILayout.Height(30f)))
            {
                GenerateFromImageFolder();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void SetTexture(Texture2D selectedTexture)
    {
        texture = selectedTexture;
        detectionMessage = string.Empty;
        if (texture != null)
        {
            fontName = texture.name;
        }
    }

    private void SetImageFolder(DefaultAsset selectedFolder)
    {
        imageFolder = selectedFolder;
        folderMessage = string.Empty;

        string folderPath = AssetDatabase.GetAssetPath(imageFolder);
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            fontName = Path.GetFileName(folderPath);
        }
    }

    private void AutoDetect()
    {
        string texturePath = AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(texturePath) || texturePath.StartsWith("Assets/") == false)
        {
            ShowError("The selected texture must be an asset inside this project.");
            return;
        }

        string projectDirectory = Directory.GetParent(Application.dataPath).FullName;
        string sourcePath = Path.Combine(projectDirectory, texturePath);
        Texture2D readableTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        try
        {
            byte[] imageData = File.ReadAllBytes(sourcePath);
            if (ImageConversion.LoadImage(readableTexture, imageData, false) == false)
            {
                ShowError("The selected image format could not be read.");
                return;
            }

            List<DetectedGlyph> detectedGlyphs = DetectGlyphs(readableTexture);
            if (detectedGlyphs.Count == 0)
            {
                ShowError("No visible glyph regions were detected in the image.");
                return;
            }

            ApplyDetectedLayout(detectedGlyphs);

            string detectedCharacters = GetCharacterPreset(detectedGlyphs.Count);
            if (string.IsNullOrEmpty(detectedCharacters) == false)
            {
                characters = detectedCharacters;
                detectionMessage =
                    "Detected " + detectedGlyphs.Count +
                    " glyph regions. Characters and Grid Layout were filled automatically.";
            }
            else
            {
                detectionMessage =
                    "Detected " + detectedGlyphs.Count +
                    " glyph regions and filled Grid Layout. The character meaning cannot be " +
                    "determined reliably, so please confirm Characters manually.";
            }
        }
        catch (IOException exception)
        {
            ShowError("Failed to read the selected image:\n" + exception.Message);
        }
        finally
        {
            DestroyImmediate(readableTexture);
        }
    }

    private static List<DetectedGlyph> DetectGlyphs(Texture2D sourceTexture)
    {
        int width = sourceTexture.width;
        int height = sourceTexture.height;
        Color32[] pixels = sourceTexture.GetPixels32();
        bool[] visited = new bool[width * height];
        List<DetectedGlyph> detectedGlyphs = new List<DetectedGlyph>();
        Queue<int> pendingPixels = new Queue<int>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int position = y * width + x;
                if (visited[position] || GetAlpha(pixels, width, height, x, y) <= AlphaThreshold)
                {
                    continue;
                }

                visited[position] = true;
                pendingPixels.Enqueue(position);

                int minX = x;
                int maxX = x;
                int minY = y;
                int maxY = y;
                int pixelCount = 0;

                while (pendingPixels.Count > 0)
                {
                    int currentPosition = pendingPixels.Dequeue();
                    int currentX = currentPosition % width;
                    int currentY = currentPosition / width;

                    minX = Mathf.Min(minX, currentX);
                    maxX = Mathf.Max(maxX, currentX);
                    minY = Mathf.Min(minY, currentY);
                    maxY = Mathf.Max(maxY, currentY);
                    pixelCount++;

                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        for (int offsetX = -1; offsetX <= 1; offsetX++)
                        {
                            if (offsetX == 0 && offsetY == 0)
                            {
                                continue;
                            }

                            int nextX = currentX + offsetX;
                            int nextY = currentY + offsetY;
                            if (nextX < 0 || nextX >= width || nextY < 0 || nextY >= height)
                            {
                                continue;
                            }

                            int nextPosition = nextY * width + nextX;
                            if (visited[nextPosition] ||
                                GetAlpha(pixels, width, height, nextX, nextY) <= AlphaThreshold)
                            {
                                continue;
                            }

                            visited[nextPosition] = true;
                            pendingPixels.Enqueue(nextPosition);
                        }
                    }
                }

                detectedGlyphs.Add(
                    new DetectedGlyph(
                        new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1),
                        pixelCount));
            }
        }

        int largestPixelCount = 0;
        foreach (DetectedGlyph detectedGlyph in detectedGlyphs)
        {
            largestPixelCount = Mathf.Max(largestPixelCount, detectedGlyph.PixelCount);
        }

        int minimumPixelCount = Mathf.Max(2, Mathf.RoundToInt(largestPixelCount * 0.01f));
        detectedGlyphs.RemoveAll(glyph => glyph.PixelCount < minimumPixelCount);
        detectedGlyphs.Sort((left, right) =>
        {
            int verticalComparison = left.Bounds.yMin.CompareTo(right.Bounds.yMin);
            return verticalComparison != 0
                ? verticalComparison
                : left.Bounds.xMin.CompareTo(right.Bounds.xMin);
        });

        return detectedGlyphs;
    }

    private void ApplyDetectedLayout(List<DetectedGlyph> detectedGlyphs)
    {
        List<List<DetectedGlyph>> rows = GroupGlyphRows(detectedGlyphs);
        List<int> horizontalSteps = new List<int>();
        List<float> rowCenters = new List<float>();

        columns = 1;
        foreach (List<DetectedGlyph> row in rows)
        {
            row.Sort((left, right) => left.Bounds.xMin.CompareTo(right.Bounds.xMin));
            columns = Mathf.Max(columns, row.Count);

            float centerTotal = 0f;
            for (int i = 0; i < row.Count; i++)
            {
                centerTotal += row[i].CenterY;
                if (i > 0)
                {
                    horizontalSteps.Add(
                        Mathf.RoundToInt(row[i].CenterX - row[i - 1].CenterX));
                }
            }

            rowCenters.Add(centerTotal / row.Count);
        }

        int horizontalStep = horizontalSteps.Count > 0
            ? GetMedian(horizontalSteps)
            : GetMaximumWidth(detectedGlyphs);

        List<int> verticalSteps = new List<int>();
        for (int i = 1; i < rowCenters.Count; i++)
        {
            verticalSteps.Add(Mathf.RoundToInt(rowCenters[i] - rowCenters[i - 1]));
        }

        int verticalStep = verticalSteps.Count > 0
            ? GetMedian(verticalSteps)
            : GetMaximumHeight(detectedGlyphs);

        startX = int.MaxValue;
        startY = int.MaxValue;

        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < rows[rowIndex].Count; columnIndex++)
            {
                DetectedGlyph glyph = rows[rowIndex][columnIndex];
                startX = Mathf.Min(
                    startX,
                    glyph.Bounds.xMin - columnIndex * horizontalStep);
                startY = Mathf.Min(
                    startY,
                    glyph.Bounds.yMin - rowIndex * verticalStep);
            }
        }

        startX = Mathf.Max(0, startX);
        startY = Mathf.Max(0, startY);
        glyphWidth = 1;
        glyphHeight = 1;

        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < rows[rowIndex].Count; columnIndex++)
            {
                RectInt bounds = rows[rowIndex][columnIndex].Bounds;
                int cellX = startX + columnIndex * horizontalStep;
                int cellY = startY + rowIndex * verticalStep;
                glyphWidth = Mathf.Max(glyphWidth, bounds.xMax - cellX);
                glyphHeight = Mathf.Max(glyphHeight, bounds.yMax - cellY);
            }
        }

        horizontalSpacing = Mathf.Max(0, horizontalStep - glyphWidth);
        verticalSpacing = Mathf.Max(0, verticalStep - glyphHeight);
        advance = horizontalStep;
        lineHeight = verticalStep;
    }

    private static List<List<DetectedGlyph>> GroupGlyphRows(List<DetectedGlyph> detectedGlyphs)
    {
        List<List<DetectedGlyph>> rows = new List<List<DetectedGlyph>>();

        foreach (DetectedGlyph detectedGlyph in detectedGlyphs)
        {
            List<DetectedGlyph> targetRow = null;
            foreach (List<DetectedGlyph> row in rows)
            {
                if (HasVerticalOverlap(row[0].Bounds, detectedGlyph.Bounds))
                {
                    targetRow = row;
                    break;
                }
            }

            if (targetRow == null)
            {
                targetRow = new List<DetectedGlyph>();
                rows.Add(targetRow);
            }

            targetRow.Add(detectedGlyph);
        }

        rows.Sort((left, right) => left[0].Bounds.yMin.CompareTo(right[0].Bounds.yMin));
        return rows;
    }

    private static bool HasVerticalOverlap(RectInt left, RectInt right)
    {
        return left.yMin < right.yMax && right.yMin < left.yMax;
    }

    private static byte GetAlpha(Color32[] pixels, int width, int height, int x, int topY)
    {
        int bottomY = height - topY - 1;
        return pixels[bottomY * width + x].a;
    }

    private static int GetMedian(List<int> values)
    {
        values.Sort();
        return values[values.Count / 2];
    }

    private static int GetMaximumWidth(List<DetectedGlyph> detectedGlyphs)
    {
        int maximumWidth = 1;
        foreach (DetectedGlyph detectedGlyph in detectedGlyphs)
        {
            maximumWidth = Mathf.Max(maximumWidth, detectedGlyph.Bounds.width);
        }

        return maximumWidth;
    }

    private static int GetMaximumHeight(List<DetectedGlyph> detectedGlyphs)
    {
        int maximumHeight = 1;
        foreach (DetectedGlyph detectedGlyph in detectedGlyphs)
        {
            maximumHeight = Mathf.Max(maximumHeight, detectedGlyph.Bounds.height);
        }

        return maximumHeight;
    }

    private static string GetCharacterPreset(int glyphCount)
    {
        switch (glyphCount)
        {
            case 10:
                return "0123456789";
            case 16:
                return "0123456789ABCDEF";
            case 26:
                return "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            case 36:
                return "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            default:
                return string.Empty;
        }
    }

    private void GenerateFromImageFolder()
    {
        string folderPath = AssetDatabase.GetAssetPath(imageFolder);
        if (AssetDatabase.IsValidFolder(folderPath) == false)
        {
            ShowError("Please select a valid folder inside Assets.");
            return;
        }

        if (folderCharacterSpacing < 0)
        {
            ShowError("Character Spacing cannot be negative.");
            return;
        }

        if (ValidateOutputSettings() == false)
        {
            return;
        }

        List<FolderGlyph> folderGlyphs = LoadFolderGlyphs(folderPath);
        if (folderGlyphs == null || folderGlyphs.Count == 0)
        {
            return;
        }

        string normalizedOutputDirectory = NormalizeAssetPath(outputDirectory);
        string atlasPath = normalizedOutputDirectory + "/" + fontName + ".png";
        string materialPath = normalizedOutputDirectory + "/" + fontName + ".mat";
        string fontPath = normalizedOutputDirectory + "/" + fontName + ".fontsettings";

        if (ValidateOutputAsset<Texture2D>(atlasPath, "texture atlas") == false ||
            ValidateOutputAsset<Material>(materialPath, "material") == false ||
            ValidateOutputAsset<Font>(fontPath, "font") == false)
        {
            DestroyFolderGlyphs(folderGlyphs);
            return;
        }

        try
        {
            EnsureAssetDirectory(normalizedOutputDirectory);
            WriteFolderAtlas(folderGlyphs, atlasPath);
            ConfigureTextureImporter(atlasPath);

            Texture2D atlasTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
            if (atlasTexture == null)
            {
                ShowError("The generated texture atlas could not be imported.");
                return;
            }

            Material material = CreateOrUpdateMaterial(materialPath, atlasTexture);
            Font font = CreateOrUpdateFolderFont(fontPath, material, folderGlyphs, atlasTexture);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = font;
            EditorGUIUtility.PingObject(font);
            folderMessage =
                "Generated " + folderGlyphs.Count + " characters from " + folderPath + ".";
            EditorUtility.DisplayDialog(
                "Font Generator",
                "Font generated successfully:\n" + fontPath +
                "\n\nTexture atlas:\n" + atlasPath,
                "OK");
        }
        finally
        {
            DestroyFolderGlyphs(folderGlyphs);
        }
    }

    private static List<FolderGlyph> LoadFolderGlyphs(string folderPath)
    {
        string projectDirectory = Directory.GetParent(Application.dataPath).FullName;
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        List<FolderGlyph> folderGlyphs = new List<FolderGlyph>();
        HashSet<char> mappedCharacters = new HashSet<char>();

        foreach (string textureGuid in textureGuids)
        {
            string texturePath = AssetDatabase.GUIDToAssetPath(textureGuid);
            string textureDirectory = NormalizeAssetPath(Path.GetDirectoryName(texturePath));
            if (textureDirectory != folderPath)
            {
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(texturePath);
            if (TryGetCharacterFromFileName(fileName, out char character) == false)
            {
                ShowError(
                    "Cannot determine the character represented by this filename:\n" +
                    texturePath +
                    "\n\nUse a single character filename, such as 0.png or A.png.");
                DestroyFolderGlyphs(folderGlyphs);
                return null;
            }

            if (mappedCharacters.Add(character) == false)
            {
                ShowError("Multiple images map to the same character: " + character);
                DestroyFolderGlyphs(folderGlyphs);
                return null;
            }

            string sourcePath = Path.Combine(projectDirectory, texturePath);
            Texture2D readableTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            try
            {
                byte[] imageData = File.ReadAllBytes(sourcePath);
                if (ImageConversion.LoadImage(readableTexture, imageData, false) == false)
                {
                    ShowError("The image format could not be read:\n" + texturePath);
                    DestroyImmediate(readableTexture);
                    DestroyFolderGlyphs(folderGlyphs);
                    return null;
                }
            }
            catch (IOException exception)
            {
                DestroyImmediate(readableTexture);
                DestroyFolderGlyphs(folderGlyphs);
                ShowError("Failed to read image:\n" + texturePath + "\n\n" + exception.Message);
                return null;
            }

            folderGlyphs.Add(new FolderGlyph(character, fileName, readableTexture));
        }

        folderGlyphs.Sort((left, right) =>
            string.CompareOrdinal(left.FileName, right.FileName));

        if (folderGlyphs.Count == 0)
        {
            ShowError("No supported image files were found directly inside the selected folder.");
            return null;
        }

        return folderGlyphs;
    }

    private static bool TryGetCharacterFromFileName(string fileName, out char character)
    {
        if (fileName.Length == 1)
        {
            character = fileName[0];
            return true;
        }

        switch (fileName)
        {
            case "句号":
                character = '.';
                return true;
            case "逗号":
                character = ',';
                return true;
            case "冒号":
                character = ':';
                return true;
            case "分号":
                character = ';';
                return true;
            case "加号":
                character = '+';
                return true;
            case "减号":
                character = '-';
                return true;
            case "空格":
                character = ' ';
                return true;
            default:
                character = '\0';
                return false;
        }
    }

    private void WriteFolderAtlas(List<FolderGlyph> folderGlyphs, string atlasPath)
    {
        int atlasWidth = folderCharacterSpacing * Mathf.Max(0, folderGlyphs.Count - 1);
        int atlasHeight = 1;

        foreach (FolderGlyph folderGlyph in folderGlyphs)
        {
            atlasWidth += folderGlyph.Texture.width;
            atlasHeight = Mathf.Max(atlasHeight, folderGlyph.Texture.height);
        }

        Texture2D atlasTexture = new Texture2D(
            atlasWidth,
            atlasHeight,
            TextureFormat.RGBA32,
            false);

        try
        {
            Color32[] clearPixels = new Color32[atlasWidth * atlasHeight];
            atlasTexture.SetPixels32(clearPixels);

            int currentX = 0;
            foreach (FolderGlyph folderGlyph in folderGlyphs)
            {
                folderGlyph.AtlasX = currentX;
                int bottomY = atlasHeight - folderGlyph.Texture.height;
                atlasTexture.SetPixels32(
                    currentX,
                    bottomY,
                    folderGlyph.Texture.width,
                    folderGlyph.Texture.height,
                    folderGlyph.Texture.GetPixels32());
                currentX += folderGlyph.Texture.width + folderCharacterSpacing;
            }

            atlasTexture.Apply(false, false);

            string projectDirectory = Directory.GetParent(Application.dataPath).FullName;
            string absoluteAtlasPath = Path.Combine(projectDirectory, atlasPath);
            File.WriteAllBytes(absoluteAtlasPath, atlasTexture.EncodeToPNG());
            AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);
        }
        finally
        {
            DestroyImmediate(atlasTexture);
        }
    }

    private Font CreateOrUpdateFolderFont(
        string fontPath,
        Material material,
        List<FolderGlyph> folderGlyphs,
        Texture2D atlasTexture)
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (font == null)
        {
            font = new Font
            {
                name = fontName
            };
            AssetDatabase.CreateAsset(font, fontPath);
        }

        int maximumHeight = 1;
        foreach (FolderGlyph folderGlyph in folderGlyphs)
        {
            maximumHeight = Mathf.Max(maximumHeight, folderGlyph.Texture.height);
        }

        SerializedObject serializedFont = new SerializedObject(font);
        SerializedProperty fontSize = serializedFont.FindProperty("m_FontSize");
        SerializedProperty serializedLineSpacing = serializedFont.FindProperty("m_LineSpacing");
        fontSize.floatValue = maximumHeight;
        serializedLineSpacing.floatValue = maximumHeight;
        serializedFont.ApplyModifiedPropertiesWithoutUndo();

        CharacterInfo[] characterInfo = new CharacterInfo[folderGlyphs.Count];
        for (int i = 0; i < folderGlyphs.Count; i++)
        {
            FolderGlyph folderGlyph = folderGlyphs[i];
            int glyphWidth = folderGlyph.Texture.width;
            int glyphHeight = folderGlyph.Texture.height;

            float left = (float)folderGlyph.AtlasX / atlasTexture.width;
            float right = (float)(folderGlyph.AtlasX + glyphWidth) / atlasTexture.width;
            float top = 1f;
            float bottom = 1f - (float)glyphHeight / atlasTexture.height;

            characterInfo[i] = new CharacterInfo
            {
                index = folderGlyph.Character,
                advance = glyphWidth + folderCharacterSpacing,
                minX = 0,
                maxX = glyphWidth,
                minY = 0,
                maxY = -glyphHeight,
                uvBottomLeft = new Vector2(left, top),
                uvBottomRight = new Vector2(right, top),
                uvTopLeft = new Vector2(left, bottom),
                uvTopRight = new Vector2(right, bottom)
            };
        }

        font.name = fontName;
        font.material = material;
        font.characterInfo = characterInfo;
        EditorUtility.SetDirty(font);
        return font;
    }

    private static void DestroyFolderGlyphs(List<FolderGlyph> folderGlyphs)
    {
        if (folderGlyphs == null)
        {
            return;
        }

        foreach (FolderGlyph folderGlyph in folderGlyphs)
        {
            if (folderGlyph.Texture != null)
            {
                DestroyImmediate(folderGlyph.Texture);
            }
        }
    }

    private void Generate()
    {
        if (ValidateSettings() == false)
        {
            return;
        }

        string texturePath = AssetDatabase.GetAssetPath(texture);
        string normalizedOutputDirectory = NormalizeAssetPath(outputDirectory);
        string materialPath = normalizedOutputDirectory + "/" + fontName + ".mat";
        string fontPath = normalizedOutputDirectory + "/" + fontName + ".fontsettings";

        if (ValidateOutputAsset<Material>(materialPath, "material") == false ||
            ValidateOutputAsset<Font>(fontPath, "font") == false)
        {
            return;
        }

        ConfigureTextureImporter(texturePath);
        EnsureAssetDirectory(normalizedOutputDirectory);

        Material material = CreateOrUpdateMaterial(materialPath, texture);
        Font font = CreateOrUpdateFont(fontPath, material);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = font;
        EditorGUIUtility.PingObject(font);
        EditorUtility.DisplayDialog(
            "Font Generator",
            "Font generated successfully:\n" + fontPath +
            "\n\nAssign it to the Font field of a UGUI Text component.",
            "OK");
    }

    private bool ValidateSettings()
    {
        if (texture == null)
        {
            ShowError("Please select a texture.");
            return false;
        }

        string texturePath = AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(texturePath) || texturePath.StartsWith("Assets/") == false)
        {
            ShowError("The selected texture must be an asset inside this project.");
            return false;
        }

        if (string.IsNullOrEmpty(characters))
        {
            ShowError("Characters cannot be empty.");
            return false;
        }

        HashSet<char> uniqueCharacters = new HashSet<char>();
        foreach (char character in characters)
        {
            if (uniqueCharacters.Add(character) == false)
            {
                ShowError("Characters cannot contain duplicates: " + character);
                return false;
            }
        }

        if (glyphWidth <= 0 || glyphHeight <= 0)
        {
            ShowError("Glyph Width and Glyph Height must be greater than 0.");
            return false;
        }

        if (startX < 0 || startY < 0 || horizontalSpacing < 0 || verticalSpacing < 0)
        {
            ShowError("Start coordinates and spacing cannot be negative.");
            return false;
        }

        if (columns <= 0 || advance <= 0 || lineHeight <= 0)
        {
            ShowError("Columns, Advance and Line Height must be greater than 0.");
            return false;
        }

        if (ValidateOutputSettings() == false)
        {
            return false;
        }

        for (int i = 0; i < characters.Length; i++)
        {
            int column = i % columns;
            int row = i / columns;
            int x = startX + column * (glyphWidth + horizontalSpacing);
            int y = startY + row * (glyphHeight + verticalSpacing);

            if (x + glyphWidth > texture.width || y + glyphHeight > texture.height)
            {
                ShowError(
                    "The glyph area for character '" + characters[i] +
                    "' exceeds the texture bounds.");
                return false;
            }
        }

        return true;
    }

    private bool ValidateOutputSettings()
    {
        if (string.IsNullOrWhiteSpace(fontName) ||
            fontName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            ShowError("Font Name is empty or contains invalid filename characters.");
            return false;
        }

        string normalizedOutputDirectory = NormalizeAssetPath(outputDirectory);
        if (normalizedOutputDirectory != "Assets" &&
            normalizedOutputDirectory.StartsWith("Assets/") == false)
        {
            ShowError("Output Directory must be inside Assets.");
            return false;
        }

        return true;
    }

    private static bool ValidateOutputAsset<T>(string path, string assetName) where T : Object
    {
        Object existingAsset = AssetDatabase.LoadMainAssetAtPath(path);
        if (existingAsset != null && existingAsset is T == false)
        {
            ShowError(
                "Cannot generate the " + assetName + " because another asset already exists:\n" +
                path);
            return false;
        }

        return true;
    }

    private static void ConfigureTextureImporter(string texturePath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool needsReimport =
            importer.mipmapEnabled ||
            importer.alphaIsTransparency == false ||
            importer.textureCompression != TextureImporterCompression.Uncompressed ||
            importer.wrapMode != TextureWrapMode.Clamp ||
            importer.filterMode != FilterMode.Bilinear ||
            importer.npotScale != TextureImporterNPOTScale.None;

        if (needsReimport == false)
        {
            return;
        }

        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();
    }

    private static void EnsureAssetDirectory(string assetDirectory)
    {
        if (AssetDatabase.IsValidFolder(assetDirectory))
        {
            return;
        }

        string[] directoryNames = assetDirectory.Split('/');
        string currentDirectory = directoryNames[0];

        for (int i = 1; i < directoryNames.Length; i++)
        {
            string nextDirectory = currentDirectory + "/" + directoryNames[i];
            if (AssetDatabase.IsValidFolder(nextDirectory) == false)
            {
                AssetDatabase.CreateFolder(currentDirectory, directoryNames[i]);
            }

            currentDirectory = nextDirectory;
        }
    }

    private Material CreateOrUpdateMaterial(string materialPath, Texture2D sourceTexture)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("UI/Default");
            if (shader == null)
            {
                shader = Shader.Find("GUI/Text Shader");
            }

            material = new Material(shader)
            {
                name = fontName
            };
            AssetDatabase.CreateAsset(material, materialPath);
        }

        material.mainTexture = sourceTexture;
        EditorUtility.SetDirty(material);
        return material;
    }

    private Font CreateOrUpdateFont(string fontPath, Material material)
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (font == null)
        {
            font = new Font
            {
                name = fontName
            };
            AssetDatabase.CreateAsset(font, fontPath);
        }

        SerializedObject serializedFont = new SerializedObject(font);
        SerializedProperty fontSize = serializedFont.FindProperty("m_FontSize");
        SerializedProperty serializedLineSpacing = serializedFont.FindProperty("m_LineSpacing");
        fontSize.floatValue = glyphHeight;
        serializedLineSpacing.floatValue = lineHeight;
        serializedFont.ApplyModifiedPropertiesWithoutUndo();

        CharacterInfo[] characterInfo = new CharacterInfo[characters.Length];
        for (int i = 0; i < characters.Length; i++)
        {
            int column = i % columns;
            int row = i / columns;
            int x = startX + column * (glyphWidth + horizontalSpacing);
            int y = startY + row * (glyphHeight + verticalSpacing);

            float left = (float)x / texture.width;
            float right = (float)(x + glyphWidth) / texture.width;
            float top = 1f - (float)y / texture.height;
            float bottom = 1f - (float)(y + glyphHeight) / texture.height;

            characterInfo[i] = new CharacterInfo
            {
                index = characters[i],
                advance = advance,
                minX = 0,
                maxX = glyphWidth,
                minY = 0,
                maxY = -glyphHeight,
                uvBottomLeft = new Vector2(left, top),
                uvBottomRight = new Vector2(right, top),
                uvTopLeft = new Vector2(left, bottom),
                uvTopRight = new Vector2(right, bottom)
            };
        }

        font.name = fontName;
        font.material = material;
        font.characterInfo = characterInfo;
        EditorUtility.SetDirty(font);
        return font;
    }

    private static string NormalizeAssetPath(string path)
    {
        return path.Trim().Replace('\\', '/').TrimEnd('/');
    }

    private static void ShowError(string message)
    {
        EditorUtility.DisplayDialog("Font Generator", message, "OK");
    }

    private class DetectedGlyph
    {
        public RectInt Bounds { get; }
        public int PixelCount { get; }
        public float CenterX => Bounds.xMin + Bounds.width * 0.5f;
        public float CenterY => Bounds.yMin + Bounds.height * 0.5f;

        public DetectedGlyph(RectInt bounds, int pixelCount)
        {
            Bounds = bounds;
            PixelCount = pixelCount;
        }
    }

    private class FolderGlyph
    {
        public char Character { get; }
        public string FileName { get; }
        public Texture2D Texture { get; }
        public int AtlasX { get; set; }

        public FolderGlyph(char character, string fileName, Texture2D texture)
        {
            Character = character;
            FileName = fileName;
            Texture = texture;
        }
    }
}
#endif