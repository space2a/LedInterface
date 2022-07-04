using LedInterface;
using System;
using System.Collections.Generic;
using System.Text;


public class PanelIdentity
{
    private int CoordsX, CoordsY = 0;
    public int xSize, ySize = 0;
    public Panel PanelElement;
    public PanelDirection Direction;

    public PanelIdentity(int[] coords)
    {
        CoordsX = coords[0];
        CoordsY = coords[1];
    }

    public int[] getCoords()
    {
        return new int[] { CoordsX, CoordsY };
    }

}

public class LedIdentity
{
    public int CoordsX, CoordsY = 0;
    public LedColor Color = new LedColor();
}

public class LedColor
{
    public int R, G, B = 0;

    public string getColor()
    {
        return R.ToString("000") + "," + G.ToString("000") + "," + B.ToString("000");
    }

}

public enum PanelDirection
{
    Origin = -1,
    Right = 0,
    Bottom = 1
}