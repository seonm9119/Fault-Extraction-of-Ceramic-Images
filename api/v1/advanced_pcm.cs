using SixLabors.ImageSharp.PixelFormats;

namespace CeramicApi.V1;

public sealed class AdvancedPcm
{
    private const int ClusterCount = 7;
    private const int MaxIterations = 5;
    private const double Fuzziness = 2.0;
    private const double SmallNumber = 1e-8;

    private static readonly Rgba32 Gold = new(255, 215, 0);
    private static readonly Rgba32 Aqua = new(0, 255, 255);
    private static readonly Rgba32 Red = new(255, 0, 0);

    private readonly Random random;

    public AdvancedPcm(int seed = 0)
    {
        random = new Random(seed);
    }

    public Rgba32[,] Run(int[,] grayMap)
    {
        var width = grayMap.GetLength(0);
        var height = grayMap.GetLength(1);
        var excludedPixels = new bool[width, height];
        var dataSet = BuildDataSet(grayMap, excludedPixels);

        if (dataSet.Length == 0)
        {
            return new Rgba32[width, height];
        }

        // Legacy PCM2: random typicality -> initial centers -> forced center spacing -> 5 updates.
        var previousTypicality = InitializeTypicality(dataSet.Length);
        var currentTypicality = new double[ClusterCount, dataSet.Length];
        var centers = ComputeCenters(dataSet, previousTypicality);
        ForceCenterSpacing(centers);

        for (var iteration = 0; iteration < MaxIterations; iteration += 1)
        {
            var volumes = ComputeVolumes(dataSet, previousTypicality, centers);
            currentTypicality = ComputeTypicality(dataSet, centers, volumes);
            CopyTypicality(currentTypicality, previousTypicality);
        }

        var labels = PickLabels(currentTypicality, dataSet.Length);
        return BuildColorMap(labels, excludedPixels, width, height);
    }

    private static double[] BuildDataSet(int[,] grayMap, bool[,] excludedPixels)
    {
        var width = grayMap.GetLength(0);
        var height = grayMap.GetLength(1);
        var dataSet = new List<double>(width * height);

        for (var x = 0; x < width; x += 1)
        {
            for (var y = 0; y < height; y += 1)
            {
                var gray = grayMap[x, y];
                if (gray != 0)
                {
                    dataSet.Add(gray);
                    continue;
                }

                excludedPixels[x, y] = true;
            }
        }

        return dataSet.ToArray();
    }

    private double[,] InitializeTypicality(int dataCount)
    {
        var typicality = new double[ClusterCount, dataCount];

        for (var cluster = 0; cluster < ClusterCount; cluster += 1)
        {
            for (var index = 0; index < dataCount; index += 1)
            {
                typicality[cluster, index] = random.NextDouble();
            }
        }

        return typicality;
    }

    private static double[] ComputeCenters(double[] dataSet, double[,] typicality)
    {
        var centers = new double[ClusterCount];

        for (var cluster = 0; cluster < ClusterCount; cluster += 1)
        {
            var weightedSum = 0.0;
            var weightSum = 0.0;

            for (var index = 0; index < dataSet.Length; index += 1)
            {
                var weight = Math.Pow(typicality[cluster, index], Fuzziness);
                weightedSum += weight * dataSet[index];
                weightSum += weight;
            }

            centers[cluster] = Math.Abs(weightedSum / Math.Max(weightSum, SmallNumber));
        }

        return centers;
    }

    private static void ForceCenterSpacing(double[] centers)
    {
        var center = centers[3];

        centers[0] = center - center / ClusterCount * 3;
        centers[1] = center - center / ClusterCount * 2;
        centers[2] = center - center / ClusterCount;
        centers[3] = center;
        centers[4] = center + center / ClusterCount;
        centers[5] = center + center / ClusterCount * 2;
        centers[6] = center + center / ClusterCount * 3;
    }

    private static double[] ComputeVolumes(double[] dataSet, double[,] typicality, double[] centers)
    {
        var volumes = new double[ClusterCount];

        for (var cluster = 0; cluster < ClusterCount; cluster += 1)
        {
            var weightedDistanceSum = 0.0;
            var weightSum = 0.0;

            for (var index = 0; index < dataSet.Length; index += 1)
            {
                var distance = Math.Pow(Math.Abs(dataSet[index] - centers[cluster]), 2);
                var weight = Math.Pow(typicality[cluster, index], Fuzziness);
                weightedDistanceSum += weight * distance;
                weightSum += weight;
            }

            volumes[cluster] = Math.Max(weightedDistanceSum / Math.Max(weightSum, SmallNumber), SmallNumber);
        }

        return volumes;
    }

    private static double[,] ComputeTypicality(double[] dataSet, double[] centers, double[] volumes)
    {
        var typicality = new double[ClusterCount, dataSet.Length];

        for (var cluster = 0; cluster < ClusterCount; cluster += 1)
        {
            for (var index = 0; index < dataSet.Length; index += 1)
            {
                var distance = Math.Pow(Math.Abs(dataSet[index] - centers[cluster]), 2);
                typicality[cluster, index] = 1 / (1 + Math.Pow(distance / volumes[cluster], 1 / (Fuzziness - 1)));
            }
        }

        return typicality;
    }

    private static void CopyTypicality(double[,] sourceTypicality, double[,] targetTypicality)
    {
        for (var cluster = 0; cluster < ClusterCount; cluster += 1)
        {
            for (var index = 0; index < sourceTypicality.GetLength(1); index += 1)
            {
                targetTypicality[cluster, index] = sourceTypicality[cluster, index];
            }
        }
    }

    private static int[] PickLabels(double[,] typicality, int dataCount)
    {
        var labels = new int[dataCount];

        for (var index = 0; index < dataCount; index += 1)
        {
            var maxTypicality = double.MinValue;
            var selectedCluster = 0;

            for (var cluster = 0; cluster < ClusterCount; cluster += 1)
            {
                if (maxTypicality < typicality[cluster, index])
                {
                    maxTypicality = typicality[cluster, index];
                    selectedCluster = cluster;
                }
            }

            labels[index] = selectedCluster;
        }

        return labels;
    }

    private static Rgba32[,] BuildColorMap(int[] labels, bool[,] excludedPixels, int width, int height)
    {
        var colorMap = new Rgba32[width, height];
        var labelIndex = 0;

        for (var x = 0; x < width; x += 1)
        {
            for (var y = 0; y < height; y += 1)
            {
                if (excludedPixels[x, y])
                {
                    colorMap[x, y] = new Rgba32(0, 0, 0);
                    continue;
                }

                colorMap[x, y] = PickColor(labels[labelIndex]);
                labelIndex += 1;
            }
        }

        return colorMap;
    }

    private static Rgba32 PickColor(int label)
    {
        return label switch
        {
            1 or 2 => Gold,
            3 or 4 => Aqua,
            5 or 6 => Red,
            _ => new Rgba32(0, 0, 0)
        };
    }
}
