using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds the whole shop test scene from nothing: sample products, customer types, prefabs,
// the scene itself and the navmesh. Tools > Shop > Build Test Scene.
// Safe to run again, it only touches the generated stuff under Assets/Osama
public static class ShopTestSceneBuilder
{
    private const string Root = "Assets/Osama";
    private const string DataDir = Root + "/Data";
    private const string ProductsDir = DataDir + "/Products";
    private const string CustomersDir = DataDir + "/Customers";
    private const string PrefabDir = Root + "/Prefabs";
    private const string ProductPrefabDir = PrefabDir + "/Products";
    private const string MaterialDir = Root + "/Materials";
    private const string SceneDir = Root + "/Scenes";
    private const string ScenePath = SceneDir + "/ShopTest.unity";
    private const string NavMeshDir = SceneDir + "/ShopTest";
    private const string PlayerActionsPath = "Assets/Ali/playerMapController/Player.inputactions";
    private const string AliScenePath = "Assets/Scenes/AliS.unity";
    private const string AliPlayerName = "Player";

    private class Materials
    {
        public Material grass, floor, wall, wood, darkWood, metal, body, mask, lamp, window, leaves, rock, bed;
        public Dictionary<string, Material> products = new Dictionary<string, Material>();
    }

    private struct ProductDef
    {
        public string name;
        public ProductCategory category;
        public ProductRarity rarity;
        public int basePrice;
        public Color color;
        public PrimitiveType shape;
        public Vector3 size;
        public float yaw;

        public ProductDef(string name, ProductCategory category, ProductRarity rarity, int basePrice,
            Color color, PrimitiveType shape, Vector3 size, float yaw = 0f)
        {
            this.name = name;
            this.category = category;
            this.rarity = rarity;
            this.basePrice = basePrice;
            this.color = color;
            this.shape = shape;
            this.size = size;
            this.yaw = yaw;
        }
    }

    // sample products, a few of them are the quest items from the GDD
    private static readonly ProductDef[] ProductDefs =
    {
        new ProductDef("Old Jar", ProductCategory.Junk, ProductRarity.Common, 10, new Color(0.72f, 0.48f, 0.30f), PrimitiveType.Cylinder, new Vector3(0.22f, 0.30f, 0.22f)),
        new ProductDef("Rusty Can", ProductCategory.Junk, ProductRarity.Common, 5, new Color(0.45f, 0.38f, 0.32f), PrimitiveType.Cylinder, new Vector3(0.14f, 0.20f, 0.14f)),
        new ProductDef("Bandages", ProductCategory.Medicine, ProductRarity.Uncommon, 20, new Color(0.92f, 0.92f, 0.88f), PrimitiveType.Cube, new Vector3(0.25f, 0.12f, 0.18f)),
        new ProductDef("Snake Venom", ProductCategory.Medicine, ProductRarity.Rare, 30, new Color(0.25f, 0.75f, 0.30f), PrimitiveType.Sphere, new Vector3(0.18f, 0.18f, 0.18f)),
        new ProductDef("Pistol Ammo", ProductCategory.Ammo, ProductRarity.Common, 12, new Color(0.35f, 0.40f, 0.25f), PrimitiveType.Cube, new Vector3(0.20f, 0.14f, 0.14f)),
        new ProductDef("Amethyst", ProductCategory.Material, ProductRarity.Rare, 40, new Color(0.60f, 0.30f, 0.85f), PrimitiveType.Cube, new Vector3(0.16f, 0.24f, 0.16f), 45f),
        new ProductDef("King's Mirror", ProductCategory.Relic, ProductRarity.Legendary, 100, new Color(0.95f, 0.80f, 0.35f), PrimitiveType.Cube, new Vector3(0.30f, 0.40f, 0.04f)),
    };

    [MenuItem("Tools/Shop/Build Test Scene")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Stop play mode first");
            return;
        }
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolder(Root);
        EnsureFolder(DataDir);
        EnsureFolder(ProductsDir);
        EnsureFolder(CustomersDir);
        EnsureFolder(PrefabDir);
        EnsureFolder(ProductPrefabDir);
        EnsureFolder(MaterialDir);
        EnsureFolder(SceneDir);
        EnsureFolder(NavMeshDir);

        Materials mats = BuildMaterials();
        List<ProductSO> products = BuildProducts(mats);
        ShopSettingsSO settings = BuildSettings();
        DaySettingsSO daySettings = BuildDaySettings();
        List<CustomerTypeSO> types = BuildCustomerTypes(products);

        GameObject shelfPrefab = BuildShelfPrefab(mats);
        GameObject customerPrefab = BuildCustomerPrefab(mats);
        GameObject checkoutPrefab = BuildCheckoutPrefab(mats);
        GameObject playerPrefab = BuildPlayerPrefab();
        GameObject hudPrefab = BuildDayHudPrefab();

        BuildScene(mats, products, settings, daySettings, types, shelfPrefab, customerPrefab, checkoutPrefab, playerPrefab, hudPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Shop test scene built at " + ScenePath);
    }

    // Unity.exe -batchmode -quit -projectPath <project> -executeMethod ShopTestSceneBuilder.BuildBatch
    public static void BuildBatch()
    {
        Build();
    }

