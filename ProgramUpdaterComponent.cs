using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgramUpdaterComponent : MonoBehaviour
{
    [SerializeField] private FirebaseRemoteConfigComponent firebaseRemoteConfigComponent;
    [SerializeField] private AssetBundleUpdater assetBundleUpdater;
    [SerializeField] private AssetBundleLoader assetBundleLoader;
    [SerializeField] private LicenseComponent licenseComponent;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Transform parentToSpawnLoadingPanel;

    private Action firebaseRemoteConfigComponentFinishedAction;
    private Action assetBundleUpdaterFinishedAction;
    private Action autoConnectWithAuthScriptFinishedAction;
    private Action assetBundleLoaderFinishedAction;
    private GameObject _loadingPanel;
    private bool isScriptFinished;
    private bool isAuthenticated;
   
    public bool IsScriptFinished => isScriptFinished;

    private void Awake()
    {
        firebaseRemoteConfigComponentFinishedAction += StartAssetBundleUpdater;
        assetBundleUpdaterFinishedAction += HideLoadingPanel;
        assetBundleUpdaterFinishedAction += StartAssetBundleLoader;
        assetBundleLoaderFinishedAction += ScriptFinished;
    }

    /// <summary>
    /// Launch your lisence componentr to check lisence
    /// </summary>
    public  void  Start()
    {
     licenseComponent.StartLogic();
    }

    /// <summary>
    /// Starts the logic updater when you need
    /// </summary>
    public async void StartUpdater()
    {
        await InitFirebaseAndCheckAuthUser();
       
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("menuList")))
        {
            SaveStandartConfigs();
        }
        //SaveStandartConfigs();//TESTing if we want to check lig on new computers
       
        if (isAuthenticated) StartLogic();       
        else ScriptFinished();        
    }
    
    private void StartLogic()
    {
        ShowLoadingPanel();
        
        firebaseRemoteConfigComponent.Setup(firebaseRemoteConfigComponentFinishedAction);
        assetBundleUpdater.Setup(assetBundleUpdaterFinishedAction);
        assetBundleLoader.Setup(assetBundleLoaderFinishedAction);
        
        if (OPS.LicenceMe.Helper.InternetHelper.CheckForInternetConnection())
        {
            StartFirebaseRemoteConfigComponent();
        }
        else
        {
            StartAssetBundleLoader();
            ScriptFinished();
        } 
    }

    private void StartFirebaseRemoteConfigComponent()
    {
        firebaseRemoteConfigComponent.StartLogic();
    }
    
    private void StartAssetBundleUpdater()
    {
        assetBundleUpdater.StartLogic();
    }

    private void ShowLoadingPanel()
    {
        if (_loadingPanel == null) _loadingPanel = Instantiate(loadingPanel,parentToSpawnLoadingPanel);
    }
    
    private void HideLoadingPanel()
    {
        Destroy(_loadingPanel);
    }

    private async void StartAssetBundleLoader()
    {
        await AssetBundleLoader.Instance.StartLogic();
    }
    
    private void ScriptFinished()
    {
        isScriptFinished = true;
        SceneManager.LoadSceneAsync("SceneLoader");
    }

    /// <summary>
    /// If we don't have an enternet, we can use our program with this configs
    /// </summary>
    private void SaveStandartConfigs()
    {
       PlayerPrefs.SetString("Material", "{\"materialStyle\":\"Default\",\"imagesList\":[\"material-001\",\"material-002\",\"material-003\",\"material-004\",\"material-005\",\"material-006\",\"material-007\",\"material-008\",\"material-009\",\"material-010\",\"material-011\",\"material-012\",\"material-013\",\"material-014\",\"material-015\",\"material-016\",\"material-017\"],\"materialList\":[\"material\"]}");
       PlayerPrefs.Save();
    }
    
    private async Task InitFirebaseAndCheckAuthUser()
    {
        var dependencyResult = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyResult == DependencyStatus.Available)
        {
            // Firebase initialization completed
            if (FirebaseAuth.DefaultInstance.CurrentUser != null)  isAuthenticated = true;           
            else  Debug.Log("false ");           
        }
        else
        {
            Debug.Log($"Failed to initialize Firebase: {dependencyResult}");
        }
    }

    public void LogOut()
    {
        PlayerPrefs.DeleteKey("loadInScene");
        PlayerPrefs.DeleteKey("menuList");
        PlayerPrefs.DeleteKey("Brabus");        
        PlayerPrefs.DeleteKey("loadBundles");
        PlayerPrefs.DeleteKey("hardwareKey");
        PlayerPrefs.DeleteKey("currentLicenceKey");
        PlayerPrefs.DeleteKey("currentMachineId");
        PlayerPrefs.DeleteKey("currentLastDate");
        FirebaseAuth.DefaultInstance.SignOut();
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}
