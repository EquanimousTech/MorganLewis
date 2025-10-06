// using UnityEngine;
// using System.Collections.Generic;
// using System.Collections;

// public class DualTreeManager : MonoBehaviour
// {
//     [Header("Tree Configuration")]
//     public float treeSpacing = 8f;
//     public float treeHeight = 4f;
//     public float trunkThickness = 0.6f;

//     [Header("Current Usage Tree (Left/Green)")]
//     public Color currentUsageTreeColor = new Color(0.2f, 0.8f, 0.2f); // Green
//     public Color currentUsageTagColor = new Color(0.4f, 1f, 0.4f); // Light Green
//     // public Transform currentUsageTreeRoot;

//     [Header("Future Usage Tree (Right/Purple)")]
//     public Color futureUsageTreeColor = new Color(0.6f, 0.2f, 0.8f); // Purple
//     public Color futureUsageTagColor = new Color(0.8f, 0.6f, 1f); // Light Purple
//     // public Transform futureUsageTreeRoot;

//     [Header("Circular Connector")]
//     public float circleRadius = 2f;
//     public int circleSegments = 32;
//     public Material barkMaterial;

//     [Header("Tag Configuration")]
//     public GameObject tagPrefab;
//     public int maxTagsPerBranch = 3; // Change from 1 to 3
//     public float minTagVerticalSpacing = 2f; // NEW FIELD
//     public float maxTagVerticalSpacing = 3f; // NEW FIELD  
//     public float tagHorizontalOffset = 0.8f; // NEW FIELD

//     [Header("Branch Configuration")]
//     public int mainBranchCount = 5;
//     public float mainBranchThickness = 0.4f;
//     public float branchAngleMin = 20f;
//     public float branchAngleMax = 60f;
//     public float baseBranchLength = 3f;
//     public float subBranchMinSpacing = 0.5f; // NEW FIELD - prevents clustering

//     [Header("Foliage Configuration")]
//     public GameObject foliagePrefab;
//     public float foliageScale = 1f;

//     [Header("Tree Trunks")]
//     public GameObject currentUsageTreeTrunk; // Your SpeedTree trunk model
//     public GameObject futureUsageTreeTrunk; // Your SpeedTree trunk model

//     [Header("Branch Spawn Configuration")]
//     [Tooltip("Manually placed empty GameObjects as branch spawn points")]
//     public Transform[] currentTreeBranchPoints;
//     public Transform[] futureTreeBranchPoints;

//     [Header("Branch Growth")]
//     public float horizontalBranchLength = 4f;
//     public float branchAngleVariance = 15f; // Small variance for natural look
//     public bool growHorizontally = true;

//     [Header("Camera Reference")]
//     public Camera mainCamera;
//     public bool alwaysFaceCamera = true;

//     private TreeStructure currentUsageTree;
//     private TreeStructure futureUsageTree;

//     // void Start()
//     // {
//     //     SetupTreePositions();
//     //     CreateTrees();

//     //     // Initialize Strapi connector
//     //     GetComponent<StrapiConnector>().Initialize();
//     // }

//     void Start()
//     {
//         if (mainCamera == null)
//             mainCamera = Camera.main;

//         // SetupTreePositions();
//         CreateTrees();

//         // Initialize Strapi connector
//         var strapiConnector = GetComponent<StrapiConnector>();
//         if (strapiConnector != null)
//             strapiConnector.Initialize();
//     }

//     // void SetupTreePositions()
//     // {
//     //     // Create root transforms if not assigned
//     //     if (currentUsageTreeRoot == null)
//     //     {
//     //         GameObject currentTreeGO = new GameObject("CurrentUsageTree");
//     //         currentTreeGO.transform.parent = transform;
//     //         currentTreeGO.transform.localPosition = new Vector3(-treeSpacing / 2, 0, 0);
//     //         currentUsageTreeRoot = currentTreeGO.transform;
//     //     }

//     //     if (futureUsageTreeRoot == null)
//     //     {
//     //         GameObject futureTreeGO = new GameObject("FutureUsageTree");
//     //         futureTreeGO.transform.parent = transform;
//     //         futureTreeGO.transform.localPosition = new Vector3(treeSpacing / 2, 0, 0);
//     //         futureUsageTreeRoot = futureTreeGO.transform;
//     //     }
//     // }

//     void SetupTreePositions()
//     {
//         // Position the trunks
//         if (currentUsageTreeTrunk != null)
//         {
//             currentUsageTreeTrunk.transform.localPosition = new Vector3(-treeSpacing / 2, 0, 0);
//         }

//         if (futureUsageTreeTrunk != null)
//         {
//             futureUsageTreeTrunk.transform.localPosition = new Vector3(treeSpacing / 2, 0, 0);
//         }
//     }

//     // void CreateTrees()
//     // {
//     //     // Create Current Usage Tree (Green)
//     //     currentUsageTree = new TreeStructure(
//     //         currentUsageTreeRoot.position,
//     //         currentUsageTreeColor,
//     //         "Current Usage"
//     //     );
//     //     GenerateTreeBranches(currentUsageTree, currentUsageTreeRoot);

//     //     // Create Future Usage Tree (Purple)
//     //     futureUsageTree = new TreeStructure(
//     //         futureUsageTreeRoot.position,
//     //         futureUsageTreeColor,
//     //         "Future Usage"
//     //     );
//     //     GenerateTreeBranches(futureUsageTree, futureUsageTreeRoot);
//     // }

//     void CreateTrees()
//     {
//         // Create Current Usage Tree
//         if (currentUsageTreeTrunk != null)
//         {
//             currentUsageTree = new TreeStructure(
//                 currentUsageTreeTrunk.transform.position,
//                 currentUsageTreeColor,
//                 "Current Usage"
//             );
//             GenerateTreeBranches(currentUsageTree, currentUsageTreeTrunk.transform);
//         }

//         // Create Future Usage Tree
//         if (futureUsageTreeTrunk != null)
//         {
//             futureUsageTree = new TreeStructure(
//                 futureUsageTreeTrunk.transform.position,
//                 futureUsageTreeColor,
//                 "Future Usage"
//             );
//             GenerateTreeBranches(futureUsageTree, futureUsageTreeTrunk.transform);
//         }
//     }

//     // void GenerateTreeBranches(TreeStructure tree, Transform parent)
//     // {
//     //     // Generate branches directly from the root position
//     //     for (int i = 0; i < mainBranchCount; i++)
//     //     {
//     //         // Calculate branch angle in a semi-circle pattern
//     //         float angleStep = 180f / (mainBranchCount - 1);
//     //         float angle = -90f + (i * angleStep);

//     //         // Add vertical angle for upward growth
//     //         float verticalAngle = Random.Range(branchAngleMin, branchAngleMax);

//     //         // Calculate branch direction
//     //         Quaternion rotation = Quaternion.Euler(-verticalAngle, angle, 0);
//     //         Vector3 branchDirection = rotation * Vector3.forward;

//     //         // Calculate branch end position
//     //         float branchLength = baseBranchLength * Random.Range(0.8f, 1.2f);
//     //         Vector3 branchEnd = tree.rootPosition + branchDirection * branchLength;

//     //         // Create the main branch
//     //         GameObject branch = ProceduralMeshGenerator.CreateCurvedBranch(
//     //             tree.rootPosition,
//     //             branchEnd,
//     //             mainBranchThickness,
//     //             mainBranchThickness * 0.5f,
//     //             barkMaterial
//     //         );
//     //         branch.transform.parent = parent;

//     //         BranchInfo branchInfo = new BranchInfo
//     //         {
//     //             startPoint = tree.rootPosition,
//     //             endPoint = branchEnd,
//     //             attachedTags = new List<GameObject>()
//     //         };

//     //         // Store branch info
//     //         tree.branches.Add(branchInfo);
//     //         tree.mainBranches.Add(branchInfo);

//     //         // Add sub-branches
//     //         Debug.Log($"Created {tree.mainBranches.Count} main branches (total branches: {tree.branches.Count})");
//     //         CreateSubBranches(tree, branchEnd, branchDirection, parent);
//     //     }

//     //     // Create foliage
//     //     CreateFoliage(tree, parent);
//     // }

//     void GenerateTreeBranches(TreeStructure tree, Transform parent)
//     {
//         Transform[] spawnPoints = (tree.usageType == "Current Usage") ?
//             currentTreeBranchPoints : futureTreeBranchPoints;

//         if (spawnPoints == null || spawnPoints.Length == 0)
//         {
//             Debug.LogError($"No spawn points assigned for {tree.usageType} tree!");
//             return;
//         }

//         // Create branches at each spawn point
//         foreach (Transform spawnPoint in spawnPoints)
//         {
//             if (spawnPoint == null) continue;

//             Vector3 branchStart = spawnPoint.position;

//             // Calculate direction toward camera
//             Vector3 branchDirection;
//             if (mainCamera != null)
//             {
//                 // Get direction from spawn point to camera
//                 Vector3 toCamera = mainCamera.transform.position - branchStart;
//                 toCamera.y = 0; // Keep it horizontal
//                 branchDirection = toCamera.normalized;

//                 // Add some spread based on tree type
//                 if (tree.usageType == "Current Usage")
//                 {
//                     // Bias slightly to the left
//                     branchDirection = Quaternion.Euler(0, -30f, 0) * branchDirection;
//                 }
//                 else
//                 {
//                     // Bias slightly to the right
//                     branchDirection = Quaternion.Euler(0, 30f, 0) * branchDirection;
//                 }
//             }
//             else
//             {
//                 // Fallback if no camera
//                 branchDirection = (tree.usageType == "Current Usage") ? Vector3.left : Vector3.right;
//             }

//             // Add slight variance
//             float angleVariance = Random.Range(-branchAngleVariance, branchAngleVariance);
//             branchDirection = Quaternion.Euler(0, angleVariance, 0) * branchDirection;

//             // Keep mostly horizontal with slight droop
//             branchDirection.y = Random.Range(-0.1f, 0.05f);
//             branchDirection.Normalize();

//             // Create main horizontal branch
//             float branchLength = horizontalBranchLength * Random.Range(0.9f, 1.1f);
//             Vector3 branchEnd = branchStart + branchDirection * branchLength;

//             GameObject branch = ProceduralMeshGenerator.CreateCurvedBranch(
//                 branchStart,
//                 branchEnd,
//                 mainBranchThickness,
//                 mainBranchThickness * 0.6f,
//                 barkMaterial
//             );
//             branch.transform.parent = parent;

//             BranchInfo branchInfo = new BranchInfo
//             {
//                 startPoint = branchStart,
//                 endPoint = branchEnd,
//                 attachedTags = new List<GameObject>(),
//                 spawnPoint = spawnPoint
//             };

//             tree.branches.Add(branchInfo);
//             tree.mainBranches.Add(branchInfo);

//             // Add smaller sub-branches along the main branch
//             CreateHorizontalSubBranches(tree, branchInfo, branchDirection, parent);
//         }

//         // Only create foliage at branch ends if needed
//         if (foliagePrefab != null)
//         {
//             CreateFoliage(tree, parent);
//         }
//     }

