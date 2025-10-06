using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

public class StrapiConnector : MonoBehaviour
{
    [Header("Strapi Configuration")]
    public string strapiUrl = "https://mlewis-dev-cms.thedemo.is/api";
    public string apiEndpoint = "/submissions";

    [Header("Authentication")]
    public string apiKey = "YOUR_READ_ONLY_API_KEY";

    [Header("Smart Polling Configuration")]
    [Tooltip("How often to check for new entries (seconds)")]
    public float pollInterval = 3f;

    [Tooltip("Perform full sync every X seconds to catch any missed entries")]
    public float fullSyncInterval = 300f; // 5 minutes

    [Tooltip("Number of entries to fetch on initial load")]
    public int initialFetchCount = 20;

    [Header("Query Parameters")]
    public bool populateRelations = true;
    public int pageSize = 100;
    public string sortBy = "createdAt:desc";

    [Header("Status")]
    [SerializeField] private int totalProcessed = 0;
    [SerializeField] private string lastCheckedTime = "";
    [SerializeField] private bool isPolling = false;

    private DualTreeManager treeManager;
    private HashSet<string> processedIds = new HashSet<string>();
    private string lastKnownTimestamp = "";
    private float lastFullSyncTime = 0f;
    private bool isInitialized = false;

    // Data classes
    [System.Serializable]
    public class StrapiResponse
    {
        public List<SubmissionData> data;
        public Meta meta;
    }

    // [System.Serializable]
    // public class SubmissionData
    // {
    //     public int id;
    //     public string documentId;
    //     public string idea_description;
    //     public string idea_details;
    //     public string usage_type;
    //     public string createdAt;
    //     public string updatedAt;
    //     public string publishedAt;
    //     public DepartmentRelation department;
    //     public IdeaCategoryRelation idea_category;
    // }

    [System.Serializable]
    public class SubmissionData
    {
        public int id;
        public string documentId;
        public string idea_description;
        public string idea_details;
        public string usage_type;
        public string createdAt;
        public string updatedAt;
        public string publishedAt;

        // Changed: These are now direct objects, not wrapped in 'data'
        public DepartmentData department;
        public IdeaCategoryData idea_category;
    }

    [System.Serializable]
    public class DepartmentRelation
    {
        public DepartmentData data;
    }

    // [System.Serializable]
    // public class DepartmentData
    // {
    //     public int id;
    //     public string documentId;
    //     public string name;
    // }

    [System.Serializable]
    public class DepartmentData
    {
        public int id;
        public string documentId;
        public string name;
        public string createdAt;
        public string updatedAt;
        public string publishedAt;
    }

    [System.Serializable]
    public class IdeaCategoryRelation
    {
        public IdeaCategoryData data;
    }

    // [System.Serializable]
    // public class IdeaCategoryData
    // {
    //     public int id;
    //     public string documentId;
    //     public string name;
    // }

    [System.Serializable]
    public class IdeaCategoryData
    {
        public int id;
        public string documentId;
        public string name;
        public string createdAt;
        public string updatedAt;
        public string publishedAt;
        public string color;
        public string filter_name;
    }

    [System.Serializable]
    public class Meta
    {
        public Pagination pagination;
    }

    [System.Serializable]
    public class Pagination
    {
        public int page;
        public int pageSize;
        public int pageCount;
        public int total;
    }

    void Start()
    {
        // Find tree manager
        treeManager = GetComponent<DualTreeManager>();
        if (treeManager == null)
        {
            Debug.LogError("DualTreeManager not found in scene!");
            return;
        }

        Initialize();
    }

    public void Initialize()
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_READ_ONLY_API_KEY")
        {
            Debug.LogError("Please set your Strapi API key in the StrapiConnector component!");
            return;
        }

