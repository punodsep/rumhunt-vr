using System;
using System.Collections.Generic;
using UnityEngine;

public class GestureDetector : MonoBehaviour
{
    [Header("References")]
    public OVRSkeleton leftSkeleton;
    public OVRSkeleton rightSkeleton;
    public Transform headTransform;
    public List<GestureData> gestures;

    [Header("Detection Settings")]
    [Range(0f, 1f)]
    public float scoreThreshold = 0.15f; // ยิ่งต่ำยิ่งแม่น

    public int stableFramesRequired = 10;

    // Events
    public event Action<GestureData> OnGestureDetected;
    public event Action OnGestureLost;

    GestureData _currentGesture;
    GestureData _holdCandidate;

    float _holdTimer;
    int _stableFrames;

    void Update()
    {
        if (!leftSkeleton.IsInitialized || !rightSkeleton.IsInitialized)
            return;

        GestureData matched = FindBestMatch();

        if (matched != null)
        {
            if (matched == _holdCandidate)
            {
                _stableFrames++;
                _holdTimer += Time.deltaTime;

                if (_stableFrames >= stableFramesRequired &&
                    _holdTimer >= matched.holdDuration &&
                    matched != _currentGesture)
                {
                    _currentGesture = matched;

                    Debug.Log($"[GestureDetector] Detected: {matched.gestureName}");

                    OnGestureDetected?.Invoke(matched);
                }
            }
            else
            {
                _holdCandidate = matched;
                _holdTimer = 0f;
                _stableFrames = 0;
            }
        }
        else
        {
            if (_currentGesture != null)
            {
                Debug.Log("[GestureDetector] Gesture Lost");

                _currentGesture = null;
                OnGestureLost?.Invoke();
            }

            _holdCandidate = null;
            _holdTimer = 0f;
            _stableFrames = 0;
        }
    }

    GestureData FindBestMatch()
    {
        GestureData best = null;
        float bestScore = float.MaxValue;

        foreach (var g in gestures)
        {
            float score = ScoreGesture(g);

            Debug.Log($"{g.gestureName} Score = {score}");

            if (score < bestScore)
            {
                bestScore = score;
                best = g;
            }
        }

        return (best != null && bestScore < scoreThreshold)
            ? best
            : null;
    }

    float ScoreGesture(GestureData data)
    {
        int matched = 0;
        int total = 0;

        Dictionary<OVRSkeleton.BoneId, Vector3> leftMap =
            GetBoneMap(leftSkeleton);

        Dictionary<OVRSkeleton.BoneId, Vector3> rightMap =
            GetBoneMap(rightSkeleton);

        // Left Hand
        foreach (var bd in data.leftHandBones)
        {
            total++;

            if (leftMap.TryGetValue(bd.boneId, out var pos))
            {
                float dist = Vector3.Distance(pos, bd.localPosition);

                if (dist < data.boneThreshold)
                    matched++;
            }
        }

        // Right Hand
        foreach (var bd in data.rightHandBones)
        {
            total++;

            if (rightMap.TryGetValue(bd.boneId, out var pos))
            {
                float dist = Vector3.Distance(pos, bd.localPosition);

                if (dist < data.boneThreshold)
                    matched++;
            }
        }

        // match ไม่พอ = fail
        if (total == 0 || matched < data.minMatchBones)
            return float.MaxValue;

        // เช็คตำแหน่งมือ relative กับหัว
        if (headTransform != null)
        {
            Vector3 lPos =
                headTransform.InverseTransformPoint(
                    leftSkeleton.transform.position);

            Vector3 rPos =
                headTransform.InverseTransformPoint(
                    rightSkeleton.transform.position);

            if (Vector3.Distance(lPos, data.leftHandPosRelHead)
                > data.positionThreshold)
                return float.MaxValue;

            if (Vector3.Distance(rPos, data.rightHandPosRelHead)
                > data.positionThreshold)
                return float.MaxValue;
        }

        // normalized score
        float accuracy = (float)matched / total;

        return 1f - accuracy;
    }

    Dictionary<OVRSkeleton.BoneId, Vector3> GetBoneMap(
        OVRSkeleton skeleton)
    {
        var map =
            new Dictionary<OVRSkeleton.BoneId, Vector3>();

        Transform wrist = null;

        foreach (var b in skeleton.Bones)
        {
            if (b.Id == OVRSkeleton.BoneId.Hand_WristRoot)
            {
                wrist = b.Transform;
                break;
            }
        }

        foreach (var b in skeleton.Bones)
        {
            Vector3 localPos =
                wrist != null
                ? wrist.InverseTransformPoint(
                    b.Transform.position)
                : b.Transform.localPosition;

            map[b.Id] = localPos;
        }

        return map;
    }
}