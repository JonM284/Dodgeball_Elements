using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool
{
    [Tooltip("Name to use, in order to access this pool.")]
    public string pool_Name;
    [Tooltip("Gameobject to spawn.")]
    public GameObject spawning_Object;
    [Tooltip("How many objects do you wish to spawn?")]
    public int amount_To_Spawn;
}

public class Object_Pool_Spawner : MonoBehaviour
{

    /// <summary>
    /// DESCRIPTION: this script handles object pooling.
    /// this code was created using the help of brackeys on youtube

    /// </summary>

    [Header("Variables")]
    [Tooltip("Amount to add towards 'amount_To_Spawn' if a player is using this ability type.")]
    public int adding_Amount;

    [Tooltip("Pool to spawn and set active from.")]
    public List<Pool> pools;

    //way to access various pools.
    public Dictionary<string, Queue<GameObject>> bombPoolDictionary;
    

    public static Object_Pool_Spawner spawner_Instance;

    private void Awake()
    {
        spawner_Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        bombPoolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Ability_Use_Behavior g in Resources.FindObjectsOfTypeAll(typeof(Ability_Use_Behavior)))
        {
            string type_String = g.GetComponent<Ability_Use_Behavior>().ability_Info[0].E_Type.ToString();
            Debug.Log(type_String+"_Passive");
            for (int i = 0; i < pools.Count; i++)
            {
                if (pools[i].pool_Name == type_String+"_Passive")
                {
                    pools[i].amount_To_Spawn += adding_Amount;
                }
            }
            
        }

        foreach (Pool pool in pools)
        {
            Queue<GameObject> bombPool = new Queue<GameObject>();
            for (int i = 0; i < pool.amount_To_Spawn; i++)
            {
                GameObject obj = Instantiate(pool.spawning_Object);
                obj.SetActive(false);
                bombPool.Enqueue(obj);
            }

            bombPoolDictionary.Add(pool.pool_Name, bombPool);
        }
    }

    /// <summary>
    /// This will turn on the gameobject at the specified position and rotation.(Also dequeues and enques gameobject automatically.)
    /// </summary>
    /// <param name="tag">Name of pool (EXACT)</param>
    /// <param name="position">Position to place this gameobject.</param>
    /// <param name="rotation">Rotation to place this gameobject.</param>
    /// <returns></returns>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!bombPoolDictionary.ContainsKey(tag))
        {
            return null;
        }
        GameObject obj_To_Spawn = bombPoolDictionary[tag].Dequeue();

        obj_To_Spawn.SetActive(true);
        obj_To_Spawn.transform.position = position;
        obj_To_Spawn.transform.rotation = rotation;

        if (obj_To_Spawn.GetComponent<Passive_Ability_Behaviour>() != null) obj_To_Spawn.GetComponent<Passive_Ability_Behaviour>().Begin_Countdown();

        bombPoolDictionary[tag].Enqueue(obj_To_Spawn);

        return obj_To_Spawn;
    }
}
