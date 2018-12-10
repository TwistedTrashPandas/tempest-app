using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MastersOfTempest.Environment.VisualEffects
{
    public class SetTornadoTexture : MonoBehaviour
    {

        public int startIdx;
        public int endIdx;
        public int skipIdx;


        public string densityTexPrefix;
        public string normalTexPrefix;
        public float g_fTimeDiff;
        public float g_fTimeStepTex;

        public Transform camPos;
        public VectorField vectorField;

        public Texture2D[] densityTextures;
        public Texture2D[] normalTextures;

        private Material material;
        private int currIdx;

        // Use this for initialization
        void Start()
        {
            material = GetComponent<MeshRenderer>().material;
            currIdx = 0;
            LoadTextures();
            //material.EnableKeyword("Albedo");
            //material.EnableKeyword("_NORMALMAP");
            //material.SetFloat("g_fTimeStepTex", g_fTimeStepTex);
            transform.position = vectorField.GetCenter();
            Vector3 dims = vectorField.GetDimensions();
            transform.localScale = new Vector3(dims.x, dims.z, dims.y) * vectorField.GetCellSize() / 10f;
            StartCoroutine(UpdateTextures());
            if (camPos == null)
                camPos = Camera.main.transform;
        }

        private void LoadTextures()
        {
            //densityTextures = new Texture2D[(endIdx - startIdx) / skipIdx];
            normalTextures = new Texture2D[(endIdx - startIdx) / skipIdx];
            for (int i = startIdx; i < endIdx; i += skipIdx)
            {
                string filepath = Application.dataPath + "/UniFiles/DensityTextures/" + densityTexPrefix + i.ToString("D" + 4) + ".png";
                byte[] buffer = Tools.FileHandling.ReadFile(filepath);
                //densityTextures[(i - startIdx) / skipIdx] = new Texture2D(720, 1024, TextureFormat.RGBA32, true);
                //densityTextures[(i - startIdx) / skipIdx].LoadImage(buffer);
                filepath = Application.dataPath + "/UniFiles/NormalTextures/" + normalTexPrefix + i.ToString("D" + 4) + ".png";
                buffer = Tools.FileHandling.ReadFile(filepath);
                normalTextures[(i - startIdx) / skipIdx] = new Texture2D(720, 1024, TextureFormat.RGBA32, true);
                normalTextures[(i - startIdx) / skipIdx].LoadImage(buffer);
            }
        }

        private void Update()
        {
            Vector3 look = -camPos.position + transform.position;
            look.y = 0;
            look = Vector3.Normalize(look);
            float angle = Mathf.Rad2Deg * (Mathf.Atan2(look.x, look.z) - Mathf.Atan2(0f, 1f));
            angle = (angle < 0f) ? 360f + angle : angle;
            transform.localRotation = Quaternion.Euler(90f, 180f + angle, 0f);
        }

        IEnumerator UpdateTextures()
        {
            while (true)
            {
                Vector3 look = -camPos.position + vectorField.GetCenter();
                look.y = 0;
                look = Vector3.Normalize(look);
                float angle = Mathf.Rad2Deg * (Mathf.Atan2(look.x, look.z) - Mathf.Atan2(0f, 1f));
                angle = (angle < 0f) ? 360f + angle : angle;

                int idx = Mathf.FloorToInt(angle / 5f);
                int tmpIdx = currIdx + idx;

                material.SetTexture("_MainTex", densityTextures[tmpIdx % densityTextures.Length]);
                material.SetTexture("_BumpMap", normalTextures[(72-idx) % normalTextures.Length]);

                // material.SetTexture("g_Tex2", densityTextures[(tmpIdx + 1) % densityTextures.Length]);
                //material.SetTexture("g_NormalTex2", normalTextures[(tmpIdx + 1) % densityTextures.Length]);
                // material.SetFloat("g_fTimeDiff", (angle - tmpIdx * 5f) / 5f * g_fTimeStepTex);
                //material.SetFloat("g_fTimeStepTex", g_fTimeStepTex);
                currIdx = (currIdx + 1) % densityTextures.Length;

                yield return new WaitForSecondsRealtime(g_fTimeStepTex);
            }
        }

    }
}
