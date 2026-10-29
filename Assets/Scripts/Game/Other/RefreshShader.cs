using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;


public class RefreshShader : MonoBehaviour
{
    void Start()
    {
        TryChangeShader(this.gameObject);
    }
    /// <summary>
    /// 刷新一下(重新挂载)shader
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    private bool TryChangeShader(Object o)
    {
        if (o == null || o.Equals(""))
            return false;
        GameObject go = null;
        try
        {
            go = o as GameObject;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("e:" + e);
        }
        if (go == null) return false;

        Renderer[] mas = go.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in mas)
        {
            Shader temp = Shader.Find(r.sharedMaterial.shader.name);

            if (temp != null)
            {
                foreach (Material mat in r.sharedMaterials)
                {
                    mat.shader = temp;
                }
                //r.sharedMaterial.shader = temp;
            }
        }

        SkinnedMeshRenderer[] all = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (Renderer r in all)
        {
            Shader temp = Shader.Find(r.sharedMaterial.shader.name);

            if (temp != null)
            {
                foreach (Material mat in r.sharedMaterials)
                {
                    mat.shader = temp;
                }
                //r.sharedMaterial.shader = temp;
                //r.materials[0].shader = temp;
            }
        }

        Image[] ima = go.GetComponentsInChildren<Image>(true);
        foreach (Image r in ima)
        {
            Shader temp = Shader.Find(r.material.shader.name);

            if (temp != null)
            {
                r.material.shader = temp;
            }
        }

        SkeletonGraphic[] skeleton = go.GetComponentsInChildren<SkeletonGraphic>(true);
        foreach (SkeletonGraphic r in skeleton)
        {
            Shader temp = Shader.Find(r.material.shader.name);

            if (temp != null)
            {
                r.material.shader = temp;
            }
        }
        return true;
        #region 省略
        //重新获取shader,加载shader
        //Shader temp = Shader.Find("Unlit/Texture");
        //Shader temp2 = Shader.Find("Axiu/EffectsShader");
        //Debug.Log($"FindShader:{temp}");

        //SkinnedMeshRenderer skinnedMeshRenderer;
        //skinnedMeshRenderer = transform.Find("huawa_all/muwa").GetComponent<SkinnedMeshRenderer>();
        //if (skinnedMeshRenderer != null)
        //{
        //    skinnedMeshRenderer.materials[0].shader = temp;
        //}
        /*
         * "laoyeye_all/Object002" "laoyeye_all/a_Object095"
         * "wuwa_all/tuwa"
         * "huowa_all/huowa"
         * "shuiwa_all/shuiwa"
         * 
         */
        #endregion
    }
}