        // Set initial timestamp to current time minus 1 hour (to catch recent entries on first load)
        System.DateTime initialTime = System.DateTime.UtcNow.AddHours(-1);
        lastKnownTimestamp = initialTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        isInitialized = true;
        StartCoroutine(SmartPollingCoroutine());
    }

    IEnumerator SmartPollingCoroutine()
    {
        isPolling = true;

        // Initial fetch - get recent submissions
        Debug.Log("Performing initial fetch...");
        yield return StartCoroutine(FetchRecentSubmissions(initialFetchCount));

        lastFullSyncTime = Time.time;

        // Start polling loop
        while (isPolling && isInitialized)
        {
            yield return new WaitForSeconds(pollInterval);

            // Check if we need a full sync
            if (Time.time - lastFullSyncTime > fullSyncInterval)
            {
                Debug.Log("Performing scheduled full sync...");
                yield return StartCoroutine(PerformFullSync());
                lastFullSyncTime = Time.time;
            }
            else
            {
                // Normal incremental check
                yield return StartCoroutine(CheckForNewSubmissions());
            }
        }
    }

    IEnumerator CheckForNewSubmissions()
    {
        if (string.IsNullOrEmpty(lastKnownTimestamp))
        {
            Debug.LogWarning("No timestamp available, performing full sync");
            yield return StartCoroutine(PerformFullSync());
            yield break;
        }

        string queryParams = "?";

        if (populateRelations)
        {
            queryParams += "populate[department]=*&populate[idea_category]=*&";
        }

        // Filter for entries created after our last known timestamp
        queryParams += $"filters[createdAt][$gt]={lastKnownTimestamp}&";
        queryParams += $"pagination[pageSize]={pageSize}&";
        queryParams += $"sort={sortBy}";

        string fullUrl = strapiUrl + apiEndpoint + queryParams;

        using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    StrapiResponse response = JsonConvert.DeserializeObject<StrapiResponse>(request.downloadHandler.text);

                    if (response != null && response.data != null && response.data.Count > 0)
                    {
                        Debug.Log($"Found {response.data.Count} new submissions");
                        ProcessSubmissions(response.data, true);

                        // Update our timestamp to the newest entry
                        var newestEntry = response.data.OrderByDescending(s => s.createdAt).FirstOrDefault();
                        if (newestEntry != null)
                        {
                            lastKnownTimestamp = newestEntry.createdAt;
                            lastCheckedTime = System.DateTime.Now.ToString("HH:mm:ss");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse response: {e.Message}");
                }
            }
            else if (request.responseCode != 404)
            {
                Debug.LogError($"Failed to check for new submissions: {request.error}");
            }
        }
    }

    IEnumerator FetchRecentSubmissions(int count)
    {
        string queryParams = "?";

        if (populateRelations)
        {
            queryParams += "populate[department]=*&populate[idea_category]=*&";
        }

        queryParams += $"pagination[pageSize]={count}&";
        queryParams += $"sort={sortBy}";

        string fullUrl = strapiUrl + apiEndpoint + queryParams;

        using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    StrapiResponse response = JsonConvert.DeserializeObject<StrapiResponse>(request.downloadHandler.text);

                    if (response != null && response.data != null)
                    {
                        Debug.Log($"Initial fetch: Found {response.data.Count} submissions");
                        ProcessSubmissions(response.data, false);

                        // Set timestamp to newest entry
                        var newestEntry = response.data.OrderByDescending(s => s.createdAt).FirstOrDefault();
                        if (newestEntry != null)
                        {
                            lastKnownTimestamp = newestEntry.createdAt;
                            lastCheckedTime = System.DateTime.Now.ToString("HH:mm:ss");
                        }

                        if (response.meta != null && response.meta.pagination != null)
                        {
                            Debug.Log($"Total submissions in database: {response.meta.pagination.total}");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse initial fetch response: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch initial submissions: {request.error}");
            }
        }
    }

    IEnumerator PerformFullSync()
    {
        string queryParams = "?";

        if (populateRelations)
        {
            queryParams += "populate[department]=*&populate[idea_category]=*&";
        }

        queryParams += $"pagination[pageSize]={pageSize}&";
        queryParams += $"sort={sortBy}";

        string fullUrl = strapiUrl + apiEndpoint + queryParams;

        using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    StrapiResponse response = JsonConvert.DeserializeObject<StrapiResponse>(request.downloadHandler.text);

                    if (response != null && response.data != null)
                    {
                        int newCount = ProcessSubmissions(response.data, false);
                        Debug.Log($"Full sync complete. Added {newCount} new submissions");

                        // Update timestamp
                        var newestEntry = response.data.OrderByDescending(s => s.createdAt).FirstOrDefault();
                        if (newestEntry != null)
                        {
                            lastKnownTimestamp = newestEntry.createdAt;
                            lastCheckedTime = System.DateTime.Now.ToString("HH:mm:ss");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse full sync response: {e.Message}");
                }
            }
        }
    }

    // int ProcessSubmissions(List<SubmissionData> submissions, bool isIncremental)
    // {
    //     int newCount = 0;

    //     foreach (var submission in submissions)
    //     {
    //         if (!processedIds.Contains(submission.documentId))
    //         {
    //             processedIds.Add(submission.documentId);
    //             newCount++;

    //             // Extract data
    //             string content = submission.idea_description ?? "No description";
    //             string department = submission.department?.data?.name ?? "Unknown";
    //             string category = submission.idea_category?.data?.name ?? "General";
    //             string usageType = submission.usage_type ?? DetermineUsageType(content, submission.idea_details);

    //             // Add to tree
    //             if (treeManager != null)
    //             {
    //                 treeManager.AddSubmissionTag(content, usageType, department, category);

    //                 if (isIncremental)
    //                 {
    //                     Debug.Log($"New submission added: {content.Substring(0, Mathf.Min(50, content.Length))}... [{usageType}]");
    //                 }
    //             }

    //             totalProcessed++;
    //         }
    //     }

    //     return newCount;
    // }

    int ProcessSubmissions(List<SubmissionData> submissions, bool isIncremental)
    {
        int newCount = 0;

        foreach (var submission in submissions)
        {
            if (!processedIds.Contains(submission.documentId))
            {
                processedIds.Add(submission.documentId);
                newCount++;

                // Extract data from the API response
                string content = submission.idea_description ?? "No description";

                // Get department name - now accessing directly without .data wrapper
                string department = "Unknown";
                if (submission.department != null)
                {
                    department = submission.department.name;
                }

                // Get usage type from idea_category name - direct access
                // string usageType = "Future"; // Default fallback
                string usageType = "AI tomorrow"; // Default fallback
                if (submission.idea_category != null)
                {
                    // usageType = submission.idea_category.name; // Will be "Current" or "Future"
                    usageType = submission.idea_category.filter_name; // Will be "Current" or "Future"
                }

                // Category for the tag
                string category = department;

                // Debug log
                Debug.Log($"Processing submission #{submission.id}: UsageType='{usageType}', Dept='{department}'");

                // Add to tree
                if (treeManager != null)
                {
                    treeManager.AddSubmissionTag(content, usageType, department, category);

                    if (isIncremental)
                    {
                        Debug.Log($"New submission added: {content.Substring(0, Mathf.Min(50, content.Length))}... [Usage: {usageType}, Dept: {department}]");
                    }
                }

                totalProcessed++;
            }
        }

        return newCount;
    }

    string DetermineUsageType(string description, string details)
    {
        // Keywords that suggest future usage
        string[] futureKeywords = { "future", "will", "plan", "upcoming", "proposed", "potential", "innovation" };

        string combined = (description + " " + (details ?? "")).ToLower();

        foreach (string keyword in futureKeywords)
        {
            if (combined.Contains(keyword))
            {
                return "Future Usage";
            }
        }

        return "Current Usage";
    }

    // Public methods for manual control
    [ContextMenu("Force Check Now")]
    public void ForceCheckNow()
    {
        if (isInitialized)
        {
            StartCoroutine(CheckForNewSubmissions());
        }
    }

    [ContextMenu("Force Full Sync")]
    public void ForceFullSync()
    {
        if (isInitialized)
        {
            StartCoroutine(PerformFullSync());
        }
    }

    [ContextMenu("Test Connection")]
    public void TestConnection()
    {
        StartCoroutine(TestConnectionCoroutine());
    }

    IEnumerator TestConnectionCoroutine()
    {
        string testUrl = strapiUrl + "/departments?pagination[pageSize]=1";

        using (UnityWebRequest request = UnityWebRequest.Get(testUrl))
        {
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✓ Successfully connected to Strapi!");
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("✗ Failed to connect to Strapi: " + request.error);
                Debug.LogError($"Response Code: {request.responseCode}");
            }
        }
    }

    void OnDestroy()
    {
        isPolling = false;
    }

    // Getters for UI
    public int GetTotalProcessed() => totalProcessed;
    public string GetLastCheckTime() => lastCheckedTime;
    public bool IsPolling() => isPolling;
}

