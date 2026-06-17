namespace CeramicApi.V1;

public readonly record struct RoiBounds(int Start, int End, int Height);

public readonly record struct ExtractedRoi(RoiBounds Bounds, int[,] RoiMap);

public static class ImageProcessingUtils
{
    public static ExtractedRoi extract_roi(int[,] sourceMap, int[,] roiSearchMap, bool useEightMillimeterWindow = false)
    {
        EnsureSameDimensions(sourceMap, roiSearchMap);

        // Legacy Binary.roi finds bounds from the processed map, then copies the original gray ROI.
        var bounds = DetectBinaryRoi(roiSearchMap, useEightMillimeterWindow);
        var roiMap = CropRoiMap(sourceMap, bounds);

        return new ExtractedRoi(bounds, roiMap);
    }

    public static int[,] BuildCeramicGaussianMap(int[,] grayMap)
    {
        const double sigma = 1.0;
        var start = (int)Math.Round(-4 * sigma);
        var size = (int)Math.Round(8 * sigma + 1);
        var gaussianLine = new double[size];
        var gaussianMask = new double[size, size];
        var position = start;

        for (var index = 0; index < size; index += 1)
        {
            gaussianLine[index] = 1 / (Math.Sqrt(2 * Math.PI) * sigma) *
                                  Math.Exp(-1 * (position * position / (2 * sigma * sigma)));
            position += 1;
        }

        for (var row = 0; row < size; row += 1)
        {
            for (var column = 0; column < size; column += 1)
            {
                gaussianMask[row, column] = gaussianLine[row] * gaussianLine[column];
            }
        }

        return Convolve(grayMap, gaussianMask);
    }

    public static int[,] BuildCeramicSobelMap(int[,] grayMap)
    {
        double[,] sobelX =
        {
            { -1.0, 0.0, 1.0 },
            { -2.0, 0.0, 2.0 },
            { -1.0, 0.0, 1.0 }
        };

        double[,] sobelY =
        {
            { -1.0, -2.0, -1.0 },
            { 0.0, 0.0, 0.0 },
            { 1.0, 2.0, 1.0 }
        };

        return ConvolveSobel(grayMap, sobelX, sobelY);
    }

    public static int[,] BuildMaxMinBinaryMap(int[,] grayMap)
    {
        var threshold = (FindMaximum(grayMap) + FindMinimum(grayMap)) / 2;
        return BuildThresholdBinaryMap(grayMap, threshold, 255, 0, 0, grayMap.GetLength(1));
    }

    public static int[,] BuildMiddleBinaryMap(int[,] grayMap)
    {
        var height = grayMap.GetLength(1);
        var threshold = (FindMaximum(grayMap) + FindMinimum(grayMap)) / 2;
        return BuildThresholdBinaryMap(grayMap, threshold, 0, 255, height / 5, 4 * (height / 5));
    }

    public static int[,] BuildEightMillimeterBinaryMap(int[,] grayMap)
    {
        var height = grayMap.GetLength(1);
        return BuildThresholdBinaryMap(grayMap, 150, 0, 255, 8 * (height / 17), 2 * (height / 3));
    }

    public static RoiBounds DetectV1Roi(int[,] grayMap)
    {
        var edgeMap = BuildCeramicSobelMap(BuildCeramicGaussianMap(grayMap));
        return DetectBinaryRoi(edgeMap, useEightMillimeterWindow: false);
    }

    public static RoiBounds DetectV1EightMillimeterRoi(int[,] grayMap)
    {
        var edgeMap = BuildCeramicGaussianMap(BuildCeramicSobelMap(BuildCeramicGaussianMap(
            BuildCeramicSobelMap(BuildCeramicGaussianMap(grayMap)))));

        return DetectBinaryRoi(edgeMap, useEightMillimeterWindow: true);
    }

