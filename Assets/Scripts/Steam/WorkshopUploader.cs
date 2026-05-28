#if UNITY_STANDALONE
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace NeonSerpent.Steam
{
    /// <summary>
    /// Handles uploading custom levels to Steam Workshop.
    /// Creates a workshop item with level data, thumbnail, and metadata.
    /// </summary>
    public class WorkshopUploader : MonoBehaviour
    {
        [Header("Steam")]
        [SerializeField] private uint appId = 0; // Set your Steam App ID

        [Header("Upload")]
        [SerializeField] private string contentFolder = "WorkshopContent";
        [SerializeField] private string previewImageName = "preview.png";

        [Header("UI")]
        [SerializeField] private GameObject uploadPanel;

        public bool IsUploading => _isUploading;
        public float UploadProgress => _uploadProgress;

        public System.Action<bool, string> OnUploadComplete; // success, errorMessage

        private bool _isUploading;
        private float _uploadProgress;

        /// <summary>
        /// Upload a level to Steam Workshop.
        /// </summary>
        public void UploadLevel(string levelName, string description, string[] tags, string levelJsonPath, string previewImagePath)
        {
            if (_isUploading)
            {
                Debug.LogWarning("[WorkshopUploader] Upload already in progress.");
                return;
            }

            if (!ValidateInputs(levelName, description, levelJsonPath))
                return;

            StartCoroutine(UploadCoroutine(levelName, description, tags, levelJsonPath, previewImagePath));
        }

        /// <summary>
        /// Update an existing workshop item.
        /// </summary>
        public void UpdateWorkshopItem(ulong itemId, string levelName, string description, string[] tags, string levelJsonPath, string previewImagePath)
        {
            if (_isUploading)
            {
                Debug.LogWarning("[WorkshopUploader] Upload already in progress.");
                return;
            }

            StartCoroutine(UpdateCoroutine(itemId, levelName, description, tags, levelJsonPath, previewImagePath));
        }

        /// <summary>
        /// Subscribe to and download a workshop item.
        /// </summary>
        public void SubscribeToItem(ulong itemId)
        {
            // TODO: Implement Steamworks subscription
            Debug.Log($"[WorkshopUploader] Subscribing to item: {itemId}");
        }

        /// <summary>
        /// Get subscribed items.
        /// </summary>
        public void GetSubscribedItems(System.Action<List<WorkshopItem>> callback)
        {
            // TODO: Implement Steamworks query
            callback?.Invoke(new List<WorkshopItem>());
        }

        private System.Collections.IEnumerator UploadCoroutine(string levelName, string description, string[] tags, string levelJsonPath, string previewImagePath)
        {
            _isUploading = true;
            _uploadProgress = 0f;

            // Step 1: Prepare content folder
            _uploadProgress = 0.1f;
            string contentPath = Path.Combine(Application.temporaryCachePath, contentFolder);
            PrepareContentFolder(contentPath, levelJsonPath, previewImagePath);

            yield return null;

            // Step 2: Create Steam Workshop item
            _uploadProgress = 0.2f;
            // TODO: Call SteamUGC.CreateItem(appId, EWorkshopFileType.k_EWorkshopFileTypeCommunity)
            // This is async - wait for callback

            yield return new WaitForSeconds(1f); // Placeholder for async operation

            // Step 3: Set item content
            _uploadProgress = 0.5f;
            // TODO: Call SteamUGC.SetItemTitle, SetItemDescription, SetItemTags, SetItemContent, SetItemPreview

            yield return new WaitForSeconds(1f);

            // Step 4: Submit item
            _uploadProgress = 0.8f;
            // TODO: Call SteamUGC.SubmitItemUpdate

            yield return new WaitForSeconds(1f);

            _uploadProgress = 1f;
            _isUploading = false;

            OnUploadComplete?.Invoke(true, null);
            Debug.Log($"[WorkshopUploader] Upload complete: {levelName}");
        }

        private System.Collections.IEnumerator UpdateCoroutine(ulong itemId, string levelName, string description, string[] tags, string levelJsonPath, string previewImagePath)
        {
            _isUploading = true;
            _uploadProgress = 0f;

            string contentPath = Path.Combine(Application.temporaryCachePath, contentFolder);
            PrepareContentFolder(contentPath, levelJsonPath, previewImagePath);

            _uploadProgress = 0.3f;
            // TODO: Start item update with SteamUGC.StartItemUpdate

            yield return new WaitForSeconds(1f);

            _uploadProgress = 0.7f;
            // TODO: Set updated content and submit

            yield return new WaitForSeconds(1f);

            _uploadProgress = 1f;
            _isUploading = false;

            OnUploadComplete?.Invoke(true, null);
        }

        private void PrepareContentFolder(string contentPath, string levelJsonPath, string previewImagePath)
        {
            // Clean and recreate content folder
            if (Directory.Exists(contentPath))
                Directory.Delete(contentPath, true);
            Directory.CreateDirectory(contentPath);

            // Copy level JSON
            if (File.Exists(levelJsonPath))
            {
                File.Copy(levelJsonPath, Path.Combine(contentPath, "level.json"), true);
            }

            // Copy preview image
            if (File.Exists(previewImagePath))
            {
                File.Copy(previewImagePath, Path.Combine(contentPath, previewImageName), true);
            }
        }

        private bool ValidateInputs(string levelName, string description, string levelJsonPath)
        {
            if (string.IsNullOrWhiteSpace(levelName))
            {
                OnUploadComplete?.Invoke(false, "Level name cannot be empty.");
                return false;
            }

            if (levelName.Length > 128)
            {
                OnUploadComplete?.Invoke(false, "Level name too long (max 128 characters).");
                return false;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                OnUploadComplete?.Invoke(false, "Description cannot be empty.");
                return false;
            }

            if (!File.Exists(levelJsonPath))
            {
                OnUploadComplete?.Invoke(false, "Level file not found.");
                return false;
            }

            return true;
        }
    }

    // ── Data Types ──

    public class WorkshopItem
    {
        public ulong itemId;
        public string title;
        public string description;
        public string[] tags;
        public ulong creatorId;
        public int subscriptions;
        public int ratings;
        public float ratingScore;
        public string previewImageUrl;
        public string contentFolderPath;
    }
}
#endif
