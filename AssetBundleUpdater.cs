using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine.UI;

public class AssetBundleUpdater : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    
    private List<AssetBundleDataUpdaterChild> queueToDownloadBundlesList = new List<AssetBundleDataUpdaterChild>();
    private Action scriptWorkFinished;
    private bool isAuthenticated;  
    
    
    public void Setup(Action scriptWorkFinished)
    {
        this.scriptWorkFinished = scriptWorkFinished;
    }

    public async void StartLogic()
    {
        await InitFirebaseAndCheckAuthUser();
        
        if (OPS.LicenceMe.Helper.InternetHelper.CheckForInternetConnection() && isAuthenticated)
        {
            string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
          
            if (userId != null)
            {
                 CheckForExistingBundlesInFolder();
                if (queueToDownloadBundlesList.Count <= 0) 
                    await CompareBundlesHash();
                
                await DownloadBundles();

                scriptWorkFinished?.Invoke();
            }
        }
        else
        {
            scriptWorkFinished?.Invoke();
        }
    }
    
    private  void CheckForExistingBundlesInFolder()
    {
        string bundles = PlayerPrefs.GetString("loadBundles", "");
        AssetBundleDataUpdater abdu = JsonUtility.FromJson<AssetBundleDataUpdater>(bundles);
        string assetBundleFolderPath = Directory.GetCurrentDirectory() + "/AssetBundles/StandaloneWindows/";
       
        foreach (var data in abdu.bundles)
        {
            if (!File.Exists(Path.Combine(assetBundleFolderPath, data.bundleName)))
            {
                queueToDownloadBundlesList.Add(new AssetBundleDataUpdaterChild
                {
                    bundleName = data.bundleName,
                    bundleUrl = data.bundleUrl,
                    hashFileName = data.hashFileName,
                    hashFileUrl = data.hashFileUrl
                });
            }
        }
    }
    
    private void AddBundleToQueueForDownload(AssetBundleDataUpdaterChild bundle)
    {
        queueToDownloadBundlesList.Add(bundle);
    }
    
    private async Task CompareBundlesHash()
    {
        string bundles = PlayerPrefs.GetString("loadBundles", "");
     
        AssetBundleDataUpdater abdu = JsonUtility.FromJson<AssetBundleDataUpdater>(bundles);

        foreach ( var data in abdu.bundles)
        {
            await CheckBundleHash(data);
        }
    }

    private async Task CheckBundleHash(AssetBundleDataUpdaterChild bundleName)
    {
        string downloadedHashCode = null;
        await GetRemoteHashCode(bundleName.hashFileUrl,hash=>downloadedHashCode=hash);
       
        //if we can't get a hash code, it means, that bundle not exists on server or user don't have a permission
        if (downloadedHashCode == null) return;
     
        Task<string>getMD5HashFromLocalFileAsync =  GetMD5HashFromLocalFileAsync(bundleName);
        
        await getMD5HashFromLocalFileAsync;
        
         string localHash = getMD5HashFromLocalFileAsync.Result;
         
        if (downloadedHashCode != localHash)
        {
            AddBundleToQueueForDownload(bundleName);
            Debug.LogWarning("Asset bundle hash codes do not match!");
        }
        else
        {
            Debug.Log("Asset bundle hash codes match!");
        }
    }
   

    private async Task GetRemoteHashCode(string url, Action<string> onHashCalculated)
    {
        using (WebClient client = new WebClient())
        {
            try
            {
                byte[] fileData = await client.DownloadDataTaskAsync(url);
                string fileContent = Encoding.UTF8.GetString(fileData);
                onHashCalculated?.Invoke(fileContent);
            }
            catch (Exception e)
            {
                Debug.LogError("File download failed: " + e.Message);
                return;
            }
        }
    }

    private async Task DownloadBundles()
    {
        foreach (var bundle in queueToDownloadBundlesList)
        {
            await DownloadFileAsync(bundle);
        }
    }
 
    private async Task DownloadFileAsync(AssetBundleDataUpdaterChild bundle)
    {
        using (WebClient client = new WebClient())
        {
            string path = Directory.GetCurrentDirectory()+"/AssetBundles/StandaloneWindows/"+bundle.bundleName;

            try
            {
                if (!Directory.Exists(Directory.GetCurrentDirectory()+"/AssetBundles/StandaloneWindows/"))
                    Directory.CreateDirectory(Directory.GetCurrentDirectory()+"/AssetBundles/StandaloneWindows/");
                // Download the file asynchronously
                _slider.gameObject.SetActive(true);
                client.DownloadProgressChanged += OnDownloadProgressChanged;
                await client.DownloadFileTaskAsync(new Uri(bundle.bundleUrl), path);
                Debug.Log("File downloaded successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError("File download failed: " + e.Message);
                return;
            }
            finally
            {
                client.DownloadProgressChanged -= OnDownloadProgressChanged;
                _slider.gameObject.SetActive(false);
            }
        }
    }
    private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        float progress = (float)e.BytesReceived / e.TotalBytesToReceive;
        _slider.value = progress;
    }
    
    private async Task<string> GetMD5HashFromLocalFileAsync(AssetBundleDataUpdaterChild bundle)
    {
        string filePath = Directory.GetCurrentDirectory() + "/AssetBundles/StandaloneWindows/"+bundle.bundleName;
    
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hashBytes = await Task.Run(() => md5.ComputeHash(stream));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
    
    private async Task InitFirebaseAndCheckAuthUser()
    {
        var dependencyResult = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyResult == DependencyStatus.Available)
        {
            // Firebase initialization completed
            if (FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                isAuthenticated = true;
            }            
        }
        else
        {
            Debug.LogWarning($"Failed to initialize Firebase: {dependencyResult}");
        }
    }
}


[Serializable]
public class AssetBundleDataUpdaterChild
{
    public string bundleName;
    public string bundleUrl;
    public string hashFileName;
    public string hashFileUrl;
}
[Serializable]
public class AssetBundleDataUpdater
{
    public List<AssetBundleDataUpdaterChild> bundles;
}
