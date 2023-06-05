using System.Collections;
using UnityEngine;
using YooAsset;

public class PatchMgr : MonoBehaviour
{
    /// <summary>
	/// ��Դϵͳ����ģʽ
	/// </summary>
	[Header("��Դϵͳ����ģʽ")] public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    /// <summary>
	/// Ĭ����Դ����
	/// </summary>
	[Header("Ĭ����Դ����")] public string DefaultPackageName = "DefaultPackage";

    /// <summary>
	/// Ĭ����Դ������IP
	/// </summary>
    [Header("Ĭ����Դ������IP")] public string hostServerIP;

    /// <summary>
	/// ��Դ�汾��
	/// </summary>
    [Header("��Դ�汾��")] public string gameVersion;

    /// <summary>
	/// ���������
	/// </summary>
    [Header("���������")] public int downloadingMaxNum = 10;

    /// <summary>
	/// ʧ�����³��Դ���
	/// </summary>
    [Header("ʧ�����³��Դ���")] public int failedTryAgain = 3;

    // ��Դ������
    private ResourceDownloaderOperation Downloader;

    private void Awake()
    {
        // ��ʼ��Դ����
        Debug.Log($"��Դϵͳ����ģʽ��{PlayMode}");
        Application.targetFrameRate = 60;
        Application.runInBackground = true;

        // ��ʼ����Դϵͳ
        YooAssets.Initialize();
        YooAssets.SetOperationSystemMaxTimeSlice(30);

        // ׼������
        PatchPrepare();
    }

    /// <summary>
    /// ׼������
    /// </summary>
    public void PatchPrepare()
    {
        // ���ظ������
        StartCoroutine(InitPackage());
    }

    /// <summary>
    /// ��ʼ����Դ��
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitPackage()
    {
        yield return new WaitForSeconds(1f);

        // ����Ĭ�ϵ���Դ��
        var package = YooAssets.TryGetPackage(DefaultPackageName);
        if (package == null)
        {
            package = YooAssets.CreatePackage(DefaultPackageName);
            YooAssets.SetDefaultPackage(package);
        }
        else
        {
            Debug.LogError("��ȡĬ����Դ��ʧ��, ����Ĭ����Դ�����Ƿ���д��ȷ");
            yield break;
        }

        // �༭���µ�ģ��ģʽ
        InitializationOperation initializationOperation = null;
        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            var createParameters = new EditorSimulateModeParameters();
            createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(DefaultPackageName);
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // ��������ģʽ
        if (PlayMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeParameters();
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // ��������ģʽ
        if (PlayMode == EPlayMode.HostPlayMode)
        {
            var createParameters = new HostPlayModeParameters();
            createParameters.QueryServices = new QueryStreamingAssetsFileServices();
            createParameters.DefaultHostServer = hostServerIP;
            createParameters.FallbackHostServer = hostServerIP;
            initializationOperation = package.InitializeAsync(createParameters);
        }

        yield return initializationOperation;
        if (initializationOperation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("[��ʼ����Դ��-���]");
            yield return GetStaticVersion();
        }
        else
        {
            Debug.LogWarning($"{initializationOperation.Error}");
        }
    }

    /// <summary>
    /// ��ȡ���°汾
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetStaticVersion()
    {
        Debug.Log("[��ʼ��ȡ���°汾]");
        yield return new WaitForSecondsRealtime(0.5f);

        var package = YooAssets.GetPackage(DefaultPackageName);
        var operation = package.UpdatePackageVersionAsync();
        yield return operation;

        if (operation.Status == EOperationStatus.Succeed)
        {
            gameVersion = operation.PackageVersion;
            Debug.Log("[��ȡ���°汾]-�ɹ�");
            yield return UpdateManifest();
        }
        else
        {
            Debug.LogWarning(operation.Error);
        }
    }

    /// <summary>
    /// ������Դ�嵥
    /// </summary>
    private IEnumerator UpdateManifest()
    {
        Debug.Log("[��ʼ������Դ�嵥]");
        yield return new WaitForSecondsRealtime(0.5f);

        bool savePackageVersion = true;
        var package = YooAssets.GetPackage(DefaultPackageName);
        var operation = package.UpdatePackageManifestAsync(gameVersion, savePackageVersion);
        yield return operation;

        if (operation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("[������Դ�嵥-���]");
            yield return CreateDownloader();
        }
        else
        {
            Debug.LogWarning(operation.Error);
        }
    }

    /// <summary>
    /// ����������
    /// </summary>
    private IEnumerator CreateDownloader()
    {
        Debug.Log("[��ʼ����������]");
        yield return new WaitForSecondsRealtime(0.5f);

        var downloader = YooAssets.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
        Downloader = downloader;

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("��Ҫ���ص��ļ�Ϊ0 !");
            yield return GameStart();
        }
        else
        {
            Debug.Log($"[����������-�ɹ�] ���θ��¹���Ҫ���� {downloader.TotalDownloadCount} ���ļ� ��");

            // �����¸����ļ��󣬹�������ϵͳ
            // ע�⣺��������Ҫ������ǰ�����̿ռ䲻��
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;

            yield return BeginDownload();
        }
    }

    /// <summary>
    /// ��ʼ����
    /// </summary>
    private IEnumerator BeginDownload()
    {
        Debug.Log("[��ʼ����]");

        // ע�����ػص�
        Downloader.OnDownloadErrorCallback = OnDownloadErrorCallback;
        Downloader.OnDownloadProgressCallback = OnDownloadProgressCallback;
        Downloader.BeginDownload();
        yield return Downloader;

        // ������ؽ��
        if (Downloader.Status != EOperationStatus.Succeed)
            yield break;

        Debug.Log("[����-���]");

        var package = YooAssets.GetPackage(DefaultPackageName);
        var operation = package.ClearUnusedCacheFilesAsync();

        yield return GameStart();
    }

    public void OnDownloadErrorCallback(string fileName, string error)
    {
        
    }

    public void OnDownloadProgressCallback(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        
    }

    public IEnumerator GameStart()
    {
        var package = YooAssets.GetPackage(DefaultPackageName);
        LoadDll loadDll = GetComponent<LoadDll>();
        yield return loadDll.StartGame(package);
    }

    // �����ļ���ѯ������
    private class QueryStreamingAssetsFileServices : IQueryServices
    {
        public bool QueryStreamingAssets(string fileName)
        {
            // StreamingAssetsHelper.cs��̫��ս�����ṩ��һ����ѯ�ű���
            string buildinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
            return StreamingAssetsHelper.FileExists($"{buildinFolderName}/{fileName}");
        }
    }
}

