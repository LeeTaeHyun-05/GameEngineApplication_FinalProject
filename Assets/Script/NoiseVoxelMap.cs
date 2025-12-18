
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseVoxelMap : MonoBehaviour
{
    private Dictionary<Vector3Int,Block> blockMap
        = new Dictionary<Vector3Int,Block>();

    [Header("Prefabs")]
    public GameObject dirtPrefab;
    public GameObject grassPrefab;
    public GameObject waterPrefab;
    public GameObject stonePrefab;

    [Header("Map Size")]
    public int width = 32;
    public int depth = 32;
    public int maxHeight = 16;

    [Header("Noise")]
    [SerializeField] float noiseScale = 20f;

    [Header("Water")]
    public int waterLevel = 4;

    float offsetX;
    float offsetZ;

    [Header("Tree")]
    public GameObject treePrefab;
    public GameObject leafPrefab;

    [Range(0f, 1f)]
    public float treeChance = 0.08f;
    public int trunkHeight = 5;
    public int leafRadius = 2;
    public int leafHeight = 2;
    public int treeBorder = 2;

    [Header("Ground Mix")]
    [Range(0f, 1f)]
    public float stoneRate = 0.35f;


    void Start()
    {
        offsetX = Random.Range(-9999f, 9999f);
        offsetZ = Random.Range(-9999f, 9999f);

        Generate();
    }

    public void Generate()
    {
        blockMap.Clear();
        // 간단히: 기존 자식 블록 제거 후 재생성
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float nx = (x + offsetX) / noiseScale;
                float nz = (z + offsetZ) / noiseScale;

                float n = Mathf.PerlinNoise(nx, nz);
                int h = Mathf.FloorToInt(n * maxHeight);
                if (h < 0) h = 0;

                // 땅 생성
                for (int y = 0; y <= h; y++)
                {
                    if (y == h)
                    {

                        PlaceTile(new Vector3Int(x, y, z), ItemType.Grass);
                    }
                    else
                    {
                        float rate = stoneRate;
                        if (y >= h - 2) rate *= 0.2f;

                        ItemType underType = (Random.value < stoneRate)
                            ? ItemType.Stone
                            : ItemType.Dirt;

                        PlaceTile(new Vector3Int(x, y, z), underType);
                    }
                }

                // 물 채우기(수면 이하인데 땅이 낮으면 물)
                for (int y = h + 1; y <= waterLevel; y++)
                    PlaceTile(new Vector3Int(x, y, z), ItemType.Water);

                TrySpawnTree(x, h, z);
            }

        }
        
    }

    void TrySpawnTree(int x, int groundY, int z)
    {
        if (groundY < waterLevel) return;

        if (Random.value > treeChance) return;

        Block baseBlock = FindBlockAt(new Vector3Int(x, groundY, z));
        if (baseBlock == null || baseBlock.type != ItemType.Grass) return;

        int startY = groundY + 1;

        for (int i = 0; i < trunkHeight; i++)
        {
            Vector3Int p = new Vector3Int(x, startY + i, z);

            if (FindBlockAt(p) != null) return;

            PlaceTreeBlock(p, ItemType.Tree);
        }

        Vector3Int top = new Vector3Int(x, startY + trunkHeight - 1, z);

        SpawnLeavesAroundTop(top);
    }

    void SpawnLeavesAroundTop(Vector3Int top)
    {
        for (int dx = -leafRadius; dx <= leafRadius; dx++)
        {
            for (int dy = 0; dy <= leafHeight; dy++)
            {
                for (int dz = -leafRadius; dz <= leafRadius; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue;

                    float dist = Mathf.Abs(dx) + Mathf.Abs(dz) + dy * 0.8f;
                    if (dist > leafRadius + 1.2f) continue;

                    Vector3Int p = new Vector3Int(top.x + dx, top.y + dy, top.z + dz);

                    if (FindBlockAt(p) != null) continue;

                    PlaceTreeBlock(p, ItemType.Leaf);
                }
            }
        }
    }

    void PlaceTreeBlock(Vector3Int pos, ItemType type)
    {
        GameObject prefab = null;

        if (type == ItemType.Tree) prefab = treePrefab;
        if (type == ItemType.Leaf) prefab = leafPrefab;

        if (prefab == null) return;

        var go = Instantiate(prefab, pos, Quaternion.identity, transform);
        go.name = $"{type}({pos.x},{pos.y},{pos.z})";

        var b = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        b.type = type;

        if (type == ItemType.Tree) { b.maxHP = 3; b.dropCount = 1; b.mineable = true; }
        if (type == ItemType.Leaf) { b.maxHP = 1; b.dropCount = 1; b.mineable = true; }
    }
    public void PlaceTile(Vector3Int pos, ItemType type)
    {
        // 이미 블록이 있으면 안 놓기(중복 방지)
        if (blockMap.ContainsKey(pos)) return;

        GameObject prefab = GetPrefab(type);
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, pos, Quaternion.identity, transform);
        go.name = $"{type}({pos.x},{pos.y},{pos.z})";

        // Block 컴포넌트 세팅(강의 스타일)
        Block b = go.GetComponent<Block>() ?? go.AddComponent<Block>();
        b.type = type;

        blockMap[pos] = b;

        // 타입별 간단 기본값
        switch (type)
        {
            case ItemType.Grass:
                b.maxHP = 2; b.dropCount = 1; b.mineable = true;
                break;
            case ItemType.Dirt:
                b.maxHP = 2; b.dropCount = 1; b.mineable = true;
                break;
            case ItemType.Stone:
                b.maxHP = 4; b.dropCount = 1; b.mineable = true;
                break;
            case ItemType.Water:
                b.maxHP = 1; b.dropCount = 0; b.mineable = false; // 물은 채집X
                break;
            case ItemType.Tree:
                b.maxHP = 4; b.dropCount = 1; b.mineable = true;
                break;
            case ItemType.Leaf:
                b.maxHP = 4; b.dropCount = 1; b.mineable = true;
                break;
        }
    }

    public bool RemoveTile(Vector3Int pos)
    {
        if (!blockMap.TryGetValue(pos, out Block block))
            return false;

        blockMap.Remove(pos);
        Destroy(block.gameObject);
        return true;
    }

    Block FindBlockAt(Vector3Int pos)
    {
        blockMap.TryGetValue(pos, out Block block);
        return block;   
    }

    GameObject GetPrefab(ItemType type)
    {
        switch (type)
        {
            case ItemType.Dirt: return dirtPrefab;
            case ItemType.Grass: return grassPrefab;
            case ItemType.Water: return waterPrefab;
            case ItemType.Stone: return stonePrefab;
            case ItemType.Tree: return treePrefab;
            case ItemType.Leaf: return leafPrefab;
        }
        return null;
    }
}
