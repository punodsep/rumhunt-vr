using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RAMHUNT/Gesture Data", fileName = "NewGesture")]
public class GestureData : ScriptableObject
{
    [Header("Info")]
    public string gestureName = "Gesture_01";

    [Header("Finger Bone Data")]
    public List<BoneData> leftHandBones = new();
    public List<BoneData> rightHandBones = new();

    [Header("Head-relative Hand Position")]
    public Vector3 leftHandPosRelHead;
    public Vector3 rightHandPosRelHead;

    [Header("Detection Settings")]
    [Range(0.01f, 0.5f)]
    public float boneThreshold = 0.08f; // tolerance ต่อ bone (เมตร)
    [Range(0.05f, 1f)]
    public float positionThreshold = 0.25f; // tolerance ตำแหน่งมือ
    public int minMatchBones = 12;    // กี่ bone ต้องตรงขั้นต่ำ
    public float holdDuration = 0.2f; // ต้องค้างไว้กี่วินาที
}

[System.Serializable]
public struct BoneData
{
    public OVRSkeleton.BoneId boneId;
    public Vector3 localPosition; // relative to wrist
}