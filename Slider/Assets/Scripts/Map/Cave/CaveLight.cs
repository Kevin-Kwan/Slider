using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveLight : MonoBehaviour
{
    public bool LightOn { get; private set; }

    [SerializeField]
    internal bool lightOnStart;

    private Texture2D _lightMask;

    public class OnLightSwitchedArgs
    {
        public bool lightOn;
    }
    public static event System.EventHandler<OnLightSwitchedArgs> OnLightSwitched;

    void OnEnable()
    {
        SetLightOn(lightOnStart);
    }

    void OnDisable()
    {
        SetLightOn(false);
    }

    public void SetLightOn(bool value)
    {
        if (LightOn != value)
        {
            LightOn = value;
            Debug.Log("Light " + gameObject.name + " is " + (value ? "on" : "off"));
            if (LightManager.instance != null)
            {
                LightManager.instance.UpdateLightMaskAll();
                LightManager.instance.UpdateMaterials();
            }

            OnLightSwitched?.Invoke(this, new OnLightSwitchedArgs { lightOn = value });
        }
    }

    /* L: Gets the light mask for THIS LIGHT ONLY (see LightManager.cs for the whole world) */
    internal Texture2D GetLightMask(Texture2D heightMask, int worldToMaskDX, int worldToMaskDY, int maskSizeX, int maskSizeY)
    {

        if (heightMask.width != maskSizeX && heightMask.height != maskSizeY)
        {
            Debug.LogError("heightMask did not match expected dimensions in CaveLight.cs");
        }

        _lightMask = new Texture2D(maskSizeX , maskSizeY);
        for (int x = 0; x < maskSizeX; x++)
        {
            for (int y = 0; y < maskSizeY; y++)
            {
                _lightMask.SetPixel(x, y, Color.black);
            }
        }
        Vector2Int lightPos = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in dirs)
        {
            Vector2Int curr = lightPos;
            for (int width = -8; width <= 8; width++)
            {
                //L: The current tile being observed in world coords.
                curr = lightPos + new Vector2Int(dir.y, dir.x) * width;

                //L: "Fake Raycast" from the light's position (+ width) up to 25 tiles before it hits a wall
                for (int j=0; j<=17+8; j++)
                {
                    int maskX = curr.x + worldToMaskDX;
                    int maskY = curr.y + worldToMaskDY;

                    //L: Bounds Check
                    if (maskX < 0 || maskX > maskSizeX-1 || maskY < 0 || maskY > maskSizeY-1)
                    {
                        break;
                    }
                    
                    _lightMask.SetPixel(maskX, maskY, Color.white);

                    // L: Hit Wall Check (Note: This is after so that the start of the tile still gets lit, but nothing else.
                    if (heightMask.GetPixel(maskX, maskY).r > 0.5)
                    {
                        break;
                    }
                    curr += dir;
                }
            }
        }

        _lightMask.Apply();
        return _lightMask;
    }

    //L: Below is for the player to interact with the light, but it's kinda useless since we're not doing that anymore
    /*
    public void SetLightDir(Vector2Int value)
    {
        UpdateLightMap(false);
        lightDir = value;
        UpdateLightMap(true);
    }

    public void RotateLight(bool ccw)
    {
        UpdateLightMap(false);
        lightDir = ccw ? new Vector2Int(-lightDir.y, lightDir.x) : new Vector2Int(lightDir.y, -lightDir.x);
        UpdateLightMap(true);
    }

    public void SetLightOn(bool value)
    {
        lightOn = value;
        UpdateLightMap(value);
    }
    */
}
