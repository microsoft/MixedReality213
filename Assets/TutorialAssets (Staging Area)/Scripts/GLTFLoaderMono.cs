using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLTFLoaderMono : MonoBehaviour {

    [SerializeField]
    UnityEngine.Object leftController;
    [SerializeField]
    UnityEngine.Object rightController;

    [SerializeField]
    Transform lcTransform;
    [SerializeField]
    Transform rcTransform;

    [SerializeField]
    Material mat;

    // Use this for initialization
    void Start() {
        TextAsset asset = Resources.Load("RightHandModel") as TextAsset;
        GLTF.GLTFLoader loader = new GLTF.GLTFLoader(asset.bytes, rcTransform);
        loader.ColorMaterial = mat;
        loader.NoColorMaterial = mat;
        StartCoroutine(loader.Load());

        asset = Resources.Load("LeftHandModel") as TextAsset;
        loader = new GLTF.GLTFLoader(asset.bytes, lcTransform);
        loader.ColorMaterial = mat;
        loader.NoColorMaterial = mat;
        StartCoroutine(loader.Load());
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
