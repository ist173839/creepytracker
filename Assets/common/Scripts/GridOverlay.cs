using UnityEngine;
using System.Collections;
// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class GridOverlay : MonoBehaviour
{
	public bool ShowMain = true;
	public bool ShowSub = false;
	public int GridSizeX;
	public int GridSizeY;
	public int GridSizeZ;
	public float SmallStep;
	public float LargeStep;
	public float StartX;
	public float StartY;
	public float StartZ;
	private float _offsetY = 0;

    public Material JointMaterial;

    private Material _lineMaterial;

    public Color mainColor;
    public Color subColor;

    //private Color mainColor = new Color(0f,1f,0f,1f);
    //private Color subColor = new Color(0f,0.5f,0f,1f);

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void Start()
    {
        Debug.Log("start?");
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Update() { }

    private void CreateLineMaterial()
    {
        if (!_lineMaterial)
        {
            _lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
                                        "SubShader { Pass { " +
                                        " Blend SrcAlpha OneMinusSrcAlpha " +
                                        " ZWrite Off Cull Off Fog { Mode Off } " +
                                        " BindChannels {" +
                                        " Bind \"vertex\", vertex Bind \"color\", color }" +
                                        "} } }");
            _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            _lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void OnPostRender()
    {
        CreateLineMaterial();
        // set the current material
        _lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        if (ShowSub)
        {
            GL.Color(subColor);
            //Layers
            for (float j = 0; j <= GridSizeY; j += SmallStep)
            {
                //X axis lines
                for (float i = 0; i <= GridSizeZ; i += SmallStep)
                {
                    GL.Vertex3(StartX + GridSizeX, j + _offsetY, StartZ + i);
                    GL.Vertex3(GridSizeX, j + _offsetY, StartZ + i);
                }
                //Z axis lines
                for (float i = 0; i <= GridSizeX; i += SmallStep)
                {
                    GL.Vertex3(StartX + i, j + _offsetY, StartZ);
                    GL.Vertex3(StartX + i, j + _offsetY, GridSizeZ);
                }
            }
            //Y axis lines
            for (float i = 0; i <= GridSizeZ; i += SmallStep)
            {
                for (float k = 0; k <= GridSizeX; k += SmallStep)
                {
                    GL.Vertex3(StartX + k, StartY + _offsetY, StartZ + i);
                    GL.Vertex3(StartX + k, GridSizeY + _offsetY, StartZ + i);
                }
            }
        }
        if (ShowMain)
        {
            GL.Color(mainColor);
            //Layers
            for (float j = 0; j <= GridSizeY; j += LargeStep)
            {
                //X axis lines
                for (float i = 0; i <= GridSizeZ; i += LargeStep)
                {
                    GL.Vertex3(StartX, j + _offsetY, StartZ + i);
                    GL.Vertex3(GridSizeX, j + _offsetY, StartZ + i);
                }
                //Z axis lines
                for (float i = 0; i <= GridSizeX; i += LargeStep)
                {
                    GL.Vertex3(StartX + i, j + _offsetY, StartZ);
                    GL.Vertex3(StartX + i, j + _offsetY, GridSizeZ);
                }
            }
            //Y axis lines
            for (float i = 0; i <= GridSizeZ; i += LargeStep)
            {
                for (float k = 0; k <= GridSizeX; k += LargeStep)
                {
                    GL.Vertex3(StartX + k, StartY + _offsetY, StartZ + i);
                    GL.Vertex3(StartX + k, GridSizeY + _offsetY, StartZ + i);
                }
            }
        }
        GL.End();
    }
}
///////////////////////////////////////////////////////////////
/*

 //<<<<<<< HEAD
//    public bool showMain = true;
//    public bool showSub = false;
//    public int gridSizeX;
//    public int gridSizeY;
//    public int gridSizeZ;
//    public float smallStep;
//    public float largeStep;
//    public float startX;
//    public float startY;
//    public float startZ;
//    private float offsetY = 0;
//    private float scrollRate = 0.1f;
//    private float lastScroll = 0f;
//    private Material lineMaterial;
//=======



 */
