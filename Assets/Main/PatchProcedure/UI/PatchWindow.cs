using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Util;
using YooAsset;

public class PatchWindow : MonoBehaviour
{
    public Slider ProgressBar;
    public PatchManager patchManager;
    public Text info;

    private void Awake()
    {
        EasyEvent.Register(EventStr.PatchInit, PatchInit);
        EasyEvent.Register(EventStr.GetLastVersion, GetLastVersion);
        EasyEvent.Register(EventStr.UpdateManifest, UpdateManifest);
        EasyEvent.Register(EventStr.CreateDownloader, CreateDownloader);
        EasyEvent.Register(EventStr.DownloadStart, DownloadStart);
        EasyEvent.Register(EventStr.GameStart, GameStart);
    }

    private void DownloadStart(object[] obj)
    {
        ProgressBar.value = 0;
        int totalDownloadCount = (int)obj[0];
        long totalDownloadBytes = (long)obj[1];

        info.text = $"按下D键开始下载资源 本次共需要下载{totalDownloadCount}个文件 大小{totalDownloadBytes / 1048576f}MB";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(BeginDownload());
        }
    }

    private void GameStart(object[] obj)
    {
        StartCoroutine(ProgressBarAnim(1));
        info.text = "准备进入游戏";
    }

    private void CreateDownloader(object[] obj)
    {
        StartCoroutine(ProgressBarAnim(1));
        info.text = "创建下载器";
    }

    private void UpdateManifest(object[] obj)
    {
        StartCoroutine(ProgressBarAnim(1));
        info.text = "更新资源清单";
    }

    private void GetLastVersion(object[] obj)
    {
        StartCoroutine(ProgressBarAnim(1));
        info.text = "获取游戏最新版本";
    }

    private void PatchInit(object[] obj)
    {
        StartCoroutine(ProgressBarAnim(1));
        info.text = "开始初始化补丁流程";
    }

    private void OnDestroy()
    {
        EasyEvent.UnRegister(EventStr.PatchInit);
        EasyEvent.UnRegister(EventStr.GetLastVersion);
        EasyEvent.UnRegister(EventStr.UpdateManifest);
        EasyEvent.UnRegister(EventStr.CreateDownloader);
        EasyEvent.UnRegister(EventStr.DownloadStart);
        EasyEvent.UnRegister(EventStr.GameStart);
    }


    IEnumerator BeginDownload()
    {
        // 注册下载回调
        patchManager.Downloader.OnDownloadErrorCallback = OnDownloadErrorCallback;
        patchManager.Downloader.OnDownloadProgressCallback = OnDownloadProgressCallback;
        patchManager.Downloader.BeginDownload();
        yield return patchManager.Downloader;

        // 检测下载结果
        if (patchManager.Downloader.Status != EOperationStatus.Succeed)
            yield break;
        yield return patchManager.DownloadComplete();
    }

    public void OnDownloadErrorCallback(string fileName, string error)
    {
        info.text = $"下载{fileName}文件出现错误 错误原因:{error}";
    }

    public void OnDownloadProgressCallback(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        info.text = $"正在下载{currentDownloadCount}/{totalDownloadCount} 进度:{(currentDownloadBytes / 1048576f).ToString("f2")}MB/{(totalDownloadBytes / 1048576f).ToString("f2")}MB";
        ProgressBar.value = (float)currentDownloadBytes / (float)totalDownloadBytes;
    }

    IEnumerator ProgressBarAnim(float time)
    {
        ProgressBar.value = 0;
        while (ProgressBar.value <= 100)
        {
            yield return new WaitForSeconds(time / 100f);
            ProgressBar.value++;
        }
    }
}
