using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharpnessDispatcher : MonoBehaviour {
    public Shader sharpnessShader;
    
    [Range(-10.0f, 10.0f)]
    public float amount = 0.0f;

    private Material sharpnessMat;
    
    void OnEnable() {
        if (sharpnessShader != null)
        {
            sharpnessMat = new Material(sharpnessShader);
            sharpnessMat.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void OnDisable() {
        if (sharpnessMat != null)
        {
            DestroyImmediate(sharpnessMat);
            sharpnessMat = null;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (sharpnessMat == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        sharpnessMat.SetFloat("_Amount", amount);
        Graphics.Blit(source, destination, sharpnessMat);
    }
}
