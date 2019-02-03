using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MastersOfTempest.Environment.VisualEffects
{
    //[ExecuteInEditMode]
    public class HeightField : MonoBehaviour
    {
        public struct heightField
        {
            public float height;
            public float velocity;
        }

        struct int2
        {
            public int x;
            public int y;
        }

        public enum WaterMode
        {
            Minimal, Reflection, Obstacles, ReflAndObstcl
        };

        //  public variables

        /// <summary>
        /// 0: simple water 
        /// 1: reflections 
        /// 2: obstacles reflect waves in realtime 
        /// 3: reflections + obstacles 
        /// </summary>       
        public WaterMode waterMode;

        /// <summary>
        /// Compute Shader for heightField updates
        /// </summary>
        public ComputeShader heightFieldCS;
        /// <summary>
        /// Main camera of the scene
        /// </summary>
        public Camera mainCam;

        /// <summary>
        /// The maximum random displacement of the vertices of the generated mesh
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float maxRandomDisplacement;

        /// <summary>
        /// Width of the generated mesh
        /// </summary>
        [Range(1, 2048)]
        public int widthHF;
        /// <summary>
        /// Depth of the generated mesh
        /// </summary>
        [Range(1, 2048)]

        public int depthHF;
        /// <summary>
        /// Width of the generated mesh
        /// </summary>
        [Range(1, 2048)]
        public int widthMesh;
        /// <summary>
        /// Depth of the generated mesh
        /// </summary>
        [Range(1, 2048)]
        public int depthMesh;
        /// <summary>
        /// Distance between vertices of the generated mesh
        /// </summary>
        public float quadSize;

        [Range(0.0f, 180.0f)]
        public float angleBias;

        [Range(1, 2048)]
        public int detailScaleFactor;

        /// <summary>
        /// Speed of waves
        /// </summary>
        public float speed;
        /// <summary>
        /// Also controls the speed of waves/updates
        /// </summary>
        public float gridSpacing;
        /// <summary>
        /// Maximum height values at the vertices
        /// </summary>       
        public float maxHeight;
        /// <summary>
        /// Maximum velocity values at the vertices
        /// </summary>       
        public float maxVelocity;
        /// <summary>
        /// Random inital velocity values at the vertices
        /// </summary>       
        public float randomInitialVelocity;
        /// <summary>
        /// Damping factor to reduce artifacts
        /// </summary>       
        public float dampingVelocity;

        //  private variables
        private ComputeBuffer randomXZ;
        private ComputeBuffer heightFieldCB;
        private ComputeBuffer reflectWavesCB;
        private ComputeBuffer heightFieldCBOut;
        private ComputeBuffer verticesCB;
        private ComputeBuffer normalsCB;
        private ComputeBuffer trianglesRCB;
        private ComputeBuffer normTrianglesCB;

        private Material material;

        private Vector2[] randomDisplacement;
        private float lastMaxRandomDisplacement;
        private float averageHeight;

        //  HEIGHTFIELD
        private heightField[] hf;
        private uint[] environment;
        private int kernel;                     ///   kernel for computeshader
        private int kernelVertices;
        private int kernelVerticesEdges;
        private int kernelTriangles;
        private int kernelNormals;

        private Mesh planeMesh;
        private Vector3[] vertices;

        private uint currentCollision;
        private float lastUpdateHF;

        private int inOutCounter;

        private CreateReflectionTexture crt;
        private float totalTime;
        private float totalTimeX;
        private float totalTimeY;

        private float quadSizeHF;

        private Vector2[,] segmentConfigurations;
        private int lastConfig;
        private Vector3 lastPosition;
        
        public void Initialize(Vector3 midPosition)
        {
            switch (QualitySettings.GetQualityLevel())
            {
                case 0:
                    waterMode = WaterMode.Minimal;
                    widthHF = 128;
                    depthHF = 128;
                    widthMesh = 32;
                    depthMesh = 32;
                    quadSize = 400;
                    break;
                case 1:
                    waterMode = WaterMode.Minimal;
                    widthHF = 128;
                    depthHF = 128;
                    widthMesh = 32;
                    depthMesh = 32;
                    quadSize = 400;
                    break;
                case 2:
                    waterMode = WaterMode.Minimal;
                    widthHF = 256;
                    depthHF = 256;
                    widthMesh = 64;
                    depthMesh = 64;
                    quadSize = 200;
                    break;
                case 3:
                    waterMode = WaterMode.Reflection;
                    widthHF = 512;
                    depthHF = 512;
                    widthMesh = 64;
                    depthMesh = 64;
                    quadSize = 200;
                    break;
                case 4:
                    waterMode = WaterMode.Reflection;
                    widthHF = 512;
                    depthHF = 512;
                    widthMesh = 128;
                    depthMesh = 128;
                    quadSize = 100;
                    break;
                case 5:
                    waterMode = WaterMode.Reflection;
                    widthHF = 1024;
                    depthHF = 1024;
                    widthMesh = 128;
                    depthMesh = 128;
                    quadSize = 100;
                    break;
                default:
                    waterMode = WaterMode.Reflection;
                    widthHF = 1024;
                    depthHF = 1024;
                    widthMesh = 128;
                    depthMesh = 128;
                    quadSize = 100;
                    break;
            }

            waterMode = WaterMode.Minimal;
            inOutCounter = 0;
            currentCollision = 1;
            //transform.position = midPosition - new Vector3(widthMesh * quadSize / 2f, midPosition.y, depthMesh * quadSize / 2f);
            totalTime = 0f;
            totalTimeX = 0f;
            totalTimeY = 0f;
            quadSizeHF = quadSize * widthMesh / (float)widthHF;

            crt = GetComponent<CreateReflectionTexture>();
            if (crt == null)
            {
                gameObject.AddComponent<CreateReflectionTexture>();
                crt = GetComponent<CreateReflectionTexture>();
            }
            mainCam = Camera.main;
            mainCam.depthTextureMode = DepthTextureMode.Depth;

            CreatePlaneMesh();
            initHeightField();
            randomXZ = new ComputeBuffer(widthMesh * depthMesh, 8);
            material = GetComponent<MeshRenderer>().material;
            setRandomDisplacementBuffer();
            CreateMeshWithSubmeshes();
            //CreateMesh();
            initBuffers();
        }
        
        void Update()
        {
            //  if noisy factor changes -> initialize randomDisplacements again
            if (!Mathf.Approximately(maxRandomDisplacement, lastMaxRandomDisplacement))
            {
                setRandomDisplacementBuffer();
            }
            UpdateSteps();
        }

        public void OnWillRenderObject()
        {
            material.SetFloat("g_fTimeMoveNoiseX", totalTimeX * Mathf.PI / 2000f);
            material.SetFloat("g_fTimeMoveNoiseY", totalTimeY * Mathf.PI / 2000f);
            totalTimeX += (UnityEngine.Random.Range(0f, 1f)) * Time.deltaTime;
            totalTimeY += (UnityEngine.Random.Range(0f, 1f)) * Time.deltaTime;
            if (waterMode == WaterMode.ReflAndObstcl || waterMode == WaterMode.Reflection)
            {
                crt.renderReflection(planeMesh, averageHeight);
            }
        }

        private void UpdateSteps()
        {
            //  propagate waves by using linear wave equations
            updateHeightfield();
            UpdateTransformMatrix();
            updateVertices();
        }
        
        public void OnCollisionStay(Collision collision)
        {
            if (waterMode == WaterMode.ReflAndObstcl || waterMode == WaterMode.Obstacles)
            {
                //  temporary indices (collision points)
                int2[] tempIndices = new int2[collision.contacts.Length];
                for (int i = 0; i < collision.contacts.Length; i++)
                {
                    Vector3 coll = collision.contacts[i].point - transform.position;
                    int x = Math.Min(Math.Max(Mathf.RoundToInt(coll.x / quadSizeHF), 0), widthHF - 1);
                    int z = Math.Min(Math.Max(Mathf.RoundToInt(coll.z / quadSizeHF), 0), depthHF - 1);
                    //if (hf[x * depth + z].height + maxHeight > coll.y)
                    environment[x * depthHF + z] = currentCollision;
                    tempIndices[i].x = x;
                    tempIndices[i].y = z;
                }
                //  fill contact points to represent mesh (for reflecting waves)
                for (int i = 0; i < tempIndices.Length; i++)
                {
                    int kTemp = tempIndices[i].x;
                    for (int k = kTemp; k < widthHF; k++)
                    {
                        if (environment[k * depthHF + tempIndices[i].y] == currentCollision)
                        {
                            kTemp = k;
                        }
                    }
                    for (int n = tempIndices[i].x + 1; n < kTemp; n++)
                        environment[n * depthHF + tempIndices[i].y] = currentCollision;

                    kTemp = tempIndices[i].x;
                    for (int k = kTemp; k >= 0; k--)
                    {
                        if (environment[k * depthHF + tempIndices[i].y] == currentCollision)
                        {
                            kTemp = k;
                        }
                    }
                    for (int n = tempIndices[i].x - 1; n >= kTemp; n--)
                        environment[n * depthHF + tempIndices[i].y] = currentCollision;

                    kTemp = tempIndices[i].y;
                    for (int k = kTemp; k < depthHF; k++)
                    {
                        if (environment[tempIndices[i].x * depthHF + k] == currentCollision)
                        {
                            kTemp = k;
                        }
                    }
                    for (int n = tempIndices[i].y + 1; n < kTemp; n++)
                        environment[tempIndices[i].x * depthHF + n] = currentCollision;

                    kTemp = tempIndices[i].y;
                    for (int k = kTemp; k >= 0; k--)
                    {
                        if (environment[tempIndices[i].x * depthHF + k] == currentCollision)
                        {
                            kTemp = k;
                        }
                    }
                    for (int n = tempIndices[i].y - 1; n >= kTemp; n--)
                        environment[tempIndices[i].x * depthHF + n] = currentCollision;
                }
                reflectWavesCB.SetData(environment);
                currentCollision = (currentCollision + 1) % int.MaxValue;
            }
        }

        private void setRandomDisplacementBuffer()
        {
            randomDisplacement = new Vector2[widthMesh * depthMesh];
            for (int i = 0; i < widthMesh; i++)
            {
                for (int j = 0; j < depthMesh; j++)
                {
                    if (i != 0 && j != 0 && i != widthMesh - 1 && j != depthMesh - 1)
                        randomDisplacement[i * depthMesh + j] = new Vector2(UnityEngine.Random.Range(-maxRandomDisplacement * quadSize / 3.0f, maxRandomDisplacement * quadSize / 3.0f),
                        UnityEngine.Random.Range(-maxRandomDisplacement * quadSize / 2.2f, maxRandomDisplacement * quadSize / 2.2f));
                }
            }
            lastMaxRandomDisplacement = maxRandomDisplacement;
            randomXZ.SetData(randomDisplacement);
        }

        private void initHeightField()
        {
            hf = new heightField[widthHF * depthHF];

            hf[(int)(widthHF / 2f * depthHF + depthHF / 2f)].height = maxHeight;
            hf[(int)((widthHF / 2f + 1) * depthHF + depthHF / 2f + 1)].height = maxHeight;
            hf[(int)((widthHF / 2f + 1) * depthHF + depthHF / 2f)].height = maxHeight;
            hf[(int)(widthHF / 2f * depthHF + depthHF / 2f + 1)].height = maxHeight;
            hf[(int)((widthHF / 2f + 1) * depthHF + depthHF / 2f - 1)].height = maxHeight;
            hf[(int)((widthHF / 2f - 1) * depthHF + depthHF / 2f + 1)].height = maxHeight;
            hf[(int)((widthHF / 2f - 1) * depthHF + depthHF / 2f - 1)].height = maxHeight;
            hf[(int)((widthHF / 2f - 1) * depthHF + depthHF / 2f)].height = maxHeight;
            hf[(int)(widthHF / 2f * depthHF + depthHF / 2f - 1)].height = maxHeight;

            for (int i = 0; i < hf.Length; i++)
            {
                hf[i].velocity += UnityEngine.Random.Range(-randomInitialVelocity, randomInitialVelocity);
            }
        }

        private void initBuffers()
        {
            //  initialize buffers
            Mesh mesh = GetComponent<MeshFilter>().mesh;

            heightFieldCB = new ComputeBuffer(widthHF * depthHF, 8);
            heightFieldCBOut = new ComputeBuffer(widthHF * depthHF, 8);
            reflectWavesCB = new ComputeBuffer(widthHF * depthHF, 4);
            verticesCB = new ComputeBuffer(mesh.vertices.Length, 12);
            normalsCB = new ComputeBuffer(mesh.vertices.Length, 12);
            trianglesRCB = new ComputeBuffer(mesh.triangles.Length / 3, 12);
            normTrianglesCB = new ComputeBuffer(mesh.triangles.Length / 3, 12);
            environment = new uint[widthHF * depthHF];

            verticesCB.SetData(mesh.vertices);
            trianglesRCB.SetData(mesh.triangles);

            heightFieldCB.SetData(hf);
            reflectWavesCB.SetData(environment);

            //  get corresponding kernel index
            kernel = heightFieldCS.FindKernel("updateHeightfield");
            kernelVertices = heightFieldCS.FindKernel("interpolateVertices");
            kernelVerticesEdges = heightFieldCS.FindKernel("interpolateVerticesEdges");
            kernelTriangles = heightFieldCS.FindKernel("calcNormTriangles");
            kernelNormals = heightFieldCS.FindKernel("averageNormVertices");
            //  set constants
            heightFieldCS.SetInt("g_iDepth", depthHF);
            heightFieldCS.SetInt("g_iDepthMesh", depthMesh);
            heightFieldCS.SetInt("g_iWidth", widthHF);
            heightFieldCS.SetInt("g_iWidthMesh", widthMesh);
            heightFieldCS.SetInt("g_iScale", detailScaleFactor);
            heightFieldCS.SetFloat("g_fGridSpacing", gridSpacing); // could be changed to quadSize, but does not yield good results

            material.SetFloat("g_fQuadSize", quadSize);
            material.SetInt("g_iDepth", depthHF);
            material.SetInt("g_iWidth", widthHF);

            heightFieldCS.SetBuffer(kernel, "heightFieldIn", heightFieldCB);
            heightFieldCS.SetBuffer(kernel, "reflectWaves", reflectWavesCB);
            heightFieldCS.SetBuffer(kernel, "heightFieldOut", heightFieldCBOut);

            heightFieldCS.SetBuffer(kernelVertices, "heightFieldIn", heightFieldCB);
            heightFieldCS.SetBuffer(kernelVertices, "verticesPosition", verticesCB);
            heightFieldCS.SetBuffer(kernelVertices, "randomDisplacement", randomXZ);
            heightFieldCS.SetBuffer(kernelVerticesEdges, "verticesPosition", verticesCB);

            heightFieldCS.SetBuffer(kernelTriangles, "triangles", trianglesRCB);
            heightFieldCS.SetBuffer(kernelTriangles, "verticesPosition", verticesCB);
            heightFieldCS.SetBuffer(kernelTriangles, "normTriangles", normTrianglesCB);

            heightFieldCS.SetBuffer(kernelNormals, "normTriangles", normTrianglesCB);
            heightFieldCS.SetBuffer(kernelNormals, "verticesNormal", normalsCB);

            material.SetBuffer("verticesPosition", verticesCB);
            material.SetBuffer("verticesNormal", normalsCB);
        }


        //  dispatch of compute shader
        private void updateHeightfield()
        {
            //  calculate approximate average of all points in the heightfield (might be unecessary)
            averageHeight = 0.0f;
            /*int length = (int)Math.Ceiling(Math.Min(depth, width)/2f);

            for (int i = 0; i < length; i++)
            {
                averageHeight += hf[i * depth + i].height;
            }
            for (int i = length - 1; i >= 0; i--)
            {
                int j = 0;
                averageHeight += hf[i * depth + j].height;
                j++;
            }
            averageHeight /= (length*2f);

            print(averageHeight);
            */
            float dt = lastUpdateHF - Time.time;
            lastUpdateHF = Time.time;
            dt = Time.deltaTime;
            totalTime -= Time.deltaTime / 2f;
            if (totalTime < -1f)
                totalTime += 1f;


            heightFieldCS.SetFloat("g_fDeltaTime", Mathf.Min(dt, 0.02f));
            heightFieldCS.SetFloat("g_fTotalTime", totalTime * 2 * Mathf.PI);
            heightFieldCS.SetFloat("g_fSpeed", speed);
            heightFieldCS.SetFloat("g_fMaxVelocity", maxVelocity);
            heightFieldCS.SetFloat("g_fMaxHeight", maxHeight);
            heightFieldCS.SetFloat("g_fDamping", dampingVelocity);
            heightFieldCS.SetFloat("g_fAvgHeight", averageHeight);
            heightFieldCS.SetFloat("g_fGridSpacing", Mathf.Max(gridSpacing, 1f));
            //heightFieldCS.SetFloat("g_fTimeMoveNoiseY", Time.time);

            if (inOutCounter == 0)
            {
                heightFieldCS.SetBuffer(kernel, "heightFieldOut", heightFieldCBOut);
                heightFieldCS.SetBuffer(kernel, "heightFieldIn", heightFieldCB);
            }
            else
            {
                heightFieldCS.SetBuffer(kernel, "heightFieldOut", heightFieldCB);
                heightFieldCS.SetBuffer(kernel, "heightFieldIn", heightFieldCBOut);
            }

            heightFieldCS.Dispatch(kernel, Mathf.CeilToInt(widthHF / 16.0f), Mathf.CeilToInt(depthHF / 16.0f), 1);

            if (inOutCounter == 0)
            {
                //  heightFieldCBOut.GetData(hf);
                heightFieldCS.SetBuffer(kernelVertices, "heightFieldIn", heightFieldCBOut);
            }
            else
            {
                //  heightFieldCB.GetData(hf);
                heightFieldCS.SetBuffer(kernelVertices, "heightFieldIn", heightFieldCB);
            }
            // heightFieldCB.SetData(hf);
            if (waterMode == WaterMode.Obstacles || waterMode == WaterMode.ReflAndObstcl)
                environment = new uint[widthHF * depthHF];

            inOutCounter = (inOutCounter + 1) % 2;
        }

        bool intersectPlane(Vector3 normal, Vector3 offsetPlane, Vector3 offsetRay, Vector3 rayDirection, out float t, out Vector3 intersection)
        {
            // assuming vectors are all normalized
            Vector3 intersect = new Vector3();
            float denom = Vector3.Dot(normal, rayDirection);

            if (Mathf.Abs(denom) > 1e-6)
            {
                Vector3 dist = offsetPlane - offsetRay;
                t = Vector3.Dot(dist, normal) / denom;
                intersection = offsetRay + t * rayDirection;
                return (t >= 0);
            }
            intersection = intersect;
            t = 0.0f;
            return false;
        }

        private void UpdateTransformMatrix()
        {
            transform.rotation = Quaternion.identity;
            float outAngle = mainCam.fieldOfView / 2f + angleBias;
            float rayHit;
            Vector3 ray = Vector3.Normalize(Quaternion.AngleAxis(outAngle, mainCam.transform.right) * mainCam.transform.forward);
            Vector3 destination;
            Vector3 intersect;
            if (intersectPlane(Vector3.up, new Vector3(), mainCam.transform.position, ray, out rayHit, out intersect))
            {
                destination = intersect;
            }
            else
            {
                destination = mainCam.transform.position;
            }

            transform.position = new Vector3(Mathf.RoundToInt(destination.x / quadSize - widthMesh * 1.5f) * quadSize, transform.position.y, Mathf.RoundToInt(destination.z / quadSize - depthMesh / 5f) * quadSize);
            Vector3 look = mainCam.transform.forward;
            int idx = 0;
            if (Mathf.Abs(look.z) > Mathf.Abs(look.x))
            {
                if (look.z > 0)
                {
                }
                else
                {
                    idx = 2;
                }
            }
            else
            {
                if (look.x > 0)
                {
                    idx = 1;
                }
                else
                {
                    idx = 3;
                }
            }

            Matrix4x4 displacementMatrix = Matrix4x4.zero;// new Matrix4x4();
            for (int i = 0; i < 6; i++)
            {
                Vector2 currVec = new Vector2();
                if (idx != lastConfig)
                {
                    currVec = segmentConfigurations[i, idx];
                    currVec -= segmentConfigurations[i, lastConfig];
                }
                displacementMatrix[i % 4, 0 + ((int)(i / 4)) * 2] = currVec.x + transform.position.x - lastPosition.x;
                displacementMatrix[i % 4, 1 + ((int)(i / 4)) * 2] = currVec.y + transform.position.z - lastPosition.z;
            }
            //heightFieldCS.SetMatrix("g_f4x4TransformMatrix", transform.localToWorldMatrix);// Matrix4x4.Rotate(transform.rotation));// Matrix4x4.Translate(translation) * Matrix4x4.Rotate(transform.rotation) * Matrix4x4.Translate(translation));
            heightFieldCS.SetMatrix("g_f4SegDisplacements1", displacementMatrix);
            lastConfig = idx;
            lastPosition = transform.position;
        }

        private void updateVertices()
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh;

            /// update vertex x/z pos -------------------------------------------------------------------------------------------------------------------------------
            Vector3 translation = Vector3.zero;// transform.position - new Vector3(0, 0, mainCam.transform.position.z - quadSize * widthMesh / 2f);


            /// update vertex height --------------------------------------------------------------------------------------------------------------------------------
            heightFieldCS.SetInt("g_iIdxOffset", 0);
            heightFieldCS.SetFloat("g_fQuadSize_w", quadSize * 3f);
            heightFieldCS.SetFloat("g_fQuadSize_d", quadSize * 3f);
            heightFieldCS.SetInt("g_iWidthMesh", widthMesh);
            heightFieldCS.SetInt("g_iDepthMesh", depthMesh);
            heightFieldCS.Dispatch(kernelVertices, Mathf.CeilToInt(widthMesh * depthMesh * 5 / 256f), 1, 1);

            heightFieldCS.SetInt("g_iIdxOffset", widthMesh * depthMesh * 5);
            heightFieldCS.SetFloat("g_fQuadSize_w", quadSize / detailScaleFactor * 3f);
            heightFieldCS.SetFloat("g_fQuadSize_d", quadSize / detailScaleFactor * 3f);
            heightFieldCS.SetInt("g_iWidthMesh", widthMesh * detailScaleFactor);
            heightFieldCS.SetInt("g_iDepthMesh", depthMesh * detailScaleFactor);
            heightFieldCS.Dispatch(kernelVertices, Mathf.CeilToInt((vertices.Length - widthMesh * depthMesh * 5) / 256f), 1, 1);

            /// interpolate vertices of detailed submesh ------------------------------------------------------------------------------------------------------------
            heightFieldCS.Dispatch(kernelVerticesEdges, Mathf.CeilToInt((vertices.Length - widthMesh * depthMesh * 5) / 256f), 1, 1);

            /// compute triangle normals ----------------------------------------------------------------------------------------------------------------------------
            heightFieldCS.Dispatch(kernelTriangles, Mathf.CeilToInt(mesh.triangles.Length / 256f), 1, 1);

            /// compute vertex normals ------------------------------------------------------------------------------------------------------------------------------
            //heightFieldCS.SetMatrix("g_f4x4TransformMatrix", Matrix4x4.Transpose(Matrix4x4.Inverse(transform.localToWorldMatrix)));
            heightFieldCS.SetInt("g_iDepthMesh", depthMesh);
            heightFieldCS.SetInt("g_iTriangleD", depthMesh - 1);
            for (int i = 0; i < 5; i++)
            {
                heightFieldCS.SetInt("g_iIdxOffset", widthMesh * depthMesh * i);
                heightFieldCS.SetInt("g_iOffset", (depthMesh - 1) * (widthMesh - 1) * 2 * i);
                heightFieldCS.Dispatch(kernelNormals, Mathf.CeilToInt(widthMesh / 16f), Mathf.CeilToInt(depthMesh / 16f), 1);
            }
            heightFieldCS.SetInt("g_iOffset", (depthMesh - 1) * (widthMesh - 1) * 2 * 5);
            heightFieldCS.SetInt("g_iIdxOffset", widthMesh * depthMesh * 5);
            heightFieldCS.SetInt("g_iTriangleD", depthMesh * detailScaleFactor - detailScaleFactor + 1);
            heightFieldCS.SetInt("g_iDepthMesh", depthMesh * detailScaleFactor);
            heightFieldCS.Dispatch(kernelNormals, Mathf.CeilToInt(widthMesh * detailScaleFactor / 16f), Mathf.CeilToInt(depthMesh * detailScaleFactor / 16f), 1);
        }

        private void CreateMeshWithSubmeshes()
        {
            Vector3[] subMeshVertices;
            Vector3[] offsets;
            int[] newTriangles;
            Vector2[] newUV;

            int scale = detailScaleFactor;
            int subM6W = (widthMesh * scale - scale + 1);
            int subM6D = (depthMesh * scale - scale + 1);

            subMeshVertices = new Vector3[widthMesh * depthMesh * 5 + subM6W * subM6D];
            newTriangles = new int[(widthMesh - 1) * (depthMesh - 1) * 6 * 5 + (widthMesh * scale - scale) * (depthMesh * scale - scale) * 6];
            newUV = new Vector2[subMeshVertices.Length];
            offsets = new Vector3[6];

            // 0 - widthMesh*depthMesh*3: three low detail submeshes
            // other vertices are part of the detailed submesh

            for (int i = 0; i < widthMesh; i++)
            {
                for (int j = 0; j < depthMesh; j++)
                {
                    subMeshVertices[i * depthMesh + j] = new Vector3(i * quadSize, 0.0f, j * quadSize);
                    subMeshVertices[i * depthMesh + j + widthMesh * depthMesh * 1] = new Vector3(i * quadSize, 0.0f, j * quadSize) + new Vector3(0, 0, (depthMesh - 1)) * quadSize;
                    subMeshVertices[i * depthMesh + j + widthMesh * depthMesh * 2] = new Vector3(i * quadSize, 0.0f, j * quadSize) + new Vector3((widthMesh - 1), 0, (depthMesh - 1)) * quadSize;
                    subMeshVertices[i * depthMesh + j + widthMesh * depthMesh * 3] = new Vector3(i * quadSize, 0.0f, j * quadSize) + new Vector3((widthMesh - 1) * 2, 0, (depthMesh - 1)) * quadSize;
                    subMeshVertices[i * depthMesh + j + widthMesh * depthMesh * 4] = new Vector3(i * quadSize, 0.0f, j * quadSize) + new Vector3((widthMesh - 1) * 2, 0, 0) * quadSize;
                }
            }
            for (int i = 0; i < subM6W; i++)
            {
                for (int j = 0; j < subM6D; j++)
                {
                    subMeshVertices[i * subM6D + j + widthMesh * depthMesh * 5] = new Vector3(i * quadSize / scale, 0.0f, j * quadSize / scale) + new Vector3((widthMesh - 1), 0, 0) * quadSize;
                }
            }

            int tri = 0;
            for (int k = 0; k < 5; k++)
            {
                for (int i = 0; i < widthMesh - 1; i++)
                {
                    for (int j = 0; j < depthMesh - 1; j++)
                    {
                        newTriangles[tri + 2] = (i + 1) * depthMesh + (j + 1) + widthMesh * depthMesh * k;
                        newTriangles[tri + 1] = i * depthMesh + (j + 1) + widthMesh * depthMesh * k;
                        newTriangles[tri] = i * depthMesh + j + widthMesh * depthMesh * k;
                        tri += 3;

                        newTriangles[tri + 2] = (i + 1) * depthMesh + j + widthMesh * depthMesh * k;
                        newTriangles[tri + 1] = (i + 1) * depthMesh + (j + 1) + widthMesh * depthMesh * k;
                        newTriangles[tri] = i * depthMesh + j + widthMesh * depthMesh * k;
                        tri += 3;
                    }
                }
            }

            for (int i = 0; i < widthMesh * scale - scale; i++)
            {
                for (int j = 0; j < depthMesh * scale - scale; j++)
                {
                    newTriangles[tri + 2] = (i + 1) * (subM6D) + (j + 1) + widthMesh * depthMesh * 5;
                    newTriangles[tri + 1] = i * (subM6D) + (j + 1) + widthMesh * depthMesh * 5;
                    newTriangles[tri] = i * (subM6D) + j + widthMesh * depthMesh * 5;
                    tri += 3;

                    newTriangles[tri + 2] = (i + 1) * (subM6D) + j + widthMesh * depthMesh * 5;
                    newTriangles[tri + 1] = (i + 1) * (subM6D) + (j + 1) + widthMesh * depthMesh * 5;
                    newTriangles[tri] = i * (subM6D) + j + widthMesh * depthMesh * 5;
                    tri += 3;
                }
            }

            //  initialize texture coordinates
            for (int i = 0; i < newUV.Length; i++)
            {
                newUV[i] = new Vector2(subMeshVertices[i].x, subMeshVertices[i].z);
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.MarkDynamic();
            mesh.vertices = subMeshVertices;
            mesh.triangles = newTriangles;
            mesh.uv = newUV;
            vertices = subMeshVertices;
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
            lastConfig = 0;
            lastPosition = transform.position;
            segmentConfigurations = new Vector2[6, 4];
            float[,,] idcs = new float[,,] {
                {{0,0},{1,-1},{0,0},{0,0} },
                {{0,0},{2,-2},{0,-2},{0,0} },
                {{0,0},{0,0},{0,-2},{0,0} },
                {{0,0},{0,0},{0,-2},{-2,-2} },
                {{0,0},{0,0},{0,0},{-1,-1} },
                {{0,0},{0,0},{0,0},{0,0} }};
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    segmentConfigurations[i, j] = new Vector2((widthMesh - 1) * quadSize * idcs[i, j, 0], (depthMesh - 1) * quadSize * idcs[i, j, 1]);
                }
            }

            GetComponent<BoxCollider>().size = new Vector3(quadSize * widthMesh * 20f, maxHeight / 4.0f, quadSize * depthMesh * 20f);
            GetComponent<BoxCollider>().center = new Vector3(quadSize * widthMesh / 2.0f, maxHeight / 4.0f, quadSize * depthMesh / 2.0f);
        }
        //  creates mesh with flat shading
        private void CreateMesh()
        {
            Vector2[] newUV;
            Vector3[] newVertices;
            int[] newTriangles;

            newVertices = new Vector3[widthMesh * depthMesh];
            newTriangles = new int[(widthMesh - 1) * (depthMesh - 1) * 6];
            newUV = new Vector2[newVertices.Length];

            for (int i = 0; i < widthMesh; i++)
            {
                for (int j = 0; j < depthMesh; j++)
                {
                    newVertices[i * depthMesh + j] = new Vector3(i * quadSize, 0.0f, j * quadSize);
                }
            }
            //  initialize texture coordinates
            for (int i = 0; i < newUV.Length; i++)
            {
                newUV[i] = new Vector2(newVertices[i].x, newVertices[i].z);
            }

            //  represent quads by two triangles
            int tri = 0;
            for (int i = 0; i < widthMesh - 1; i++)
            {
                for (int j = 0; j < depthMesh - 1; j++)
                {
                    newTriangles[tri + 2] = (i + 1) * depthMesh + (j + 1);
                    newTriangles[tri + 1] = i * depthMesh + (j + 1);
                    newTriangles[tri] = i * depthMesh + j;
                    tri += 3;

                    newTriangles[tri + 2] = (i + 1) * depthMesh + j;
                    newTriangles[tri + 1] = (i + 1) * depthMesh + (j + 1);
                    newTriangles[tri] = i * depthMesh + j;
                    tri += 3;
                }
            }
            //  create new mesh
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.MarkDynamic();
            mesh.vertices = newVertices;
            mesh.triangles = newTriangles;
            mesh.uv = newUV;
            vertices = newVertices;

            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<BoxCollider>().size = new Vector3(quadSize * widthMesh, maxHeight / 2.0f, quadSize * depthMesh);
            GetComponent<BoxCollider>().center = new Vector3(quadSize * widthMesh / 2.0f, maxHeight / 4.0f, quadSize * depthMesh / 2.0f);
        }

        private void CreatePlaneMesh()
        {
            planeMesh = new Mesh();// GetComponent<MeshFilter>().mesh;
            //  create plane mesh for reflection
            Vector3[] planeVertices = new Vector3[4];
            Vector3[] planeNormals = new Vector3[4];
            int[] planeTriangles = new int[6];
            planeVertices[0] = new Vector3();
            planeVertices[1] = new Vector3(quadSize * (depthMesh - 1), 0, quadSize * (widthMesh - 1));
            planeVertices[2] = new Vector3(quadSize * (depthMesh - 1), 0, 0);
            planeVertices[3] = new Vector3(0, 0, quadSize * (widthMesh - 1));
            planeNormals[0] = Vector3.up;
            planeNormals[1] = Vector3.up;
            planeNormals[2] = Vector3.up;
            planeNormals[3] = Vector3.up;
            planeTriangles[0] = 0;
            planeTriangles[1] = 2;
            planeTriangles[2] = 1;
            planeTriangles[3] = 0;
            planeTriangles[4] = 1;
            planeTriangles[5] = 3;
            planeMesh.vertices = planeVertices;
            planeMesh.triangles = planeTriangles;
            planeMesh.normals = planeNormals;
        }

        /// <summary>
        /// Calculates the Y-value of the water-heightfield at the given X- and Z-values of a position in world space.
        /// </summary>
        /// <param name="worldPosition">X- and Z- Value will be taken from this Vector3</param>
        public float getHeightAtWorldPosition(Vector3 worldPosition)
        {
            int k, m;
            k = Mathf.Max(Mathf.Min(Mathf.RoundToInt((worldPosition.x - transform.position.x) / quadSizeHF), widthHF - 1), 0);
            m = Mathf.Max(Mathf.Min(Mathf.RoundToInt((worldPosition.z - transform.position.z) / quadSizeHF), depthHF - 1), 0);

            float x1, x2, x3, x4;
            //	get surrounding height values at the vertex position (can be randomly displaced)
            x1 = hf[k * depthHF + m].height;
            x2 = hf[Mathf.Min((k + 1), widthHF - 1) * depthHF + Mathf.Min(m + 1, depthHF - 1)].height;
            x3 = hf[k * depthHF + Mathf.Min(m + 1, depthHF - 1)].height;
            x4 = hf[Mathf.Min((k + 1), widthHF - 1) * depthHF + m].height;
            //	get x and y value between 0 and 1 for interpolation
            float x = ((worldPosition.x - transform.position.x) / quadSizeHF - k);
            float y = ((worldPosition.z - transform.position.z) / quadSizeHF - m);

            //	bilinear interpolation to get height at vertex i
            //	note if x == 0 and y == 0 vertex position is at heightfield position.
            float resultingHeight = (x1 * (1 - x) + x4 * (x)) * (1 - y) + (x3 * (1 - x) + x2 * (x)) * (y);

            return resultingHeight;
        }

        void OnDestroy()
        {
            ReleaseComputeBuffers();
        }

        public void ReleaseComputeBuffers()
        {
            heightFieldCB.Release();
            reflectWavesCB.Release();
            heightFieldCBOut.Release();
            verticesCB.Release();
            normalsCB.Release();
            randomXZ.Release();
            trianglesRCB.Release();
            normTrianglesCB.Release();
        }
    }
}