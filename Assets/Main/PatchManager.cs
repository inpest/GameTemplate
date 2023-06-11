using System.Collections;
using UnityEngine;
using Util;
using YooAsset;

public class PatchManager : MonoBehaviour
{
    /// <summary>
	/// 资源系统运行模式
	/// </summary>
	[Header("资源系统运行模式")] public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    /// <summary>
	/// 默认资源包名
	/// </summary>
	[Header("默认资源包名")] public string DefaultPackageName = "DefaultPackage";

    /// <summary>
	/// 默认资源服务器地址
	/// </summary>
    [Header("默认资源服务器IP")] public string HostServerIP;

    /// <summary>
	/// 默认资源服务器地址
	/// </summary>
    [Header("默认资源服务器IP")] public string FallbackHostServerIP;

    /// <summary>
	/// 资源版本号
	/// </summary>
    [Header("资源版本号")] public string gameVersion;

    /// <summary>
	/// 最大下载数
	/// </summary>
    [Header("最大下载数")] public int downloadingMaxNum = 10;

    /// <summary>
	/// 失败重新尝试次数
	/// </summary>
    [Header("失败重新尝试次数")] public int failedTryAgain = 3;

    /// <summary>
	/// 是否在末尾添加时间戳
	/// </summary>
    [Header("是否在末尾添加时间戳")] public bool appendTimeTicks;

    // 资源下载器
    public ResourceDownloaderOperation Downloader;

    private void Awake()
    {
        // 开始资源加载
        Debug.Log($"资源系统运行模式：{PlayMode}");

        // 初始化资源系统
        YooAssets.Initialize();
        YooAssets.SetOperationSystemMaxTimeSlice(30);
    }

    private void Start()
    {
        // 准备工作
        PatchPrepare();
    }

    /// <summary>
    /// 准备工作
    /// </summary>
    public void PatchPrepare()
    {
        // 加载更新面板
        StartCoroutine(InitPackage());
    }

    /// <summary>
    /// 初始化资源包
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitPackage()
    {
        EasyEvent.Fire(EventStr.PatchInit);
        yield return new WaitForSecondsRealtime(1f);

        // 创建默认的资源包
        var package = YooAssets.TryGetPackage(DefaultPackageName);
        if (package == null)
        {
            package = YooAssets.CreatePackage(DefaultPackageName);
            YooAssets.SetDefaultPackage(package);
        }
        else
        {
            Debug.LogError("获取默认资源包失败, 请检查默认资源包名是否填写正确");
            yield break;
        }

        // 编辑器下的模拟模式
        InitializationOperation initializationOperation = null;
        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            var createParameters = new EditorSimulateModeParameters();
            createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(DefaultPackageName);
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // 单机运行模式
        if (PlayMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeParameters();
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // 联机运行模式
        if (PlayMode == EPlayMode.HostPlayMode)
        {
            var createParameters = new HostPlayModeParameters();
            createParameters.QueryServices = new QueryStreamingAssetsFileServices();
            createParameters.DefaultHostServer = HostServerIP;
            createParameters.FallbackHostServer = FallbackHostServerIP;
            initializationOperation = package.InitializeAsync(createParameters);
        }

        yield return initializationOperation;
        if (initializationOperation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("[初始化资源包-完成]");
            yield return GetStaticVersion();
        }
        else
        {
            Debug.LogWarning($"{initializationOperation.Error}");
        }
    }

    /// <summary>
    /// 获取最新版本
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetStaticVersion()
    {
        Debug.Log("[开始获取最新版本]");
        EasyEvent.Fire(EventStr.GetLastVersion);
        yield return new WaitForSecondsRealtime(1f);

        var package = YooAssets.GetPackage(DefaultPackageName);
        var operation = package.UpdatePackageVersionAsync(appendTimeTicks);
        yield return operation;

        if (operation.Status == EOperationStatus.Succeed)
        {
            gameVersion = operation.PackageVersion;
            Debug.Log("[获取最新版本]-成功");
            yield return UpdateManifest();
        }
        else
        {
            Debug.LogWarning(operation.Error);
        }
    }

    /// <summary>
    /// 更新资源清单
    /// </summary>
    private IEnumerator UpdateManifest()
    {
        Debug.Log("[开始更新资源清单]");
        EasyEvent.Fire(EventStr.UpdateManifest);
        yield return new WaitForSecondsRealtime(1f);

        bool savePackageVersion = true;
        var package = YooAssets.GetPackage(DefaultPackageName);
        var operation = package.UpdatePackageManifestAsync(gameVersion, savePackageVersion);
        yield return operation;

        if (operation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("[更新资源清单-完成]");
            yield return CreateDownloader();
        }
        else
        {
            Debug.LogWarning(operation.Error);
        }
    }

    /// <summary>
    /// 创建下载器
    /// </summary>
    private IEnumerator CreateDownloader()
    {
        Debug.Log("[开始创建下载器]");
        EasyEvent.Fire(EventStr.CreateDownloader);
        yield return new WaitForSecondsRealtime(1f);

        var downloader = YooAssets.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
        Downloader = downloader;

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("需要下载的文件为0 !");
            yield return GameStart();
        }
        else
        {
            Debug.Log($"[创建下载器-成功] 本次更新共需要下载 {downloader.TotalDownloadCount} 个文件 ！");

            // 发现新更新文件后，挂起流程系统
            // 注意：开发者需要在下载前检测磁盘空间不足
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;

            EasyEvent.Fire(EventStr.DownloadStart, new object[] { totalDownloadCount, totalDownloadBytes });
        }
    }

    /// <summary>
    /// 下载完成
    /// </summary>
    public IEnumerator DownloadComplete()
    {
        Debug.Log("[下载-完成]");
        var package = YooAssets.GetPackage(DefaultPackageName);
        var operation = package.ClearUnusedCacheFilesAsync();
        yield return operation;
        yield return GameStart();
    }

    public IEnumerator GameStart()
    {
        var package = YooAssets.GetPackage(DefaultPackageName);
        LoadDll loadDll = GetComponent<LoadDll>();
        yield return loadDll.StartGame(package);
        EasyEvent.Fire(EventStr.GameStart);
    }

    // 内置文件查询服务类
    private class QueryStreamingAssetsFileServices : IQueryServices
    {
        public bool QueryStreamingAssets(string fileName)
        {
            // StreamingAssetsHelper.cs是太空战机里提供的一个查询脚本。
            string buildinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
            return StreamingAssetsHelper.FileExists($"{buildinFolderName}/{fileName}");
        }
    }
}