    // TMP needs its essential resources (default font etc) before any label renders.
    // Same thing as Window > TextMeshPro > Import TMP Essential Resources
    public static void ImportTmpEssentials()
    {
        if (AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
        {
            Debug.Log("TMP essentials already imported");
            return;
        }

        string package = null;
        string cache = Path.Combine(Directory.GetCurrentDirectory(), "Library/PackageCache");
        if (Directory.Exists(cache))
        {
            foreach (string dir in Directory.GetDirectories(cache, "com.unity.ugui*"))
            {
                string candidate = Path.Combine(dir, "Package Resources/TMP Essential Resources.unitypackage");
                if (File.Exists(candidate))
                {
                    package = candidate;
                    break;
                }
            }
        }

        if (package == null)
        {
            Debug.LogError("Could not find TMP Essential Resources.unitypackage in Library/PackageCache");
            return;
        }

        AssetDatabase.ImportPackage(package, false);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Imported TMP essentials from " + package);
    }

    // ---------------------------------------------------------------- materials

    private static Materials BuildMaterials()
    {
        Materials m = new Materials
        {
            grass = MakeMaterial("Grass", new Color(0.36f, 0.48f, 0.28f)),
            floor = MakeMaterial("ShopFloor", new Color(0.42f, 0.32f, 0.22f)),
            wall = MakeMaterial("ShopWall", new Color(0.60f, 0.56f, 0.50f)),
            wood = MakeMaterial("ShelfWood", new Color(0.55f, 0.40f, 0.25f)),
            darkWood = MakeMaterial("CounterWood", new Color(0.30f, 0.20f, 0.12f)),
            metal = MakeMaterial("Metal", new Color(0.25f, 0.25f, 0.28f), 0.7f),
            body = MakeMaterial("CustomerBody", new Color(0.45f, 0.45f, 0.50f)),
            mask = MakeMaterial("CustomerMask", new Color(0.90f, 0.90f, 0.85f)),
            lamp = MakeMaterial("Lamp", new Color(1f, 0.95f, 0.8f), 0.5f, new Color(1f, 0.85f, 0.55f) * 2f),
            window = MakeMaterial("Window", new Color(0.12f, 0.16f, 0.22f), 0.9f),
            leaves = MakeMaterial("Leaves", new Color(0.18f, 0.35f, 0.16f)),
            rock = MakeMaterial("Rock", new Color(0.36f, 0.36f, 0.37f), 0.2f),
            bed = MakeMaterial("Bed", new Color(0.45f, 0.12f, 0.12f)),
        };

        foreach (ProductDef def in ProductDefs)
            m.products[def.name] = MakeMaterial("Product_" + SafeName(def.name), def.color, def.rarity >= ProductRarity.Rare ? 0.7f : 0.3f);

        return m;
    }

    private static Material MakeMaterial(string name, Color color, float smoothness = 0.3f, Color? emission = null)
    {
        string path = MaterialDir + "/" + name + ".mat";
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }

        mat.color = color;
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (emission.HasValue && mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.SetColor("_EmissionColor", emission.Value);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // ---------------------------------------------------------------- data

    private static List<ProductSO> BuildProducts(Materials mats)
    {
        List<ProductSO> list = new List<ProductSO>();

        foreach (ProductDef def in ProductDefs)
        {
            string safe = SafeName(def.name);

            // display prefab: empty root + model as a child so the origin sits at the bottom of the model
            GameObject root = new GameObject(safe);
            GameObject model = GameObject.CreatePrimitive(def.shape);
            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = new Vector3(0f, def.size.y * 0.5f, 0f);
            model.transform.localRotation = Quaternion.Euler(0f, def.yaw, 0f);
            model.transform.localScale = ScaleFor(def.shape, def.size);
            StripCollider(model);
            model.GetComponent<Renderer>().sharedMaterial = mats.products[def.name];
            GameObject prefab = SavePrefab(root, ProductPrefabDir + "/" + safe + ".prefab");

            ProductSO so = LoadOrCreate<ProductSO>(ProductsDir + "/" + safe + ".asset");
            so.productName = def.name;
            so.category = def.category;
            so.rarity = def.rarity;
            so.basePrice = def.basePrice;
            so.displayPrefab = prefab;
            EditorUtility.SetDirty(so);
            list.Add(so);
        }

        return list;
    }

    private static Vector3 ScaleFor(PrimitiveType shape, Vector3 size)
    {
        // cylinder and capsule meshes are 2 units tall, the others are 1
        if (shape == PrimitiveType.Cylinder || shape == PrimitiveType.Capsule)
            return new Vector3(size.x, size.y * 0.5f, size.z);
        return size;
    }

    private static ShopSettingsSO BuildSettings()
    {
        ShopSettingsSO s = LoadOrCreate<ShopSettingsSO>(DataDir + "/ShopSettings.asset");
        s.currencySymbol = "$";
        s.startingMoney = 0;
        s.rarityMultipliers = new[] { 1f, 1.5f, 2.5f, 5f };
        EditorUtility.SetDirty(s);
        return s;
    }

    private static DaySettingsSO BuildDaySettings()
    {
        DaySettingsSO s = LoadOrCreate<DaySettingsSO>(DataDir + "/DaySettings.asset");
        s.realSecondsPerHour = 30f; // quick days for testing, a full day is 12 real minutes
        s.startDay = 1;
        s.startHour = 8f;
        s.wakeUpHour = 8f;
        s.morningStart = 6f;
        s.dayStart = 10f;
        s.eveningStart = 17f;
        s.nightStart = 20f;
        EditorUtility.SetDirty(s);
        return s;
    }

    private static List<CustomerTypeSO> BuildCustomerTypes(List<ProductSO> products)
    {
        List<CustomerTypeSO> list = new List<CustomerTypeSO>();

        CustomerTypeSO villager = LoadOrCreate<CustomerTypeSO>(CustomersDir + "/Villager.asset");
        villager.displayName = "Villager";
        villager.isSpecial = false;
        villager.spawnWeight = 3f;
        villager.moveSpeed = 2.5f;
        villager.minItems = 1;
        villager.maxItems = 2;
        villager.pickDelay = 1f;
        villager.patience = 60f;
        villager.wantedProducts = new ProductSO[0];
        villager.bodyColor = new Color(0.45f, 0.45f, 0.50f);
        villager.maskColor = new Color(0.90f, 0.90f, 0.85f);
        EditorUtility.SetDirty(villager);
        list.Add(villager);

        CustomerTypeSO stranger = LoadOrCreate<CustomerTypeSO>(CustomersDir + "/MaskedStranger.asset");
        stranger.displayName = "Masked Stranger";
        stranger.isSpecial = false;
        stranger.spawnWeight = 1.5f;
        stranger.moveSpeed = 3.2f;
        stranger.minItems = 1;
        stranger.maxItems = 3;
        stranger.pickDelay = 0.6f;
        stranger.patience = 40f;
        stranger.wantedProducts = new ProductSO[0];
        stranger.bodyColor = new Color(0.15f, 0.15f, 0.18f);
        stranger.maskColor = new Color(0.60f, 0.10f, 0.10f);
        EditorUtility.SetDirty(stranger);
        list.Add(stranger);

        // the old man from the GDD, only comes for the mirror
        CustomerTypeSO oldMan = LoadOrCreate<CustomerTypeSO>(CustomersDir + "/OldMan.asset");
        ProductSO mirror = products.Find(p => p.productName == "King's Mirror");
        oldMan.displayName = "Old Man";
        oldMan.isSpecial = true;
        oldMan.spawnWeight = 0.7f;
        oldMan.moveSpeed = 1.6f;
        oldMan.minItems = 1;
        oldMan.maxItems = 1;
        oldMan.pickDelay = 2f;
        oldMan.patience = 120f;
        oldMan.wantedProducts = mirror != null ? new[] { mirror } : new ProductSO[0];
        oldMan.bodyColor = new Color(0.35f, 0.30f, 0.25f);
        oldMan.maskColor = new Color(0.80f, 0.70f, 0.50f);
        EditorUtility.SetDirty(oldMan);
        list.Add(oldMan);

        return list;
    }

    // ---------------------------------------------------------------- prefabs

    private static GameObject BuildShelfPrefab(Materials mats)
    {
        GameObject shelf = new GameObject("Shelf");
        Shelf shelfComp = shelf.AddComponent<Shelf>();

        GameObject frame = new GameObject("Frame");
        frame.transform.SetParent(shelf.transform, false);
        Box(frame, "Back", new Vector3(0f, 0.9f, 0.2f), new Vector3(1.6f, 1.8f, 0.05f), mats.wood);
        Box(frame, "SideL", new Vector3(-0.8f, 0.9f, 0f), new Vector3(0.05f, 1.8f, 0.45f), mats.wood);
        Box(frame, "SideR", new Vector3(0.8f, 0.9f, 0f), new Vector3(0.05f, 1.8f, 0.45f), mats.wood);
        Box(frame, "Base", new Vector3(0f, 0.02f, 0f), new Vector3(1.6f, 0.04f, 0.45f), mats.wood);
        Box(frame, "Board1", new Vector3(0f, 0.60f, 0f), new Vector3(1.6f, 0.04f, 0.45f), mats.wood);
        Box(frame, "Board2", new Vector3(0f, 1.20f, 0f), new Vector3(1.6f, 0.04f, 0.45f), mats.wood);

        GameObject slots = new GameObject("Slots");
        slots.transform.SetParent(shelf.transform, false);

        float[] rows = { 0.62f, 1.22f };
        float[] cols = { -0.5f, 0f, 0.5f };
        int index = 0;
        foreach (float y in rows)
        {
            foreach (float x in cols)
            {
                GameObject slotGo = new GameObject("Slot_" + index++);
                slotGo.transform.SetParent(slots.transform, false);
                slotGo.transform.localPosition = new Vector3(x, y, 0f);

                // trigger so it never blocks anyone, the raycast still hits it
                BoxCollider trigger = slotGo.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = new Vector3(0f, 0.25f, 0f);
                trigger.size = new Vector3(0.45f, 0.5f, 0.4f);

                GameObject anchor = new GameObject("Anchor");
                anchor.transform.SetParent(slotGo.transform, false);

                // price tag hanging under the front edge of the board
                TextMeshPro label = MakeWorldText(slotGo.transform, "PriceTag", new Vector3(0f, -0.075f, -0.235f), new Vector2(0.45f, 0.12f), 1.4f);

                ShelfSlot slot = slotGo.AddComponent<ShelfSlot>();
                SetRef(slot, "anchor", anchor.transform);
                SetRef(slot, "priceLabel", label);
            }
        }

        GameObject stand = new GameObject("CustomerStandPoint");
        stand.transform.SetParent(shelf.transform, false);
        stand.transform.localPosition = new Vector3(0f, 0f, -0.9f);
        SetRef(shelfComp, "customerStandPoint", stand.transform);

        return SavePrefab(shelf, PrefabDir + "/Shelf.prefab");
    }

    private static GameObject BuildCustomerPrefab(Materials mats)
    {
        GameObject go = new GameObject("Customer");

        NavMeshAgent agent = go.AddComponent<NavMeshAgent>();
        agent.radius = 0.35f;
        agent.height = 1.8f;
        agent.speed = 2.5f;
        agent.angularSpeed = 360f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.2f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;

        CapsuleCollider col = go.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 0.9f, 0f);
        col.height = 1.8f;
        col.radius = 0.35f;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform, false);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(visual.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        body.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
        StripCollider(body);
        body.GetComponent<Renderer>().sharedMaterial = mats.body;

        GameObject mask = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mask.name = "Mask";
        mask.transform.SetParent(visual.transform, false);
        mask.transform.localPosition = new Vector3(0f, 1.5f, 0.38f);
        mask.transform.localScale = new Vector3(0.36f, 0.42f, 0.06f);
        StripCollider(mask);
        mask.GetComponent<Renderer>().sharedMaterial = mats.mask;

        GameObject hand = new GameObject("HandPoint");
        hand.transform.SetParent(go.transform, false);
        hand.transform.localPosition = new Vector3(0.38f, 1.0f, 0.35f);

        go.AddComponent<NavMeshCustomerMover>();
        Customer customer = go.AddComponent<Customer>();
        SetRef(customer, "handPoint", hand.transform);
        SetRef(customer, "visualRoot", visual.transform);
        SetRef(customer, "bodyRenderer", body.GetComponent<Renderer>());
        SetRef(customer, "maskRenderer", mask.GetComponent<Renderer>());

        return SavePrefab(go, PrefabDir + "/Customer.prefab");
    }

    private static GameObject BuildCheckoutPrefab(Materials mats)
    {
        GameObject go = new GameObject("CheckoutCounter");
        Box(go, "Counter", new Vector3(0f, 0.5f, 0f), new Vector3(2f, 1f, 0.8f), mats.darkWood);
        Box(go, "Register", new Vector3(0.6f, 1.15f, 0f), new Vector3(0.4f, 0.3f, 0.3f), mats.metal);

        GameObject points = new GameObject("QueuePoints");
        points.transform.SetParent(go.transform, false);

        Transform[] queue = new Transform[3];
        for (int i = 0; i < queue.Length; i++)
        {
            GameObject p = new GameObject("Queue_" + i);
            p.transform.SetParent(points.transform, false);
            p.transform.localPosition = new Vector3(0f, 0f, -1.0f - 1.2f * i);
            queue[i] = p.transform;
        }

        CheckoutCounter counter = go.AddComponent<CheckoutCounter>();
        SetRefArray(counter, "queuePoints", queue);

        return SavePrefab(go, PrefabDir + "/CheckoutCounter.prefab");
    }

    // the top-left clock. Built from Unity's built-in UI sprites, the real art just replaces the sprites on the Images
    private static GameObject BuildDayHudPrefab()
    {
        Sprite background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Sprite plain = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        GameObject root = new GameObject("DayTimeHUD", typeof(RectTransform), typeof(Image));
        Anchor(root.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.zero, new Vector2(340f, 110f));
        Image panel = root.GetComponent<Image>();
        panel.sprite = background;
        panel.type = Image.Type.Sliced;
        panel.color = new Color(0.10f, 0.08f, 0.07f, 0.85f);
        panel.raycastTarget = false;

        // sky window on the left. Masked, so the sun / moon disappear under the horizon
        GameObject skyGo = new GameObject("Sky", typeof(RectTransform), typeof(Image), typeof(Mask));
        skyGo.transform.SetParent(root.transform, false);
        Anchor(skyGo.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(88f, 88f));
        Image skyImage = skyGo.GetComponent<Image>();
        skyImage.sprite = knob;
        skyImage.color = new Color(0.45f, 0.70f, 0.95f);
        skyImage.raycastTarget = false;
        skyGo.GetComponent<Mask>().showMaskGraphic = true;

        GameObject horizon = MakeIcon(skyGo.transform, "Horizon", plain, new Color(0f, 0f, 0f, 0.35f), 88f);
        horizon.GetComponent<RectTransform>().sizeDelta = new Vector2(88f, 2f);
        horizon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -18f);
        GameObject sun = MakeIcon(skyGo.transform, "Sun", knob, new Color(1f, 0.85f, 0.30f), 26f);
        GameObject moon = MakeIcon(skyGo.transform, "Moon", knob, new Color(0.85f, 0.90f, 1f), 22f);

        // empty slots for the real art: a ring drawn over the sky window and one icon per phase
        GameObject skyFrame = MakeIcon(root.transform, "SkyFrame", null, new Color(1f, 1f, 1f, 0f), 88f);
        Anchor(skyFrame.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(88f, 88f));
        GameObject phaseIconGo = MakeIcon(root.transform, "PhaseIcon", null, Color.white, 32f);
        Anchor(phaseIconGo.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(-16f, -14f), new Vector2(32f, 32f));
        phaseIconGo.GetComponent<Image>().enabled = false;

        TextMeshProUGUI dayText = MakeUiText(root.transform, "DayText", 30f, TextAlignmentOptions.TopLeft);
        Anchor(dayText.rectTransform, new Vector2(0f, 1f), new Vector2(116f, -12f), new Vector2(210f, 40f));
        dayText.fontStyle = FontStyles.Bold;
        dayText.text = "DAY 1";

        TextMeshProUGUI timeText = MakeUiText(root.transform, "TimeText", 26f, TextAlignmentOptions.TopLeft);
        Anchor(timeText.rectTransform, new Vector2(0f, 1f), new Vector2(116f, -54f), new Vector2(110f, 40f));
        timeText.text = "08:00";

        TextMeshProUGUI phaseText = MakeUiText(root.transform, "PhaseText", 18f, TextAlignmentOptions.TopRight);
        Anchor(phaseText.rectTransform, new Vector2(1f, 1f), new Vector2(-16f, -60f), new Vector2(140f, 30f));
        phaseText.color = new Color(1f, 1f, 1f, 0.7f);
        phaseText.text = "Morning";

        DayTimeHUD hud = root.AddComponent<DayTimeHUD>();
        SetRef(hud, "dayText", dayText);
        SetRef(hud, "timeText", timeText);
        SetRef(hud, "phaseText", phaseText);
        SetRef(hud, "sky", skyImage);
        SetRef(hud, "sun", sun.GetComponent<RectTransform>());
        SetRef(hud, "moon", moon.GetComponent<RectTransform>());
        SetRef(hud, "phaseIcon", phaseIconGo.GetComponent<Image>());

        return SavePrefab(root, PrefabDir + "/DayTimeHUD.prefab");
    }

