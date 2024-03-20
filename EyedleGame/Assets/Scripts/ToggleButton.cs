using System;
using Tobii.Gaming;
using UnityEngine;

[Serializable]
public class ToggleButton : MonoBehaviour
{
    [SerializeField] bool mouseHoverEnabled = true;
    [SerializeField] protected FollowPoint followComponent;
    [SerializeField] Material activeMat;
    [SerializeField] Material deactiveMat;

    private bool focused = false;
    public enum FocusedMode 
    {
        ACTIVATING,
        DEACTIVATING,
        ACTIVE,
        DEACTIVE
    }

    private FocusedMode focusedMode = FocusedMode.DEACTIVE;
    float lastFocusChange = 0;
    float DEACTIVATE_TIME = 1.5f;
    float ACTIVATE_TIME = 1.5f;
    public void init(float DEACTIVATE_TIME, float ACTIVATE_TIME, FocusedMode focusedMode)
    {
        TobiiAPI.Start(null);

        this.DEACTIVATE_TIME = DEACTIVATE_TIME;
        this.ACTIVATE_TIME = ACTIVATE_TIME;
        this.focusedMode = focusedMode;
    }

    //private static bool startedTobii = false;
    void Start()
    {
        //if children override start, call this in them
        //TobiiAPI.Start(null);
    }

    // Please override these methods.
    /*
    public virtual GameObject GetGameObject() 
    {
        Debug.LogWarning("Please override getself");
        return null;
    }
    */
    public virtual GameObject GetGameObject() => null;
    public virtual void OnActivate() => print("Please override activate");
    public virtual void OnDeactivate() => print("Please override deactivate");

    public void UpdateFocus()
    {
        bool newFocused = IsFocused(); 
        lastFocusChange += Time.deltaTime;

        if (GetGameObject().name == "slidebar")
        {
            print(focused);
            print("state " + focusedMode);
        }

        bool recur = true;
        while (recur)
        {
            recur = false;
            switch (focusedMode)
            {
                case FocusedMode.DEACTIVATING:
                {
                    if (!focused)
                    {
                        lastFocusChange = 0;
                        focusedMode = FocusedMode.ACTIVE;
                        break;
                    }
                    if (lastFocusChange > DEACTIVATE_TIME)
                    {
                        focusedMode = FocusedMode.DEACTIVE;
                        followComponent.isWalking = false;
                        var renderer = GetComponent<Renderer>();
                            if (deactiveMat != null)
                            {
                                renderer.material = deactiveMat;
                            }
                            else
                            {
                                renderer.material.color = Color.red;
                            }
                            recur = true;
                        OnDeactivate();
                    }
                }
                break;
                case FocusedMode.ACTIVATING:
                {
                    if (!focused)
                    {
                        lastFocusChange = 0;
                        focusedMode = FocusedMode.DEACTIVE;
                        break;
                    }
                    if (lastFocusChange > ACTIVATE_TIME)
                    {
                        focusedMode = FocusedMode.ACTIVE;
                        followComponent.isWalking = true;
                        var renderer = GetComponent<Renderer>();
                            if (activeMat != null)
                            {
                                renderer.material = activeMat;
                            }
                            else 
                            {
                                renderer.material.color = Color.green;
                            }
                                                       
                        recur = true;
                        OnActivate();
                    }
                }
                break;
                case FocusedMode.DEACTIVE:
                {
                    if (focused)
                    {
                        lastFocusChange = 0;
                        focusedMode = FocusedMode.ACTIVATING;
                    }
                }
                break;
                case FocusedMode.ACTIVE:
                {
                    if (focused)
                    {
                        lastFocusChange = 0;
                        focusedMode = FocusedMode.DEACTIVATING;
                    }
                }
                break;
            }
        }

        /*
        if (newFocused != focused)
            print("changed to " + Enum.GetName(typeof(FocusedMode), focusedMode));
        */

        focused = newFocused;
    }

    private bool IsFocused()
    {
        var focused = TobiiAPI.GetFocusedObject();
        bool tobiiFocus = focused != null && focused == GetGameObject();
        if (!mouseHoverEnabled) return tobiiFocus;
        if (tobiiFocus) return true;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, layerMask: LayerMask.GetMask("UI")))
        {
            if (hit.collider.gameObject == GetGameObject())
                print("eurkea " + GetGameObject());

            return hit.collider.gameObject == GetGameObject();
        }
        return false;
    }
}