using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class EventCameraManager : MonoBehaviour
{
    [Serializable]
    public class BotView
    {
        public BattleBotInterface battleBot;
        public CameraSet cameras;
        [ReadOnly] public float currentWeight;
        [ReadOnly] public float startWeight;
        [ReadOnly] public int groupIndex;
    }

    [Serializable]
    public class CameraSet
    {
        public CinemachineVirtualCameraBase primary;
        public CinemachineVirtualCameraBase alternative;
    }

    [SerializeField] CinemachineTargetGroup botTargetGroup;
    [SerializeField] BotView[] botViews;
    [SerializeField] CameraSet targetGroupCamSet;

    [SerializeField] KeyCode nextCamera = KeyCode.D;
    [SerializeField] KeyCode previousCamera = KeyCode.A;
    [SerializeField] KeyCode toggleAlternativeCamera = KeyCode.S;
    [SerializeField] KeyCode toggleEventCamera = KeyCode.W;

    [SerializeField, ReadOnly] int currentManualFocus = 4;
    [SerializeField, ReadOnly] int currentEventFocus = 4;

    [SerializeField, ReadOnly] bool isAlternativeActive = false;
    [SerializeField, ReadOnly] bool isEventCameraActive = false;

    [SerializeField, ReadOnly] GameObject activeVirtualCameraGO;

    void Start()
    {
        Initialize();
        ShiftFocus(botViews.Length); // Group camera
    }

    private void Initialize()
    {
        targetGroupCamSet.primary.gameObject.SetActive(false);
        targetGroupCamSet.alternative.gameObject.SetActive(false);

        foreach (var botView in botViews)
        {
            var bot = botView.battleBot;
            var index = botTargetGroup.FindMember(bot.transform);
            botView.groupIndex = index;
            botView.currentWeight = botView.startWeight = botTargetGroup.m_Targets[index].weight;

            botView.cameras.primary.gameObject.SetActive(false);
            botView.cameras.alternative.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isEventCameraActive)
        {
            if (Input.GetKeyDown(nextCamera))
                ShiftFocus(currentManualFocus + 1);

            if (Input.GetKeyDown(previousCamera))
                ShiftFocus(currentManualFocus - 1);

            if (Input.GetKeyDown(toggleAlternativeCamera))
            {
                isAlternativeActive = !isAlternativeActive;
                ShiftFocus(currentManualFocus);
            }
        }

        if (Input.GetKeyDown(toggleEventCamera))
        {
            isEventCameraActive = !isEventCameraActive;
        }

        
    }

    void UpdateTargetGroupWeight(bool reset = false)
    {
        foreach (var botView in botViews)
        {
            var bot = botView.battleBot;
            var weight = botView.currentWeight;
            if (reset)
            {
                weight = botView.startWeight;
            }
            botTargetGroup.m_Targets[botView.groupIndex].weight = weight;
        }
    }

    void ShiftFocus(int botViewIndex)
    {
        CameraSet cameraSet;
        var focusIndex = (botViewIndex + botViews.Length + 1) % (botViews.Length + 1);
        if (isEventCameraActive)
        {
            currentEventFocus = focusIndex;
            Debug.Log("Event Focus: " + currentManualFocus);
        }
        else
        {
            currentManualFocus = focusIndex;
            Debug.Log("Manual Focus: " + currentManualFocus);
        }

        if (focusIndex == botViews.Length)
        {
            UpdateTargetGroupWeight(true);
            cameraSet = targetGroupCamSet;
        }
        else
        {
            cameraSet = botViews[focusIndex].cameras;
        }

        ActivateCameraSet(cameraSet);
    }

    void ActivateCameraSet(CameraSet cameraSet)
    {
        if (activeVirtualCameraGO != null)
        {
            activeVirtualCameraGO.SetActive(false);
        }

        if (!isAlternativeActive)
        {
            cameraSet.primary.gameObject.SetActive(true);
            activeVirtualCameraGO = cameraSet.primary.gameObject;
        }
        else
        {
            cameraSet.alternative.gameObject.SetActive(true);
            activeVirtualCameraGO = cameraSet.alternative.gameObject;
        }
    }
}
