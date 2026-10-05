using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Rendering/Editor Mesh")]
public class EditorMesh : MonoBehaviour
{
    public Mesh mesh;
    public Color wireColor = new Color(1f, 1f, 1f, 0.5f);

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Draw();
    }

    private void Draw()
    {
        if(mesh == null){ return; }

        Matrix4x4 prev = Gizmos.matrix;
        Color prevColor = Gizmos.color;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = wireColor;

        for(int i = 0; i < mesh.subMeshCount; i++){
            Gizmos.DrawWireMesh(mesh, i);
        }

        Gizmos.matrix = prev;
        Gizmos.color = prevColor;
    }

    private void Reset()
    {
        var mf = GetComponent<MeshFilter>();
        if(mf != null){ mesh = mf.sharedMesh; }
    }
#endif
}