//     // void CreateHorizontalSubBranches(TreeStructure tree, BranchInfo mainBranch, Vector3 mainDirection, Transform parent)
//     // {
//     //     int subBranchCount = Random.Range(2, 4);

//     //     for (int i = 0; i < subBranchCount; i++)
//     //     {
//     //         // Position along main branch
//     //         float t = Random.Range(0.3f, 0.8f);
//     //         Vector3 subBranchStart = Vector3.Lerp(mainBranch.startPoint, mainBranch.endPoint, t);

//     //         // Sub-branch direction - maintain camera-facing tendency
//     //         Vector3 subDirection = mainDirection;

//     //         if (mainCamera != null)
//     //         {
//     //             // Recalculate direction to camera from sub-branch start point
//     //             Vector3 toCamera = mainCamera.transform.position - subBranchStart;
//     //             toCamera.y = 0;
//     //             toCamera.Normalize();

//     //             // Blend with main direction to maintain overall branch flow
//     //             subDirection = Vector3.Lerp(mainDirection, toCamera, 0.3f);
//     //         }

//     //         // Add perpendicular variation
//     //         Vector3 perpendicular = Vector3.Cross(subDirection, Vector3.up);
//     //         subDirection += perpendicular * Random.Range(-0.3f, 0.3f);

//     //         // Slight downward tendency
//     //         subDirection.y = Random.Range(-0.2f, 0f);
//     //         subDirection.Normalize();

//     //         float subBranchLength = horizontalBranchLength * Random.Range(0.3f, 0.6f);
//     //         Vector3 subBranchEnd = subBranchStart + subDirection * subBranchLength;

//     //         GameObject subBranch = ProceduralMeshGenerator.CreateCurvedBranch(
//     //             subBranchStart,
//     //             subBranchEnd,
//     //             mainBranchThickness * 0.4f,
//     //             mainBranchThickness * 0.2f,
//     //             barkMaterial
//     //         );
//     //         subBranch.transform.parent = parent;

//     //         tree.branches.Add(new BranchInfo
//     //         {
//     //             startPoint = subBranchStart,
//     //             endPoint = subBranchEnd,
//     //             attachedTags = new List<GameObject>()
//     //         });
//     //     }
//     // }

//     void CreateHorizontalSubBranches(TreeStructure tree, BranchInfo mainBranch, Vector3 mainDirection, Transform parent)
//     {
//         int subBranchCount = Random.Range(3, 5); // More sub-branches
//         List<float> usedPositions = new List<float>();

//         for (int i = 0; i < subBranchCount; i++)
//         {
//             // Find well-spaced position along main branch
//             float t = 0;
//             int attempts = 0;
//             bool validPosition = false;

//             while (!validPosition && attempts < 15)
//             {
//                 t = Random.Range(0.25f, 0.9f); // Use more of the branch length
//                 validPosition = true;

//                 // Ensure minimum spacing from other sub-branches
//                 foreach (float usedT in usedPositions)
//                 {
//                     if (Mathf.Abs(t - usedT) < subBranchMinSpacing)
//                     {
//                         validPosition = false;
//                         break;
//                     }
//                 }
//                 attempts++;
//             }

//             if (!validPosition) continue;
//             usedPositions.Add(t);

//             Vector3 subBranchStart = Vector3.Lerp(mainBranch.startPoint, mainBranch.endPoint, t);

//             // Enhanced camera-facing direction
//             Vector3 subDirection = mainDirection;

//             if (mainCamera != null)
//             {
//                 Vector3 toCamera = mainCamera.transform.position - subBranchStart;
//                 toCamera.y = 0;
//                 toCamera.Normalize();
//                 subDirection = Vector3.Lerp(mainDirection, toCamera, 0.5f);
//             }

//             // MORE perpendicular variation for spread
//             Vector3 perpendicular = Vector3.Cross(subDirection, Vector3.up);
//             subDirection += perpendicular * Random.Range(-0.5f, 0.5f);

//             // Slight downward angle
//             subDirection.y = Random.Range(-0.15f, 0.05f);
//             subDirection.Normalize();

//             // Varied sub-branch lengths
//             float subBranchLength = horizontalBranchLength * Random.Range(0.4f, 0.7f);
//             Vector3 subBranchEnd = subBranchStart + subDirection * subBranchLength;

//             GameObject subBranch = ProceduralMeshGenerator.CreateCurvedBranch(
//                 subBranchStart,
//                 subBranchEnd,
//                 mainBranchThickness * 0.35f,
//                 mainBranchThickness * 0.18f,
//                 barkMaterial
//             );
//             subBranch.transform.parent = parent;

//             // Store sub-branch for potential tag attachment
//             tree.branches.Add(new BranchInfo
//             {
//                 startPoint = subBranchStart,
//                 endPoint = subBranchEnd,
//                 attachedTags = new List<GameObject>()
//             });
//         }
//     }

//     void CreateMainBranches(TreeStructure tree, Vector3 trunkTop, Transform parent)
//     {
//         int branchCount = 5;
//         float branchSpread = 3f;

//         for (int i = 0; i < branchCount; i++)
//         {
//             float angle = (i / (float)branchCount) * 180f - 90f; // Spread from -90 to +90 degrees
//             float heightVariation = Random.Range(-0.5f, 0.5f);

//             Vector3 branchDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
//             Vector3 branchEnd = trunkTop + branchDirection * branchSpread + Vector3.up * heightVariation;

//             // Create branch
//             GameObject branch = ProceduralMeshGenerator.CreateCurvedBranch(
//                 trunkTop + Vector3.up * heightVariation * 0.5f,
//                 branchEnd,
//                 trunkThickness * 0.4f,
//                 trunkThickness * 0.2f,
//                 barkMaterial
//             );
//             branch.transform.parent = parent;

//             // Store branch info for tag placement
//             tree.branches.Add(new BranchInfo
//             {
//                 startPoint = trunkTop,
//                 endPoint = branchEnd,
//                 attachedTags = new List<GameObject>()
//             });

//             // Add sub-branches
//             CreateSubBranches(tree, branchEnd, branchDirection, parent);
//         }
//     }

//     void CreateSubBranches(TreeStructure tree, Vector3 branchPoint, Vector3 parentDirection, Transform parent)
//     {
//         int subBranchCount = Random.Range(2, 4);

//         for (int i = 0; i < subBranchCount; i++)
//         {
//             Vector3 randomDirection = parentDirection + Random.insideUnitSphere * 0.5f;
//             if (alwaysFaceCamera && mainCamera != null)
//             {
//                 Vector3 toCamera = (mainCamera.transform.position - branchPoint).normalized;
//                 randomDirection = Vector3.Lerp(randomDirection, toCamera, 0.3f);
//             }
//             randomDirection.y = Mathf.Abs(randomDirection.y); // Ensure upward growth
//             randomDirection.Normalize();

//             float subBranchLength = Random.Range(1f, 2f);
//             Vector3 subBranchEnd = branchPoint + randomDirection * subBranchLength;

//             GameObject subBranch = ProceduralMeshGenerator.CreateCurvedBranch(
//                 branchPoint,
//                 subBranchEnd,
//                 trunkThickness * 0.2f,
//                 trunkThickness * 0.1f,
//                 barkMaterial
//             );
//             subBranch.transform.parent = parent;

//             // Store sub-branch info
//             tree.branches.Add(new BranchInfo
//             {
//                 startPoint = branchPoint,
//                 endPoint = subBranchEnd,
//                 attachedTags = new List<GameObject>()
//             });
//         }
//     }

//     void CreateFoliage(TreeStructure tree, Transform parent)
//     {
//         if (foliagePrefab == null)
//         {
//             Debug.LogWarning("Foliage prefab not assigned! Please assign a foliage prefab.");
//             return;
//         }

//         foreach (var branch in tree.branches)
//         {
//             // Instantiate foliage prefab at branch end
//             GameObject foliage = Instantiate(foliagePrefab, branch.endPoint, Quaternion.identity);
//             foliage.transform.parent = parent;

//             // Apply random rotation for variety
//             foliage.transform.rotation = Random.rotation;

//             // Apply scale with some variation
//             float scaleVariation = Random.Range(0.8f, 1.2f);
//             foliage.transform.localScale = Vector3.one * foliageScale * scaleVariation;

//             // Apply color tint if the prefab has a renderer
//             Renderer[] renderers = foliage.GetComponentsInChildren<Renderer>();
//             foreach (var renderer in renderers)
//             {
//                 renderer.material.color = tree.foliageColor;
//                 // Use material property block to tint without creating new materials
//                 // MaterialPropertyBlock props = new MaterialPropertyBlock();
//                 // props.SetColor("_Color", tree.foliageColor);
//                 // props.SetColor("_BaseColor", tree.foliageColor); // For URP
//                 // renderer.SetPropertyBlock(props);
//             }

//             // Add slight position randomness
//             foliage.transform.position += Random.insideUnitSphere * 0.2f;
//         }
//     }

//     // public void AddSubmissionTag(string content, string usageType, string department, string category)
//     // {
//     //     TreeStructure targetTree;
//     //     Color tagColor;
//     //     Transform parentTransform;

//     //     // Determine which tree based on usage type
//     //     // if (usageType.ToLower().Contains("current"))
//     //     // {
//     //     //     targetTree = currentUsageTree;
//     //     //     tagColor = currentUsageTagColor;
//     //     //     parentTransform = currentUsageTreeRoot;
//     //     // }
//     //     // else // Future usage
//     //     // {
//     //     //     targetTree = futureUsageTree;
//     //     //     tagColor = futureUsageTagColor;
//     //     //     parentTransform = futureUsageTreeRoot;
//     //     // }

//     //     if (usageType.ToLower().Contains("current"))
//     //     {
//     //         targetTree = currentUsageTree;
//     //         tagColor = currentUsageTagColor;
//     //         parentTransform = currentUsageTreeTrunk.transform; // Changed this line
//     //     }
//     //     else
//     //     {
//     //         targetTree = futureUsageTree;
//     //         tagColor = futureUsageTagColor;
//     //         parentTransform = futureUsageTreeTrunk.transform; // Changed this line
//     //     }

//     //     // Check if we need to grow new branches
//     //     int totalCapacity = targetTree.branches.Count * maxTagsPerBranch;
//     //     int totalTags = 0;
//     //     foreach (var branch in targetTree.branches)
//     //     {
//     //         totalTags += branch.attachedTags.Count;
//     //     }

//     //     // If tree is getting full, grow new branches
//     //     if (totalTags >= totalCapacity * 0.8f) // 80% full
//     //     {
//     //         Debug.Log($"Tree {targetTree.usageType} is getting full, growing new branch");
//     //         GrowNewBranch(targetTree, parentTransform);
//     //     }

//     //     // Find available branch
//     //     BranchInfo availableBranch = FindAvailableBranch(targetTree);
//     //     if (availableBranch != null)
//     //     {
//     //         GameObject tagObject = Instantiate(tagPrefab);
//     //         tagObject.transform.parent = parentTransform;

