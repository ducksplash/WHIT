using UnityEngine;
using UnityEngine.UI;

public class ControlPad : MonoBehaviour
{


    [Header("Button Images")]
    public Image A;
    public Image B;
    public Image X;
    public Image Y;

    public Image R1;
    public Image R2;
    public Image R3;

    public Image L1;
    public Image L2;
    public Image L3;

    public Image DPAD;

    public Image START;
    public Image SELECT;

    public void HighlightButton(ControlPadButton button)
    {
        SetAllAlpha(0f);

        Image target = GetImage(button);

        if (target != null)
        {
            SetAlpha(target, 1f);
        }
    }

    private void SetAllAlpha(float alpha)
    {
        SetAlpha(A, alpha);
        SetAlpha(B, alpha);
        SetAlpha(X, alpha);
        SetAlpha(Y, alpha);

        SetAlpha(R1, alpha);
        SetAlpha(R2, alpha);
        SetAlpha(R3, alpha);

        SetAlpha(L1, alpha);
        SetAlpha(L2, alpha);
        SetAlpha(L3, alpha);

        SetAlpha(DPAD, alpha);

        SetAlpha(START, alpha);
        SetAlpha(SELECT, alpha);
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    private Image GetImage(ControlPadButton button)
    {
        switch (button)
        {
            case ControlPadButton.A: return A;
            case ControlPadButton.B: return B;
            case ControlPadButton.X: return X;
            case ControlPadButton.Y: return Y;

            case ControlPadButton.R1: return R1;
            case ControlPadButton.R2: return R2;
            case ControlPadButton.R3: return R3;

            case ControlPadButton.L1: return L1;
            case ControlPadButton.L2: return L2;
            case ControlPadButton.L3: return L3;

            case ControlPadButton.DPAD: return DPAD;

            case ControlPadButton.START: return START;
            case ControlPadButton.SELECT: return SELECT;
        }

        return null;
    }
}

public enum ControlPadButton
{
    A,
    B,
    X,
    Y,
    R1,
    R2,
    R3,
    L1,
    L2,
    L3,
    DPAD,
    START,
    SELECT
}