    public static int[,] BuildFuzzyStretchingMap(
        int[,] grayMap,
        double adjustmentScale = 1.0,
        double outputGamma = 1.0)
    {
        var width = grayMap.GetLength(0);
        var height = grayMap.GetLength(1);
        var stretchedMap = new int[width, height];
        var minGray = int.MaxValue;
        var maxGray = int.MinValue;
        var graySum = 0;
        var nonZeroCount = 0;

        for (var x = 0; x < width; x += 1)
        {
            for (var y = 0; y < height; y += 1)
            {
                var gray = grayMap[x, y];
                if (gray == 0)
                {
                    continue;
                }

                nonZeroCount += 1;
                graySum += gray;
                minGray = Math.Min(minGray, gray);
                maxGray = Math.Max(maxGray, gray);
            }
        }

        if (nonZeroCount == 0)
        {
            return stretchedMap;
        }

        var midGray = graySum / nonZeroCount;
        var adjustment = Math.Max(1, (int)Math.Round(CalculateFuzzyAdjustment(midGray, minGray, maxGray) * adjustmentScale));
        var maxIntensity = midGray + adjustment;
        var minIntensity = midGray - adjustment;
        var midIntensity = (maxIntensity + minIntensity) / 2;
        var low = minIntensity;
        var high = maxIntensity;
        var middleLow = (low + midIntensity) / 2.0;
        var middleHigh = (midIntensity + high) / 2.0;
        var lowerSum = 0;
        var upperSum = 0;
        var lowerMembershipSum = 0.0;
        var upperMembershipSum = 0.0;

        for (var x = 0; x < width; x += 1)
        {
            for (var y = 0; y < height; y += 1)
            {
                var brightness = grayMap[x, y];
                if (brightness == 0)
                {
                    continue;
                }

                var lowerWeight = 0.0;
                var upperWeight = 0.0;

                if (brightness <= middleLow)
                {
                    lowerSum += brightness;
                    var lowMembership = low >= brightness
                        ? 1.0
                        : low < brightness && middleLow > brightness
                            ? (brightness - middleLow) / (middleLow - low) + 1
                            : 0;
                    var midMembership = Math.Abs(middleLow - brightness) < double.Epsilon
                        ? 1.0
                        : low < brightness && middleLow > brightness
                            ? -1 * (brightness - low) / (middleLow - low) + 1
                            : 0;
                    lowerWeight = Math.Max(Math.Max(midMembership, lowMembership), Math.Min(midMembership, lowMembership));
                }

                if (brightness >= middleHigh)
                {
                    upperSum += brightness;
                    var lowMembership = Math.Abs(middleHigh - brightness) < double.Epsilon
                        ? 1.0
                        : middleHigh < brightness && high > brightness
                            ? (brightness - high) / (high - middleHigh) + 1
                            : 0;
                    var midMembership = high <= brightness
                        ? 1.0
                        : high > brightness && middleHigh < brightness
                            ? -1 * (brightness - middleHigh) / (high - middleHigh) + 1
                            : 0;
                    upperWeight = Math.Max(Math.Max(midMembership, lowMembership), Math.Min(midMembership, lowMembership));
                }

                lowerMembershipSum += lowerWeight;
                upperMembershipSum += upperWeight;
            }
        }

        var alpha = lowerMembershipSum == 0 ? 0 : (int)((lowerSum - lowerMembershipSum) / lowerMembershipSum);
        var beta = upperMembershipSum == 0 ? 255 : (int)((upperSum - upperMembershipSum) / upperMembershipSum);
        alpha = Math.Max(alpha, 0);
        beta = Math.Min(beta, 255);

        if (beta <= alpha)
        {
            return stretchedMap;
        }

        for (var x = 0; x < width; x += 1)
        {
            for (var y = 0; y < height; y += 1)
            {
                var gray = grayMap[x, y];
                if (gray == 0)
                {
                    continue;
                }

                if (gray > beta)
                {
                    stretchedMap[x, y] = 255;
                    continue;
                }

                if (gray >= alpha)
                {
                    var normalized = (gray - alpha) / (double)(beta - alpha);
                    if (Math.Abs(outputGamma - 1.0) > double.Epsilon)
                    {
                        normalized = Math.Pow(normalized, outputGamma);
                    }

                    stretchedMap[x, y] = ClampChannel(normalized * 255.0);
                }
            }
        }

        return stretchedMap;
    }

