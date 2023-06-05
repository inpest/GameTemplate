using HybridCLR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class LoadDll : MonoBehaviour
{
    public IEnumerator StartGame(ResourcePackage package)
    {
        Debug.Log("[StartGame]");
        LoadMetadataForAOTAssemblies();

#if !UNITY_EDITOR
        var handle1 = package.LoadRawFileAsync("Assembly-CSharp.dll");
        yield return handle1;
        byte[] fileData = handle1.GetRawFileData();
        System.Reflection.Assembly.Load(fileData);
#endif
        AssetOperationHandle handle = package.LoadAssetAsync<GameObject>("HotUpdatePrefab");
        yield return handle;
        GameObject go = handle.InstantiateSync();
    }



    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private static void LoadMetadataForAOTAssemblies()
    {
        List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
        };
        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        /// 
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in aotMetaAssemblyFiles)
        {
            byte[] dllBytes = Resources.Load<TextAsset>("AOTDLLs/" + aotDllName).bytes;
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }
}