    private static GameObject MakeIcon(Transform parent, string name, Sprite sprite, Color color, float size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Anchor(go.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));

        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return go;
    }

    // copies Ali's player out of AliS.unity (arms, gun, movement, input) and adds the shop bits on top.
    // AliS itself is never saved. Falls back to a bare player if his scene or the Player object isn't there
    private static GameObject BuildPlayerPrefab()
    {
        if (!File.Exists(AliScenePath))
        {
            Debug.LogWarning("AliS.unity not found, building a bare player instead");
            return BuildBarePlayer();
        }

        Scene aliScene = EditorSceneManager.OpenScene(AliScenePath, OpenSceneMode.Additive);
        GameObject source = null;
        foreach (GameObject root in aliScene.GetRootGameObjects())
        {
            if (root.name == AliPlayerName && root.GetComponent<Movement>() != null)
            {
                source = root;
                break;
            }
        }

        GameObject prefab;
        if (source == null)
        {
            Debug.LogWarning("No '" + AliPlayerName + "' with Movement in AliS.unity, building a bare player instead");
            prefab = BuildBarePlayer();
        }
        else
        {
            GameObject player = Object.Instantiate(source);
            player.name = "ShopPlayer";
            player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            AddShopComponents(player);
            prefab = SavePrefab(player, PrefabDir + "/ShopPlayer.prefab");
        }

        // close without saving, we only borrowed the player
        EditorSceneManager.CloseScene(aliScene, true);
        return prefab;
    }

    // PlayerInteraction + PlayerCarry, and the Interact action moved off Movement.OnInteract
    // (that one fires every frame while the key is held) onto PlayerInteraction.OnInteract
    private static void AddShopComponents(GameObject player)
    {
        Movement movement = player.GetComponent<Movement>();
        Camera cam = null;
        if (movement != null)
        {
            SerializedProperty camProp = new SerializedObject(movement).FindProperty("mainCamera");
            if (camProp != null) cam = camProp.objectReferenceValue as Camera;
        }
        if (cam == null) cam = player.GetComponentInChildren<Camera>();

        PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
        if (interaction == null) interaction = player.AddComponent<PlayerInteraction>();
        SetRef(interaction, "playerCamera", cam);

        if (player.GetComponent<PlayerCarry>() == null) player.AddComponent<PlayerCarry>();

        foreach (PlayerInput input in player.GetComponentsInChildren<PlayerInput>(true))
        {
            // read the asset through the serialized field, the actions getter would clone it
            InputActionAsset asset = new SerializedObject(input).FindProperty("m_Actions").objectReferenceValue as InputActionAsset;
            if (asset == null) continue;

            InputAction interact = asset.FindAction("Movement_Normal/Interact", false);
            if (interact == null) continue;

            string id = interact.id.ToString();
            foreach (PlayerInput.ActionEvent e in input.actionEvents)
            {
                if (e.actionId != id) continue;

                for (int i = e.GetPersistentEventCount() - 1; i >= 0; i--)
                    UnityEventTools.RemovePersistentListener(e, i);

                if (input.defaultActionMap == "Movement_Normal")
                    UnityEventTools.AddPersistentListener(e, (UnityAction<InputAction.CallbackContext>)interaction.OnInteract);
            }
            EditorUtility.SetDirty(input);
        }
    }

    // fallback: same idea as Ali's player (Movement + PlayerInput with unity events) minus the arms and gun
    private static GameObject BuildBarePlayer()
    {
        GameObject player = new GameObject("ShopPlayer");

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = Vector3.zero;
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;
        cc.skinWidth = 0.08f;
        cc.minMoveDistance = 0.001f;

        GameObject camGo = new GameObject("PlayerCamera");
        camGo.transform.SetParent(player.transform, false);
        camGo.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.nearClipPlane = 0.05f;
        camGo.AddComponent<AudioListener>();

        AudioSource footsteps = player.AddComponent<AudioSource>();
        footsteps.playOnAwake = false;

        Movement movement = player.AddComponent<Movement>();
        SerializedObject so = new SerializedObject(movement);
        SetProp(so, "mainCamera", cam);
        SetProp(so, "footStepAudioSource", footsteps);
        SerializedProperty jump = so.FindProperty("jumpForce");
        if (jump != null) jump.floatValue = 8f;
        SetClips(so, "woodClips", "Assets/Ali/Sounds/Wood");
        SetClips(so, "metalClips", "Assets/Ali/Sounds/Metal");
        SetClips(so, "grassClips", "Assets/Ali/Sounds/Grass");
        so.ApplyModifiedPropertiesWithoutUndo();

        PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();
        SetRef(interaction, "playerCamera", cam);
        player.AddComponent<PlayerCarry>();

        InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayerActionsPath);
        if (actions != null)
        {
            PlayerInput input = player.AddComponent<PlayerInput>();
            input.actions = actions;
            input.defaultActionMap = "Movement_Normal";
            input.notificationBehavior = PlayerNotifications.InvokeUnityEvents;

            // same list the inspector builds, one event per action
            List<PlayerInput.ActionEvent> events = new List<PlayerInput.ActionEvent>();
            foreach (InputActionMap map in actions.actionMaps)
                foreach (InputAction action in map.actions)
                    events.Add(new PlayerInput.ActionEvent(action));

            Wire(events, actions, "Movement_Normal", "Move", movement.OnMove);
            Wire(events, actions, "Movement_Normal", "Look", movement.OnLook);
            Wire(events, actions, "Movement_Normal", "Jump", movement.OnJump);
            Wire(events, actions, "Movement_Normal", "Sprint", movement.OnSprint);
            Wire(events, actions, "Movement_Normal", "Crouch", movement.OnCrouch);
            Wire(events, actions, "Movement_Normal", "Interact", interaction.OnInteract);

            input.actionEvents = new ReadOnlyArray<PlayerInput.ActionEvent>(events.ToArray());
            EditorUtility.SetDirty(input);
        }
        else
        {
            Debug.LogWarning("Player.inputactions not found at " + PlayerActionsPath + ", the test player has no input");
        }

        return SavePrefab(player, PrefabDir + "/ShopPlayer.prefab");
    }

    private static void Wire(List<PlayerInput.ActionEvent> events, InputActionAsset asset, string mapName, string actionName,
        UnityAction<InputAction.CallbackContext> call)
    {
        InputActionMap map = asset.FindActionMap(mapName);
        InputAction action = map != null ? map.FindAction(actionName) : null;
        if (action == null)
        {
            Debug.LogWarning("No input action " + mapName + "/" + actionName);
            return;
        }

        string id = action.id.ToString();
        foreach (PlayerInput.ActionEvent e in events)
        {
            if (e.actionId != id) continue;
            UnityEventTools.AddPersistentListener(e, call);
            return;
        }
    }

    private static void SetClips(SerializedObject so, string field, string folder)
    {
        SerializedProperty prop = so.FindProperty(field);
        if (prop == null || !AssetDatabase.IsValidFolder(folder)) return;

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folder });
        prop.arraySize = guids.Length;
        for (int i = 0; i < guids.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guids[i]));
    }

    // ---------------------------------------------------------------- scene

    private static void BuildScene(Materials mats, List<ProductSO> products, ShopSettingsSO settings, DaySettingsSO daySettings,
        List<CustomerTypeSO> types, GameObject shelfPrefab, GameObject customerPrefab, GameObject checkoutPrefab, GameObject playerPrefab,
        GameObject hudPrefab)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // light fog so the outside reads a bit more like the village and less like a test grid
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.01f;
        RenderSettings.fogColor = new Color(0.55f, 0.6f, 0.65f);

        // the player brings his own camera
        Camera defaultCam = Object.FindFirstObjectByType<Camera>();
        if (defaultCam != null) Object.DestroyImmediate(defaultCam.gameObject);

        // ---- environment, everything under here gets baked into the navmesh ----
        GameObject env = new GameObject("Environment");

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(env.transform, false);
        ground.transform.localScale = new Vector3(8f, 1f, 8f);
        ground.GetComponent<Renderer>().sharedMaterial = mats.grass;
        SetTag(ground, "Grass");

        // 12 x 10 room, door in the middle of the front wall (-z side)
        GameObject building = new GameObject("ShopBuilding");
        building.transform.SetParent(env.transform, false);
        Box(building, "Floor", new Vector3(0f, 0.01f, 1f), new Vector3(12f, 0.02f, 10f), mats.floor, "Wood");
        Box(building, "WallBack", new Vector3(0f, 1.5f, 6f), new Vector3(12.2f, 3f, 0.2f), mats.wall);
        Box(building, "WallLeft", new Vector3(-6f, 1.5f, 1f), new Vector3(0.2f, 3f, 10f), mats.wall);
        Box(building, "WallRight", new Vector3(6f, 1.5f, 1f), new Vector3(0.2f, 3f, 10f), mats.wall);
        Box(building, "WallFrontLeft", new Vector3(-3.6f, 1.5f, -4f), new Vector3(4.8f, 3f, 0.2f), mats.wall);
        Box(building, "WallFrontRight", new Vector3(3.6f, 1.5f, -4f), new Vector3(4.8f, 3f, 0.2f), mats.wall);
        Box(building, "DoorFrame", new Vector3(0f, 2.6f, -4f), new Vector3(2.4f, 0.8f, 0.2f), mats.wall);
        Box(building, "Roof", new Vector3(0f, 3.1f, 1f), new Vector3(12.2f, 0.2f, 10.2f), mats.wall);
        Box(building, "RoofTrim", new Vector3(0f, 3.15f, 1f), new Vector3(12.6f, 0.15f, 10.6f), mats.darkWood);
        Box(building, "WindowLeft", new Vector3(-3.6f, 1.7f, -4.11f), new Vector3(1.8f, 1.1f, 0.02f), mats.window);
        Box(building, "WindowRight", new Vector3(3.6f, 1.7f, -4.11f), new Vector3(1.8f, 1.1f, 0.02f), mats.window);
        Box(building, "Porch", new Vector3(0f, 0.02f, -4.9f), new Vector3(3.2f, 0.04f, 1.6f), mats.darkWood);

        // some trees and rocks so the outside isn't a bare plane. Trunks and rocks block the navmesh, fine
        GameObject nature = new GameObject("Nature");
        nature.transform.SetParent(env.transform, false);
        Vector3[] treeSpots =
        {
            new Vector3(-10f, 0f, -8f), new Vector3(10f, 0f, -8f), new Vector3(-12f, 0f, 2f), new Vector3(12f, 0f, 3f),
            new Vector3(-9f, 0f, -16f), new Vector3(9f, 0f, -17f), new Vector3(-15f, 0f, -12f), new Vector3(15f, 0f, -11f),
            new Vector3(-6f, 0f, 10f), new Vector3(6f, 0f, 10f), new Vector3(0f, 0f, 12f), new Vector3(-4f, 0f, -20f),
            new Vector3(5f, 0f, -21f), new Vector3(-13f, 0f, -3f), new Vector3(13f, 0f, -2f),
        };
        for (int i = 0; i < treeSpots.Length; i++)
            Tree(nature, treeSpots[i], 0.8f + 0.1f * (i % 5), mats);

        Vector3[] rockSpots = { new Vector3(-7f, 0f, -5f), new Vector3(8f, 0f, -14f), new Vector3(-2f, 0f, -18f), new Vector3(7.5f, 0f, -6f) };
        for (int i = 0; i < rockSpots.Length; i++)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Rock";
            rock.transform.SetParent(nature.transform, false);
            rock.transform.localPosition = rockSpots[i] + Vector3.up * 0.15f;
            rock.transform.localRotation = Quaternion.Euler(0f, i * 50f, 0f);
            rock.transform.localScale = new Vector3(1.3f, 0.7f, 1.0f) * (0.8f + 0.15f * i);
            rock.GetComponent<Renderer>().sharedMaterial = mats.rock;
        }

        // a few warm ceiling lights since the roof blocks the sun
        GameObject lights = new GameObject("Lights");
        lights.transform.SetParent(building.transform, false);
        Vector3[] lightSpots =
        {
            new Vector3(-3f, 2.85f, -1.5f), new Vector3(3f, 2.85f, -1.5f),
            new Vector3(-3f, 2.85f, 3.5f), new Vector3(3f, 2.85f, 3.5f),
        };
        foreach (Vector3 spot in lightSpots)
        {
            GameObject lamp = new GameObject("CeilingLight");
            lamp.transform.SetParent(lights.transform, false);
            lamp.transform.localPosition = spot;

            Light light = lamp.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8f;
            light.intensity = 2.5f;
            light.color = new Color(1f, 0.9f, 0.75f);
            light.shadows = LightShadows.None;

            GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.name = "Bulb";
            bulb.transform.SetParent(lamp.transform, false);
            bulb.transform.localScale = Vector3.one * 0.25f;
            StripCollider(bulb);
            bulb.GetComponent<Renderer>().sharedMaterial = mats.lamp;
        }

        TextMeshPro sign = MakeWorldText(building.transform, "Sign", new Vector3(0f, 2.6f, -4.13f), new Vector2(4f, 0.8f), 6f);
        sign.text = "STORE";
        sign.color = new Color(0.75f, 0.12f, 0.12f);
        sign.fontStyle = FontStyles.Bold;

        // bed in the back corner, look at it and press E to skip the day / sleep
        GameObject bed = new GameObject("Bed");
        bed.transform.SetParent(building.transform, false);
        bed.transform.localPosition = new Vector3(5.3f, 0f, 3.5f);
        Box(bed, "Mattress", new Vector3(0f, 0.15f, 0f), new Vector3(1.0f, 0.3f, 2.0f), mats.bed);
        Box(bed, "Pillow", new Vector3(0f, 0.36f, 0.7f), new Vector3(0.6f, 0.12f, 0.4f), mats.mask);
        bed.AddComponent<SleepSpot>();

        GameObject shelves = new GameObject("Shelves");
        shelves.transform.SetParent(env.transform, false);
        Place(shelfPrefab, shelves.transform, new Vector3(-3.5f, 0f, 5.6f), Quaternion.identity);
        Place(shelfPrefab, shelves.transform, new Vector3(0f, 0f, 5.6f), Quaternion.identity);
        Place(shelfPrefab, shelves.transform, new Vector3(3.5f, 0f, 5.6f), Quaternion.identity);
        Place(shelfPrefab, shelves.transform, new Vector3(-5.6f, 0f, 0.5f), Quaternion.Euler(0f, -90f, 0f));
        Place(shelfPrefab, shelves.transform, new Vector3(-5.6f, 0f, 2.5f), Quaternion.Euler(0f, -90f, 0f));

        // counter on the right, the line forms along the room towards the door
        GameObject checkoutGo = Place(checkoutPrefab, env.transform, new Vector3(4.3f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));

        // ---- points ----
        GameObject points = new GameObject("Points");
        Transform spawn = MakePoint(points.transform, "CustomerSpawn", new Vector3(0f, 0f, -12f));
        Transform entrance = MakePoint(points.transform, "ShopEntrance", new Vector3(0f, 0f, -2.2f));

        // ---- managers ----
        GameObject dayGo = new GameObject("DayManager");
        DayManager dayManager = dayGo.AddComponent<DayManager>();
        SetRef(dayManager, "settings", daySettings);

        GameObject managerGo = new GameObject("ShopManager");
        ShopManager manager = managerGo.AddComponent<ShopManager>();
        SetRef(manager, "settings", settings);
        SetRef(manager, "checkout", checkoutGo.GetComponent<CheckoutCounter>());

        GameObject spawnerGo = new GameObject("CustomerSpawner");
        CustomerSpawner spawner = spawnerGo.AddComponent<CustomerSpawner>();
        SetRef(spawner, "customerPrefab", customerPrefab.GetComponent<Customer>());
        SetRefArray(spawner, "customerTypes", types.ToArray());
        SetRef(spawner, "spawnPoint", spawn);
        SetRef(spawner, "entrancePoint", entrance);
        SetRef(spawner, "exitPoint", spawn);

        // ---- stuff lying around outside ----
        GameObject pickups = new GameObject("Pickups");
        Vector2[] spots =
        {
            new Vector2(-3f, -7f), new Vector2(-5f, -9f), new Vector2(-6.5f, -12f), new Vector2(-2.5f, -11f),
            new Vector2(-4f, -13.5f), new Vector2(3f, -7f), new Vector2(5f, -9f), new Vector2(6.5f, -12f),
            new Vector2(2.5f, -11f), new Vector2(4f, -13.5f), new Vector2(-1.8f, -15f), new Vector2(1.8f, -15f),
        };
        for (int i = 0; i < spots.Length; i++)
        {
            ProductSO product = products[i % products.Count];
            GameObject go = new GameObject(product.DisplayName);
            go.transform.SetParent(pickups.transform, false);
            go.transform.position = new Vector3(spots[i].x, 0f, spots[i].y);
            go.transform.rotation = Quaternion.Euler(0f, i * 37f, 0f);

            PickupItem pickup = go.AddComponent<PickupItem>();
            SetRef(pickup, "product", product);
            pickup.BuildVisual();
        }

        // ---- player, outside facing the door ----
        Place(playerPrefab, null, new Vector3(0f, 1f, -8f), Quaternion.identity);

        BuildUi(hudPrefab);

        // ---- navmesh ----
        NavMeshSurface surface = env.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        surface.BuildNavMesh();
        if (surface.navMeshData != null)
            AssetDatabase.CreateAsset(surface.navMeshData, NavMeshDir + "/NavMesh-Environment.asset");
        else
            Debug.LogWarning("NavMesh bake failed, bake it by hand on the Environment object");

        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    private static void BuildUi(GameObject hudPrefab)
    {
        GameObject canvasGo = new GameObject("ShopUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.layer = LayerMask.NameToLayer("UI");
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject cross = new GameObject("Crosshair", typeof(RectTransform), typeof(Image));
        cross.transform.SetParent(canvasGo.transform, false);
        Anchor(cross.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6f, 6f));
        cross.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.8f);
        cross.GetComponent<Image>().raycastTarget = false;

        GameObject panel = new GameObject("PromptPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGo.transform, false);
        Anchor(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(760f, 100f));
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        panel.GetComponent<Image>().raycastTarget = false;

        TextMeshProUGUI prompt = MakeUiText(panel.transform, "PromptText", 30f, TextAlignmentOptions.Center);
        prompt.rectTransform.anchorMin = Vector2.zero;
        prompt.rectTransform.anchorMax = Vector2.one;
        prompt.rectTransform.offsetMin = new Vector2(16f, 8f);
        prompt.rectTransform.offsetMax = new Vector2(-16f, -8f);

        // day / time clock top-left, money right under it
        if (hudPrefab != null)
        {
            GameObject hudGo = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
            hudGo.transform.SetParent(canvasGo.transform, false);
            hudGo.GetComponent<RectTransform>().anchoredPosition = new Vector2(24f, -24f);
        }

        TextMeshProUGUI money = MakeUiText(canvasGo.transform, "MoneyText", 40f, TextAlignmentOptions.TopLeft);
        Anchor(money.rectTransform, new Vector2(0f, 1f), new Vector2(24f, -150f), new Vector2(600f, 60f));

        TextMeshProUGUI carry = MakeUiText(canvasGo.transform, "CarryText", 26f, TextAlignmentOptions.BottomRight);
        Anchor(carry.rectTransform, new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(800f, 80f));

        InteractionPromptUI promptUi = canvasGo.AddComponent<InteractionPromptUI>();
        SetRef(promptUi, "panel", panel);
        SetRef(promptUi, "promptText", prompt);

        ShopHUD hud = canvasGo.AddComponent<ShopHUD>();
        SetRef(hud, "moneyText", money);
        SetRef(hud, "carryText", carry);

        TextMeshProUGUI state = MakeUiText(canvasGo.transform, "ShopStateText", 32f, TextAlignmentOptions.TopRight);
        Anchor(state.rectTransform, new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(400f, 50f));
        SetRef(hud, "shopStateText", state);

        TextMeshProUGUI sale = MakeUiText(canvasGo.transform, "SaleText", 34f, TextAlignmentOptions.Top);
        Anchor(sale.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(800f, 60f));
        sale.color = new Color(0.55f, 1f, 0.55f);
        SetRef(hud, "saleText", sale);

        TextMeshProUGUI hint = MakeUiText(canvasGo.transform, "ControlsHint", 18f, TextAlignmentOptions.Top);
        Anchor(hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(700f, 40f));
        hint.color = new Color(1f, 1f, 1f, 0.5f);
        hint.text = "WASD move   |   Mouse look   |   E interact   |   Shift run   |   C crouch";
    }

    // ---------------------------------------------------------------- helpers

    private static GameObject Box(GameObject parent, string name, Vector3 localPos, Vector3 size, Material mat, string tag = null)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        if (!string.IsNullOrEmpty(tag)) SetTag(go, tag);
        return go;
    }

    private static void Tree(GameObject parent, Vector3 position, float scale, Materials mats)
    {
        GameObject tree = new GameObject("Tree");
        tree.transform.SetParent(parent.transform, false);
        tree.transform.localPosition = position;
        tree.transform.localScale = Vector3.one * scale;

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform, false);
        trunk.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        trunk.transform.localScale = new Vector3(0.4f, 1.5f, 0.4f);
        trunk.GetComponent<Renderer>().sharedMaterial = mats.wood;

        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Crown";
        crown.transform.SetParent(tree.transform, false);
        crown.transform.localPosition = new Vector3(0f, 3.7f, 0f);
        crown.transform.localScale = new Vector3(2.8f, 2.6f, 2.8f);
        StripCollider(crown);
        crown.GetComponent<Renderer>().sharedMaterial = mats.leaves;
    }

    private static void SetTag(GameObject go, string tag)
    {
        // Movement.cs picks footstep sounds by tag, skip quietly if the project doesn't have it
        foreach (string t in UnityEditorInternal.InternalEditorUtility.tags)
        {
            if (t != tag) continue;
            go.tag = tag;
            return;
        }
    }

    private static void StripCollider(GameObject go)
    {
        Collider col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
    }

    private static Transform MakePoint(Transform parent, string name, Vector3 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        return go.transform;
    }

    private static GameObject Place(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (parent != null) go.transform.SetParent(parent, true);
        go.transform.SetPositionAndRotation(position, rotation);
        return go;
    }

    private static GameObject SavePrefab(GameObject temp, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
        Object.DestroyImmediate(temp);
        return prefab;
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string name = Path.GetFileName(path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static string SafeName(string name)
    {
        char[] chars = name.ToCharArray();
        string result = "";
        foreach (char c in chars)
            if (char.IsLetterOrDigit(c)) result += c;
        return result;
    }

    private static TextMeshPro MakeWorldText(Transform parent, string name, Vector3 localPos, Vector2 size, float fontSize)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        TextMeshPro text = go.AddComponent<TextMeshPro>();
        text.rectTransform.sizeDelta = size;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        text.text = "";
        return text;
    }

    private static TextMeshProUGUI MakeUiText(Transform parent, string name, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = "";
        text.raycastTarget = false;
        return text;
    }

    private static void Anchor(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    private static void SetRef(Object target, string field, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SetProp(so, field, value);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetProp(SerializedObject so, string field, Object value)
    {
        SerializedProperty prop = so.FindProperty(field);
        if (prop == null)
        {
            Debug.LogWarning("No field " + field + " on " + so.targetObject.GetType().Name);
            return;
        }
        prop.objectReferenceValue = value;
    }

    private static void SetRefArray(Object target, string field, Object[] values)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(field);
        if (prop == null)
        {
            Debug.LogWarning("No field " + field + " on " + target.GetType().Name);
            return;
        }

        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
