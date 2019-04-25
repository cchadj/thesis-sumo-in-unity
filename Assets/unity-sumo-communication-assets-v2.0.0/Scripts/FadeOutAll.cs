using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutAll : MonoBehaviour 
{

	public float fadeSpeed = 0.5f;
	public float duration = 1f;
	private Renderer[] _renderers;
	
	private void Awake()
	{
		/* cache renderers so I don't have to GetComponents on update */
		_renderers =  gameObject.GetComponentsInChildren<Renderer> ();
	}

	public void Fade(float targetAlpha)
	{
		StartCoroutine(Fade(targetAlpha, duration));
	}

	private	IEnumerator Fade(float targetAlpha, float duration)
	{
		var startingAlpha = _renderers[0].material.color.a;

		var timeElapsed = 0f;

 		StandardShaderUtils.ChangerRenderersMaterialMode(_renderers, StandardShaderUtils.BlendMode.Fade);
		
		while(timeElapsed < duration)
		{
			SetRenderersAlpha(Mathf.Lerp(startingAlpha, targetAlpha, duration/timeElapsed));
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		SetRenderersAlpha(targetAlpha);

		StandardShaderUtils.ChangerRenderersMaterialMode(_renderers, StandardShaderUtils.BlendMode.Opaque);
	}

	private void SetRenderersAlpha(float alpha)
	{
			foreach (var r in _renderers ) 
			if (r) 
				foreach (var mat in r.materials) 
				{
					var bC = mat.color;
					mat.color = new Color(bC.r, bC.g, bC.b, alpha);
				}
	}
}
public static class StandardShaderUtils
{
     public enum BlendMode
     {
         Opaque,
         Cutout,
         Fade,
         Transparent
     }
 
	public static void ChangerRenderersMaterialMode(Renderer[] rendereds, BlendMode blendMode)
	{
		foreach (var r in rendereds ) 
			if (r)
			 StandardShaderUtils.ChangeRenderMode(r.material, blendMode);
	}

	public static void ChangerRenderersMaterialMode(Material[] materials, BlendMode blendMode)
	{
		foreach (var m in materials ) 
			if (m)
			 StandardShaderUtils.ChangeRenderMode(m, blendMode);
	}

     public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
     {
         switch (blendMode)
         {
             case BlendMode.Opaque:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                 standardShaderMaterial.SetInt("_ZWrite", 1);
                 standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = -1;
                 break;
             case BlendMode.Cutout:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                 standardShaderMaterial.SetInt("_ZWrite", 1);
                 standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = 2450;
                 break;
             case BlendMode.Fade:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                 standardShaderMaterial.SetInt("_ZWrite", 0);
                 standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = 3000;
                 break;
             case BlendMode.Transparent:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                 standardShaderMaterial.SetInt("_ZWrite", 0);
                 standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = 3000;
                 break;
         }
 
     }
 }