// using UnityEngine;
// using UnityEngine.Networking;
// using System.Collections;
// using System.Collections.Generic;
// using Newtonsoft.Json;
// using System.Linq;

// public class StrapiConnector : MonoBehaviour
// {
//     [Header("Strapi Configuration")]
//     public string strapiUrl = "https://mlewis-dev-cms.thedemo.is/api";
//     public string apiEndpoint = "/submissions";

//     [Header("Authentication")]
//     public string apiKey = "YOUR_READ_ONLY_API_KEY";

//     [Header("Webhook Configuration")]
//     [Tooltip("Enable webhook polling for real-time updates")]
//     public bool enableWebhooks = true;
    
//     [Tooltip("Local webhook relay server URL")]
//     public string webhookRelayUrl = "http://localhost:3000";
    
//     [Tooltip("How often to poll relay for new webhooks (seconds)")]
//     public float webhookPollInterval = 2f;

//     [Header("Fallback Polling Configuration")]
//     [Tooltip("Fallback polling if webhooks unavailable (seconds)")]
//     public float fallbackPollInterval = 30f;

//     [Tooltip("Perform full sync every X seconds")]
//     public float fullSyncInterval = 300f;

//     [Tooltip("Number of entries to fetch on initial load")]
//     public int initialFetchCount = 20;

