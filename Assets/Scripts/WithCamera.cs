using System.Collections;
using System.Collections.Generic;
using UnityEngine;
   
public class RotateWithCam : MonoBehaviour
    {
        public GameObject go;
        private Camera mainCam;
        private Transform target;
    void Awake()
        {
            mainCam = Camera.main;
            if (mainCam != null) target = mainCam.transform;
        }
    void LateUpdate()
        {
        // Не обрабатываем во время паузы
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
            
          //attach Game Object go to target
            if (target == null && mainCam != null) target = mainCam.transform;
            if (target != null && go != null) go.transform.parent = target;
        }
    }