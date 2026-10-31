using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Silah slot sistemini assetlere ve açık sahneye kuran tek tıklık araç.
/// Menü: Tools > Elemental Fissure > Silah Slot Kurulumu
/// Tekrar çalıştırmak güvenlidir: zaten kurulu olanları atlar, sadece referansları tazeler.
/// </summary>
public static class WeaponSlotSetupTool
{
    private const string DialogTitle = "Silah Slot Kurulumu";
    private const string DatabasePath = "Assets/SO/AllSpells.asset";
    private const string SpellsFolder = "Assets/Prefabs/Spells";
    private const string TestSpellPath = SpellsFolder + "/TestSpell.prefab";

    [MenuItem("Tools/Elemental Fissure/Silah Slot Kurulumu")]
    private static void Run()
    {
        AllSpellsSO database = AssetDatabase.LoadAssetAtPath<AllSpellsSO>(DatabasePath);
        if (database == null)
        {
            EditorUtility.DisplayDialog(DialogTitle, $"{DatabasePath} bulunamadı.", "Tamam");
            return;
        }

        List<string> log = new List<string>();
        SetupDatabase(database, log);
        SetupScene(database, log);

        string summary = log.Count > 0 ? string.Join("\n", log) : "Her şey zaten kurulu.";
        Debug.Log($"[{DialogTitle}]\n{summary}");
        EditorUtility.DisplayDialog(DialogTitle, summary + "\n\nSahneyi kaydetmeyi unutma (Ctrl+S).", "Tamam");
    }

    // ------------------------------------------------------------------ Database

