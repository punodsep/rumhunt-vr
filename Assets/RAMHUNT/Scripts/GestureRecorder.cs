using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GestureRecorder : MonoBehaviour
{
    [Header("References")]
    public OVRSkeleton leftSkeleton;
    public OVRSkeleton rightSkeleton;
    public Transform headTransform;

    [Header("Save Settings")]
    public string savePath = "Assets/RAMHUNT/Gestures/";
    public string gestureName = "Gesture_01";

    [Header("Delay")]
    public float recordDelay = 5f;

    private bool isRecording = false;

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One) && !isRecording)
        {
            StartCoroutine(RecordWithDelay());
        }
    }

    IEnumerator RecordWithDelay()
    {
        isRecording = true;

        Debug.Log($"[Recorder] จะบันทึกใน {recordDelay} วินาที...");

        yield return new WaitForSeconds(recordDelay);

        RecordCurrentPose();

        isRecording = false;
    }

    public void RecordCurrentPose()
    {
#if UNITY_EDITOR
        var data = ScriptableObject.CreateInstance<GestureData>();
        data.gestureName = gestureName;

        data.leftHandBones = CaptureBones(leftSkeleton);
        data.rightHandBones = CaptureBones(rightSkeleton);

        if (headTransform != null)
        {
            data.leftHandPosRelHead = headTransform.InverseTransformPoint(
                leftSkeleton.transform.position);

            data.rightHandPosRelHead = headTransform.InverseTransformPoint(
                rightSkeleton.transform.position);
        }

        string path = savePath + gestureName + ".asset";

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"[Recorder] บันทึก '{gestureName}' → {path}");
#endif
    }

    List<BoneData> CaptureBones(OVRSkeleton skeleton)
    {
        var list = new List<BoneData>();

        if (skeleton == null || !skeleton.IsInitialized)
            return list;

        Transform wrist = null;

        foreach (var b in skeleton.Bones)
        {
            if (b.Id == OVRSkeleton.BoneId.Hand_WristRoot)
                wrist = b.Transform;
        }

        foreach (var b in skeleton.Bones)
        {
            Vector3 localPos = wrist != null
                ? wrist.InverseTransformPoint(b.Transform.position)
                : b.Transform.localPosition;

            list.Add(new BoneData
            {
                boneId = b.Id,
                localPosition = localPos
            });
        }

        return list;
    }
}