using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public GameObject[] Bones;
    public SkinnedMeshRenderer SMRenderer;
    public Mesh playerBakedMesh;

    Vector3 Center;


    void Awake ()
    {
        Instance = this;
	}
	
	void FixedUpdate ()
    {
		for(int i = 0; i < Bones.Length; i++)
        {
            Center += Bones[i].transform.position;
        }

        Center = Center / Bones.Length;

        transform.position = Center;

        Center = Vector3.zero;

        PlayerBakeMeshToCollider();
	}

    void PlayerBakeMeshToCollider()
    {
        SMRenderer.BakeMesh(playerBakedMesh);
        gameObject.GetComponent<MeshCollider>().sharedMesh = playerBakedMesh;
    }
}
