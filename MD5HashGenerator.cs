using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class MD5HashGenerator : MonoBehaviour
{
    [SerializeField] private string bundleName;
    
    private string textFileName ;
    private string md5Hash;
    
    void Start()
    {
        textFileName = bundleName + "hash.txt";
        GetMD5HashFromLocalFile(bundleName);
        SaveToPc();
    }

    private void  GetMD5HashFromLocalFile(string bundleName)
    {
        string filePath = Directory.GetCurrentDirectory() + "/AssetBundles/StandaloneWindows/"+bundleName;
        
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hashBytes = md5.ComputeHash(stream);
                md5Hash=  BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }

    private void SaveToPc()
    {
        string dirPath = Directory.GetCurrentDirectory() + "/AssetBundles/StandaloneWindows/";
        string filePath = dirPath+textFileName;

        if (Directory.Exists(dirPath))
        {
            // Check if the file exists
            if (File.Exists(filePath))
            {
                // Delete the old file
                File.Delete(filePath);
            }
          
            File.WriteAllText(filePath, md5Hash);
        }
    }
}
