using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Block Stat")]
    public ItemType type = ItemType.Dirt;

    public int maxHP = 10;
    [HideInInspector] public int hp;

    public int dropCount = 1;
    public bool mineable = true;

    void Awake()
    {
        hp = maxHP;

        // 콜라이더 없으면 자동 추가(강의 스타일)
        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();

        // 레이캐스트용 태그/레이어는 프로젝트 설정에 맞춰 사용
        // (필수는 아님)
    }

    public void Hit(int damage, Inventory inv)
    {
        if (!mineable) return;

        hp -= damage;

        if (hp <= 0)
        {
            if (inv != null && dropCount > 0)
                inv.Add(type, dropCount);

            Destroy(gameObject);
        }
    }
}