    public static int[,] BuildEraseMapV1(
        int[,] maskGrayMap,
        int[,] originalGrayMap,
        int eraseThreshold = 110,
        int? candidateThreshold = null)
    {
        return BuildEraseMap(maskGrayMap, originalGrayMap, eraseThreshold, candidateThreshold);
    }

    public static int[,] BuildPercentileContrastMap(
        int[,] grayMap,
        double lowPercent,
        double highPercent)
    {
        var width = grayMap.GetLength(0);
        var height = grayMap.GetLength(1);
        var values = new List<int>(width * height);

        for (var x = 0; x < width; x += 1)
        {
            for (var y = 0; y < height; y += 1)
            {
                var gray = grayMap[x, y];
                if (gray > 0)
                {
                    values.Add(gray);
                }
            }
        }

        if (values.Count == 0)
        {
            return grayMap;
        }

        values.Sort();
        var low = values[Math.Clamp((int)Math.Round((values.Count - 1) * lowPercent), 0, values.Count - 1)];
        var high = values[Math.Clamp((int)Math.Round((values.Count - 1) * highPercent), 0, values.Count - 1)];

        if (high <= low)
        {
            return grayMap;
        }

        var contrastMap = new int[width, height];
        for (var x = 0; x < width; x += 1)
        {
            for (var y = 0; y < height; y += 1)
            {
                var gray = grayMap[x, y];
                if (gray == 0)
                {
                    continue;
                }

                contrastMap[x, y] = ClampChannel((gray - low) * 255.0 / (high - low));
            }
        }

        return contrastMap;
    }

    public static RoiBounds BuildPeakRoiBounds(
        int[,] edgeMap,
        RoiBounds initialBounds,
        int topPadding,
        int bottomPadding,
        int peakSearchMargin)
    {
        var imageHeight = edgeMap.GetLength(1);
        var peakRow = FindPeakRow(edgeMap, initialBounds, peakSearchMargin);
        var start = Math.Clamp(peakRow - topPadding, 0, imageHeight - 1);
        var end = Math.Clamp(peakRow + bottomPadding, start + 1, imageHeight);

        return new RoiBounds(start, end, end - start);
    }

    public static RoiBounds BuildPolarityPeakRoiBounds(
        int[,] grayMap,
        int[,] edgeMap,
        RoiBounds initialBounds,
        int brightSidePadding,
        int darkSidePadding,
        int peakSearchMargin)
    {
        EnsureSameDimensions(grayMap, edgeMap);

        var imageHeight = edgeMap.GetLength(1);
        var peakRow = FindPeakRow(edgeMap, initialBounds, peakSearchMargin);
        var aboveMean = CalculateSideMean(grayMap, initialBounds, peakRow, true, peakSearchMargin);
        var belowMean = CalculateSideMean(grayMap, initialBounds, peakRow, false, peakSearchMargin);
        var topPadding = aboveMean >= belowMean ? brightSidePadding : darkSidePadding;
        var bottomPadding = aboveMean >= belowMean ? darkSidePadding : brightSidePadding;
        var start = Math.Clamp(peakRow - topPadding, 0, imageHeight - 1);
        var end = Math.Clamp(peakRow + bottomPadding, start + 1, imageHeight);

        return new RoiBounds(start, end, end - start);
    }

    public static int[,] CropToRoiBounds(int[,] sourceMap, RoiBounds bounds)
    {
        return CropRoiMap(sourceMap, bounds);
    }

