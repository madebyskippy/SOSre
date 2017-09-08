using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Material DB")]
public class MaterialDB : ScriptableObject {
    [SerializeField]
    private Material[] tileMats;
    public Material[] TileMats { get { return tileMats; } }


  /*  [SerializeField]
    private Material tileMatY;
    public Material TileMatY { get { return tileMatY; }}

	[SerializeField]
	private Material tileMatB;
	public Material TileMatB { get { return tileMatB; } }

	[SerializeField]
	private Material tileMatG;
	public Material TileMatG { get { return tileMatG; } }

	[SerializeField]
	private Material tileMatR;
	public Material TileMatR { get { return tileMatR; } }*/

    [SerializeField]
    private Material[] boardMats;
    public Material[] BoardMats { get { return boardMats; }}

}
