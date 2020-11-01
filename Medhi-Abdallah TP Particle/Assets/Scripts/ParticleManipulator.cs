using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Matrix4x4 = System.Numerics.Matrix4x4;

public struct Particle
{
    public Particle(Vector3 newPos, Vector3 newVelocity, float newLifeTime)
    {
        position = newPos;
        velocity = newVelocity;
        life = newLifeTime;
    }
    
    public Vector3 position;
    public Vector3 velocity;
    public float life;
}

public class ParticleManipulator : MonoBehaviour
{
    
    [SerializeField] private Camera myCamera;
    [SerializeField] private GameObject cube;
    [SerializeField] private ComputeShader myComputeShader;
    [SerializeField] private Material myCubeMaterial;


    private ComputeBuffer myComputeBuffer;
    private Vector3 _mousePosition;
    private Vector3 _newCubePosition;
    private Vector3 _cameraPosition;
    private float[] output = new float[3];

    int kernelHandle;
    private RenderTexture tex;
    
    // Start is called before the first frame update

    void RunShader()
    {
        
        kernelHandle = myComputeShader.FindKernel("CSMain");
        tex = new RenderTexture(256, 256, 24);
        tex.enableRandomWrite = true;
        tex.Create();
        
    }

    void UpdateShader()
    {
        
        myComputeBuffer = new ComputeBuffer(output.Length, 3*4);
        myComputeBuffer.SetData(output);
        myComputeShader.SetFloat("time", Time.deltaTime);
        myComputeShader.SetVector("newPos", _newCubePosition);
        myComputeShader.SetTexture(kernelHandle, "Result", tex);
        myComputeShader.SetBuffer(kernelHandle, "debugger", myComputeBuffer);
        myComputeShader.Dispatch(kernelHandle, 256/8, 256/8, 1);
        myCubeMaterial.SetTexture("_MainTex", tex);
        myComputeBuffer.GetData(output);
        
        print("X : " + output[0] + " Y : " + output[1] + " Z : " + output[2]);
        
        

    }
    
    void Start()
    {
        
        RunShader();
        Particle tst = new Particle(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 15.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
        _cameraPosition = myCamera.transform.position;
        _mousePosition = Input.mousePosition;
        _newCubePosition = myCamera.ScreenToWorldPoint(new Vector3(_mousePosition.x, _mousePosition.y, -_cameraPosition.z));
       
        Graphics.DrawProcedural(MeshTopology.Points, 1, 50);
        
        ParticleSystem tst = new ParticleSystem();
        tst.is;
        
        cube.transform.position = _newCubePosition;
        
        UpdateShader();

    }
}
