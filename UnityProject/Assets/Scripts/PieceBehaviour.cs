using System;
using UnityEngine;

public class PieceBehaviour : MonoBehaviour
{
    // Orange
    private Color color1 = new Color(0.96f, 0.48f, 0f);

    private Color color2 = Color.gray;

    private SpriteRenderer spriteRenderer;

    private int x;

    private int y;

    public static event Action<int, int> OnPress;
    public bool CurrentState { get; private set; }

    public void Initialize(int x, int y)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CurrentState = true;
        UpdateColor();
        this.x = x;
        this.y = y;
    }
    public void OnMouseDown()
    {
        Press();
    }

    public void Press(bool fireOnPress = true)
    {
        if (!PuzzleGeneration.AllowInput)
            return;

        CurrentState = !CurrentState;
        UpdateColor();

        if (fireOnPress && OnPress != null)
        {
            OnPress(x, y);
        }
    }

    private void UpdateColor()
    {
        spriteRenderer.color = CurrentState ? color1 : color2;
    }
}