//     [Header("Query Parameters")]
//     public bool populateRelations = true;
//     public int pageSize = 100;
//     public string sortBy = "createdAt:desc";

//     [Header("Status")]
//     [SerializeField] private int totalProcessed = 0;
//     [SerializeField] private string lastCheckedTime = "";
//     [SerializeField] private bool isPolling = false;
//     [SerializeField] private bool webhookRelayConnected = false;
//     [SerializeField] private int webhookEventsReceived = 0;

//     private DualTreeManager treeManager;
//     private HashSet<string> processedIds = new HashSet<string>();
//     private string lastKnownTimestamp = "";
//     private float lastFullSyncTime = 0f;
//     private bool isInitialized = false;

//     // Webhook payload structure
//     [System.Serializable]
//     public class WebhookPayload
//     {
//         public string @event;
//         public string createdAt;
//         public string model;
//         public SubmissionData entry;
//         public string receivedAt;
//         public bool processed;
//     }

//     // Data classes
//     [System.Serializable]
//     public class StrapiResponse
//     {
//         public List<SubmissionData> data;
//         public Meta meta;
//     }

//     [System.Serializable]
//     public class SubmissionData
//     {
//         public int id;
//         public string documentId;
//         public string idea_description;
//         public string idea_details;
//         public string usage_type;
//         public string createdAt;
//         public string updatedAt;
//         public string publishedAt;
//         public DepartmentData department;
//         public IdeaCategoryData idea_category;
//     }

//     [System.Serializable]
//     public class DepartmentData
//     {
//         public int id;
//         public string documentId;
//         public string name;
//         public string createdAt;
//         public string updatedAt;
//         public string publishedAt;
//     }

//     [System.Serializable]
//     public class IdeaCategoryData
//     {
//         public int id;
//         public string documentId;
//         public string name;
//         public string createdAt;
//         public string updatedAt;
//         public string publishedAt;
//         public string color;
//         public string filter_name;
//     }

//     [System.Serializable]
//     public class Meta
//     {
//         public Pagination pagination;
//     }

//     [System.Serializable]
//     public class Pagination
//     {
//         public int page;
//         public int pageSize;
//         public int pageCount;
//         public int total;
//     }

//     void Start()
//     {
//         treeManager = GetComponent<DualTreeManager>();
//         if (treeManager == null)
//         {
//             Debug.LogError("DualTreeManager not found in scene!");
//             return;
//         }

//         Initialize();
//     }

//     public void Initialize()
//     {
//         if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_READ_ONLY_API_KEY")
//         {
//             Debug.LogError("Please set your Strapi API key in the StrapiConnector component!");
//             return;
//         }

//         System.DateTime initialTime = System.DateTime.UtcNow.AddHours(-1);
//         lastKnownTimestamp = initialTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

//         isInitialized = true;

//         // Test webhook relay connection
//         if (enableWebhooks)
//         {
//             StartCoroutine(TestWebhookRelayConnection());
//         }

//         // Start main polling coroutine
//         StartCoroutine(SmartPollingCoroutine());
//     }

