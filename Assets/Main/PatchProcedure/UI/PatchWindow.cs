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

        info.text = $"����D����ʼ������Դ ���ι���Ҫ����{totalDownloadCount}���ļ� ��С{totalDownloadBytes / 1048576f}MB";
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
        info.text = "׼��������Ϸ";
    }

    private void CreateDownloader(object[] obj)
    {
        StartCoroutine(ProgressBarAnim(1));
        info.text = "����������";
    }

    private void UpdateManifest(object[] obj)
    {
        StartCoroutine(ProgressBarAnim(1));
        info.text = "������Դ�嵥";
    }

    private void GetLastVersion(object[] obj)
    {
        StartCoroutine(ProgressBarAnim(1));
        info.text = "��ȡ��Ϸ���°汾";
    }

    private void PatchInit(object[] obj)
    {
        StartCoroutine(ProgressBarAnim(1));
        info.text = "��ʼ��ʼ����������";
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
        // ע�����ػص�
        patchManager.Downloader.OnDownloadErrorCallback = OnDownloadErrorCallback;
        patchManager.Downloader.OnDownloadProgressCallback = OnDownloadProgressCallback;
        patchManager.Downloader.BeginDownload();
        yield return patchManager.Downloader;

        // ������ؽ��
        if (patchManager.Downloader.Status != EOperationStatus.Succeed)
            yield break;
        yield return patchManager.DownloadComplete();
    }

    public void OnDownloadErrorCallback(string fileName, string error)
    {
        info.text = $"����{fileName}�ļ����ִ��� ����ԭ��:{error}";
    }

    public void OnDownloadProgressCallback(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        info.text = $"��������{currentDownloadCount}/{totalDownloadCount} ����:{(currentDownloadBytes / 1048576f).ToString("f2")}MB/{(totalDownloadBytes / 1048576f).ToString("f2")}MB";
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
