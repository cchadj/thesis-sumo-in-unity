using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TestPerformanceOfDifferentAccessors : MonoBehaviour
{
    public int numberOfIterations;
    [ReadOnly] public long getComponentTime = 0;
    [ReadOnly] public long dictionaryAccessTime = 0;
    [ReadOnly] public long propertyAccessTime = 0;
    [ReadOnly] public long fieldAccessTime = 0;
    [ReadOnly] public long serializedFieldTime = 0;
    [ReadOnly] public long readOnlyStaticFieldTime = 0;
    [ReadOnly] public long staticFieldTime = 0;
    [ReadOnly] public long allocationTime  = 0;
    
    private Transform _transform;
    private Rigidbody _rb;
    private TestPerformanceOfDifferentAccessors _gvc;
    private CharacterJoint _cj;
    
    
    [SerializeField] private Transform trf;
    [SerializeField] private Rigidbody rgb;
    [SerializeField] private TestPerformanceOfDifferentAccessors gvc;
    [SerializeField] private CharacterJoint cjo;

    
    private static  Transform _strf;
    private static Rigidbody _srgb;
    private static TestPerformanceOfDifferentAccessors _sgvc;
    private static CharacterJoint _scjo;
    
    private static readonly  string Rstrf = "hahaha";
    private static readonly string Rsrgb = "hahaha";
    private static readonly string Rsgvc= "hahaha";
    private static readonly string Rscjo= "hahaha";
    
    private Transform Transform
    {
        get => _transform;
        set => _transform = value;
    }


    private static readonly Dictionary<string, Component> dict
        = new Dictionary<string, Component> ();
    // Start is called before the first frame update
    void Start()
    {
        var tr = "tr";
        var ri = "ri";
        var gc = "gc";
        var cj = "cj";

        Transform = GetComponent<Transform>();
        RB = GetComponent<Rigidbody>();
        GVC = GetComponent<TestPerformanceOfDifferentAccessors>();
        CJ = GetComponent < CharacterJoint>();   
        
        _strf = trf = Transform;
        _srgb = rgb = RB;
        _sgvc = gvc = GVC;
        _scjo = cjo = CJ;
        
        dict.Add(tr, GetComponent<Transform>());
        dict.Add(ri, GetComponent<Rigidbody>());
        dict.Add(gc, GetComponent<TestPerformanceOfDifferentAccessors>());
        dict.Add(cj, GetComponent<CharacterJoint>());
        
        var sw = new Stopwatch();

        for (int i = 0; i < numberOfIterations; i++)
        {
            var a = GetComponent<Transform>();
            var b = GetComponent<Rigidbody>();
            var c = GetComponent<TestPerformanceOfDifferentAccessors>();
            var d = GetComponent<CharacterJoint>();
        }

        for (int i = 0; i < numberOfIterations; i++)
        {
          var a = dict[tr];
          var b = dict[ri];
          var c = dict[gc];
          var d = dict[cj];  
        }


        
        for (int i = 0; i < numberOfIterations; i++)
        {
            var a = Transform;
            var b = RB;
            var c = GVC;
            var d = CJ;
        }
        
        for (int i = 0; i < numberOfIterations; i++)
        {
            var a = trf;
            var b = rgb;
            var c = gvc;
            var d = cjo;
        }
        
        for (int i = 0; i < numberOfIterations; i++)
        {
            var a = _strf;
            var b = _srgb;
            var c = _sgvc;
            var d = _scjo;
        }
        
        
        const int n = 10;
        for (int i = 0; i < n; i++)
        {
            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                var a = GetComponent<Transform>();
                var b = GetComponent<Rigidbody>();
                var c = GetComponent<TestPerformanceOfDifferentAccessors>();
                var d = GetComponent<CharacterJoint>();
            }
            sw.Stop();
            getComponentTime += sw.ElapsedMilliseconds;
            
            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                var a = (Transform)dict[tr];
                var b = (Rigidbody)dict[ri];
                var c = (TestPerformanceOfDifferentAccessors)dict[gc];
                var d = (CharacterJoint)dict[cj];  
            }
            sw.Stop();
            dictionaryAccessTime += sw.ElapsedMilliseconds;
            
            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                var a = Transform;
                var b = RB;
                var c = GVC;
                var d = CJ;
            }
            sw.Stop();
            propertyAccessTime += sw.ElapsedMilliseconds;
            
            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                var a = _transform;
                var b = _rb;
                var c = _gvc;
                var d = _cj;
            }
            sw.Stop();
            fieldAccessTime += sw.ElapsedMilliseconds;

            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                var a = trf;
                var b = rgb;
                var c = gvc;
                var d = cjo;
            }
            sw.Stop();
            serializedFieldTime += sw.ElapsedMilliseconds;
            
            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                var a = _strf;
                var b = _srgb;
                var c = _sgvc;
                var d = _scjo;
            }
            sw.Stop();
            staticFieldTime += sw.ElapsedMilliseconds;

            string aa = null;
            string bb = null;
            string cc = null;
            string dd = null;
            
            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                aa = Rstrf;
                bb = Rsrgb;
                cc = Rsgvc;
                dd = Rscjo;
            }
            sw.Stop();
            Debug.Log(aa + bb  + cc + dd);
            readOnlyStaticFieldTime += sw.ElapsedMilliseconds;
            
            sw.Restart();
            for (int j = 0; j < numberOfIterations; j++)
            {
                var a =  new EventArgs();
            }
            allocationTime += sw.ElapsedMilliseconds;
            sw.Stop();
        }

        getComponentTime /= n;
        dictionaryAccessTime /= n;
        propertyAccessTime /= n;
        fieldAccessTime /= n;
        serializedFieldTime /= n;
        staticFieldTime /= n;
        readOnlyStaticFieldTime /= n;
        allocationTime /= n;
    }

    public CharacterJoint CJ
    {
        get => _cj;
        set => _cj = value;
    }

    public TestPerformanceOfDifferentAccessors GVC
    {
        get => _gvc;
        set => _gvc = value;
    }

    public Rigidbody RB
    {
        get => _rb;
        set => _rb = value;
    }
    
    
}
