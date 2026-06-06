using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/*
Type: Unity Editor Script

Role defines:
    - Adds the "Combat System" dropdown menu to the Unity Editor toolbar.
    - Automates the creation of a standardized combat scene (Player, EnemyDummy, Hitboxes, etc.).
    - Sets up movement (PlayerMovement + dynamic Rigidbody + CapsuleCollider + floor plane).
    - Sets up Cinemachine third-person camera + lock-on VirtualCamera + TargetGroup.
    - Enables Playmode tests to run properly by preparing the required GameObjects and environment.
*/
public static class SetupScene
{
    [MenuItem("Combat System/Setup Scene")]
    public static void CreateCombatScene()
    {
        // ── 1. CombatResolver ──────────────────────────────────────────────
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

        // ── 2. CombatFeedbackUI ────────────────────────────────────────────
#if UNITY_2023_1_OR_NEWER
        CombatFeedbackUI ui = Object.FindFirstObjectByType<CombatFeedbackUI>();
#else
        CombatFeedbackUI ui = Object.FindObjectOfType<CombatFeedbackUI>();
#endif
        if (ui == null)
        {
            GameObject uiObj = new GameObject("CombatFeedbackUI");
            ui = uiObj.AddComponent<CombatFeedbackUI>();
            Undo.RegisterCreatedObjectUndo(uiObj, "Create CombatFeedbackUI");
        }

        // ── 3. Floor ───────────────────────────────────────────────────────
        GameObject floor = GameObject.Find("Floor");
        if (floor == null)
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(30f, 0.2f, 30f);
            Undo.RegisterCreatedObjectUndo(floor, "Create Floor");
            Debug.Log("[SetupScene] Floor created.");
        }

        // ── 4. Player ──────────────────────────────────────────────────────
#if UNITY_2023_1_OR_NEWER
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
#else
        PlayerController player = Object.FindObjectOfType<PlayerController>();
#endif

        Transform playerCameraTarget = null;

        if (player == null)
        {
            GameObject playerObj = new GameObject("Player");
            playerObj.transform.position = new Vector3(0f, 0.1f, 0f); // Sit on the floor

            // ── Rigidbody (dynamic, gravity on, high drag for grounded feel) ──
            Rigidbody rb = playerObj.AddComponent<Rigidbody>();
            rb.isKinematic  = false;
            rb.useGravity   = true;
            rb.linearDamping  = 8f;    // Provides friction / prevents floatiness
            rb.angularDamping = 5f;
            rb.constraints  = RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationZ; // Only Y-axis rotation

            // ── CapsuleCollider for ground contact ─────────────────────────
            CapsuleCollider capsule = playerObj.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 1f, 0f);
            capsule.height = 2f;
            capsule.radius = 0.4f;

            // ── Combat components (unchanged from original) ────────────────
            player = playerObj.AddComponent<PlayerController>();

            // ── Placeholder body mesh (blue capsule) ───────────────────────
            // This is a visual stand-in. Replace with your real character model later.
            // The Animator on this child is what PlayerController drives.
            GameObject playerMesh = CreatePlaceholderMesh(
                "PlayerModel", PrimitiveType.Capsule, playerObj.transform,
                new Vector3(0f, 1f, 0f), Vector3.one,
                new Color(0.2f, 0.45f, 0.85f)); // blue

            // Add Animator to the placeholder body and wire it into PlayerController
            Animator playerAnimator = playerMesh.AddComponent<Animator>();
            SerializedObject serializedPlayer0 = new SerializedObject(player);
            serializedPlayer0.FindProperty("animator").objectReferenceValue = playerAnimator;
            serializedPlayer0.ApplyModifiedProperties();

            GameObject weaponObj = new GameObject("DummyWeapon");
            weaponObj.transform.SetParent(playerObj.transform);
            weaponObj.transform.localPosition = new Vector3(0.5f, 1f, 0f);
            Weapon weaponComponent = weaponObj.AddComponent<Weapon>();

            // ── Placeholder weapon mesh (grey cylinder) ────────────────────
            CreatePlaceholderMesh(
                "WeaponModel", PrimitiveType.Cylinder, weaponObj.transform,
                new Vector3(0f, 0.4f, 0f),
                new Vector3(0.12f, 0.55f, 0.12f),
                new Color(0.55f, 0.55f, 0.55f)); // grey

            GameObject hitboxObj = new GameObject("Hitbox");
            hitboxObj.transform.SetParent(weaponObj.transform);
            hitboxObj.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            BoxCollider hitboxCollider = hitboxObj.AddComponent<BoxCollider>();
            hitboxCollider.isTrigger = true;
            hitboxCollider.size = new Vector3(1.5f, 2f, 1.5f);
            hitboxObj.AddComponent<Hitbox>();

