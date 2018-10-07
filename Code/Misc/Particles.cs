using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles {

    public static Dictionary<string,GameObject> cache = new Dictionary<string, GameObject>();
    public Particles(string name, Vector3 pos)
    {
        init(name, pos, new Dictionary<string, object>());
    }
    public Particles(string name, Vector3 pos, Dictionary<string, object> options)
    {
        init(name, pos, options);
    }
    void init(string name,Vector3 pos,Dictionary<string,object> options)
    {
        if (!cache.ContainsKey(name)) cache.Add(name, Resources.Load<GameObject>("Particles/"+name));
        GameObject prefab = cache[name];
        GameObject.Instantiate(prefab, pos, Quaternion.identity);
    }
}
