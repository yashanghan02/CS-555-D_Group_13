using UnityEngine;
using System.Collections.Generic;

namespace AmazingAssets
{
    namespace CurvedWorld
    {
        [ExecuteInEditMode]
        public class CurvedWorldBoundingBox : MonoBehaviour  
        {
            public float scale = 1;
            float currentScale;
            Vector3 boundingBoxSize; 
            Bounds originalBounds;

            SkinnedMeshRenderer skinnedMeshRenderer;
            MeshFilter meshFilter;
            bool bIsSkinned;
             
            static Dictionary<int, Bounds> boundsDictionary;
             
            public bool visualizeInEditor;


            void OnEnable()
            {
                currentScale = -1;
            }

            void OnDisable()
            {
                if (bIsSkinned == true)
                {
                    if (skinnedMeshRenderer != null)
                        skinnedMeshRenderer.sharedMesh.bounds = originalBounds;
                }
                else
                {
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                        meshFilter.sharedMesh.bounds = originalBounds;
                }
            }

            void Start()
            {
                if (boundsDictionary == null)
                    boundsDictionary = new Dictionary<int, Bounds>();

                meshFilter = GetComponent<MeshFilter>();
                skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    bIsSkinned = false;

                    if (boundsDictionary.ContainsKey(meshFilter.sharedMesh.GetInstanceID()))
                        originalBounds = boundsDictionary[meshFilter.sharedMesh.GetInstanceID()];
                    else
                    {
                        originalBounds = meshFilter.sharedMesh.bounds;
                        boundsDictionary.Add(meshFilter.sharedMesh.GetInstanceID(), originalBounds);
                    }

                    boundingBoxSize = originalBounds.size;

                    float size = 1.0f;
                    if (boundingBoxSize.x > size) size = boundingBoxSize.x;
                    if (boundingBoxSize.y > size) size = boundingBoxSize.y;
                    if (boundingBoxSize.z > size) size = boundingBoxSize.z;

                    boundingBoxSize.x = boundingBoxSize.y = boundingBoxSize.z = size;
                }
                else if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
                {
                    bIsSkinned = true;

                    if (boundsDictionary.ContainsKey(skinnedMeshRenderer.sharedMesh.GetInstanceID()))
                        originalBounds = boundsDictionary[skinnedMeshRenderer.sharedMesh.GetInstanceID()];
                    else
                    {
                        originalBounds = skinnedMeshRenderer.sharedMesh.bounds;
                        boundsDictionary.Add(skinnedMeshRenderer.sharedMesh.GetInstanceID(), originalBounds);
                    }

                    boundingBoxSize = originalBounds.size;

                    float size = 1.0f;
                    if (boundingBoxSize.x > size) size = boundingBoxSize.x;
                    if (boundingBoxSize.y > size) size = boundingBoxSize.y;
                    if (boundingBoxSize.z > size) size = boundingBoxSize.z;

                    boundingBoxSize.x = boundingBoxSize.y = boundingBoxSize.z = size;
                }

                currentScale = 0;
            }

            void Update()
            {
                if (currentScale != scale)
                {
                    if (scale < 0)
                        scale = 0;

                    currentScale = scale;

                    if (bIsSkinned == true)
                    {
                        if (skinnedMeshRenderer != null)
                            skinnedMeshRenderer.localBounds = new Bounds(skinnedMeshRenderer.localBounds.center, boundingBoxSize * scale);
                    }
                    else
                    {
                        if (meshFilter != null && meshFilter.sharedMesh != null)
                            meshFilter.sharedMesh.bounds = new Bounds(meshFilter.sharedMesh.bounds.center, boundingBoxSize * scale);
                    }
                }
            }

            void OnDrawGizmos()
            {
                if (visualizeInEditor)
                {
                    Gizmos.color = Color.yellow;

                    if (bIsSkinned == true && skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
                    {
                        Gizmos.DrawWireCube(transform.TransformPoint(skinnedMeshRenderer.localBounds.center), boundingBoxSize * scale);
                    }
                    else
                    {
                        if (meshFilter != null && meshFilter.sharedMesh != null)
                            Gizmos.DrawWireCube(transform.TransformPoint(meshFilter.sharedMesh.bounds.center), boundingBoxSize * scale);
                    }
                }
            }

            void Reset()
            {
                scale = 1;

                RecalculateBounds();
                Update();                
            }

            public void RecalculateBounds()
            {
                if (bIsSkinned == true)
                {
                    if (skinnedMeshRenderer != null)
                    {
                        skinnedMeshRenderer.sharedMesh.RecalculateBounds();

                        originalBounds = skinnedMeshRenderer.sharedMesh.bounds;

                        if(boundsDictionary != null && boundsDictionary.ContainsKey(skinnedMeshRenderer.sharedMesh.GetInstanceID()))
                        {
                            boundsDictionary[skinnedMeshRenderer.sharedMesh.GetInstanceID()] = originalBounds;
                        }
                    }
                }
                else
                {
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        meshFilter.sharedMesh.RecalculateBounds();

                        originalBounds = meshFilter.sharedMesh.bounds;

                        if (boundsDictionary != null && boundsDictionary.ContainsKey(meshFilter.sharedMesh.GetInstanceID()))
                        {
                            boundsDictionary[meshFilter.sharedMesh.GetInstanceID()] = originalBounds;
                        }
                    }
                }
            }
        }
    }
}
