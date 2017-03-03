// @cartzhang
// 2017-03-03
// Get current lod level.
using UnityEngine;

[ExecuteInEditMode]
public class LODGroupLog : MonoBehaviour
{
    private LODGroup lodGroup;
    private int lodsCount;
    private Renderer[] lodChildTransformRenders;
    private bool isUsingLOD;
        
	void Start ()
    {
        lodGroup = this.GetComponent<LODGroup>();
        Debug.Assert(null != lodGroup);
        lodsCount = lodGroup.lodCount;
        lodChildTransformRenders = new Renderer[lodsCount];
        Transform lodTransform = lodGroup.transform;
        int i = 0;
        foreach (Transform child in lodTransform)
        {
            lodChildTransformRenders[i] = child.GetComponentInChildren<Renderer>();
            ++i;
        }
        isUsingLOD = false;
    }
	

    //string directly output: 9.6K ,1.7ms
	void Update ()
    {
        isUsingLOD = false;
        for (int i = 0; i < lodsCount;i++)
        {
            var renderer = lodChildTransformRenders[i];
            if (renderer != null && renderer.isVisible)
            {
                isUsingLOD = true;
                Debug.Log("This LODlevel is used: " + i);
            }
        }

        if (!isUsingLOD)
        {
            Debug.Log(this.gameObject.name + "LOD is culled ");
        }
    }
}
