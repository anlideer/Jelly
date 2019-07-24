using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jellyfier : MonoBehaviour
{
    public float bounceSpeed = 400f;
    public float fallForce = 0.03f;
    public float stiffness = 40f;
    public float maxSpeed = 10f;
    public float bounceForce = 100f;
    public bool useBounce = true;

    private MeshFilter meshFilter;
    private Mesh mesh;

    JellyVertex[] jellyVertices;
    Vector3[] currentMeshVertices;
    Rigidbody rigid;
    Vector3 vel;    // 记录每帧的速度，用中间值来模拟真实速度（也不太准就是了），这样碰撞的时候至少会好一些
    Vector3 lastV;  // 上一帧的速度


    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        GetVertices();

        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateVertices();
        SpeedControll();
        
    }

    private void GetVertices()
    {
        jellyVertices = new JellyVertex[mesh.vertices.Length];
        currentMeshVertices = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            jellyVertices[i] = new JellyVertex(i, mesh.vertices[i], mesh.vertices[i], Vector3.zero);
            currentMeshVertices[i] = mesh.vertices[i];
        }
    }

    private void UpdateVertices()
    {
        for (int i = 0; i < jellyVertices.Length; i++)
        {
            jellyVertices[i].UpdateVelocity(bounceSpeed);
            jellyVertices[i].Settle(stiffness);

            jellyVertices[i].currentVertexPosition += jellyVertices[i].currentVelocity * Time.deltaTime;
            currentMeshVertices[i] = jellyVertices[i].currentVertexPosition;
        }

        mesh.vertices = currentMeshVertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (useBounce)
        {
            Bounce();
        }
        ContactPoint[] collisionPoints = collision.contacts;
        for (int i = 0; i < collisionPoints.Length; i++)
        {
            Vector3 inputPoint = collisionPoints[i].point + (collisionPoints[i].point * .1f);
            ApplyPressureToPoint(inputPoint, vel.magnitude * fallForce);
            //Debug.Log(vel.magnitude);
        }
    }

    private void Bounce()
    {
        rigid.AddForce(vel * -1 * bounceSpeed);
    }

    public void ApplyPressureToPoint(Vector3 _point, float _pressure)
    {
        for (int i = 0; i < jellyVertices.Length; i++)
        {
            jellyVertices[i].ApplyPressureToVertex(transform, _point, _pressure);
        }
    }

    private void SpeedControll()
    {
        Vector3 v = rigid.velocity;
        if (v.magnitude > maxSpeed)
        {
            Vector3 normalV = v.normalized;
            v = normalV * maxSpeed;
            rigid.velocity = v;
        }

        vel = (lastV + v) / 2;
        lastV = v;
    }
}


public class JellyVertex
{
    public int verticeIndex;
    public Vector3 initialVertexPosition;
    public Vector3 currentVertexPosition;
    public Vector3 currentVelocity;

    // constructor
    public JellyVertex(int _verticeIndex, Vector3 _initialVertexPosition, Vector3 _currentVertexPosition, Vector3 _currentVelocity)
    {
        verticeIndex = _verticeIndex;
        initialVertexPosition = _initialVertexPosition;
        currentVertexPosition = _currentVertexPosition;
        currentVelocity = _currentVelocity;
    }


    // methods
    public Vector3 GetCurrentDisplacement()
    {
        return currentVertexPosition - initialVertexPosition;
    }

    public void UpdateVelocity(float _bounceSpeed)
    {
        currentVelocity = currentVelocity - GetCurrentDisplacement() * _bounceSpeed * Time.deltaTime;
    }

    public void Settle(float _stiffness)
    {
        currentVelocity *= 1f - _stiffness * Time.deltaTime;
    }

    public void ApplyPressureToVertex(Transform _transform, Vector3 _position, float _pressure)
    {
        Vector3 distanceVerticePoint = currentVertexPosition - _transform.InverseTransformPoint(_position);
        float adaptPressure = _pressure / (1f * distanceVerticePoint.sqrMagnitude);
        float velocity = adaptPressure * Time.deltaTime;
        currentVelocity += distanceVerticePoint.normalized * velocity;
    }

}
