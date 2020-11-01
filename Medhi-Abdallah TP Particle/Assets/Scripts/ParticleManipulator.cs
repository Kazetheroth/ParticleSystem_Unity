using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Matrix4x4 = System.Numerics.Matrix4x4;

public struct Particle
{
    public Vector3 position;
    public Vector3 velocity;
    public float life;
}

public class ParticleManipulator : MonoBehaviour
{
    
    [SerializeField] private Camera myCamera;
    //[SerializeField] private GameObject cube;
    [SerializeField] private ComputeShader myComputeShader;
    [SerializeField] private Material myCubeMaterial;


    private ComputeBuffer _myComputeBuffer;
    private Vector3 _mousePosition;
    private Vector3 _newCubePosition;
    private Vector3 _cameraPosition;
    private Particle[] _myParticles;
    private int _nbThread;
    
    private float[] output = new float[3];

    private int _kernelHandle;
    private RenderTexture _tex;
    
    // Start is called before the first frame update
    void Start()
    {
        RunShader();
        //Particle tst = new Particle{ position = new Vector3(0, 0, 0), velocity = new Vector3(0, 0, 0), life = 15.0f};
    }

    // Update is called once per frame
    void Update()
    {
        _cameraPosition = myCamera.transform.position;
        _mousePosition = Input.mousePosition;
        _newCubePosition = myCamera.ScreenToWorldPoint(new Vector3(_mousePosition.x, _mousePosition.y, -_cameraPosition.z));
       
        //Graphics.DrawProceduralNow(MeshTopology.Points, 1, 1);
        
        //ParticleSystem tst = new ParticleSystem();
        //tst.is;
        
        //cube.transform.position = _newCubePosition;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            UpdateShader();
        }

        if (_myComputeBuffer == null)
            return;
        myComputeShader.SetFloat("deltaTime", Time.deltaTime);
        myComputeShader.SetFloat("particleSize", 0.5f);
        myComputeShader.SetVector("mousePosition", _newCubePosition);
        myComputeShader.SetBuffer(_kernelHandle, "buffer", _myComputeBuffer);
        myComputeShader.Dispatch(_kernelHandle, _nbThread, 1, 1);
    }

    private void OnRenderObject()
    {
        if (_myComputeBuffer != null)
        {
            _myComputeBuffer.GetData(_myParticles);
            myCubeMaterial.SetBuffer("_Buffer", _myComputeBuffer);
            myCubeMaterial.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Points, 1, _myParticles.Length);
        }
    }

    private void OnDestroy()
    {
        if (_myComputeBuffer != null)
        {
            _myComputeBuffer.Release();
        }
    }

    void RunShader()
    {
        _kernelHandle = myComputeShader.FindKernel("CSMain");
        _myParticles = new Particle[0];
    }

    void UpdateShader()
    {
        print("Bonjour");
        var newParticles = new Particle[10000];
        for (int i = 0; i < newParticles.Length; i++)
        {
            newParticles[i] = new Particle{position = new Vector3(_mousePosition.x, _mousePosition.y, -_cameraPosition.z), velocity = new Vector3(0, 0, 0), life = 5f};
        }
        List<Particle> tempParticle = new List<Particle>(_myParticles.Length + newParticles.Length);
        tempParticle.AddRange(_myParticles);
        tempParticle.AddRange(newParticles);
        _myParticles = tempParticle.ToArray();
        _nbThread = Mathf.CeilToInt((float)_myParticles.Length / 256);
        if(_myComputeBuffer != null)
            _myComputeBuffer.Release();
        
        print(_myParticles.Length);
        
        _myComputeBuffer = new ComputeBuffer(_myParticles.Length, 7 * sizeof(float));
        _myComputeBuffer.SetData(_myParticles);
    }
    
    
}
