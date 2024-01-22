using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LineScript : NetworkBehaviour
{
    [SyncVar (hook = nameof(OnStartPositionChanged))]
    public Vector3 startPosition;

    [SyncVar(hook = nameof(OnEndPositionChanged))]
    public Vector3 endPosition;

    [SyncVar(hook = nameof(OnActiveChanged))]
    public bool active;

    [SyncVar(hook = nameof(OnColorChanged))]
    public Color color;

    public void Awake()
    {
        active = true;
    }

    public void SetLineColor(Color color)
    {
        this.color = color;
    }

    public void OnColorChanged(Color oldColor, Color newColor)
    {
        transform.Find("Line").gameObject.GetComponent<LineRenderer>().material.color = newColor;
    }
    
    public void SetLineActive(bool active)
    {
        this.active = active;
    }

    public void OnActiveChanged(bool oldActive, bool newActive)
    {
        transform.Find("Line").gameObject.SetActive(newActive);
    }

    public void SetStartPosition(Vector3 startPosition)
    {
        this.startPosition = startPosition;
    }

    public void SetEndPosition(Vector3 endPosition)
    {
        this.endPosition = endPosition;
    }

    public void OnStartPositionChanged(Vector3 oldSP, Vector3 newSP)
    {
        transform.Find("Line").gameObject.GetComponent<LineRenderer>().SetPosition(0, newSP);
    }

    public void OnEndPositionChanged(Vector3 oldEP, Vector3 newEP)
    {
        transform.Find("Line").gameObject.GetComponent<LineRenderer>().SetPosition(1, newEP);
    }
}
