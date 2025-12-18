using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHarvester : MonoBehaviour
{
    public float rayDistance = 5f;
    public LayerMask hitMask = ~0;

    public int toolDamage = 1;
    public float hitCooldown = 0.15f;

    public float _nextHitTime;

    Camera _cam;

    public Inventory inventory;     // 자동으로 붙일 수도 있음
    InventoryUI invenUI;      // 선택된 슬롯 가져오려면 필요(강의 흐름)
    public GameObject selectedBlock;

    void Awake()
    {
        _cam = Camera.main;

        if (inventory == null)
            inventory = gameObject.AddComponent<Inventory>();
        invenUI = FindObjectOfType<InventoryUI>();
    }

    void Update()
    {
        if (invenUI.selectedIndex < 0)
        {
            selectedBlock.transform.localScale = Vector3.zero;
            if (Input.GetMouseButtonDown(0))
            {
                _nextHitTime = Time.time + hitCooldown;

                Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                if (Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
                {
                    var block = hit.collider.GetComponent<Block>();
                    if (block != null)
                    {
                        block.Hit(toolDamage, inventory);
                    }
                }
            }

            return;
        }
        ItemType selected = invenUI.GetInventorySlot();
        bool isTool = (selected == ItemType.Axe || selected == ItemType.PickAxe);

        if (selectedBlock != null)
        {
            if (!isTool)
            {
                Ray previewRay = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                if (Physics.Raycast(previewRay, out var previewHit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
                {
                    Vector3Int placePos = AdjacentCellOnHitFace(previewHit);

                    selectedBlock.transform.localScale = Vector3.one;
                    selectedBlock.transform.position = placePos;
                    selectedBlock.transform.rotation = Quaternion.identity;
                }
                else
                {
                    selectedBlock.transform.localScale = Vector3.zero;
                }
            }
            else
            {
                selectedBlock.transform.localScale = Vector3.zero;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out var hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (isTool)
                {
                    var block = hit.collider.GetComponent<Block>();
                    if (block != null)
                    {
                        int dmg = toolDamage;

                        if (selected == ItemType.Axe && block.type == ItemType.Tree)
                            dmg += 2;

                        if (selected == ItemType.PickAxe && block.type == ItemType.Stone)
                            dmg += 2;

                        block.Hit(dmg, inventory);
                    }

                    return;
                }

                Vector3Int placePos = AdjacentCellOnHitFace(hit);

                if (inventory.Consume(selected, 1))
                {
                    FindObjectOfType<NoiseVoxelMap>().PlaceTile(placePos, selected);
                }
            }
        }
    }

    Ray GetCenterRay()
    {
        return _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    }

    // “맞은 면 기준으로 옆 칸” 계산(강의와 같은 아이디어) :contentReference[oaicite:11]{index=11}
    static Vector3Int AdjacentCellOnHitFace(RaycastHit hit)
    {
        Vector3 baseCenter = hit.collider.transform.position;
        Vector3 adjCenter = baseCenter + hit.normal; // 노말 방향으로 한 칸
        return Vector3Int.RoundToInt(adjCenter);
    }

    ItemType GetSelectedItem()
    {
        // UI가 없으면 기본값
        if (invenUI == null) return ItemType.Dirt;

        try
        {
            return invenUI.GetInventorySlot();
        }
        catch
        {
            return ItemType.Dirt;
        }
    }
}
