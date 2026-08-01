using UnityEngine;
using System.Collections; // จำเป็นต้องใช้เพื่อให้รัน Coroutine ได้

public class HealthPickup : MonoBehaviour
{
    [Header("Heal Settings")]
    [SerializeField] private float healAmount = 10f;

    [Header("Lifetime Settings")]
    [SerializeField] private float lifetime = 3f;           // เวลาทั้งหมดก่อนหายไป
    [SerializeField] private float startBlinkingAt = 2f;    // เริ่มกระพริบที่วินาทีที่เท่าไหร่
    [SerializeField] private float blinkInterval = 0.1f;    // ความเร็วในการกระพริบ

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // ดึง SpriteRenderer มาเพื่อใช้เปิด/ปิดภาพตอนกระพริบ
        spriteRenderer = GetComponent<SpriteRenderer>();

        // เริ่มนับเวลาถอยหลังทันทีที่ไอเทมถูกสร้างขึ้นมา
        StartCoroutine(LifeRoutine());
    }

    private IEnumerator LifeRoutine()
    {
        // 1. รอจนกว่าจะถึงเวลาที่กำหนดให้เริ่มกระพริบ (เช่น 3 วินาที)
        yield return new WaitForSeconds(startBlinkingAt);

        // 2. คำนวณเวลาที่เหลือสำหรับกระพริบ (5 - 3 = 2 วินาที)
        float blinkDuration = lifetime - startBlinkingAt;
        float blinkTimer = 0f;

        // 3. เริ่มกระพริบสลับไปมา
        if (spriteRenderer != null)
        {
            while (blinkTimer < blinkDuration)
            {
                // สลับสถานะเปิดเป็นปิด ปิดเป็นเปิด
                spriteRenderer.enabled = !spriteRenderer.enabled;

                yield return new WaitForSeconds(blinkInterval);
                blinkTimer += blinkInterval;
            }
        }

        // 4. เมื่อหมดเวลา (ครบ 5 วินาที) ให้ทำลายทิ้ง
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
                Destroy(gameObject); // ถูกเก็บปุ๊บก็ทำลายทิ้งทันที
            }
        }
    }
}