    public static void ClearEdgeMargin(int[,] map, int margin)
    {
        if (margin <= 0)
        {
            return;
        }

        var width = map.GetLength(0);
        var height = map.GetLength(1);
        var safeMargin = Math.Min(margin, width / 2);

        for (var y = 0; y < height; y += 1)
        {
            for (var x = 0; x < safeMargin; x += 1)
            {
                map[x, y] = 0;
                map[width - 1 - x, y] = 0;
            }
        }
    }

    private static RoiBounds DetectBinaryRoi(int[,] edgeMap, bool useEightMillimeterWindow)
    {
        var binaryMap = useEightMillimeterWindow
            ? BuildEightMillimeterBinaryMap(edgeMap)
            : BuildMiddleBinaryMap(edgeMap);
        var width = edgeMap.GetLength(0);
        var height = edgeMap.GetLength(1);
        var startSearch = useEightMillimeterWindow ? 8 * (height / 17) : height / 5;
        var endSearch = useEightMillimeterWindow ? 2 * (height / 3) : 4 * (height / 5);
        var start = 0;
        var end = 0;

        for (var y = startSearch; y < endSearch; y += 1)
        {
            for (var x = 0; x < width; x += 1)
            {
                if (binaryMap[x, y] == 0)
                {
                    start = y;
                    break;
                }
            }

            if (start == y)
            {
                break;
            }
        }

        for (var y = endSearch - 1; y > startSearch; y -= 1)
        {
            for (var x = 0; x < width; x += 1)
            {
                if (binaryMap[x, y] == 0)
                {
                    end = y;
                    break;
                }
            }

            if (end == y)
            {
                break;
            }
        }

        return new RoiBounds(start, end, Math.Max(0, end - start));
    }

    private static int FindPeakRow(int[,] edgeMap, RoiBounds initialBounds, int margin)
    {
        var width = edgeMap.GetLength(0);
        var height = edgeMap.GetLength(1);
        var safeMargin = Math.Clamp(margin, 0, width / 3);
        var start = Math.Clamp(initialBounds.Start + 2, 0, height - 1);
        var end = Math.Clamp(initialBounds.End - 2, start + 1, height);
        var rowScores = new long[height];

        for (var y = start; y < end; y += 1)
        {
            var score = 0L;
            for (var x = safeMargin; x < width - safeMargin; x += 1)
            {
                score += edgeMap[x, y];
            }

            rowScores[y] = score;
        }

        var peakRow = start;
        var peakScore = long.MinValue;
        for (var y = start; y < end; y += 1)
        {
            var smoothScore = 0L;
            for (var offset = -2; offset <= 2; offset += 1)
            {
                var sampleY = Math.Clamp(y + offset, start, end - 1);
                smoothScore += rowScores[sampleY];
            }

            if (smoothScore > peakScore)
            {
                peakScore = smoothScore;
                peakRow = y;
            }
        }

        return peakRow;
    }

    private static double CalculateSideMean(
        int[,] grayMap,
        RoiBounds initialBounds,
        int peakRow,
        bool searchAbove,
        int margin)
    {
        var width = grayMap.GetLength(0);
        var height = grayMap.GetLength(1);
        var safeMargin = Math.Clamp(margin, 0, width / 3);
        var sampleHeight = 12;
        var startY = searchAbove
            ? Math.Max(initialBounds.Start, peakRow - sampleHeight)
            : Math.Min(height - 1, peakRow + 1);
        var endY = searchAbove
            ? Math.Max(startY, peakRow)
            : Math.Min(initialBounds.End, peakRow + 1 + sampleHeight);
        var sum = 0L;
        var count = 0;

        for (var y = startY; y < endY; y += 1)
        {
            for (var x = safeMargin; x < width - safeMargin; x += 1)
            {
                sum += grayMap[x, y];
                count += 1;
            }
        }

        return count == 0 ? 0 : sum / (double)count;
    }