            GameObject hurtboxObj = new GameObject("Hurtbox");
            hurtboxObj.transform.SetParent(playerObj.transform);
            hurtboxObj.transform.localPosition = new Vector3(0f, 1f, 0f);
            BoxCollider hurtboxCollider = hurtboxObj.AddComponent<BoxCollider>();
            hurtboxCollider.isTrigger = true;
            hurtboxCollider.size = new Vector3(1f, 2f, 1f);
            Hurtbox hurtboxComponent = hurtboxObj.AddComponent<Hurtbox>();

            MoveData idleMove = AssetDatabase.LoadAssetAtPath<MoveData>("Assets/_moves/Idle.asset");
            SerializedObject serializedPlayer = new SerializedObject(player);
            serializedPlayer.FindProperty("idleMove").objectReferenceValue = idleMove;
            serializedPlayer.FindProperty("equippedWeapon").objectReferenceValue = weaponComponent;
            serializedPlayer.FindProperty("hurtbox").objectReferenceValue = hurtboxComponent;
            serializedPlayer.ApplyModifiedProperties();

            // ── PlayerMovement ─────────────────────────────────────────────
            playerObj.AddComponent<PlayerMovement>();

            // ── Player CameraTarget ────────────────────────────────────────
            GameObject playerCamTargetObj = new GameObject("CameraTarget");
            playerCamTargetObj.transform.SetParent(playerObj.transform);
            playerCamTargetObj.transform.localPosition = new Vector3(0f, 1.6f, 0f); // Head height
            playerCameraTarget = playerCamTargetObj.transform;

            Undo.RegisterCreatedObjectUndo(playerObj, "Create Player");
            Debug.Log("[SetupScene] Player created successfully.");
        }
        else
        {
            // Player already exists — upgrade its Rigidbody if still kinematic
            Rigidbody existingRb = player.GetComponent<Rigidbody>();
            if (existingRb != null && existingRb.isKinematic)
            {
                existingRb.isKinematic = false;
                existingRb.useGravity  = true;
                existingRb.linearDamping = 8f;
                existingRb.angularDamping = 5f;
                existingRb.constraints = RigidbodyConstraints.FreezeRotationX
                                       | RigidbodyConstraints.FreezeRotationZ;
                EditorUtility.SetDirty(existingRb);
                Debug.Log("[SetupScene] Upgraded existing Player Rigidbody to dynamic.");
            }

            // Ensure CapsuleCollider exists
            if (player.GetComponent<CapsuleCollider>() == null)
            {
                CapsuleCollider cap = player.gameObject.AddComponent<CapsuleCollider>();
                cap.center = new Vector3(0f, 1f, 0f);
                cap.height = 2f;
                cap.radius = 0.4f;
                Debug.Log("[SetupScene] Added CapsuleCollider to existing Player.");
            }

            // Ensure PlayerMovement exists
            if (player.GetComponent<PlayerMovement>() == null)
            {
                player.gameObject.AddComponent<PlayerMovement>();
                Debug.Log("[SetupScene] Added PlayerMovement to existing Player.");
            }

            // Ensure CameraTarget exists
            Transform existing = player.transform.Find("CameraTarget");
            if (existing == null)
            {
                GameObject ct = new GameObject("CameraTarget");
                ct.transform.SetParent(player.transform);
                ct.transform.localPosition = new Vector3(0f, 1.6f, 0f);
                playerCameraTarget = ct.transform;
                Debug.Log("[SetupScene] Added CameraTarget to existing Player.");
            }
            else
            {
                playerCameraTarget = existing;
            }
        }

        // ── 5. EnemyDummy ─────────────────────────────────────────────────
#if UNITY_2023_1_OR_NEWER
        Enemy enemy = Object.FindFirstObjectByType<Enemy>();
#else
        Enemy enemy = Object.FindObjectOfType<Enemy>();