//     //         ModernTag tagComponent = tagObject.GetComponent<ModernTag>();
//     //         if (tagComponent == null)
//     //             tagComponent = tagObject.AddComponent<ModernTag>();

//     //         // Position tag below branch
//     //         tagHangDistance = Random.Range(2, 5);
//     //         Vector3 hangPoint = availableBranch.endPoint + Vector3.down * (tagHangDistance + availableBranch.attachedTags.Count * 0.8f);

//     //         tagComponent.Initialize(content, department, category, hangPoint, tagColor, availableBranch.endPoint);

//     //         availableBranch.attachedTags.Add(tagObject);

//     //         Debug.Log($"Added tag to branch. Branch now has {availableBranch.attachedTags.Count} tags");
//     //     }
//     //     else
//     //     {
//     //         Debug.LogError("No branch available for tag!");
//     //     }
//     // }

//     public void AddSubmissionTag(string content, string usageType, string department, string category)
//     {
//         TreeStructure targetTree;
//         Color tagColor;
//         Transform parentTransform;

//         if (usageType.ToLower().Contains("current"))
//         {
//             targetTree = currentUsageTree;
//             tagColor = currentUsageTagColor;
//             parentTransform = currentUsageTreeTrunk.transform;
//         }
//         else
//         {
//             targetTree = futureUsageTree;
//             tagColor = futureUsageTagColor;
//             parentTransform = futureUsageTreeTrunk.transform;
//         }

//         // Check if we need to grow new branches (more aggressive)
//         int totalCapacity = targetTree.mainBranches.Count * maxTagsPerBranch;
//         int totalTags = 0;
//         foreach (var branch in targetTree.mainBranches)
//         {
//             totalTags += branch.attachedTags.Count;
//         }

//         // Grow new branches at 60% capacity
//         if (totalTags >= totalCapacity * 0.6f)
//         {
//             Debug.Log($"Tree {targetTree.usageType} is at {totalTags}/{totalCapacity} capacity, growing new branch");
//             GrowNewBranch(targetTree, parentTransform);
//         }

//         // Find available branch
//         BranchInfo availableBranch = FindAvailableBranch(targetTree);
//         if (availableBranch != null)
//         {
//             GameObject tagObject = Instantiate(tagPrefab);
//             tagObject.transform.parent = parentTransform;

//             ModernTag tagComponent = tagObject.GetComponent<ModernTag>();
//             if (tagComponent == null)
//                 tagComponent = tagObject.AddComponent<ModernTag>();

//             // IMPROVED SPACING - vertical stagger with variety
//             int tagIndex = availableBranch.attachedTags.Count;

//             // First tag hangs lower, subsequent tags spread out more
//             float baseHangDistance = Random.Range(2.5f, 3.5f);
//             float verticalSpacing = Random.Range(minTagVerticalSpacing, maxTagVerticalSpacing);
//             float totalVerticalOffset = baseHangDistance + (tagIndex * verticalSpacing);

//             // Add horizontal offset perpendicular to branch direction
//             Vector3 branchDirection = (availableBranch.endPoint - availableBranch.startPoint).normalized;
//             Vector3 perpendicular = Vector3.Cross(branchDirection, Vector3.up).normalized;
//             Vector3 horizontalOffset = perpendicular * Random.Range(-tagHorizontalOffset, tagHorizontalOffset);

//             // Alternate sides for visual balance
//             if (tagIndex % 2 == 0)
//                 horizontalOffset = -horizontalOffset;

//             // Final hang point with spacing
//             Vector3 hangPoint = availableBranch.endPoint +
//                                Vector3.down * totalVerticalOffset +
//                                horizontalOffset;

//             tagComponent.Initialize(content, department, category, hangPoint, tagColor, availableBranch.endPoint);

//             availableBranch.attachedTags.Add(tagObject);

//             Debug.Log($"Added tag {tagIndex + 1}/{maxTagsPerBranch} to branch. Vertical offset: {totalVerticalOffset:F2}");
//         }
//         else
//         {
//             Debug.LogWarning("No available branch found! Consider adding more spawn points.");
//         }
//     }

//     // BranchInfo FindAvailableBranch(TreeStructure tree)
//     // {
//     //     // Find branch with least tags
//     //     BranchInfo bestBranch = null;
//     //     int minTags = maxTagsPerBranch;

//     //     foreach (var branch in tree.branches)
//     //     {
//     //         if (branch.attachedTags.Count < minTags)
//     //         {
//     //             minTags = branch.attachedTags.Count;
//     //             bestBranch = branch;
//     //         }
//     //     }

//     //     return bestBranch;
//     // }
//     BranchInfo FindAvailableBranch(TreeStructure tree)
//     {
//         Debug.Log($"=== Finding available branch for {tree.usageType} ===");
//         Debug.Log($"Total branches: {tree.branches.Count}");
//         Debug.Log($"Max tags per branch: {maxTagsPerBranch}");

//         // Log all branches and their tag counts
//         for (int i = 0; i < tree.branches.Count; i++)
//         {
//             Debug.Log($"Branch {i}: {tree.branches[i].attachedTags.Count} tags");
//         }

//         // Find branch with least tags that hasn't reached the limit
//         BranchInfo bestBranch = null;
//         int minTags = int.MaxValue;

//         foreach (var branch in tree.mainBranches)
//         {
//             if (branch.attachedTags.Count < maxTagsPerBranch)
//             {
//                 if (branch.attachedTags.Count < minTags)
//                 {
//                     minTags = branch.attachedTags.Count;
//                     bestBranch = branch;
//                 }
//             }
//         }

//         if (bestBranch != null)
//         {
//             Debug.Log($"Selected branch with {bestBranch.attachedTags.Count} tags");
//         }
//         else
//         {
//             Debug.LogWarning("No available branch found under limit!");

//             // If all branches are at capacity, find the one with least tags
//             foreach (var branch in tree.branches)
//             {
//                 if (bestBranch == null || branch.attachedTags.Count < bestBranch.attachedTags.Count)
//                 {
//                     bestBranch = branch;
//                 }
//             }
//         }

//         return bestBranch;
//     }

//     // void GrowNewBranch(TreeStructure tree, Transform parent)
//     // {
//     //     if (tree.mainBranches.Count == 0) return;

//     //     // Pick a main branch to extend from
//     //     BranchInfo parentBranch = tree.mainBranches[Random.Range(0, tree.mainBranches.Count)];

//     //     // Grow from the end of the branch
//     //     Vector3 growthDirection = (parentBranch.endPoint - parentBranch.startPoint).normalized;

//     //     // Continue in same general direction with slight variation
//     //     float angleVariance = Random.Range(-branchAngleVariance, branchAngleVariance);
//     //     growthDirection = Quaternion.Euler(0, angleVariance, 0) * growthDirection;
//     //     growthDirection.y = Random.Range(-0.1f, 0.05f); // Slight droop
//     //     growthDirection.Normalize();

//     //     float newBranchLength = horizontalBranchLength * Random.Range(0.6f, 0.8f);
//     //     Vector3 newBranchEnd = parentBranch.endPoint + growthDirection * newBranchLength;

//     //     GameObject newBranch = ProceduralMeshGenerator.CreateCurvedBranch(
//     //         parentBranch.endPoint,
//     //         newBranchEnd,
//     //         mainBranchThickness * 0.3f,
//     //         mainBranchThickness * 0.15f,
//     //         barkMaterial
//     //     );
//     //     newBranch.transform.parent = parent;

//     //     tree.branches.Add(new BranchInfo
//     //     {
//     //         startPoint = parentBranch.endPoint,
//     //         endPoint = newBranchEnd,
//     //         attachedTags = new List<GameObject>()
//     //     });
//     // }

//     void GrowNewBranch(TreeStructure tree, Transform parent)
//     {
//         if (tree.mainBranches.Count == 0) return;

//         // Pick a main branch to extend
//         BranchInfo parentBranch = tree.mainBranches[Random.Range(0, tree.mainBranches.Count)];

//         // Continue in same direction with variation
//         Vector3 growthDirection = (parentBranch.endPoint - parentBranch.startPoint).normalized;

//         // Add MORE spread for new growth
//         float angleVariance = Random.Range(-branchAngleVariance * 1.5f, branchAngleVariance * 1.5f);
//         growthDirection = Quaternion.Euler(0, angleVariance, 0) * growthDirection;

//         // Slight upward tendency for new growth
//         growthDirection.y = Random.Range(-0.05f, 0.15f);
//         growthDirection.Normalize();

//         // New branches are slightly shorter
//         float newBranchLength = horizontalBranchLength * Random.Range(0.7f, 0.9f);
//         Vector3 newBranchEnd = parentBranch.endPoint + growthDirection * newBranchLength;

//         GameObject newBranch = ProceduralMeshGenerator.CreateCurvedBranch(
//             parentBranch.endPoint,
//             newBranchEnd,
//             mainBranchThickness * 0.35f,
//             mainBranchThickness * 0.18f,
//             barkMaterial
//         );
//         newBranch.transform.parent = parent;

//         BranchInfo newBranchInfo = new BranchInfo
//         {
//             startPoint = parentBranch.endPoint,
//             endPoint = newBranchEnd,
//             attachedTags = new List<GameObject>()
//         };

//         tree.branches.Add(newBranchInfo);
//         tree.mainBranches.Add(newBranchInfo); // Important: also add to main branches

//         // Add sub-branches to new growth
//         CreateHorizontalSubBranches(tree, newBranchInfo, growthDirection, parent);

//         Debug.Log($"Grew new branch for {tree.usageType}. Total main branches: {tree.mainBranches.Count}");
//     }
// }

// [System.Serializable]
// public class TreeStructure
// {
//     public Vector3 rootPosition;
//     public Color foliageColor;
//     public string usageType;
//     public List<BranchInfo> branches = new List<BranchInfo>();
//     public List<BranchInfo> mainBranches = new List<BranchInfo>();

//     public TreeStructure(Vector3 root, Color color, string usage)
//     {
//         rootPosition = root;
//         foliageColor = color;
//         usageType = usage;
//     }
// }

// [System.Serializable]
// public class BranchInfo
// {
//     public Vector3 startPoint;
//     public Vector3 endPoint;
//     public List<GameObject> attachedTags;
//     public Transform spawnPoint; // Add this to track original spawn point
// }

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

public class DualTreeManager : MonoBehaviour
{
    [Header("Tree Configuration")]
    public float treeSpacing = 15f; // INCREASED for better separation
    public float treeHeight = 4f;
    public float trunkThickness = 0.6f;
    public GameObject exitPanel;

    [Header("Wave Animation")]
    [Tooltip("Enable gentle up/down wave motion")]
    public bool enableWaveMotion = true;

    [Tooltip("How high the trees move up and down")]
    [Range(0f, 2f)]
    public float waveAmplitude = 0.3f;

    [Tooltip("How fast the wave oscillates")]
    [Range(0f, 2f)]
    public float waveSpeed = 0.5f;

