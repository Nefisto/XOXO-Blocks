using System.Collections.Generic;
using System.Linq;
using NTools;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

[CreateAssetMenu]
public partial class PieceData : ScriptableObject
{
    // To send through network to request a specific type of construction
    //      0 - name of the piece
    //      1 - IsMirrored?
    //          0 no
    //          1 yes
    //      2 - Amount of clockwise rotations
    private const string PieceCodeTemplate = "{0}_{1}_{2}";

    [TitleGroup("Settings")]
    [SerializeField]
    private bool shouldMirror;

    [SerializeField]
    private PieceRotation additionalRotations;

    [field: SerializeField]
    public bool IsBomb { get; private set; }

    // CLEAN: Move it to some common place 
    [SerializeField]
    private NDictionary<Color32, TileSide> colorToTileKind;

    [TitleGroup("References")]
    [SerializeField]
    private Texture2D pieceTexture;

    public List<string> GetAllPieceCodes()
    {
        var allCodes = new List<string> { string.Format(PieceCodeTemplate, name, 0, 0) };

        AddCodes(0);

        if (!shouldMirror)
            return allCodes;

        allCodes.Add(string.Format(PieceCodeTemplate, name, 1, 0));
        AddCodes(1);

        return allCodes;

        void AddCodes (int mirrorFlag)
        {
            for (var i = 1; i <= 3; i++)
            {
                var rotation = (PieceRotation)(1 << (i - 1));
                if (additionalRotations.HasFlag(rotation))
                    allCodes.Add(string.Format(PieceCodeTemplate, name, mirrorFlag, i));
            }
        }
    }

    public PieceConfiguration GetPieceConfiguration (string pieceConfigurationCode)
    {
        var splitString = pieceConfigurationCode.Split("_");
        var isMirrored = int.Parse(splitString.Skip(1).First()) == 1;
        var amountOfRotations = int.Parse(splitString.Skip(2).First());

        var matrix = GetPixelsAsMatrix(pieceTexture);

        if (isMirrored)
            matrix = MirrorHorizontally(matrix);

        for (var i = 0; i < amountOfRotations; i++)
            matrix = RotateClockwise(matrix);

        var configuration = Internal_GetConfiguration(matrix);
        configuration.pieceCode = pieceConfigurationCode;
        configuration.isMirrored = isMirrored;
        configuration.amountOfRotations = amountOfRotations;

        return configuration;
    }

    private static Color[,] MirrorHorizontally (Color[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var mirrored = new Color[rows, cols];

        for (var y = 0; y < rows; y++)
        for (var x = 0; x < cols; x++)
            mirrored[y, x] = matrix[y, cols - 1 - x];

        return mirrored;
    }

    private static Color[,] GetPixelsAsMatrix (Texture2D texture)
    {
        var width = texture.width;
        var height = texture.height;
        var pixels = texture.GetPixels();
        var matrix = new Color[height, width];

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            matrix[y, x] = pixels[y * width + x];

        return matrix;
    }

    private Color[,] RotateClockwise (Color[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var rotated = new Color[cols, rows];

        for (var i = 0; i < rows; i++)
        for (var j = 0; j < cols; j++)
            rotated[j, rows - 1 - i] = matrix[i, j];

        return rotated;
    }

    private PieceConfiguration Internal_GetConfiguration (Color[,] matrix)
    {
        // By default when reading a texture the i is the line and j is the collumn 
        //  but when drawing the peice we are drawing it as coordinates, so the i = x (columns) and j = y (lines)
        var result = new PieceConfiguration
        {
            pieceWidth = matrix.GetLength(1),
            pieceHeight = matrix.GetLength(0)
        };

        for (var i = 0; i < matrix.GetLength(0); i++)
        for (var j = 0; j < matrix.GetLength(1); j++)
        {
            var pixel = matrix[i, j];
            var kind = GetTileSide(pixel);

            result.tiles.TryAdd(new Vector2(j, i), kind);
        }

        return result;
    }

    private TileSide GetTileSide (Color32 color)
    {
        var foundKey = colorToTileKind.Keys.First(c => c.CompareRGB(color));
        return colorToTileKind[foundKey];
    }
}