//     IEnumerator TestWebhookRelayConnection()
//     {
//         using (UnityWebRequest request = UnityWebRequest.Get($"{webhookRelayUrl}/health"))
//         {
//             request.timeout = 3;
//             yield return request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 webhookRelayConnected = true;
//                 Debug.Log("✓ Connected to webhook relay server");
//                 StartCoroutine(PollWebhookRelay());
//             }
//             else
//             {
//                 webhookRelayConnected = false;
//                 Debug.LogWarning("⚠ Webhook relay not available. Using fallback polling only.");
//                 Debug.LogWarning($"  Make sure webhook relay is running at {webhookRelayUrl}");
//             }
//         }
//     }

//     IEnumerator PollWebhookRelay()
//     {
//         while (isPolling && enableWebhooks && isInitialized)
//         {
//             yield return new WaitForSeconds(webhookPollInterval);

//             using (UnityWebRequest request = UnityWebRequest.Get($"{webhookRelayUrl}/get-webhooks"))
//             {
//                 request.timeout = 5;
//                 yield return request.SendWebRequest();

//                 if (request.result == UnityWebRequest.Result.Success)
//                 {
//                     try
//                     {
//                         string json = request.downloadHandler.text;
                        
//                         if (!string.IsNullOrEmpty(json) && json != "[]")
//                         {
//                             var webhooks = JsonConvert.DeserializeObject<List<WebhookPayload>>(json);

//                             if (webhooks != null && webhooks.Count > 0)
//                             {
//                                 Debug.Log($"📥 Received {webhooks.Count} webhook(s) from relay");
                                
//                                 foreach (var webhook in webhooks)
//                                 {
//                                     HandleWebhookEvent(webhook);
//                                 }
//                             }
//                         }
//                     }
//                     catch (System.Exception e)
//                     {
//                         Debug.LogError($"Failed to parse webhooks: {e.Message}");
//                     }
//                 }
//                 else if (request.responseCode == 0)
//                 {
//                     // Connection lost
//                     webhookRelayConnected = false;
//                     Debug.LogWarning("⚠ Lost connection to webhook relay. Will retry...");
//                     yield return new WaitForSeconds(5f);
//                     yield return StartCoroutine(TestWebhookRelayConnection());
//                 }
//             }
//         }
//     }

//     void HandleWebhookEvent(WebhookPayload payload)
//     {
//         if (payload.model != "submission" && 
//             payload.model != "api::submission.submission")
//         {
//             return;
//         }

//         webhookEventsReceived++;

//         switch (payload.@event)
//         {
//             case "entry.create":
//                 Debug.Log($"🆕 New submission via webhook: {payload.entry?.documentId}");
//                 StartCoroutine(FetchAndProcessSingleEntry(payload.entry.documentId));
//                 break;

//             case "entry.update":
//                 Debug.Log($"✏️ Submission updated via webhook: {payload.entry?.documentId}");
//                 StartCoroutine(FetchAndProcessSingleEntry(payload.entry.documentId));
//                 break;

//             case "entry.delete":
//                 Debug.Log($"🗑️ Submission deleted via webhook: {payload.entry?.documentId}");
//                 processedIds.Remove(payload.entry.documentId);
//                 break;
//         }
//     }

//     IEnumerator FetchAndProcessSingleEntry(string documentId)
//     {
//         if (string.IsNullOrEmpty(documentId))
//         {
//             Debug.LogWarning("Cannot fetch entry: documentId is null or empty");
//             yield break;
//         }

//         string queryParams = "?";
//         if (populateRelations)
//         {
//             queryParams += "populate[department]=*&populate[idea_category]=*&";
//         }
//         queryParams += $"filters[documentId][$eq]={documentId}";

//         string fullUrl = strapiUrl + apiEndpoint + queryParams;

//         using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
//         {
//             request.SetRequestHeader("Content-Type", "application/json");
//             request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

//             yield return request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 try
//                 {
//                     StrapiResponse response = JsonConvert.DeserializeObject<StrapiResponse>(request.downloadHandler.text);

//                     if (response?.data != null && response.data.Count > 0)
//                     {
//                         ProcessSubmissions(response.data, true);
//                     }
//                 }
//                 catch (System.Exception e)
//                 {
//                     Debug.LogError($"Failed to parse single entry: {e.Message}");
//                 }
//             }
//             else
//             {
//                 Debug.LogWarning($"Failed to fetch entry {documentId}: {request.error}");
//             }
//         }
//     }

//     IEnumerator SmartPollingCoroutine()
//     {
//         isPolling = true;