    [Tooltip("Phase offset between left and right tree (creates rolling wave effect)")]
    [Range(0f, 6.28f)]
    public float wavePhaseOffset = 1.57f; // π/2 radians (90 degrees)

    private Vector3 currentTreeOriginalPosition;
    private Vector3 futureTreeOriginalPosition;

    [Header("Planar Growth Constraints")]
    [Tooltip("Maximum allowed Z-axis deviation (depth). Keep very small for flat growth.")]
    [Range(0f, 1f)]
    public float maxDepthDeviation = 0.05f;

    [Tooltip("Allow slight upward branch growth for visual interest")]
    public bool allowVerticalGrowth = true;

    [Range(0f, 0.5f)]
    [Tooltip("How much branches can angle upward (0 = purely horizontal, 0.5 = 45° max)")]
    public float maxVerticalAngle = 0.3f;

    [Header("Current Usage Tree (Left/Green)")]
    public Color currentUsageTreeColor = new Color(0.2f, 0.8f, 0.2f);
    public Color currentUsageTagColor = new Color(0.4f, 1f, 0.4f);

    [Header("Future Usage Tree (Right/Purple)")]
    public Color futureUsageTreeColor = new Color(0.6f, 0.2f, 0.8f);
    public Color futureUsageTagColor = new Color(0.8f, 0.6f, 1f);

    [Header("Circular Connector")]
    public float circleRadius = 2f;
    public int circleSegments = 32;
    public Material barkMaterial;

    [Header("Tag Configuration")]
    public GameObject tagPrefab;
    public int maxTagsPerBranch = 3;
    public float minTagVerticalSpacing = 2f;
    public float maxTagVerticalSpacing = 3f;
    public float tagHorizontalOffset = 0.8f;

    [Header("Branch Configuration")]
    public int mainBranchCount = 5;
    public float mainBranchThickness = 0.4f;
    public float branchAngleMin = 20f;
    public float branchAngleMax = 60f;
    public float baseBranchLength = 3f;
    public float subBranchMinSpacing = 0.5f;

    [Header("Foliage Configuration")]
    public GameObject foliagePrefab;
    public float foliageScale = 1f;
    [Tooltip("How far from branch end to place foliage (0 = at end, 1 = at start)")]
    public float foliageOffsetFromEnd = 0.15f;
    public int foliageClustersPerBranch = 2; // Multiple clusters per branch
    public float foliageClusterSpacing = 0.3f; // Distance between clusters along branch

    [Header("Tree Trunks")]
    public GameObject currentUsageTreeTrunk;
    public GameObject futureUsageTreeTrunk;

    [Header("Branch Spawn Configuration")]
    [Tooltip("Manually placed empty GameObjects as branch spawn points")]
    public Transform[] currentTreeBranchPoints;
    public Transform[] futureTreeBranchPoints;

    [Header("Branch Growth")]
    public float horizontalBranchLength = 7f;
    public float branchAngleVariance = 30f;
    [Range(0f, 1f)]
    [Tooltip("How much branches prefer to grow away from center (0 = no bias, 1 = always away)")]
    public float outwardGrowthBias = 0.7f;
    public bool growHorizontally = true;

    [Header("Collision Avoidance")]
    [Tooltip("Minimum distance between tree centers before branches adjust")]
    public float minTreeSeparation = 12f;
    [Tooltip("Safety margin to prevent branch collision")]
    public float branchCollisionMargin = 2f;

    [Header("Events")]
    public UnityEvent<Transform> onTreeMoved = new UnityEvent<Transform>();

    [Header("Camera Reference")]
    public Camera mainCamera;
    public bool alwaysFaceCamera = true;

    [Header("Realistic Growth Accents")]
    [Range(0, 5)]
    [Tooltip("Number of branches growing inward (toward the other tree)")]
    public int innerBranchCount = 2;

    [Range(0, 10)]
    [Tooltip("Number of foliage clusters on top of canopy")]
    public int topFoliageCount = 5;

    [Tooltip("Height offset for top foliage above highest branch")]
    public float topFoliageHeightOffset = 1.5f;
    public float tagForwardOffsetMin = 2.0f;
    public float tagForwardOffsetMax = 3.0f;

    private TreeStructure currentUsageTree;
    private TreeStructure futureUsageTree;
    private Vector3 midPoint; // Center point between trees

    // void Start()
    // {
    //     if (mainCamera == null)
    //         mainCamera = Camera.main;

    //     CreateTrees();

    //     var strapiConnector = GetComponent<StrapiConnector>();
    //     if (strapiConnector != null)
    //         strapiConnector.Initialize();
    // }

    void Start()
    {
        exitPanel.SetActive(false);
        if (mainCamera == null)
            mainCamera = Camera.main;

        CreateTrees();

        // ✨ NEW: Store original positions for wave animation
        if (currentUsageTreeTrunk != null)
        {
            currentTreeOriginalPosition = currentUsageTreeTrunk.transform.localPosition;
        }

        if (futureUsageTreeTrunk != null)
        {
            futureTreeOriginalPosition = futureUsageTreeTrunk.transform.localPosition;
        }

        var strapiConnector = GetComponent<StrapiConnector>();
        if (strapiConnector != null)
            strapiConnector.Initialize();
    }

    void CreateTrees()
    {
        // Calculate midpoint for collision avoidance
        if (currentUsageTreeTrunk != null && futureUsageTreeTrunk != null)
        {
            Vector3 currentPos = currentUsageTreeTrunk.transform.position;
            Vector3 futurePos = futureUsageTreeTrunk.transform.position;
            midPoint = (currentPos + futurePos) / 2f;

            // Check if trees are too close
            float distance = Vector3.Distance(currentPos, futurePos);
            if (distance < minTreeSeparation)
            {
                Debug.LogWarning($"Trees are too close ({distance}m). Adjusting spacing to {treeSpacing}m");
                currentUsageTreeTrunk.transform.localPosition = new Vector3(-treeSpacing / 2, 0, 0);
                futureUsageTreeTrunk.transform.localPosition = new Vector3(treeSpacing / 2, 0, 0);
                midPoint = transform.position;
            }
        }

        // Create Current Usage Tree
        if (currentUsageTreeTrunk != null)
        {
            currentUsageTree = new TreeStructure(
                currentUsageTreeTrunk.transform.position,
                currentUsageTreeColor,
                "Current Usage"
            );
            GenerateTreeBranches(currentUsageTree, currentUsageTreeTrunk.transform, true); // true = left tree
        }

        // Create Future Usage Tree
        if (futureUsageTreeTrunk != null)
        {
            futureUsageTree = new TreeStructure(
                futureUsageTreeTrunk.transform.position,
                futureUsageTreeColor,
                "Future Usage"
            );
            GenerateTreeBranches(futureUsageTree, futureUsageTreeTrunk.transform, false); // false = right tree
        }
    }

    void CreateInnerBranches(TreeStructure tree, Transform parent, bool isLeftTree)
    {
        if (tree.mainBranches.Count == 0) return;

        // Direction toward the OTHER tree (inward)
        Vector3 inwardDirection = isLeftTree ? Vector3.right : Vector3.left;

        for (int i = 0; i < innerBranchCount; i++)
        {
            // Pick a random main branch to grow from
            BranchInfo parentBranch = tree.mainBranches[Random.Range(0, tree.mainBranches.Count)];

            // Start point along the parent branch (not at the very end)
            float t = Random.Range(0.3f, 0.7f);
            Vector3 branchStart = Vector3.Lerp(parentBranch.startPoint, parentBranch.endPoint, t);

            // Direction: slightly inward with upward curve
            Vector3 direction = inwardDirection;
            direction.y = Random.Range(0.1f, 0.25f); // Slight upward

            // Add some angular variance
            float angleVariance = Random.Range(-15f, 15f);
            Quaternion rotation = Quaternion.Euler(0, 0, angleVariance);
            direction = rotation * direction;

            // LOCK Z
            direction.z = 0;
            direction.Normalize();

            // Shorter than main branches
            float branchLength = horizontalBranchLength * Random.Range(0.4f, 0.6f);
            Vector3 branchEnd = branchStart + direction * branchLength;
            branchEnd.z = branchStart.z; // Enforce planar

            // Create the inner branch
            GameObject innerBranch = ProceduralMeshGenerator.CreateCurvedBranch(
                branchStart,
                branchEnd,
                mainBranchThickness * 0.3f,
                mainBranchThickness * 0.15f,
                barkMaterial
            );
            innerBranch.transform.parent = parent;

            BranchInfo innerBranchInfo = new BranchInfo
            {
                startPoint = branchStart,
                endPoint = branchEnd,
                attachedTags = new List<GameObject>()
            };

            tree.branches.Add(innerBranchInfo);

            // Add foliage to inner branch
            CreateFoliageClusters(tree, innerBranchInfo, parent);
        }

        Debug.Log($"🌿 Added {innerBranchCount} inner branches for {tree.usageType}");
    }

    void CreateTopFoliage(TreeStructure tree, Transform parent)
    {
        if (foliagePrefab == null || tree.mainBranches.Count == 0) return;

        // Find the highest point in the canopy
        float maxHeight = float.MinValue;
        Vector3 centerPosition = Vector3.zero;

        foreach (var branch in tree.mainBranches)
        {
            float branchMaxHeight = Mathf.Max(branch.startPoint.y, branch.endPoint.y);
            if (branchMaxHeight > maxHeight)
            {
                maxHeight = branchMaxHeight;
                centerPosition = (branch.startPoint + branch.endPoint) / 2f;
            }
        }

        // Create foliage clusters on top
        for (int i = 0; i < topFoliageCount; i++)
        {
            // Random position around the top of the canopy
            Vector3 topPosition = new Vector3(
                centerPosition.x + Random.Range(-2f, 2f),
                maxHeight + topFoliageHeightOffset + Random.Range(-0.3f, 0.5f),
                centerPosition.z + Random.Range(-0.2f, 0.2f) // Minimal Z variance
            );

            // Instantiate top foliage
            GameObject topFoliage = Instantiate(foliagePrefab, topPosition, Quaternion.identity);
            topFoliage.transform.parent = parent;

            // Random rotation
            topFoliage.transform.rotation = Random.rotation;

            // Slightly larger scale for prominence
            float scaleVariation = Random.Range(1.0f, 1.5f);
            topFoliage.transform.localScale = Vector3.one * foliageScale * scaleVariation;

            // Apply tree color
            Renderer[] renderers = topFoliage.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                Material foliageMat = new Material(renderer.material);
                foliageMat.color = tree.foliageColor;
                renderer.material = foliageMat;
            }
        }

