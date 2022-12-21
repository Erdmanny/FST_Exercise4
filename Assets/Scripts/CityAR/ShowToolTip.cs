using DefaultNamespace;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

public class ShowToolTip : MonoBehaviour, IMixedRealityTouchHandler 
{
    public int metric;
    public Entry entry;
    public GameObject toolTipPrefab;
    private GameObject toolTipInstance;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnTouchStarted(HandTrackingInputEventData eventData)
    {
        toolTipInstance = Instantiate(toolTipPrefab);

        switch(metric){
            case 1:
                toolTipInstance.GetComponent<ToolTip>().ToolTipText = "Lines: " + entry.numberOfLines;
                break;
            case 2:
                toolTipInstance.GetComponent<ToolTip>().ToolTipText = "Methods: " + entry.numberOfMethods;
                break;
            case 3:
                toolTipInstance.GetComponent<ToolTip>().ToolTipText = "Abstr. Classes: " + entry.numberOfAbstractClasses;
                break;
            case 4:
                toolTipInstance.GetComponent<ToolTip>().ToolTipText = "Interfaces: " + entry.numberOfInterfaces;
                break;
        }


        toolTipInstance.transform.localPosition = eventData.InputData;
        Destroy(toolTipInstance, 10.0f);
    }

    public void OnTouchCompleted(HandTrackingInputEventData eventData)
    {
        if(toolTipInstance != null){
            Destroy(toolTipInstance);
        }
    }

    public void OnTouchUpdated(HandTrackingInputEventData eventData)
    {
        if(toolTipInstance != null){
            toolTipInstance.transform.localPosition = eventData.InputData;
        }
    }
}
