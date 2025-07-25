using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public partial class PieceData
{
    [Button]
    public List<PieceConfiguration> GetAllPieceConfiguration()
    {
        var allConfigurations = new List<PieceConfiguration>();
        var originalMatrix = GetPixelsAsMatrix(pieceTexture);
        var rotations = new List<(int DegreeIndex, Color[,] Matrix)> { (0, originalMatrix) };

        if (additionalRotations != PieceRotation.None)
        {
            var rotated90 = RotateClockwise(originalMatrix);
            var rotated180 = RotateClockwise(rotated90);
            var rotated270 = RotateClockwise(rotated180);

            rotations.Add((1, rotated90));
            rotations.Add((2, rotated180));
            rotations.Add((4, rotated270));
        }

        foreach (var (degree, matrix) in rotations)
            if (degree == 0 || additionalRotations.HasFlag((PieceRotation)degree))
            {
                var configuration = Internal_GetConfiguration(matrix);
                configuration.amountOfRotations = degree - 1;
                allConfigurations.Add(configuration);
            }

        if (!shouldMirror)
            return allConfigurations;

        foreach (var (degree, matrix) in rotations)
        {
            var mirroredMatrix = MirrorHorizontally(matrix);
            var mirroredConfig = Internal_GetConfiguration(mirroredMatrix);
            mirroredConfig.isMirrored = true;
            mirroredConfig.amountOfRotations = degree - 1;
            allConfigurations.Add(mirroredConfig);
        }

        return allConfigurations;
    }
}