    private static int[,] CropRoiMap(int[,] sourceMap, RoiBounds bounds)
    {
        var width = sourceMap.GetLength(0);
        var height = sourceMap.GetLength(1);
        var roiHeight = Math.Clamp(bounds.Height, 0, Math.Max(0, height - bounds.Start));
        var roiMap = new int[width, roiHeight];

        for (var x = 0; x < width; x += 1)
        {
            for (var y = 0; y < roiHeight; y += 1)
            {
                roiMap[x, y] = sourceMap[x, bounds.Start + y];
            }
        }

        return roiMap;
    }

    private static void EnsureSameDimensions(int[,] sourceMap, int[,] roiSearchMap)
    {
        if (sourceMap.GetLength(0) == roiSearchMap.GetLength(0) &&
            sourceMap.GetLength(1) == roiSearchMap.GetLength(1))
        {
            return;
        }

        throw new ArgumentException("sourceMap and roiSearchMap must have the same dimensions.");
    }

    private static int[,] BuildEraseMap(
        int[,] maskGrayMap,
        int[,] originalGrayMap,
        int eraseThreshold,
        int? candidateThreshold)
    {
        var width = maskGrayMap.GetLength(0);
        var height = maskGrayMap.GetLength(1);
        var eraseMap = new int[width, height];
        var downMap = new int[width, height];
        var upMap = new int[width, height];

        for (var x = 0; x < width; x += 1)
        {
            for (var y = 0; y < height - 1; y += 1)
            {
                if (maskGrayMap[x, y] > eraseThreshold)
                {
                    downMap[x, y + 1] = 255;
                }

                if (downMap[x, y] == 255)
                {
                    downMap[x, y + 1] = 255;
                }
            }
        }

        for (var x = 0; x < width; x += 1)
        {
            for (var y = height - 1; y > 2; y -= 1)
            {
                if (maskGrayMap[x, y - 2] > eraseThreshold)
                {
                    upMap[x, y - 1] = 255;
                }

                if (upMap[x, y] == 255)
                {
                    upMap[x, y - 1] = 255;
                }
            }
        }

        for (var x = 0; x < width - 1; x += 1)
        {
            for (var y = 0; y < height - 1; y += 1)
            {
                if (downMap[x, y] == 255 &&
                    upMap[x, y] == 255 &&
                    IsCandidatePixel(maskGrayMap[x, y], candidateThreshold))
                {
                    eraseMap[x, y] = originalGrayMap[x, y];
                    eraseMap[x + 1, y] = originalGrayMap[x + 1, y];
                    eraseMap[x, y + 1] = originalGrayMap[x, y + 1];
                }
            }
        }

        return eraseMap;
    }

    private static bool IsCandidatePixel(int gray, int? candidateThreshold)
    {
        return candidateThreshold is null || gray > candidateThreshold.Value;
    }

    private static int CalculateFuzzyAdjustment(int midGray, int minGray, int maxGray)
    {
        var maxDistance = Math.Abs(maxGray - midGray);
        var minDistance = midGray - minGray;

        if (midGray > 128)
        {
            return 255 - midGray;
        }

        if (midGray <= minDistance)
        {
            return minDistance;
        }

        if (midGray >= maxDistance)
        {
            return maxDistance;
        }

        return midGray;
    }

    private static int[,] BuildAverageBinaryMap(int[,] grayMap, int highValue, int lowValue)
    {
        var width = grayMap.GetLength(0);
        var height = grayMap.GetLength(1);
        var graySum = 0;

        for (var x = 0; x < width; x += 1)
        {
            for (var y = 0; y < height; y += 1)
            {
                graySum += grayMap[x, y];
            }
        }

        var threshold = graySum / (width * height);
        return BuildThresholdBinaryMap(grayMap, threshold, highValue, lowValue, 0, height);
    }

