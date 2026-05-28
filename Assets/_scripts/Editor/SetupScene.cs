using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/*
Type: Unity Editor Script

Role defines:
    - Adds the "Combat System" dropdown menu to the Unity Editor toolbar.
    - Automates the creation of a standardized combat scene (Player, EnemyDummy, Hitboxes, etc.).
    - Enables Playmode tests to run properly by preparing the required GameObjects and environment before executing the test runner.
*/
public static class SetupScene
{
    [MenuItem("Combat System/Setup Scene")]
    public static void CreateCombatScene()
    {
  
        // 1. Create or Find CombatResolver
#if UNITY_2023_1_OR_NEWER
        CombatResolver resolver = Object.FindFirstObjectByType<CombatResolver>();
#else
        CombatResolver resolver = Object.FindObjectOfType<CombatResolver>();
#endif
        if (resolver == null)
        {
            GameObject resolverObj = new GameObject("CombatResolver");
            resolver = resolverObj.AddComponent<CombatResolver>();
            Undo.RegisterCreatedObjectUndo(resolverObj, "Create CombatResolver");
        }

        // 2. Create or Find CombatDebugger
#if UNITY_2023_1_OR_NEWER
        CombatDebugger debugger = Object.FindFirstObjectByType<CombatDebugger>();
#else
        CombatDebugger debugger = Object.FindObjectOfType<CombatDebugger>();
#endif
        if (debugger == null)
        {
            GameObject debuggerObj = new GameObject("CombatDebugger");
            debugger = debuggerObj.AddComponent<CombatDebugger>();
            Undo.RegisterCreatedObjectUndo(debuggerObj, "Create CombatDebugger");
        }

        // 3. Create Player GameObject
#if UNITY_2023_1_OR_NEWER
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
#else
        PlayerController player = Object.FindObjectOfType<PlayerController>();
#endif
        if (player == null)
        {
            GameObject playerObj = new GameObject("Player");
            playerObj.transform.position = Vector3.zero;
            
            // Add Rigidbody so physics trigger detection works reliably
            Rigidbody rb = playerObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            player = playerObj.AddComponent<PlayerController>();

            // Create Hitbox child
            GameObject hitboxObj = new GameObject("Hitbox");
            hitboxObj.transform.SetParent(playerObj.transform);
            hitboxObj.transform.localPosition = new Vector3(0f, 1f, 0.5f); // Centered at chest height, slightly forward
            
            BoxCollider hitboxCollider = hitboxObj.AddComponent<BoxCollider>();
            hitboxCollider.isTrigger = true;
            hitboxCollider.size = new Vector3(1.5f, 2f, 1.5f);

            Hitbox hitboxComponent = hitboxObj.AddComponent<Hitbox>();

            // Create Hurtbox child
            GameObject hurtboxObj = new GameObject("Hurtbox");
            hurtboxObj.transform.SetParent(playerObj.transform);
            hurtboxObj.transform.localPosition = new Vector3(0f, 1f, 0f);

            BoxCollider hurtboxCollider = hurtboxObj.AddComponent<BoxCollider>();
            hurtboxCollider.isTrigger = true;
            hurtboxCollider.size = new Vector3(1f, 2f, 1f);

            Hurtbox hurtboxComponent = hurtboxObj.AddComponent<Hurtbox>();

            // Load move asset
            MoveData idleMove = AssetDatabase.LoadAssetAtPath<MoveData>("Assets/_moves/Idle.asset");
            
            // Assign fields via SerializedObject to maintain serialization
            SerializedObject serializedPlayer = new SerializedObject(player);
            serializedPlayer.FindProperty("idleMove").objectReferenceValue = idleMove;
            serializedPlayer.FindProperty("hitbox").objectReferenceValue = hitboxComponent;
            serializedPlayer.FindProperty("hurtbox").objectReferenceValue = hurtboxComponent;
            serializedPlayer.ApplyModifiedProperties();

            // Set up Weapon component for testing
            Weapon weaponComponent = playerObj.AddComponent<Weapon>();

            Undo.RegisterCreatedObjectUndo(playerObj, "Create Player");
            Debug.Log("[SetupScene] Player created successfully.");
        }

        // 4. Create Enemy GameObject
#if UNITY_2023_1_OR_NEWER
        Enemy enemy = Object.FindFirstObjectByType<Enemy>();
#else
        Enemy enemy = Object.FindObjectOfType<Enemy>();
#endif
        if (enemy == null)
        {
            GameObject enemyObj = new GameObject("EnemyDummy");
            enemyObj.transform.position = new Vector3(0f, 0f, 1.2f); // Placed in front within player's hitbox range
            
            // Add Rigidbody so physics trigger detection works reliably
            Rigidbody rb = enemyObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            enemy = enemyObj.AddComponent<Enemy>();

            // Enemy component expects Hurtbox to be on the same GameObject as per Enemy.Awake()
            BoxCollider enemyCollider = enemyObj.AddComponent<BoxCollider>();
            enemyCollider.isTrigger = true;
            enemyCollider.size = new Vector3(1f, 2f, 1f);

            Hurtbox enemyHurtbox = enemyObj.AddComponent<Hurtbox>();

            Undo.RegisterCreatedObjectUndo(enemyObj, "Create EnemyDummy");
            Debug.Log("[SetupScene] EnemyDummy created successfully.");
        }

        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(activeScene.path))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
            EditorSceneManager.SaveScene(activeScene, "Assets/Scenes/CombatTest.unity");
            Debug.Log("[SetupScene] Scene saved to Assets/Scenes/CombatTest.unity");
        }
        else
        {
            // Mark the active scene as dirty so the editor knows it has unsaved changes
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
        }
        Debug.Log("[SetupScene] Scene setup completed successfully!");
    }

    [MenuItem("Combat System/Run Playmode Tests")]
    public static void RunPlaymodeTests()
    {
        // Setup the scene first
        CreateCombatScene();

        // Find or create TestRunner
        GameObject testRunnerObj = GameObject.Find("TestRunner");
        if (testRunnerObj == null)
        {
            testRunnerObj = new GameObject("TestRunner");
            testRunnerObj.AddComponent<CombatSystemTestRunner>();
        }

        // Save scene
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        // Hook into playmode state changed to quit when finished
        EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;

        // Enter Playmode
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlaymodeStateChanged(PlayModeStateChange change)
    {
        // If we exited playmode, exit editor
        if (change == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
            
            // Clean up the TestRunner GameObject so the scene stays clean
            GameObject testRunnerObj = GameObject.Find("TestRunner");
            if (testRunnerObj != null)
            {
                Object.DestroyImmediate(testRunnerObj);
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }

            Debug.Log("[SetupScene] Playmode tests completed successfully!");
            EditorApplication.Exit(0);
        }
    }
}