//         Debug.Log("Performing initial fetch...");
//         yield return StartCoroutine(FetchRecentSubmissions(initialFetchCount));

//         lastFullSyncTime = Time.time;

//         while (isPolling && isInitialized)
//         {
//             // Use longer interval if webhooks are working
//             float waitTime = (enableWebhooks && webhookRelayConnected) 
//                 ? fallbackPollInterval 
//                 : fallbackPollInterval / 3f;

//             yield return new WaitForSeconds(waitTime);

//             if (Time.time - lastFullSyncTime > fullSyncInterval)
//             {
//                 Debug.Log("Performing scheduled full sync...");
//                 yield return StartCoroutine(PerformFullSync());
//                 lastFullSyncTime = Time.time;
//             }
//             else if (!webhookRelayConnected)
//             {
//                 // Only poll frequently if webhooks are down
//                 yield return StartCoroutine(CheckForNewSubmissions());
//             }
//         }
//     }

//     IEnumerator CheckForNewSubmissions()
//     {
//         if (string.IsNullOrEmpty(lastKnownTimestamp))
//         {
//             Debug.LogWarning("No timestamp available, performing full sync");
//             yield return StartCoroutine(PerformFullSync());
//             yield break;
//         }

//         string queryParams = "?";

//         if (populateRelations)
//         {
//             queryParams += "populate[department]=*&populate[idea_category]=*&";
//         }

//         queryParams += $"filters[createdAt][$gt]={lastKnownTimestamp}&";
//         queryParams += $"pagination[pageSize]={pageSize}&";
//         queryParams += $"sort={sortBy}";

//         string fullUrl = strapiUrl + apiEndpoint + queryParams;

//         using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
//         {
//             request.SetRequestHeader("Content-Type", "application/json");
//             request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

//             yield return request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 try
//                 {
//                     StrapiResponse response = JsonConvert.DeserializeObject<StrapiResponse>(request.downloadHandler.text);

//                     if (response != null && response.data != null && response.data.Count > 0)
//                     {
//                         Debug.Log($"Found {response.data.Count} new submissions (polling)");
//                         ProcessSubmissions(response.data, true);

//                         var newestEntry = response.data.OrderByDescending(s => s.createdAt).FirstOrDefault();
//                         if (newestEntry != null)
//                         {
//                             lastKnownTimestamp = newestEntry.createdAt;
//                             lastCheckedTime = System.DateTime.Now.ToString("HH:mm:ss");
//                         }
//                     }
//                 }
//                 catch (System.Exception e)
//                 {
//                     Debug.LogError($"Failed to parse response: {e.Message}");
//                 }
//             }
//             else if (request.responseCode != 404)
//             {
//                 Debug.LogError($"Failed to check for new submissions: {request.error}");
//             }
//         }
//     }

//     IEnumerator FetchRecentSubmissions(int count)
//     {
//         string queryParams = "?";

//         if (populateRelations)
//         {
//             queryParams += "populate[department]=*&populate[idea_category]=*&";
//         }

//         queryParams += $"pagination[pageSize]={count}&";
//         queryParams += $"sort={sortBy}";

//         string fullUrl = strapiUrl + apiEndpoint + queryParams;

//         using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
//         {
//             request.SetRequestHeader("Content-Type", "application/json");
//             request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

//             yield return request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 try
//                 {
//                     StrapiResponse response = JsonConvert.DeserializeObject<StrapiResponse>(request.downloadHandler.text);

//                     if (response != null && response.data != null)
//                     {
//                         Debug.Log($"Initial fetch: Found {response.data.Count} submissions");
//                         ProcessSubmissions(response.data, false);

//                         var newestEntry = response.data.OrderByDescending(s => s.createdAt).FirstOrDefault();
//                         if (newestEntry != null)
//                         {
//                             lastKnownTimestamp = newestEntry.createdAt;
//                             lastCheckedTime = System.DateTime.Now.ToString("HH:mm:ss");
//                         }

//                         if (response.meta != null && response.meta.pagination != null)
//                         {
//                             Debug.Log($"Total submissions in database: {response.meta.pagination.total}");
//                         }
//                     }
//                 }
//                 catch (System.Exception e)
//                 {
//                     Debug.LogError($"Failed to parse initial fetch response: {e.Message}");
//                 }
//             }
//             else
//             {
//                 Debug.LogError($"Failed to fetch initial submissions: {request.error}");
//             }
//         }
//     }

