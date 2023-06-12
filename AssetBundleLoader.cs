using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class AssetBundleLoader : MonoBehaviour
{   
    [SerializeField] private GameObject loadingPanel; // Panel to display while loading

    private List<AssetBundle> assetBundlesList = new List<AssetBundle>();
    private GameObject _loadingPanel;
    private Canvas _canvas;
    private Action allBundleWasLoadedAction;
    private bool isRunning;
    
    public static AssetBundleLoader Instance { get; private set; }
    public bool IsRunning => isRunning;
    
    public void Setup(Action allBundleWasLoadedAction)
    {
        this.allBundleWasLoadedAction = allBundleWasLoadedAction;
    }
    
    private void Awake()
    {
        // Make sure there is only one instance of the AssetBundleLoader
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }    

    public async Task  StartLogic()
    {       
        ShowLoadingPanel();
        isRunning = true;
        string bundleFromPlayerPrefs = PlayerPrefs.GetString("loadBundles","");
        AssetBundleDataUpdater abdu = JsonUtility.FromJson<AssetBundleDataUpdater>(bundleFromPlayerPrefs);
        
        foreach (AssetBundleDataUpdaterChild bundle in abdu.bundles)
        {
            string path = Directory.GetCurrentDirectory()+"/AssetBundles/StandaloneWindows/"+bundle.bundleName;          
          
            if (!string.IsNullOrEmpty(bundle.bundleName))
            {
                if (File.Exists(path))
                {                   
                    await LoadLocalAssetBundle(path);
                }
            }
        }
        HideLoadingPanel();
        allBundleWasLoadedAction?.Invoke();        
    }
    
    private void OnDestroy()
    {
        foreach (var assetBundle in assetBundlesList)
        {
            assetBundle.Unload(false);
        }
    }

    #region loaders

    private async Task LoadLocalAssetBundle(string path)
    {
        var tcs = new TaskCompletionSource<bool>();

        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);

        request.completed += operation =>
        {
            assetBundlesList.Add(request.assetBundle);
            bool success = request.assetBundle != null;
            tcs.SetResult(success);
        };

        await tcs.Task;
    }

    
    #endregion

    #region getters
    public Sprite GetSpriteFromBundle(string assetName)
    {
        ShowLoadingPanel();
        Sprite sprite = null;
        
        foreach (var assetBundle in assetBundlesList.Where(ab => ab != null))
        {
            sprite = assetBundle.LoadAsset<Sprite>(assetName);
            if (sprite != null)
            {
                break;
            }
        }
        HideLoadingPanel();
        return sprite;
    }
    
    public Texture2D GetTexture2D(string assetName)
    {
        ShowLoadingPanel();
        Texture2D texture2D = null;
        
        foreach (var assetBundle in assetBundlesList.Where(ab => ab != null))
        {
            texture2D = assetBundle.LoadAsset<Texture2D>(assetName);
            if (texture2D != null)
            {
                break;
            }
        }
        HideLoadingPanel();
        return texture2D;
    }
    
    public Material GetMaterial(string assetName)
    {
        ShowLoadingPanel();
        Material material = null;
        foreach (var assetBundle in assetBundlesList.Where(ab => ab != null))
        {
            material = assetBundle.LoadAsset<Material>(assetName);
            if (material != null)
            {
                break;
            }
        }
        HideLoadingPanel();
        return material;
    }
    
    public Texture GetTexture(string assetName)
    {
        ShowLoadingPanel();
        Texture texture = null;
        foreach (var assetBundle in assetBundlesList.Where(ab => ab != null))
        {
            texture = assetBundle.LoadAsset<Texture>(assetName);
            if (texture != null)
            {
                break;
            }
        }

        HideLoadingPanel();
        return texture;
    }
    public GameObject GetGameObject(string assetName)
    {
        ShowLoadingPanel();
        GameObject gameObject = null;
        foreach (var assetBundle in assetBundlesList.Where(ab => ab != null))
        {
            gameObject = assetBundle.LoadAsset<GameObject>(assetName);
            if (gameObject != null)
            {
                break;
            }
        }

        HideLoadingPanel();
        return gameObject;
    }
    #endregion
    
    private void ShowLoadingPanel()
    {
        if (_canvas == null)  _canvas = FindObjectOfType<Canvas>();
        if (_loadingPanel == null) _loadingPanel = Instantiate(loadingPanel,_canvas.transform);
    }
    
    private void HideLoadingPanel()
    {
        Destroy(_loadingPanel);
    }   
}
