using UnityEngine;
using UnityEngine.VFX;

namespace VFXTools
{
    public class SlashArcEffect : MonoBehaviour
    {
        [Header("Arc Settings")]
        public Transform pivotPoint;      // Spawn Point ที่สร้างไว้ตรงหน้าตัวละคร
        public float radius = 2f;         // ความกว้างของวงกลม
        public float arcAngle = 180f;     // ปาดเป็นครึ่งวงกลม (180 องศา)
        public float sweepSpeed = 300f;   // ความเร็วในการปาด
        public float startAngle = 0f;     // จุดเริ่มปาด (0 = ขวา, 90 = บน, 180 = ซ้าย)

        [Header("Rotation Settings (ปรับองศาให้ตั้งตรงหน้า)")]
        [Tooltip("แกน X: ควบคุมการก้ม/เงยของวงกลม")]
        public float rotationX = 0f;
        [Tooltip("แกน Y: ควบคุมการหันซ้าย/ขวา")]
        public float rotationY = 0f;
        [Tooltip("แกน Z: ควบคุมการเอียงเฉียง (ปาดเฉียงลงซ้ายลองปรับแกนนี้)")]
        public float rotationZ = -40f;

        [Header("VFX")]
        public VisualEffect slashVFX;

        private float currentAngle;
        private bool isSlashing = false;
        private float targetAngle;

        void Start()
        {
            if (slashVFX != null)
                slashVFX.enabled = false;
        }

        public void TriggerSlash()
        {
            if (pivotPoint == null) return;

            currentAngle = startAngle;
            targetAngle = startAngle + arcAngle;
            isSlashing = true;

            UpdatePosition();

            if (slashVFX != null)
            {
                slashVFX.enabled = true;
                slashVFX.Play();
            }
        }

        void Update()
        {
            if (!isSlashing || pivotPoint == null) return;

            // 1. คำนวณมุมเพิ่มขึ้นตามเฟรมเรต
            currentAngle += sweepSpeed * Time.deltaTime;

            // 2. เช็คก่อนว่ามุมเกินเป้าหมายหรือยัง
            if (currentAngle >= targetAngle)
            {
                // บังคับให้อยู่ที่มุมจบพอดี ไม่ให้เลยไปไกล
                currentAngle = targetAngle;
                UpdatePosition();

                // หยุดการวิ่งปาดทันที เพื่อไม่ให้ฟังก์ชันนี้ทำงานซ้ำในเฟรมถัดไป
                isSlashing = false;

                // สั่งให้ VFX Graph หยุดการสร้างเม็ดปาดเพิ่ม (ส่งสัญญาณ Stop) 
                // แต่ยังไม่สั่งเด็ดขาด เพื่อเปิดโอกาสให้หาง Trail ที่เหลือจางหายไปเอง
                if (slashVFX != null)
                {
                    slashVFX.Stop();
                }

                // รอเคลียร์ระบบและซ่อน Component ทั้งหมดตามเวลาที่ตั้งไว้
                Invoke(nameof(StopVFX), 0.5f);
            }
            else
            {
                // ถ้ายังปาดไม่เสร็จ ให้ขยับตำแหน่งเอฟเฟกต์ตามปกติ
                UpdatePosition();
            }
        }

        private void StopVFX()
        {
            if (slashVFX != null)
            {
                // ปิดการทำงานของ Component ทั้งหมดหลังจากที่ Trail จางหายลับตาไปแล้ว
                slashVFX.enabled = false;
            }
        }

        // ฟังก์ชันคำนวณตำแหน่งและองศาการตั้งระนาบ
        private Vector3 CalculateArcPosition(float angle)
        {
            float rad = angle * Mathf.Deg2Rad;

            // 1. สร้างวงกลมแนวราบ (Flat บนพื้นแกน X และ Z)
            Vector3 localPos = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;

            // 2. ใช้ค่า Rotation จาก Inspector มาหมุนตั้งระนาบขึ้นมาแบบอิสระ
            Quaternion localRotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
            localPos = localRotation * localPos;

            // 3. แปลงเป็นพิกัดโลกโดยอิงจากตำแหน่งและทิศทางการหันของ Pivot Point
            return pivotPoint.position + (pivotPoint.rotation * localPos);
        }

        private void UpdatePosition()
        {
            slashVFX.transform.position = CalculateArcPosition(currentAngle);
            slashVFX.transform.LookAt(pivotPoint.position);
        }

        

        // เปลี่ยนเป็น OnDrawGizmosSelected เพื่อให้แสดง "เส้นเดียว" เฉพาะตัวที่เราคลิกเลือกใน Hierarchy
        void OnDrawGizmosSelected()
        {
            if (pivotPoint == null) return;

            Gizmos.color = Color.red;
            float endAngle = startAngle + arcAngle;

            Vector3 previousPoint = CalculateArcPosition(startAngle);

            for (float a = startAngle + 5f; a <= endAngle; a += 5f)
            {
                Vector3 nextPoint = CalculateArcPosition(a);
                Gizmos.DrawLine(previousPoint, nextPoint);
                Gizmos.DrawSphere(nextPoint, 0.03f);
                previousPoint = nextPoint;
            }
        }
    }
}