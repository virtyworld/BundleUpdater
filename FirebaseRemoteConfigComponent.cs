using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public class FirebaseRemoteConfigComponent : MonoBehaviour
{
    private string loadInScene;
    private string bundles;
    private string menuList;
    private Action scriptFinishedAction;
    private Dictionary<string, string> materialRemoteConfigDictionary = new Dictionary<string, string>();
    private List<string> materialNameList = new List<string>();
    private bool isRequestToUserDb;

    public void Setup(Action scriptFinishedAction)
    {
        this.scriptFinishedAction = scriptFinishedAction;
    }
   

    public async void StartLogic()
    {
        if (OPS.LicenceMe.Helper.InternetHelper.CheckForInternetConnection())
        {
           await SendRequestToGetConfigsAndSaveIt();
        }
        
        scriptFinishedAction?.Invoke();
    }
    private async Task SendRequestToGetConfigsAndSaveIt()
    {
        await RequestToUserDb();
        await MenuListLogicAsync();
        SaveConfigsToPlayerPrefs();
    }

    private async Task RequestToUserDb()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        Task<DataSnapshot> getRequestToUserDb = FirebaseDatabase.DefaultInstance.GetReference("remoteConfigs/" + userId).GetValueAsync();
           
        await getRequestToUserDb;
     
        DataSnapshot snapshot = getRequestToUserDb.Result;
       
        if (snapshot != null && snapshot.Child("config").Child("loadBundles").Value != null)
        {
            loadInScene = bundles = menuList = null;
            loadInScene = snapshot.Child("config").Child("loadInScene").Value.ToString();
            bundles = snapshot.Child("config").Child("loadBundles").Value.ToString();
            menuList = snapshot.Child("config").Child("menuList").Value.ToString();
        }
        else
        {
            loadInScene = PlayerPrefs.GetString("loadInScene");
            bundles = PlayerPrefs.GetString("loadBundles");
            menuList = PlayerPrefs.GetString("menuList");
            Debug.Log("Got null value from database. Retrying...");
        }
        
    }

    private async Task MenuListLogicAsync()
    {
        ParseMenuList();
        await  SendRequestToGetRemoteMaterialConfigs();
    }

    private void ParseMenuList()
    {
        RootMenuListInEditorData rootMenu = JsonUtility.FromJson<RootMenuListInEditorData>(menuList);

        foreach (var menu in rootMenu.menuList)
        {
            materialNameList.Add(menu.buttonName);
            ParseMenuListRecursion(menu.menuList);
        }
    }

    private void ParseMenuListRecursion(List<MenuListInEditorData> menuListInEditorDatas)
    {
        foreach (var menu in menuListInEditorDatas)
        {
            materialNameList.Add(menu.buttonName);
            ParseMenuListRecursion(menu.menuList);
        }
    }

    private async Task SendRequestToGetRemoteMaterialConfigs()
    {
        List<Task> requestTasks = new List<Task>();
        
        foreach (string matName in materialNameList)
        {
            requestTasks.Add(RequestToMaterialsDB(matName));
        }

        await Task.WhenAll(requestTasks);
    }

    private async Task RequestToMaterialsDB(string materialName)
    {
        Task<DataSnapshot> getRequestToMaterialsDb = FirebaseDatabase.DefaultInstance.GetReference("materials/" + materialName)
            .GetValueAsync();

         await getRequestToMaterialsDb;
       
         DataSnapshot snapshot = getRequestToMaterialsDb.Result;

         if (snapshot != null  && snapshot.Value != null)
        {
            materialRemoteConfigDictionary.Add(materialName,snapshot.Value.ToString());
        }
        else
        {
            Debug.Log("Got null value from database.");
        }
        
    }

    private void SaveConfigsToPlayerPrefs()
    {
        PlayerPrefs.SetString("loadInScene", loadInScene);
        PlayerPrefs.SetString("loadBundles", bundles);
        PlayerPrefs.SetString("menuList", menuList);
        foreach (var thing in materialRemoteConfigDictionary)
        {
            PlayerPrefs.SetString(thing.Key, thing.Value);
        }
        PlayerPrefs.Save();
    }
}  
