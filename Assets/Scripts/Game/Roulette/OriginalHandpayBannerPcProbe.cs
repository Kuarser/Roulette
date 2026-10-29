using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed class OriginalHandpayBannerPcProbe:MonoBehaviour
    {
        [Serializable] private sealed class Report {public string engine,backend;public int captures=16,checks=50;public bool financialExecution=false;}
        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            try
            {
                if(Application.unityVersion!="2020.3.17f1"||SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Direct3D11)
                    throw new Exception("Exact Unity and actual D3D11 required");
                string[] args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-nativeBannerOutput");
                if(index<0||index+1>=args.Length)throw new Exception("Fresh output argument required");
                string output=Path.GetFullPath(args[index+1]);
                bool w2g=Array.IndexOf(args,"-nativeW2gProbe")>=0;
                if(w2g)OriginalW2GHandpayRenderValidation.Validate();else OriginalHandpayBannerUpdateRenderValidation.Validate();
                int checks=w2g?100:50;
                File.WriteAllText(Path.Combine(output,"player_report.json"),JsonUtility.ToJson(new Report {engine=Application.unityVersion,backend=SystemInfo.graphicsDeviceType.ToString(),checks=checks},true)+"\n");
                Debug.Log((w2g?"W2G_HANDPAY_PC_PASSED":"HANDPAY_BANNER_PC_PASSED")+" captures=16 checks="+checks);Application.Quit(0);
            }
            catch(Exception error){Debug.LogException(error);Application.Quit(8);}
        }
    }
}
