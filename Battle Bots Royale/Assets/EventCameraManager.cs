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
    [SerializeField] KeyCode nextCamera;
    [SerializeField] KeyCode previousCamera;
    [SerializeField] KeyCode alternativeCamera;

    [SerializeField, ReadOnly] int currentManualFocus = -1;
    [SerializeField, ReadOnly] int currentEventFocus = -1;
    [SerializeField, ReadOnly] bool isAlternativeActive = false;
    [SerializeField, ReadOnly] GameObject activeVirtualCameraGO;

    void Start()
    {
        Initialize();
        
        if (activeVirtualCameraGO == null)
        {
            currentManualFocus = botViews.Length;
        }

        ShiftFocus(currentManualFocus);
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
        if (Input.GetKeyDown(nextCamera)) ShiftFocus(currentManualFocus + 1);
        if (Input.GetKeyDown(previousCamera)) ShiftFocus(currentManualFocus - 1);
        if (Input.GetKeyDown(alternativeCamera))
        {
            isAlternativeActive = !isAlternativeActive;
            ShiftFocus(currentManualFocus);
        }

        if (currentManualFocus == -1)
        {
            // Run event focus
            UpdateTargetGroupWeight();
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
        if (botViewIndex <= -1)
        {
            currentManualFocus = -1; // set to event camera mode
            Debug.Log(currentManualFocus);
            return;
        }
        else
        {
            CameraSet cameraSet;
            currentManualFocus = botViewIndex;
            Debug.Log(currentManualFocus);

            if (currentManualFocus >= botViews.Length)
            {
                currentManualFocus = botViews.Length;
                UpdateTargetGroupWeight(true);
                cameraSet = targetGroupCamSet;
            }
            else
            {
                cameraSet = botViews[botViewIndex].cameras;
            }
            ActivateCameraSet(cameraSet);
        }
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