//     IEnumerator PerformFullSync()
//     {
//         string queryParams = "?";

//         if (populateRelations)
//         {
//             queryParams += "populate[department]=*&populate[idea_category]=*&";
//         }

//         queryParams += $"pagination[pageSize]={pageSize}&";
//         queryParams += $"sort={sortBy}";

//         string fullUrl = strapiUrl + apiEndpoint + queryParams;

//         using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
//         {
//             request.SetRequestHeader("Content-Type", "application/json");
//             request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

//             yield return request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 try
//                 {
//                     StrapiResponse response = JsonConvert.DeserializeObject<StrapiResponse>(request.downloadHandler.text);

//                     if (response != null && response.data != null)
//                     {
//                         int newCount = ProcessSubmissions(response.data, false);
//                         Debug.Log($"Full sync complete. Added {newCount} new submissions");

//                         var newestEntry = response.data.OrderByDescending(s => s.createdAt).FirstOrDefault();
//                         if (newestEntry != null)
//                         {
//                             lastKnownTimestamp = newestEntry.createdAt;
//                             lastCheckedTime = System.DateTime.Now.ToString("HH:mm:ss");
//                         }
//                     }
//                 }
//                 catch (System.Exception e)
//                 {
//                     Debug.LogError($"Failed to parse full sync response: {e.Message}");
//                 }
//             }
//         }
//     }

//     int ProcessSubmissions(List<SubmissionData> submissions, bool isIncremental)
//     {
//         int newCount = 0;

//         foreach (var submission in submissions)
//         {
//             if (!processedIds.Contains(submission.documentId))
//             {
//                 processedIds.Add(submission.documentId);
//                 newCount++;

//                 string content = submission.idea_description ?? "No description";

//                 string department = "Unknown";
//                 if (submission.department != null)
//                 {
//                     department = submission.department.name;
//                 }

//                 string usageType = "AI tomorrow";
//                 if (submission.idea_category != null)
//                 {
//                     usageType = submission.idea_category.filter_name;
//                 }

//                 string category = department;

//                 if (isIncremental)
//                 {
//                     Debug.Log($"Processing submission #{submission.id}: UsageType='{usageType}', Dept='{department}'");
//                 }

//                 if (treeManager != null)
//                 {
//                     treeManager.AddSubmissionTag(content, usageType, department, category);

//                     if (isIncremental)
//                     {
//                         Debug.Log($"✓ New: {content.Substring(0, Mathf.Min(50, content.Length))}... [{usageType}|{department}]");
//                     }
//                 }

//                 totalProcessed++;
//             }
//         }

//         return newCount;
//     }

//     [ContextMenu("Force Check Now")]
//     public void ForceCheckNow()
//     {
//         if (isInitialized)
//         {
//             StartCoroutine(CheckForNewSubmissions());
//         }
//     }

//     [ContextMenu("Force Full Sync")]
//     public void ForceFullSync()
//     {
//         if (isInitialized)
//         {
//             StartCoroutine(PerformFullSync());
//         }
//     }

//     [ContextMenu("Test Strapi Connection")]
//     public void TestConnection()
//     {
//         StartCoroutine(TestConnectionCoroutine());
//     }

//     [ContextMenu("Test Webhook Relay")]
//     public void TestWebhookRelay()
//     {
//         StartCoroutine(TestWebhookRelayConnection());
//     }

//     IEnumerator TestConnectionCoroutine()
//     {
//         string testUrl = strapiUrl + "/departments?pagination[pageSize]=1";

//         using (UnityWebRequest request = UnityWebRequest.Get(testUrl))
//         {
//             request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

//             yield return request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 Debug.Log("✓ Successfully connected to Strapi!");
//             }
//             else
//             {
//                 Debug.LogError($"✗ Failed to connect to Strapi: {request.error}");
//             }
//         }
//     }

//     void OnDestroy()
//     {
//         isPolling = false;
//     }

//     // Public getters
//     public int GetTotalProcessed() => totalProcessed;
//     public string GetLastCheckTime() => lastCheckedTime;
//     public bool IsPolling() => isPolling;
//     public bool IsWebhookRelayConnected() => webhookRelayConnected;
//     public int GetWebhookEventsReceived() => webhookEventsReceived;
// }