        Debug.Log($"🌸 Added {topFoliageCount} top foliage clusters for {tree.usageType}");
    }

    // void GenerateTreeBranches(TreeStructure tree, Transform parent, bool isLeftTree)
    // {
    //     Transform[] spawnPoints = isLeftTree ? currentTreeBranchPoints : futureTreeBranchPoints;

    //     if (spawnPoints == null || spawnPoints.Length == 0)
    //     {
    //         Debug.LogError($"No spawn points assigned for {tree.usageType} tree!");
    //         return;
    //     }

    //     Vector3 treeCenter = parent.position;
    //     Vector3 awayFromCenter = isLeftTree ? Vector3.left : Vector3.right;

    //     // Create branches with directional bias
    //     foreach (Transform spawnPoint in spawnPoints)
    //     {
    //         if (spawnPoint == null) continue;

    //         Vector3 branchStart = spawnPoint.position;
    //         Vector3 branchDirection = CalculateSmartBranchDirection(
    //             branchStart,
    //             treeCenter,
    //             awayFromCenter,
    //             isLeftTree
    //         );

    //         float branchLength = horizontalBranchLength * Random.Range(0.9f, 1.2f);
    //         Vector3 branchEnd = branchStart + branchDirection * branchLength;

    //         // Create main branch
    //         GameObject branch = ProceduralMeshGenerator.CreateCurvedBranch(
    //             branchStart,
    //             branchEnd,
    //             mainBranchThickness,
    //             mainBranchThickness * 0.6f,
    //             barkMaterial
    //         );
    //         branch.transform.parent = parent;

    //         BranchInfo branchInfo = new BranchInfo
    //         {
    //             startPoint = branchStart,
    //             endPoint = branchEnd,
    //             attachedTags = new List<GameObject>(),
    //             spawnPoint = spawnPoint
    //         };

    //         tree.branches.Add(branchInfo);
    //         tree.mainBranches.Add(branchInfo);

    //         // Add sub-branches
    //         CreateEnhancedSubBranches(tree, branchInfo, branchDirection, parent, isLeftTree);

    //         // Add foliage clusters along the branch
    //         CreateFoliageClusters(tree, branchInfo, parent);
    //     }
    // }

    void GenerateTreeBranches(TreeStructure tree, Transform parent, bool isLeftTree)
    {
        Transform[] spawnPoints = isLeftTree ? currentTreeBranchPoints : futureTreeBranchPoints;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError($"No spawn points assigned for {tree.usageType} tree!");
            return;
        }

        Vector3 treeCenter = parent.position;
        Vector3 awayFromCenter = isLeftTree ? Vector3.left : Vector3.right;

        // Create main branches (existing code)
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;

            Vector3 branchStart = spawnPoint.position;

            Vector3 branchDirection = CalculateSmartBranchDirection(
                branchStart,
                treeCenter,
                awayFromCenter,
                isLeftTree
            );

            branchDirection.z = Mathf.Clamp(branchDirection.z, -0.05f, 0.05f);
            branchDirection.Normalize();

            float branchLength = horizontalBranchLength * Random.Range(0.95f, 1.15f);
            Vector3 branchEnd = branchStart + branchDirection * branchLength;

            float zDisplacement = Mathf.Abs(branchEnd.z - branchStart.z);
            if (zDisplacement > 0.5f)
            {
                Debug.LogWarning($"⚠️ Branch has Z displacement of {zDisplacement:F2}. Correcting...");
                branchEnd.z = branchStart.z + Mathf.Clamp(branchEnd.z - branchStart.z, -0.3f, 0.3f);
            }

            GameObject branch = ProceduralMeshGenerator.CreateCurvedBranch(
                branchStart,
                branchEnd,
                mainBranchThickness,
                mainBranchThickness * 0.6f,
                barkMaterial
            );
            branch.transform.parent = parent;

            BranchInfo branchInfo = new BranchInfo
            {
                startPoint = branchStart,
                endPoint = branchEnd,
                attachedTags = new List<GameObject>(),
                spawnPoint = spawnPoint
            };

            tree.branches.Add(branchInfo);
            tree.mainBranches.Add(branchInfo);

            CreateEnhancedSubBranches(tree, branchInfo, branchDirection, parent, isLeftTree);
            CreateFoliageClusters(tree, branchInfo, parent);
        }

        // ✨ NEW: Add inner branches and top foliage
        CreateInnerBranches(tree, parent, isLeftTree);
        CreateTopFoliage(tree, parent);

        Debug.Log($"✅ Generated {tree.mainBranches.Count} main branches for {tree.usageType} (planar growth)");
    }

    // void GenerateTreeBranches(TreeStructure tree, Transform parent, bool isLeftTree)
    // {
    //     Transform[] spawnPoints = isLeftTree ? currentTreeBranchPoints : futureTreeBranchPoints;

    //     if (spawnPoints == null || spawnPoints.Length == 0)
    //     {
    //         Debug.LogError($"No spawn points assigned for {tree.usageType} tree!");
    //         return;
    //     }

    //     Vector3 treeCenter = parent.position;
    //     Vector3 awayFromCenter = isLeftTree ? Vector3.left : Vector3.right;

    //     // Create branches with strict planar constraint
    //     foreach (Transform spawnPoint in spawnPoints)
    //     {
    //         if (spawnPoint == null) continue;

    //         Vector3 branchStart = spawnPoint.position;

    //         // Calculate PLANAR direction (XY plane only)
    //         Vector3 branchDirection = CalculateSmartBranchDirection(
    //             branchStart,
    //             treeCenter,
    //             awayFromCenter,
    //             isLeftTree
    //         );

    //         // DOUBLE CHECK: Ensure minimal Z component
    //         branchDirection.z = Mathf.Clamp(branchDirection.z, -0.05f, 0.05f);
    //         branchDirection.Normalize();

    //         float branchLength = horizontalBranchLength * Random.Range(0.95f, 1.15f);
    //         Vector3 branchEnd = branchStart + branchDirection * branchLength;

    //         // VERIFY: Log if Z displacement is too large
    //         float zDisplacement = Mathf.Abs(branchEnd.z - branchStart.z);
    //         if (zDisplacement > 0.5f)
    //         {
    //             Debug.LogWarning($"⚠️ Branch has Z displacement of {zDisplacement:F2}. Correcting...");
    //             branchEnd.z = branchStart.z + Mathf.Clamp(branchEnd.z - branchStart.z, -0.3f, 0.3f);
    //         }

    //         // Create main branch
    //         GameObject branch = ProceduralMeshGenerator.CreateCurvedBranch(
    //             branchStart,
    //             branchEnd,
    //             mainBranchThickness,
    //             mainBranchThickness * 0.6f,
    //             barkMaterial
    //         );
    //         branch.transform.parent = parent;

    //         BranchInfo branchInfo = new BranchInfo
    //         {
    //             startPoint = branchStart,
    //             endPoint = branchEnd,
    //             attachedTags = new List<GameObject>(),
    //             spawnPoint = spawnPoint
    //         };

    //         tree.branches.Add(branchInfo);
    //         tree.mainBranches.Add(branchInfo);

    //         // Add sub-branches with planar constraint
    //         CreateEnhancedSubBranches(tree, branchInfo, branchDirection, parent, isLeftTree);

    //         // Add foliage clusters
    //         CreateFoliageClusters(tree, branchInfo, parent);
    //     }

    //     Debug.Log($"✅ Generated {tree.mainBranches.Count} main branches for {tree.usageType} (planar growth)");
    // }

    // Vector3 CalculateSmartBranchDirection(Vector3 branchStart, Vector3 treeCenter, Vector3 awayDirection, bool isLeftTree)
    // {
    //     Vector3 direction = Vector3.zero;

    //     // 1. Base direction away from center
    //     Vector3 baseDirection = awayDirection;

    //     // 2. Camera facing component
    //     if (mainCamera != null && alwaysFaceCamera)
    //     {
    //         Vector3 toCamera = mainCamera.transform.position - branchStart;
    //         toCamera.y = 0;
    //         toCamera.Normalize();

    //         // Blend with camera direction (less influence than away direction)
    //         baseDirection = Vector3.Lerp(baseDirection, toCamera, 0.3f);
    //     }

    //     // 3. Apply outward bias
    //     Vector3 fromTreeCenter = (branchStart - treeCenter).normalized;
    //     fromTreeCenter.y = 0;
    //     direction = Vector3.Lerp(baseDirection, fromTreeCenter, outwardGrowthBias);

    //     // 4. Add angular variance for natural look
    //     float angleVariance = Random.Range(-branchAngleVariance * 0.5f, branchAngleVariance);
    //     direction = Quaternion.Euler(0, angleVariance, 0) * direction;

    //     // 5. Check for potential collision with opposite tree
    //     Vector3 oppositeTreePos = isLeftTree ? futureUsageTreeTrunk.transform.position : currentUsageTreeTrunk.transform.position;
    //     Vector3 toOppositeTree = (oppositeTreePos - branchStart).normalized;

    //     // If branch is aiming toward opposite tree, redirect it
    //     if (Vector3.Dot(direction, toOppositeTree) > 0.3f)
    //     {
    //         // Redirect more strongly away
    //         direction = Vector3.Lerp(direction, -toOppositeTree, 0.6f);
    //         Debug.Log($"Redirected branch away from opposite tree");
    //     }

    //     // 6. Slight upward curve for aesthetic
    //     direction.y = Random.Range(-0.05f, 0.15f);
    //     direction.Normalize();

    //     return direction;
    // }

    // Vector3 CalculateSmartBranchDirection(Vector3 branchStart, Vector3 treeCenter, Vector3 awayDirection, bool isLeftTree)
    // {
    //     Vector3 direction = Vector3.zero;

    //     // 1. PRIMARY: Strong horizontal direction (X-axis only)
    //     Vector3 baseDirection = awayDirection; // Pure left or right

    //     // 2. CONSTRAINT: Lock to XY plane (no Z-axis depth)
    //     baseDirection.z = 0; // CRITICAL: No forward/backward movement

    //     // 3. Add controlled angular variance IN THE XY PLANE ONLY
    //     // Only rotate around Z-axis to maintain planar growth
    //     float angleVariance = Random.Range(-branchAngleVariance * 0.3f, branchAngleVariance * 0.3f);
    //     // Apply rotation around Z-axis (which affects X and Y, not depth)
    //     Quaternion planarRotation = Quaternion.Euler(0, 0, angleVariance); // Changed from (0, angleVariance, 0)
    //     direction = planarRotation * baseDirection;

    //     // 4. Add vertical component (Y-axis) for natural upward growth
    //     float verticalBias = Random.Range(0.1f, 0.3f); // Slight upward tendency
    //     direction.y += verticalBias;

    //     // 5. FORCE Z to zero to ensure planar growth
    //     direction.z = 0;
    //     direction.Normalize();

    //     // 6. Optional: Very slight Z offset for visual depth (minimal)
    //     // Only add tiny Z variation for the entire branch, not per segment
    //     float minimalDepthVariation = Random.Range(-0.05f, 0.05f);
    //     direction.z = minimalDepthVariation;

    //     direction.Normalize();

    //     Debug.Log($"Branch direction - X: {direction.x:F2}, Y: {direction.y:F2}, Z: {direction.z:F3} (should be ~0)");

    //     return direction;
    // }

    // Vector3 CalculateSmartBranchDirection(Vector3 branchStart, Vector3 treeCenter, Vector3 awayDirection, bool isLeftTree)
    // {
    //     // PURE HORIZONTAL GROWTH - Nearly flat with slight upward curve
    //     Vector3 direction = awayDirection; // Pure left or right

    //     // Very slight upward angle (5-10° max for aesthetic curve)
    //     direction.y = Random.Range(0.05f, 0.15f);

    //     // Add minimal angular variance in XY plane
    //     float angleVariance = Random.Range(-8f, 8f);
    //     Quaternion planarRotation = Quaternion.Euler(0, 0, angleVariance);
    //     direction = planarRotation * direction;

    //     // LOCK Z-axis completely
    //     direction.z = 0;
    //     direction.Normalize();

    //     return direction;
    // }

    Vector3 CalculateSmartBranchDirection(Vector3 branchStart, Vector3 treeCenter, Vector3 awayDirection, bool isLeftTree)
    {
        // BALANCED HORIZONTAL GROWTH

        // 70% chance to grow outward, 30% chance to grow in other directions
        Vector3 direction;
        float randomChoice = Random.value;

        if (randomChoice < 0.7f)
        {
            // Grow outward (away from center)
            direction = awayDirection;
        }
        else if (randomChoice < 0.85f)
        {
            // Grow inward (toward center) - creates natural fullness
            direction = -awayDirection;
        }
        else
        {
            // Grow at an angle
            direction = awayDirection;
            float angleVariance = Random.Range(-45f, 45f);
            Quaternion rotation = Quaternion.Euler(0, 0, angleVariance);
            direction = rotation * direction;
        }

        // Slight upward angle for natural curve
        direction.y = Random.Range(0.05f, 0.15f);

        // LOCK Z-axis completely
        direction.z = 0;
        direction.Normalize();

        return direction;
    }

    // void CreateEnhancedSubBranches(TreeStructure tree, BranchInfo mainBranch, Vector3 mainDirection, Transform parent, bool isLeftTree)
    // {
    //     int subBranchCount = Random.Range(3, 6); // More sub-branches
    //     List<float> usedPositions = new List<float>();

    //     for (int i = 0; i < subBranchCount; i++)
    //     {
    //         // Find well-spaced position along main branch
    //         float t = 0;
    //         int attempts = 0;
    //         bool validPosition = false;

    //         while (!validPosition && attempts < 15)
    //         {
    //             t = Random.Range(0.3f, 0.95f);
    //             validPosition = true;

    //             foreach (float usedT in usedPositions)
    //             {
    //                 if (Mathf.Abs(t - usedT) < subBranchMinSpacing)
    //                 {
    //                     validPosition = false;
    //                     break;
    //                 }
    //             }
    //             attempts++;
    //         }

    //         if (!validPosition) continue;
    //         usedPositions.Add(t);

    //         Vector3 subBranchStart = Vector3.Lerp(mainBranch.startPoint, mainBranch.endPoint, t);

    //         // Sub-branch direction - maintain outward flow
    //         Vector3 subDirection = mainDirection;

    //         // Add perpendicular variation
    //         Vector3 perpendicular = Vector3.Cross(subDirection, Vector3.up);
    //         float perpAmount = Random.Range(-0.6f, 0.6f);

    //         // Bias perpendicular variation outward
    //         if (isLeftTree && perpAmount > 0) perpAmount *= 1.5f; // Enhance leftward sub-branches
    //         if (!isLeftTree && perpAmount < 0) perpAmount *= 1.5f; // Enhance rightward sub-branches

    //         subDirection += perpendicular * perpAmount;

    //         // Slight upward tendency for sub-branches near end
    //         subDirection.y = Mathf.Lerp(-0.1f, 0.2f, t);
    //         subDirection.Normalize();

    //         float subBranchLength = horizontalBranchLength * Random.Range(0.4f, 0.7f);
    //         Vector3 subBranchEnd = subBranchStart + subDirection * subBranchLength;

    //         GameObject subBranch = ProceduralMeshGenerator.CreateCurvedBranch(
    //             subBranchStart,
    //             subBranchEnd,
    //             mainBranchThickness * 0.35f,
    //             mainBranchThickness * 0.18f,
    //             barkMaterial
    //         );
    //         subBranch.transform.parent = parent;

    //         BranchInfo subBranchInfo = new BranchInfo
    //         {
    //             startPoint = subBranchStart,
    //             endPoint = subBranchEnd,
    //             attachedTags = new List<GameObject>()
    //         };

    //         tree.branches.Add(subBranchInfo);

    //         // Add foliage to sub-branches too
    //         CreateFoliageClusters(tree, subBranchInfo, parent);
    //     }
    // }

    void CreateEnhancedSubBranches(TreeStructure tree, BranchInfo mainBranch, Vector3 mainDirection, Transform parent, bool isLeftTree)
    {
        int subBranchCount = Random.Range(5, 8); // MORE sub-branches for density
        List<float> usedPositions = new List<float>();

        for (int i = 0; i < subBranchCount; i++)
        {
            float t = 0;
            int attempts = 0;
            bool validPosition = false;

            while (!validPosition && attempts < 15)
            {
                t = Random.Range(0.3f, 0.95f);
                validPosition = true;

                foreach (float usedT in usedPositions)
                {
                    if (Mathf.Abs(t - usedT) < 0.12f) // Tighter spacing
                    {
                        validPosition = false;
                        break;
                    }
                }
                attempts++;
            }

            if (!validPosition) continue;
            usedPositions.Add(t);

            Vector3 subBranchStart = Vector3.Lerp(mainBranch.startPoint, mainBranch.endPoint, t);

            // Sub-branches grow HORIZONTALLY in same direction
            Vector3 subDirection = mainDirection;

            // Slight upward/downward variation for layering
            subDirection.y += Random.Range(-0.1f, 0.2f); // Mostly flat, slight up

            // Minimal outward/inward spread
            float horizontalSpread = Random.Range(-0.15f, 0.15f);
            subDirection.x += horizontalSpread * (isLeftTree ? -1f : 1f);

            // LOCK Z
            subDirection.z = 0;
            subDirection.Normalize();

            float subBranchLength = horizontalBranchLength * Random.Range(0.5f, 0.8f);
            Vector3 subBranchEnd = subBranchStart + subDirection * subBranchLength;
            subBranchEnd.z = subBranchStart.z; // Enforce Z

            GameObject subBranch = ProceduralMeshGenerator.CreateCurvedBranch(
                subBranchStart,
                subBranchEnd,
                mainBranchThickness * 0.3f,
                mainBranchThickness * 0.15f,
                barkMaterial
            );
            subBranch.transform.parent = parent;

            BranchInfo subBranchInfo = new BranchInfo
            {
                startPoint = subBranchStart,
                endPoint = subBranchEnd,
                attachedTags = new List<GameObject>()
            };

            tree.branches.Add(subBranchInfo);
            CreateFoliageClusters(tree, subBranchInfo, parent);
        }
    }

    // void CreateEnhancedSubBranches(TreeStructure tree, BranchInfo mainBranch, Vector3 mainDirection, Transform parent, bool isLeftTree)
    // {
    //     int subBranchCount = Random.Range(3, 5); // Slightly reduced for cleaner look
    //     List<float> usedPositions = new List<float>();

    //     for (int i = 0; i < subBranchCount; i++)
    //     {
    //         // Find well-spaced position along main branch
    //         float t = 0;
    //         int attempts = 0;
    //         bool validPosition = false;

    //         while (!validPosition && attempts < 15)
    //         {
    //             t = Random.Range(0.3f, 0.9f);
    //             validPosition = true;

    //             foreach (float usedT in usedPositions)
    //             {
    //                 if (Mathf.Abs(t - usedT) < subBranchMinSpacing)
    //                 {
    //                     validPosition = false;
    //                     break;
    //                 }
    //             }
    //             attempts++;
    //         }

    //         if (!validPosition) continue;
    //         usedPositions.Add(t);

    //         Vector3 subBranchStart = Vector3.Lerp(mainBranch.startPoint, mainBranch.endPoint, t);

    //         // Sub-branch direction - CONSTRAINED TO XY PLANE
    //         Vector3 subDirection = mainDirection;

    //         // CRITICAL: Zero out any Z component from main direction
    //         subDirection.z = 0;
    //         subDirection.Normalize();

    //         // Add variation in XY plane only
    //         // Use UP/DOWN variation instead of forward/backward
    //         float verticalVariation = Random.Range(-0.3f, 0.5f); // Mostly upward
    //         float horizontalVariation = Random.Range(-0.2f, 0.2f); // Slight horizontal spread

    //         subDirection.y += verticalVariation;
    //         subDirection.x += horizontalVariation * (isLeftTree ? -1f : 1f); // Enhance outward spread

    //         // FORCE Z to minimal value
    //         subDirection.z = Random.Range(-0.02f, 0.02f); // Nearly zero
    //         subDirection.Normalize();

    //         float subBranchLength = horizontalBranchLength * Random.Range(0.4f, 0.7f);
    //         Vector3 subBranchEnd = subBranchStart + subDirection * subBranchLength;

    //         GameObject subBranch = ProceduralMeshGenerator.CreateCurvedBranch(
    //             subBranchStart,
    //             subBranchEnd,
    //             mainBranchThickness * 0.35f,
    //             mainBranchThickness * 0.18f,
    //             barkMaterial
    //         );
    //         subBranch.transform.parent = parent;

    //         BranchInfo subBranchInfo = new BranchInfo
    //         {
    //             startPoint = subBranchStart,
    //             endPoint = subBranchEnd,
    //             attachedTags = new List<GameObject>()
    //         };

    //         tree.branches.Add(subBranchInfo);

    //         // Add foliage to sub-branches
    //         CreateFoliageClusters(tree, subBranchInfo, parent);
    //     }
    // }

    void CreateFoliageClusters(TreeStructure tree, BranchInfo branch, Transform parent)
    {
        if (foliagePrefab == null) return;

        // Create multiple foliage clusters along the branch
        for (int i = 0; i < foliageClustersPerBranch; i++)
        {
            // Position clusters toward the end of the branch
            float t = 1f - (i * foliageClusterSpacing) - foliageOffsetFromEnd;
            t = Mathf.Clamp01(t);

            if (t < 0.5f) continue; // Don't put foliage too close to trunk

            Vector3 foliagePosition = Vector3.Lerp(branch.startPoint, branch.endPoint, t);

            // Instantiate foliage
            GameObject foliage = Instantiate(foliagePrefab, foliagePosition, Quaternion.identity);
            foliage.transform.parent = parent;

            // Random rotation
            foliage.transform.rotation = Random.rotation;

            // Scale with variation
            float scaleVariation = Random.Range(0.8f, 1.3f);
            foliage.transform.localScale = Vector3.one * foliageScale * scaleVariation;

            // Apply tree color
            Renderer[] renderers = foliage.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                // Create a new material instance to avoid shared material issues
                Material foliageMat = new Material(renderer.material);
                foliageMat.color = tree.foliageColor;
                renderer.material = foliageMat;
            }

            // Slight position randomness for organic look
            foliage.transform.position += Random.insideUnitSphere * 0.3f;
        }
    }

    // public void AddSubmissionTag(string content, string usageType, string department, string category)
    // {
    //     TreeStructure targetTree;
    //     Color tagColor;
    //     Transform parentTransform;

    //     if (usageType.ToLower().Contains("current"))
    //     {
    //         targetTree = currentUsageTree;
    //         tagColor = currentUsageTagColor;
    //         parentTransform = currentUsageTreeTrunk.transform;
    //     }
    //     else
    //     {
    //         targetTree = futureUsageTree;
    //         tagColor = futureUsageTagColor;
    //         parentTransform = futureUsageTreeTrunk.transform;
    //     }

    //     // Check capacity
    //     int totalCapacity = targetTree.mainBranches.Count * maxTagsPerBranch;
    //     int totalTags = 0;
    //     foreach (var branch in targetTree.mainBranches)
    //     {
    //         totalTags += branch.attachedTags.Count;
    //     }

    //     // Grow new branches if needed
    //     if (totalTags >= totalCapacity * 0.6f)
    //     {
    //         Debug.Log($"Tree {targetTree.usageType} is at capacity, growing new branch");
    //         GrowNewBranch(targetTree, parentTransform);
    //     }

    //     // Find available branch
    //     // Find available branch
    //     BranchInfo availableBranch = FindAvailableBranch(targetTree);
    //     if (availableBranch != null)
    //     {
    //         GameObject tagObject = Instantiate(tagPrefab);
    //         tagObject.transform.parent = parentTransform;

    //         ModernTag tagComponent = tagObject.GetComponent<ModernTag>();
    //         if (tagComponent == null)
    //             tagComponent = tagObject.AddComponent<ModernTag>();

    //         // Calculate tag positioning with improved spacing
    //         int tagIndex = availableBranch.attachedTags.Count;

    //         // Base hang distance with variation
    //         float baseHangDistance = Random.Range(2.5f, 3.5f);
    //         float verticalSpacing = Random.Range(minTagVerticalSpacing, maxTagVerticalSpacing);
    //         float totalVerticalOffset = baseHangDistance + (tagIndex * verticalSpacing);

    //         // Horizontal offset perpendicular to branch
    //         Vector3 branchDirection = (availableBranch.endPoint - availableBranch.startPoint).normalized;
    //         Vector3 perpendicular = Vector3.Cross(branchDirection, Vector3.up).normalized;
    //         Vector3 horizontalOffset = perpendicular * Random.Range(-tagHorizontalOffset, tagHorizontalOffset);

    //         // Alternate sides for visual balance
    //         if (tagIndex % 2 == 0)
    //             horizontalOffset = -horizontalOffset;

    //         // Final hang point
    //         Vector3 hangPoint = availableBranch.endPoint +
    //                            Vector3.down * totalVerticalOffset +
    //                            horizontalOffset;

    //         tagComponent.Initialize(content, department, category, hangPoint, tagColor, availableBranch.endPoint);

    //         availableBranch.attachedTags.Add(tagObject);

    //         Debug.Log($"✅ Tag added to {targetTree.usageType} tree. Branch has {availableBranch.attachedTags.Count}/{maxTagsPerBranch} tags");
    //     }
    //     else
    //     {
    //         Debug.LogWarning("⚠️ No available branch found! Growing emergency branch...");
    //         GrowNewBranch(targetTree, parentTransform);

    //         // Try again after growing
    //         availableBranch = FindAvailableBranch(targetTree);
    //         if (availableBranch != null)
    //         {
    //             AddSubmissionTag(content, usageType, department, category);
    //         }
    //     }
    // }

    public void AddSubmissionTag(string content, string usageType, string department, string category)
    {
        TreeStructure targetTree;
        Color tagColor;
        Transform parentTransform;

        // if (usageType.ToLower().Contains("current"))
        if (usageType.ToLower().Contains("ai today"))
        {
            targetTree = currentUsageTree;
            tagColor = currentUsageTagColor;
            parentTransform = currentUsageTreeTrunk.transform;
        }
        else
        {
            targetTree = futureUsageTree;
            tagColor = futureUsageTagColor;
            parentTransform = futureUsageTreeTrunk.transform;
        }

        // Check capacity
        int totalCapacity = targetTree.mainBranches.Count * maxTagsPerBranch;
        int totalTags = 0;
        foreach (var branch in targetTree.mainBranches)
        {
            totalTags += branch.attachedTags.Count;
        }

        // Grow new branches if needed (EARLIER threshold for more spreading)
        if (totalTags >= totalCapacity * 0.5f) // Changed from 0.6f
        {
            Debug.Log($"Tree {targetTree.usageType} is at capacity, growing new branch");
            GrowNewBranch(targetTree, parentTransform);
        }

        // Find available branch
        BranchInfo availableBranch = FindAvailableBranch(targetTree);
        if (availableBranch != null)
        {
            GameObject tagObject = Instantiate(tagPrefab);
            tagObject.transform.parent = parentTransform;

            ModernTag tagComponent = tagObject.GetComponent<ModernTag>();
            if (tagComponent == null)
                tagComponent = tagObject.AddComponent<ModernTag>();

            // Calculate tag positioning with ENHANCED spacing
            int tagIndex = availableBranch.attachedTags.Count;

            // INCREASED base hang distance
            float baseHangDistance = Random.Range(3f, 4f); // Increased from 2.5-3.5
            float verticalSpacing = Random.Range(minTagVerticalSpacing, maxTagVerticalSpacing);
            float totalVerticalOffset = baseHangDistance + (tagIndex * verticalSpacing);

            // INCREASED horizontal offset for better spread
            Vector3 branchDirection = (availableBranch.endPoint - availableBranch.startPoint).normalized;
            Vector3 perpendicular = Vector3.Cross(branchDirection, Vector3.up).normalized;
            float horizontalVariation = Random.Range(-tagHorizontalOffset, tagHorizontalOffset);
            Vector3 horizontalOffset = perpendicular * horizontalVariation;

            // Alternate sides for visual balance
            if (tagIndex % 2 == 0)
                horizontalOffset = -horizontalOffset;

            // ENHANCED: Add STAGGERED forward offset (more depth variation)
            // Tags at different depths to prevent overlap
            // float forwardOffset = Random.Range(tagForwardOffsetMin, tagForwardOffsetMax);

            // // Additional staggering based on tag index (each successive tag slightly behind)
            // float depthStagger = tagIndex * 0.5f;
            // forwardOffset += depthStagger;

            // Vector3 frontOffset = Vector3.forward * forwardOffset;

            // Final hang point WITH enhanced positioning
            Vector3 hangPoint = availableBranch.endPoint +
                               Vector3.down * totalVerticalOffset +
                               horizontalOffset;
            // + frontOffset;

            tagComponent.Initialize(content, department, category, hangPoint, tagColor, availableBranch.endPoint);

            availableBranch.attachedTags.Add(tagObject);

            Debug.Log($"✅ Tag added to {targetTree.usageType} tree. Branch: {availableBranch.attachedTags.Count}/{maxTagsPerBranch}, Total: {totalTags + 1}");
        }
        else
        {
            Debug.LogWarning("⚠️ No available branch found! Growing emergency branch...");
            GrowNewBranch(targetTree, parentTransform);

            // Try again after growing
            availableBranch = FindAvailableBranch(targetTree);
            if (availableBranch != null)
            {
                AddSubmissionTag(content, usageType, department, category);
            }
        }
    }

    BranchInfo FindAvailableBranch(TreeStructure tree)
    {
        // Prioritize main branches
        BranchInfo bestBranch = null;
        int minTags = int.MaxValue;

        foreach (var branch in tree.mainBranches)
        {
            if (branch.attachedTags.Count < maxTagsPerBranch)
            {
                if (branch.attachedTags.Count < minTags)
                {
                    minTags = branch.attachedTags.Count;
                    bestBranch = branch;
                }
            }
        }

        // If no main branches available, use any branch
        if (bestBranch == null)
        {
            foreach (var branch in tree.branches)
            {
                if (branch.attachedTags.Count < maxTagsPerBranch)
                {
                    if (bestBranch == null || branch.attachedTags.Count < bestBranch.attachedTags.Count)
                    {
                        bestBranch = branch;
                    }
                }
            }
        }

        return bestBranch;
    }

    // void GrowNewBranch(TreeStructure tree, Transform parent)
    // {
    //     if (tree.mainBranches.Count == 0) return;

    //     bool isLeftTree = tree.usageType == "Current Usage";

    //     // Pick a random existing branch to grow from
    //     BranchInfo parentBranch = tree.mainBranches[Random.Range(0, tree.mainBranches.Count)];

    //     // Simple horizontal direction - alternate between left and right randomly
    //     Vector3 growthDirection;

    //     // 50/50 chance to grow left or right (balanced expansion)
    //     if (Random.value > 0.5f)
    //     {
    //         growthDirection = isLeftTree ? Vector3.left : Vector3.right; // Outward
    //     }
    //     else
    //     {
    //         growthDirection = isLeftTree ? Vector3.right : Vector3.left; // Inward/mixed
    //     }

    //     // Very slight upward angle for natural look
    //     growthDirection.y = Random.Range(0.05f, 0.12f);

    //     // LOCK Z-axis completely
    //     growthDirection.z = 0;
    //     growthDirection.Normalize();

    //     // Branch length
    //     float newBranchLength = horizontalBranchLength * Random.Range(0.8f, 1.0f);
    //     Vector3 newBranchEnd = parentBranch.endPoint + growthDirection * newBranchLength;
    //     newBranchEnd.z = parentBranch.endPoint.z; // Force same Z

    //     // CHECK: Will this branch invade the other tree's space?
    //     Vector3 oppositeTreePos = isLeftTree ?
    //         futureUsageTreeTrunk.transform.position :
    //         currentUsageTreeTrunk.transform.position;

    //     float distanceToOppositeTree = Vector3.Distance(
    //         new Vector3(newBranchEnd.x, 0, 0),
    //         new Vector3(oppositeTreePos.x, 0, 0)
    //     );

    //     // If branch gets too close to opposite tree, increase tree separation
    //     if (distanceToOppositeTree < minTreeSeparation / 2f)
    //     {
    //         Debug.LogWarning($"⚠️ Branch too close to opposite tree! Increasing tree spacing...");
    //         IncreaseTreeSpacing();
    //         return; // Don't create this branch, try again after spacing adjustment
    //     }

    //     // Create the new branch
    //     GameObject newBranch = ProceduralMeshGenerator.CreateCurvedBranch(
    //         parentBranch.endPoint,
    //         newBranchEnd,
    //         mainBranchThickness * 0.35f,
    //         mainBranchThickness * 0.18f,
    //         barkMaterial
    //     );
    //     newBranch.transform.parent = parent;

    //     BranchInfo newBranchInfo = new BranchInfo
    //     {
    //         startPoint = parentBranch.endPoint,
    //         endPoint = newBranchEnd,
    //         attachedTags = new List<GameObject>()
    //     };

    //     tree.branches.Add(newBranchInfo);
    //     tree.mainBranches.Add(newBranchInfo);

    //     // Add sub-branches and foliage
    //     CreateEnhancedSubBranches(tree, newBranchInfo, growthDirection, parent, isLeftTree);
    //     CreateFoliageClusters(tree, newBranchInfo, parent);

    //     Debug.Log($"🌱 Grew balanced branch for {tree.usageType}. Total: {tree.mainBranches.Count}");
    // }

    void GrowNewBranch(TreeStructure tree, Transform parent)
    {
        if (tree.mainBranches.Count == 0) return;

        bool isLeftTree = tree.usageType == "Current Usage";
        BranchInfo parentBranch = tree.mainBranches[Random.Range(0, tree.mainBranches.Count)];

        // Simple horizontal direction
        Vector3 growthDirection;

        if (Random.value > 0.5f)
        {
            growthDirection = isLeftTree ? Vector3.left : Vector3.right;
        }
        else
        {
            growthDirection = isLeftTree ? Vector3.right : Vector3.left;
        }

        growthDirection.y = Random.Range(0.05f, 0.12f);
        growthDirection.z = 0;
        growthDirection.Normalize();

        // INCREASED branch length for more spread
        float newBranchLength = horizontalBranchLength * Random.Range(1.0f, 1.3f); // Increased from 0.8-1.0
        Vector3 newBranchEnd = parentBranch.endPoint + growthDirection * newBranchLength;
        newBranchEnd.z = parentBranch.endPoint.z;

        // Check collision
        Vector3 oppositeTreePos = isLeftTree ?
            futureUsageTreeTrunk.transform.position :
            currentUsageTreeTrunk.transform.position;

        float distanceToOppositeTree = Vector3.Distance(
            new Vector3(newBranchEnd.x, 0, 0),
            new Vector3(oppositeTreePos.x, 0, 0)
        );

        if (distanceToOppositeTree < minTreeSeparation / 2f)
        {
            Debug.LogWarning($"⚠️ Branch too close! Increasing tree spacing...");
            IncreaseTreeSpacing();
            return;
        }

        // Create branch
        GameObject newBranch = ProceduralMeshGenerator.CreateCurvedBranch(
            parentBranch.endPoint,
            newBranchEnd,
            mainBranchThickness * 0.35f,
            mainBranchThickness * 0.18f,
            barkMaterial
        );
        newBranch.transform.parent = parent;

        BranchInfo newBranchInfo = new BranchInfo
        {
            startPoint = parentBranch.endPoint,
            endPoint = newBranchEnd,
            attachedTags = new List<GameObject>()
        };

        tree.branches.Add(newBranchInfo);
        tree.mainBranches.Add(newBranchInfo);

        CreateEnhancedSubBranches(tree, newBranchInfo, growthDirection, parent, isLeftTree);
        CreateFoliageClusters(tree, newBranchInfo, parent);

        Debug.Log($"🌱 Grew LONGER branch for {tree.usageType}. Total: {tree.mainBranches.Count}");
    }

    // void IncreaseTreeSpacing()
    // {
    //     if (currentUsageTreeTrunk == null || futureUsageTreeTrunk == null) return;

    //     // Increase spacing (horizontal + backward)
    //     float horizontalIncrease = 2.5f;  // Left/Right separation
    //     float backwardIncrease = 1f;      // Move both trees back (toward camera)

    //     // Calculate target positions
    //     Vector3 currentPos = currentUsageTreeTrunk.transform.localPosition;
    //     Vector3 futurePos = futureUsageTreeTrunk.transform.localPosition;

    //     Vector3 currentTarget = new Vector3(
    //         currentPos.x - horizontalIncrease / 2f,  // Move left
    //         currentPos.y,
    //         currentPos.z + backwardIncrease           // Move back (positive Z = away from camera)
    //     );

    //     Vector3 futureTarget = new Vector3(
    //         futurePos.x + horizontalIncrease / 2f,   // Move right
    //         futurePos.y,
    //         futurePos.z + backwardIncrease            // Move back
    //     );

    //     // Start smooth movement coroutine
    //     StartCoroutine(SmoothMoveTree(currentUsageTreeTrunk, currentTarget, 1.5f)); // 1.5 sec duration
    //     StartCoroutine(SmoothMoveTree(futureUsageTreeTrunk, futureTarget, 1.5f));

    //     // Update midpoint
    //     midPoint = (currentTarget + futureTarget) / 2f;

    //     // Update minimum separation
    //     minTreeSeparation += horizontalIncrease;

    //     Debug.Log($"📏 Smoothly increasing tree spacing to {minTreeSeparation}m (with backward offset)");
    // }

    void IncreaseTreeSpacing()
    {
        if (currentUsageTreeTrunk == null || futureUsageTreeTrunk == null) return;

        float horizontalIncrease = 2.5f;
        float backwardIncrease = 1f;

        Vector3 currentPos = currentUsageTreeTrunk.transform.localPosition;
        Vector3 futurePos = futureUsageTreeTrunk.transform.localPosition;

        Vector3 currentTarget = new Vector3(
            currentPos.x - horizontalIncrease / 2f,
            currentTreeOriginalPosition.y, // ✨ Use original Y, not current
            currentPos.z + backwardIncrease
        );

        Vector3 futureTarget = new Vector3(
            futurePos.x + horizontalIncrease / 2f,
            futureTreeOriginalPosition.y, // ✨ Use original Y, not current
            futurePos.z + backwardIncrease
        );

        // ✨ NEW: Update original positions after spacing change
        currentTreeOriginalPosition = currentTarget;
        futureTreeOriginalPosition = futureTarget;

        StartCoroutine(SmoothMoveTree(currentUsageTreeTrunk, currentTarget, 1.5f));
        StartCoroutine(SmoothMoveTree(futureUsageTreeTrunk, futureTarget, 1.5f));

        midPoint = (currentTarget + futureTarget) / 2f;
        minTreeSeparation += horizontalIncrease;

        Debug.Log($"📏 Smoothly increasing tree spacing to {minTreeSeparation}m");
    }

    // NEW: Smooth movement coroutine
    // IEnumerator SmoothMoveTree(GameObject tree, Vector3 targetPosition, float duration)
    // {
    //     Vector3 startPosition = tree.transform.localPosition;
    //     float elapsed = 0f;

    //     while (elapsed < duration)
    //     {
    //         elapsed += Time.deltaTime;
    //         float t = elapsed / duration;

    //         // Smooth ease-out curve
    //         float smoothT = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease-out

    //         tree.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, smoothT);

    //         onTreeMoved?.Invoke(tree.transform);

    //         yield return null;
    //     }

    //     // Ensure final position is exact
    //     tree.transform.localPosition = targetPosition;

    //     onTreeMoved?.Invoke(tree.transform);

    //     Debug.Log($"✅ {tree.name} moved to {targetPosition}");
    // }

    IEnumerator SmoothMoveTree(GameObject tree, Vector3 targetPosition, float duration)
    {
        // ✨ CHANGED: Get the original position without wave offset
        Vector3 startOriginalPos = tree == currentUsageTreeTrunk ? currentTreeOriginalPosition : futureTreeOriginalPosition;

        // Temporarily disable wave during movement
        bool wasWaveEnabled = enableWaveMotion;
        enableWaveMotion = false;

        Vector3 startPosition = tree.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            tree.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, smoothT);

            onTreeMoved?.Invoke(tree.transform);

            yield return null;
        }

        tree.transform.localPosition = targetPosition;
        onTreeMoved?.Invoke(tree.transform);

        // Re-enable wave
        enableWaveMotion = wasWaveEnabled;

        Debug.Log($"✅ {tree.name} moved to {targetPosition}");
    }

    void Update()
    {
        if (enableWaveMotion)
        {
            AnimateTreeWave();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitPanel.SetActive(true);
        }
    }

    void AnimateTreeWave()
    {
        if (currentUsageTreeTrunk == null || futureUsageTreeTrunk == null) return;

        float time = Time.time * waveSpeed;

        // Current Usage Tree (left) - sine wave
        float currentWaveOffset = Mathf.Sin(time) * waveAmplitude;
        Vector3 currentNewPos = currentTreeOriginalPosition;
        currentNewPos.y += currentWaveOffset;
        currentUsageTreeTrunk.transform.localPosition = currentNewPos;

        // Future Usage Tree (right) - sine wave with phase offset (creates rolling effect)
        float futureWaveOffset = Mathf.Sin(time + wavePhaseOffset) * waveAmplitude;
        Vector3 futureNewPos = futureTreeOriginalPosition;
        futureNewPos.y += futureWaveOffset;
        futureUsageTreeTrunk.transform.localPosition = futureNewPos;

        // Trigger event so tags follow
        onTreeMoved?.Invoke(currentUsageTreeTrunk.transform);
        onTreeMoved?.Invoke(futureUsageTreeTrunk.transform);
    }

    void CheckAndAdjustTreeSpacing()
    {
        if (currentUsageTreeTrunk == null || futureUsageTreeTrunk == null) return;

        float currentDistance = Vector3.Distance(
            currentUsageTreeTrunk.transform.position,
            futureUsageTreeTrunk.transform.position
        );

        Debug.Log($"Current tree separation: {currentDistance:F2}m (Minimum: {minTreeSeparation}m)");

        if (currentDistance < minTreeSeparation)
        {
            Debug.LogWarning("⚠️ Trees are too close! Consider increasing Tree Spacing in inspector.");
        }
        else
        {
            Debug.Log("✅ Tree spacing is adequate.");
        }
    }

    // Debug visualization in Scene view
    void OnDrawGizmosSelected()
    {
        // Draw tree positions and separation
        if (currentUsageTreeTrunk != null && futureUsageTreeTrunk != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentUsageTreeTrunk.transform.position, 1f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(futureUsageTreeTrunk.transform.position, 1f);

            // Draw midpoint
            Vector3 mid = (currentUsageTreeTrunk.transform.position + futureUsageTreeTrunk.transform.position) / 2f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(mid, 0.5f);

            // Draw minimum separation boundary
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentUsageTreeTrunk.transform.position, minTreeSeparation / 2f);
            Gizmos.DrawWireSphere(futureUsageTreeTrunk.transform.position, minTreeSeparation / 2f);
        }

        // Draw branch spawn points
        if (currentTreeBranchPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var point in currentTreeBranchPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.2f);
            }
        }

        if (futureTreeBranchPoints != null)
        {
            Gizmos.color = Color.magenta;
            foreach (var point in futureTreeBranchPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.2f);
            }
        }
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}

[System.Serializable]
public class TreeStructure
{
    public Vector3 rootPosition;
    public Color foliageColor;
    public string usageType;
    public List<BranchInfo> branches = new List<BranchInfo>();
    public List<BranchInfo> mainBranches = new List<BranchInfo>();

    public TreeStructure(Vector3 root, Color color, string usage)
    {
        rootPosition = root;
        foliageColor = color;
        usageType = usage;
    }
}

[System.Serializable]
public class BranchInfo
{
    public Vector3 startPoint;
    public Vector3 endPoint;
    public List<GameObject> attachedTags;
    public Transform spawnPoint;
}