#endif

        Transform enemyCameraTarget = null;

        if (enemy == null)
        {
            GameObject enemyObj = new GameObject("EnemyDummy");
            enemyObj.transform.position = new Vector3(0f, 0.1f, 4f);

            Rigidbody rb = enemyObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;

            enemy = enemyObj.AddComponent<Enemy>();

            // ── Placeholder body mesh (red capsule) ────────────────────────
            CreatePlaceholderMesh(
                "EnemyModel", PrimitiveType.Capsule, enemyObj.transform,
                new Vector3(0f, 1f, 0f), Vector3.one,
                new Color(0.85f, 0.2f, 0.2f)); // red

            GameObject enemyHurtboxObj = new GameObject("BodyHitzone");
            enemyHurtboxObj.transform.SetParent(enemyObj.transform);
            enemyHurtboxObj.transform.localPosition = new Vector3(0f, 1f, 0f);
            BoxCollider enemyCollider = enemyHurtboxObj.AddComponent<BoxCollider>();
            enemyCollider.isTrigger = true;
            enemyCollider.size = new Vector3(1f, 2f, 1f);
            enemyHurtboxObj.AddComponent<Hurtbox>();

            // ── Enemy CameraTarget ─────────────────────────────────────────
            GameObject enemyCamTargetObj = new GameObject("CameraTarget");
            enemyCamTargetObj.transform.SetParent(enemyObj.transform);
            enemyCamTargetObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            enemyCameraTarget = enemyCamTargetObj.transform;

            Undo.RegisterCreatedObjectUndo(enemyObj, "Create EnemyDummy");
            Debug.Log("[SetupScene] EnemyDummy created successfully.");
        }
        else
        {
            // Enemy already exists — ensure CameraTarget child exists
            Transform existing = enemy.transform.Find("CameraTarget");
            if (existing == null)
            {
                GameObject ct = new GameObject("CameraTarget");
                ct.transform.SetParent(enemy.transform);
                ct.transform.localPosition = new Vector3(0f, 1.6f, 0f);
                enemyCameraTarget = ct.transform;
                Debug.Log("[SetupScene] Added CameraTarget to existing EnemyDummy.");
            }
            else
            {
                enemyCameraTarget = existing;
            }
        }

        // ── 6. Camera Setup ────────────────────────────────────────────────
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            Undo.RegisterCreatedObjectUndo(camObj, "Create Main Camera");
        }

        // Remove VCam_Follow — no longer used
        GameObject vcamFollowObj = GameObject.Find("VCam_Follow");
        if (vcamFollowObj != null)
        {
            Undo.DestroyObjectImmediate(vcamFollowObj);
            Debug.Log("[SetupScene] VCam_Follow removed.");
        }

        // Remove VCam_LockOn — lock-on handled by ThirdPersonCamera now
        GameObject vcamLockOnObj = GameObject.Find("VCam_LockOn");
        if (vcamLockOnObj != null)
        {
    Undo.DestroyObjectImmediate(vcamLockOnObj);
    Debug.Log("[SetupScene] VCam_LockOn removed.");
        }

        // Remove stale world-space PlayerCameraTarget if present
        GameObject oldTarget = GameObject.Find("PlayerCameraTarget");
        if (oldTarget != null)
        {
            Undo.DestroyObjectImmediate(oldTarget);
            Debug.Log("[SetupScene] Stale PlayerCameraTarget removed.");
        }

        GameObject targetGroupObj = GameObject.Find("CM_TargetGroup");
        if (targetGroupObj != null)
        {
            Undo.DestroyObjectImmediate(targetGroupObj);
            Debug.Log("[SetupScene] CM_TargetGroup removed.");
        }

        // Add ThirdPersonCamera to Main Camera (idempotent)
        ThirdPersonCamera tpCam = mainCam.GetComponent<ThirdPersonCamera>();
        if (tpCam == null)
            tpCam = mainCam.gameObject.AddComponent<ThirdPersonCamera>();

        tpCam.SetPlayer(player.transform);
        EditorUtility.SetDirty(mainCam.gameObject);
        Debug.Log("[SetupScene] ThirdPersonCamera configured on Main Camera.");

        // ── 7. Wire LockOnController onto the Player ───────────────────────
        LockOnController lockOn = player.GetComponent<LockOnController>();
        if (lockOn == null)
            lockOn = player.gameObject.AddComponent<LockOnController>();

        lockOn.SetReferences(tpCam, enemyCameraTarget);
        EditorUtility.SetDirty(player.gameObject);

        // ── 8. Link UI references ──────────────────────────────────────────
        if (ui != null)
        {
            ui.player = player;
            ui.enemy  = enemy;
            EditorUtility.SetDirty(ui);
        }

        // ── 9. Save scene ──────────────────────────────────────────────────
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(activeScene.path))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(activeScene, "Assets/Scenes/CombatTest.unity");
            Debug.Log("[SetupScene] Scene saved to Assets/Scenes/CombatTest.unity");
        }
        else
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
        }

        Debug.Log("[SetupScene] Scene setup completed successfully!");
    }

    [MenuItem("Combat System/Run Playmode Tests")]
    public static void RunPlaymodeTests()
    {
        CreateCombatScene();

        GameObject testRunnerObj = GameObject.Find("TestRunner");
        if (testRunnerObj == null)
        {
            testRunnerObj = new GameObject("TestRunner");
            testRunnerObj.AddComponent<CombatSystemTestRunner>();
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlaymodeStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;

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

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a coloured primitive child used as a placeholder model.
    /// The primitive's own collider is removed — physics/combat use their
    /// own dedicated collider GameObjects.
    /// </summary>
    private static GameObject CreatePlaceholderMesh(
        string name,
        PrimitiveType type,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Color colour)
    {
        GameObject mesh = GameObject.CreatePrimitive(type);
        mesh.name = name;
        mesh.transform.SetParent(parent);
        mesh.transform.localPosition = localPosition;
        mesh.transform.localScale    = localScale;

        // Remove the auto-generated collider — collisions are handled by
        // the dedicated Hitbox / Hurtbox / CapsuleCollider objects.
        Collider col = mesh.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);

        // Apply a simple unlit colour material so the model is clearly visible.
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
            mat = new Material(Shader.Find("Standard")); // URP fallback
        mat.color = colour;
        mesh.GetComponent<MeshRenderer>().sharedMaterial = mat;

        return mesh;
    }
}
