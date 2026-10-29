using SBoxApi;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SBoxInit : MonoSingleton<SBoxInit>
{
    private UnityAction onComplete;

    public void Init(string matchIp, UnityAction onComplete = null)
    {
        MatchDebugManager.Instance.InitUdpNet(matchIp);//ip¡¨Ω”
        this.onComplete = onComplete;
        if (!transform.TryGetComponent(out SBox Sbox))
            gameObject.AddComponent<SBox>();

        SBox.Init();
        AddEventListener();
        StartCoroutine(CheckSBoxReady());
    }

    private void AddEventListener()
    {
        EventCenter.Instance.AddEventListener<SBoxResetData>(SBoxEventHandle.SBOX_RESET, OnSBoxReset);
        EventCenter.Instance.AddEventListener<int>(SBoxEventHandle.SBOX_SADNBOX_RESET, OnSBoxSanboxRest);
    }



    private void OnSBoxReset(SBoxResetData resetData)
    {
        StartCoroutine(CheckSanboxReady());
    }

    private void OnSBoxSanboxRest(int result)
    {
        SBoxModel.Instance.macId = SBoxSandbox.DeviceId();

        ////Test macId
        SBoxModel.Instance.macId = 1;

        onComplete?.Invoke();
    }

    private IEnumerator CheckSanboxReady()
    { 
        while (!SBoxModel.Instance.isSboxSandboxReady)
        {
            SBoxModel.Instance.isSboxSandboxReady = SBoxSandbox.Ready();
            yield return new WaitForSeconds(0.5f);
        }
        SBoxSandbox.Reset();
        SBoxSandboxListener.Instance.Init();
    }

    private IEnumerator CheckSBoxReady()
    {
        while (!SBoxModel.Instance.isSboxReady)
        {
            SBoxModel.Instance.isSboxReady = SBoxIdea.Ready();
            yield return new WaitForSeconds(0.5f);
        }
        SBoxIdea.Reset();
    }
}
