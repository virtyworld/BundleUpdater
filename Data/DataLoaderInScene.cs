using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedDataRemoteLoaderCatalog
{
    public int FactoryId;
    public string ModelName;
    public string PathToModelSprite;
    public List<SerializedDataRemoteLoaderList> list;
}
[Serializable]
public class SerializedDataRemoteLoaderList
{
    public string SpritePath;
    public float Width;
    public float Length;
    public string ModelName;
    public string ModelPath;
    public int Id;
}
[Serializable]
public class SerializedDataRemoteLoaderRoot
{
    public List<SerializedDataRemoteLoaderCatalog> catalogs;
}