    private static void SetupDatabase(AllSpellsSO database, List<string> log)
    {
        Undo.RecordObject(database, DialogTitle);

        foreach (ElementType element in ElementUtility.AllElements)
        {
            if (database.GetElementEntry(element) != null) continue;

            string path = $"{SpellsFolder}/{element}Spell.prefab";
            Spell spell = LoadSpell(path);

            database.elementSpells.Add(new AllSpellsSO.ElementSpellEntry
            {
                element = element,
                spellPrefab = spell,
                pureBonus = DefaultPureBonus(element)
            });

            log.Add(spell != null
                ? $"• {element} büyüsü AllSpells'e eklendi"
                : $"• UYARI: {path} bulunamadı, {element} büyüsünü elle ata");
        }

        if (database.fallbackSpell == null)
        {
            Spell testSpell = LoadSpell(TestSpellPath);
            if (testSpell == null)
                testSpell = CreateTestSpellPrefab(database, log);

            database.fallbackSpell = testSpell;
            if (testSpell != null)
                log.Add("• TestSpell yedek büyü olarak atandı");
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// PURE bonusları için başlangıç değerleri. AllSpells asset'inden istediğin gibi değiştirebilirsin.
    /// </summary>
    private static BonusStats DefaultPureBonus(ElementType element)
    {
        BonusStats bonus = new BonusStats();
        switch (element)
        {
            case ElementType.Fire: bonus.bonusDamage = 0.2f; break;
            case ElementType.Water: bonus.bonusDuration = 0.2f; break;
            case ElementType.Air: bonus.bonusCooldown = 0.1f; break;
            case ElementType.Earth: bonus.bonusArea = 0.2f; break;
        }
        return bonus;
    }

    private static Spell LoadSpell(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return null;

        Spell spell = prefab.GetComponent<Spell>();
        return spell != null ? spell : null;
    }

    private static Spell CreateTestSpellPrefab(AllSpellsSO database, List<string> log)
    {
        // Layer ve düşman layer'ı mevcut ateş büyüsünden kopyalanır
        AllSpellsSO.ElementSpellEntry fireEntry = database.GetElementEntry(ElementType.Fire);
        Spell reference = fireEntry != null ? fireEntry.spellPrefab : null;

        GameObject go = new GameObject("TestSpell");
        try
        {
            if (reference != null)
                go.layer = reference.gameObject.layer;

            SpriteRenderer sprite = go.AddComponent<SpriteRenderer>();
            sprite.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sprite.sortingOrder = 5;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            SpellStats stats = go.AddComponent<SpellStats>();
            SerializedObject statsSO = new SerializedObject(stats);
            statsSO.FindProperty("baseDamage").floatValue = 8f;
            statsSO.FindProperty("baseCooldown").floatValue = 1.2f;
            statsSO.FindProperty("baseProjectileSpeed").floatValue = 6f;
            statsSO.FindProperty("baseDuration").floatValue = 2.5f;
            statsSO.FindProperty("baseArea").floatValue = 5f;
            statsSO.FindProperty("baseProjectileAmount").intValue = 1;
            statsSO.FindProperty("baseKnockback").intValue = 1;
            statsSO.ApplyModifiedPropertiesWithoutUndo();

            TestSpell spell = go.AddComponent<TestSpell>();
            SerializedObject spellSO = new SerializedObject(spell);
            spellSO.FindProperty("spellStats").objectReferenceValue = stats;
            spellSO.FindProperty("sprite").objectReferenceValue = sprite;
            spellSO.FindProperty("rb").objectReferenceValue = rb;
            if (reference != null)
            {
                int enemyMask = new SerializedObject(reference).FindProperty("enemyLayer").intValue;
                spellSO.FindProperty("enemyLayer").intValue = enemyMask;
            }
            spellSO.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, TestSpellPath);
            if (prefab == null)
            {
                log.Add($"• UYARI: {TestSpellPath} oluşturulamadı");
                return null;
            }

            log.Add($"• {TestSpellPath} oluşturuldu");
            return prefab.GetComponent<Spell>();
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    // ------------------------------------------------------------------ Scene

    private static void SetupScene(AllSpellsSO database, List<string> log)
    {
        PlayerController player = Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (player == null)
        {
            log.Add("• UYARI: Açık sahnede PlayerController yok, sahne kurulumu atlandı");
            return;
        }

        WeaponSlotManager manager = player.GetComponent<WeaponSlotManager>();
        if (manager == null)
        {
            manager = Undo.AddComponent<WeaponSlotManager>(player.gameObject);
            log.Add("• Player'a WeaponSlotManager eklendi");
        }
        SetReference(manager, "spellDatabase", database);

        SlotSelectionMenu menu = Object.FindAnyObjectByType<SlotSelectionMenu>(FindObjectsInactive.Include);
        if (menu == null)
        {
            menu = BuildMenu();
            log.Add("• Slot seçim menüsü (SlotMenuCanvas) oluşturuldu");
        }
        SetReference(menu, "slotManager", manager);
        SetReference(player, "slotMenu", menu);

        SlotDebugTester tester = player.GetComponent<SlotDebugTester>();
        if (tester == null)
        {
            tester = Undo.AddComponent<SlotDebugTester>(player.gameObject);
            log.Add("• Player'a SlotDebugTester eklendi (1-5 tuşları, Shift = PURE)");
        }
        SetReference(tester, "slotMenu", menu);

        if (Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include) == null)
            log.Add("• UYARI: Sahnede EventSystem yok, menü butonları tıklanmaz");

        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
    }

    private static void SetReference(Object target, string propertyName, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning($"[{DialogTitle}] {target.GetType().Name}.{propertyName} bulunamadı.");
            return;
        }

        property.objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }

    // ------------------------------------------------------------------ Menu UI

    private static SlotSelectionMenu BuildMenu()
    {
        int uiLayer = LayerMask.NameToLayer("UI");

        GameObject canvasObject = new GameObject("SlotMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        if (uiLayer >= 0) canvasObject.layer = uiLayer;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Arka planı karartan panel (menü kapalıyken gizli)
        RectTransform panel = CreateRect("Panel", canvasObject.transform, Vector2.zero, Vector2.zero);
        Stretch(panel, 0f);
        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);

        CreateText("Title", panel, new Vector2(0f, 440f), new Vector2(1400f, 90f), "Elementi bir slota yerleştir", 52f);

        // Ortada gelen element
        RectTransform center = CreateRect("CenterElement", panel, Vector2.zero, new Vector2(200f, 200f));
        Image centerImage = center.gameObject.AddComponent<Image>();
        centerImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        centerImage.raycastTarget = false;
        TextMeshProUGUI centerLabel = CreateText("Label", center, Vector2.zero, Vector2.zero, "Element", 40f);
        Stretch(centerLabel.rectTransform, 0f);
        centerLabel.color = Color.black;

        // + şeklinde 4 slot: 0 Üst, 1 Sağ, 2 Alt, 3 Sol
        Vector2[] positions =
        {
            new Vector2(0f, 250f),
            new Vector2(410f, 0f),
            new Vector2(0f, -250f),
            new Vector2(-410f, 0f)
        };
        string[] names = { "Slot1_Up", "Slot2_Right", "Slot3_Down", "Slot4_Left" };

        SlotButtonUI[] buttons = new SlotButtonUI[WeaponSlotManager.SlotCount];
        for (int i = 0; i < buttons.Length; i++)
            buttons[i] = CreateSlotButton(names[i], panel, positions[i]);

        SlotSelectionMenu menu = canvasObject.AddComponent<SlotSelectionMenu>();
        SerializedObject so = new SerializedObject(menu);
        so.FindProperty("panel").objectReferenceValue = panel.gameObject;
        so.FindProperty("centerImage").objectReferenceValue = centerImage;
        so.FindProperty("centerLabel").objectReferenceValue = centerLabel;
        SerializedProperty buttonArray = so.FindProperty("slotButtons");
        buttonArray.arraySize = buttons.Length;
        for (int i = 0; i < buttons.Length; i++)
            buttonArray.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        panel.gameObject.SetActive(false);
        Undo.RegisterCreatedObjectUndo(canvasObject, DialogTitle);
        return menu;
    }

    private static SlotButtonUI CreateSlotButton(string name, RectTransform parent, Vector2 position)
    {
        RectTransform rect = CreateRect(name, parent, position, new Vector2(340f, 180f));

        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI label = CreateText("Label", rect, Vector2.zero, Vector2.zero, "", 26f);
        Stretch(label.rectTransform, 12f);

        SlotButtonUI slotButton = rect.gameObject.AddComponent<SlotButtonUI>();
        SerializedObject so = new SerializedObject(slotButton);
        so.FindProperty("button").objectReferenceValue = button;
        so.FindProperty("background").objectReferenceValue = image;
        so.FindProperty("label").objectReferenceValue = label;
        so.ApplyModifiedPropertiesWithoutUndo();

        return slotButton;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;

        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 position, Vector2 size, string text, float fontSize)
    {
        RectTransform rect = CreateRect(name, parent, position, size);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void Stretch(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
    }
}