    private static int[,] BuildThresholdBinaryMap(
        int[,] grayMap,
        int threshold,
        int highValue,
        int lowValue,
        int startY,
        int endY)
    {
        var width = grayMap.GetLength(0);
        var height = grayMap.GetLength(1);
        var binaryMap = new int[width, height];
        var safeStartY = Math.Clamp(startY, 0, height);
        var safeEndY = Math.Clamp(endY, safeStartY, height);

        for (var x = 0; x < width; x += 1)
        {
            for (var y = safeStartY; y < safeEndY; y += 1)
            {
                binaryMap[x, y] = grayMap[x, y] > threshold ? highValue : lowValue;
            }
        }

        return binaryMap;
    }

    private static int[,] Convolve(int[,] grayMap, double[,] mask)
    {
        var width = grayMap.GetLength(0);
        var height = grayMap.GetLength(1);
        var maskWidth = mask.GetLength(0);
        var maskHeight = mask.GetLength(1);
        var xPadding = maskWidth / 2;
        var yPadding = maskHeight / 2;
        var outputMap = new int[width, height];

        for (var y = 0; y < height - 2 * yPadding; y += 1)
        {
            for (var x = 0; x < width - 2 * xPadding; x += 1)
            {
                var sum = 0.0;
                for (var row = 0; row < maskHeight; row += 1)
                {
                    for (var column = 0; column < maskWidth; column += 1)
                    {
                        sum += grayMap[x + column, y + row] * mask[row, column];
                    }
                }

                outputMap[x + xPadding, y + yPadding] = ClampChannel(sum);
            }
        }

        return ApplyPadding(outputMap, xPadding, yPadding);
    }

    private static int[,] ConvolveSobel(int[,] grayMap, double[,] maskX, double[,] maskY)
    {
        var width = grayMap.GetLength(0);
        var height = grayMap.GetLength(1);
        var maskWidth = maskX.GetLength(0);
        var maskHeight = maskX.GetLength(1);
        var xPadding = maskWidth / 2;
        var yPadding = maskHeight / 2;
        var outputMap = new int[width, height];

        for (var y = 0; y < height - 2 * yPadding; y += 1)
        {
            for (var x = 0; x < width - 2 * xPadding; x += 1)
            {
                var sumX = 0.0;
                var sumY = 0.0;
                for (var row = 0; row < maskHeight; row += 1)
                {
                    for (var column = 0; column < maskWidth; column += 1)
                    {
                        sumX += grayMap[x + column, y + row] * maskX[row, column];
                        sumY += grayMap[x + column, y + row] * maskY[row, column];
                    }
                }

                outputMap[x + xPadding, y + yPadding] = ClampChannel(Math.Abs(sumX) + Math.Abs(sumY));
            }
        }

        return ApplyPadding(outputMap, xPadding, yPadding);
    }

    private static int[,] ApplyPadding(int[,] map, int xPadding, int yPadding)
    {
        var width = map.GetLength(0);
        var height = map.GetLength(1);

        for (var y = 0; y < yPadding; y += 1)
        {
            for (var x = xPadding; x < width - xPadding; x += 1)
            {
                map[x, y] = map[x, yPadding];
                map[x, height - 1 - y] = map[x, height - 1 - yPadding];
            }
        }

        for (var x = 0; x < xPadding; x += 1)
        {
            for (var y = 0; y < height; y += 1)
            {
                map[x, y] = map[xPadding, y];
                map[width - 1 - x, y] = map[width - 1 - xPadding, y];
            }
        }

        return map;
    }

    private static int FindMaximum(int[,] map)
    {
        var maximum = 0;

        for (var x = 0; x < map.GetLength(0); x += 1)
        {
            for (var y = 0; y < map.GetLength(1); y += 1)
            {
                maximum = Math.Max(maximum, map[x, y]);
            }
        }

        return maximum;
    }

    private static int FindMinimum(int[,] map)
    {
        var minimum = 255;

        for (var x = 0; x < map.GetLength(0); x += 1)
        {
            for (var y = 0; y < map.GetLength(1); y += 1)
            {
                minimum = Math.Min(minimum, map[x, y]);
            }
        }

        return minimum;
    }

    private static int ClampChannel(double channel)
    {
        return Math.Clamp((int)channel, 0, 255);
    }
}
