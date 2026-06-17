using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CeramicApi.V1;

public sealed class CeramicV1Processing
{
    private const int TargetWidth = 454;
    private const int TargetHeight = 426;
    private const int DefaultPeakSearchMargin = 18;

    public Task<byte[]> RunAsync(byte[] imageBytes, string fileName, CancellationToken cancellationToken)
    {
        var profile = BuildProfile(fileName);

        // 1단계: legacy v1처럼 입력 이미지를 고정 작업 크기인 454x426으로 맞춘다.
        cancellationToken.ThrowIfCancellationRequested();
        using var resizedImage = Image.Load<Rgba32>(imageBytes);
        resizedImage.Mutate(command => command.Resize(new ResizeOptions
        {
            Size = new Size(TargetWidth, TargetHeight),
            Mode = ResizeMode.Stretch,
            Sampler = KnownResamplers.Bicubic
        }));

        // 2단계: ROI 탐색과 후속 처리를 위해 RGB 이미지를 grayscale map으로 바꾼다.
        var originalGrayMap = BuildGrayMap(resizedImage);

        // 3단계: legacy v1처럼 Gaussian blur와 Sobel edge를 적용한 map에서 ROI 높이를 찾는다.
        var roiSearchMap = BuildRoiSearchMap(originalGrayMap, profile.UseEightMillimeterWindow);
        var initialRoi = ImageProcessingUtils.extract_roi(
            originalGrayMap,
            roiSearchMap,
            profile.UseEightMillimeterWindow);
        var initialRoiBounds = ResolveInitialRoiBounds(initialRoi.Bounds, resizedImage.Height);
        var roiBounds = profile.UsePeakBand
            ? ImageProcessingUtils.BuildPolarityPeakRoiBounds(
                originalGrayMap,
                roiSearchMap,
                initialRoiBounds,
                profile.BrightSidePadding,
                profile.DarkSidePadding,
                DefaultPeakSearchMargin)
            : initialRoiBounds;
        ValidateRoi(roiBounds, resizedImage.Height);

        // 4단계: 원본 grayscale map에서 최종 ROI만 잘라서 PCM 입력으로 되돌릴 원본 gray 값을 보관한다.
        var originalRoiMap = ImageProcessingUtils.CropToRoiBounds(originalGrayMap, roiBounds);

        // 5단계: ROI 원본 gray에 Sobel edge를 적용해 결함 후보 mask의 시작점을 만든다.
        var roiSobelMap = ImageProcessingUtils.BuildCeramicSobelMap(originalRoiMap);

        // 6단계: fuzzy stretching으로 ROI edge mask의 contrast를 legacy 방식에 맞춰 확장한다.
        var stretchedRoiMap = ImageProcessingUtils.BuildFuzzyStretchingMap(roiSobelMap);

        // 7단계: legacy Erase처럼 위/아래 scan이 모두 닿는 후보 위치의 원본 ROI gray 값만 복사한다.
        var eraseMap = ImageProcessingUtils.BuildEraseMapV1(
            stretchedRoiMap,
            originalRoiMap,
            profile.EraseThreshold,
            candidateThreshold: null);

        // 8단계: legacy PCM2 방식의 advanced PCM으로 결함 후보 영역의 원본 gray 값을 클러스터링한다.
        var advancedPcm = new AdvancedPcm();
        var pcmColorMap = advancedPcm.Run(eraseMap);

        // 9단계: legacy fullImageProcessing처럼 검정이 아닌 PCM 결과만 원본 위치에 overlay 한다.
        return Task.FromResult(BuildFinalOverlayPng(resizedImage, roiBounds, pcmColorMap));
    }

    private static CeramicV1Profile BuildProfile(string fileName)
    {
        var normalizedFileName = fileName.ToLowerInvariant();

        if (normalizedFileName.Contains("8mm", StringComparison.Ordinal))
        {
            return new CeramicV1Profile(
                UseEightMillimeterWindow: true,
                UsePeakBand: true,
                BrightSidePadding: 6,
                DarkSidePadding: 30,
                EraseThreshold: 230);
        }

        if (normalizedFileName.Contains("10mm", StringComparison.Ordinal))
        {
            return new CeramicV1Profile(
                UseEightMillimeterWindow: false,
                UsePeakBand: true,
                BrightSidePadding: 12,
                DarkSidePadding: 18,
                EraseThreshold: 150);
        }

        if (normalizedFileName.Contains("11mm", StringComparison.Ordinal) ||
            normalizedFileName.Contains("16mm", StringComparison.Ordinal))
        {
            return new CeramicV1Profile(
                UseEightMillimeterWindow: false,
                UsePeakBand: true,
                BrightSidePadding: 20,
                DarkSidePadding: 24,
                EraseThreshold: 110);
        }

        return new CeramicV1Profile(
            UseEightMillimeterWindow: false,
            UsePeakBand: true,
            BrightSidePadding: 8,
            DarkSidePadding: 24,
            EraseThreshold: 110);
    }

    private static int[,] BuildRoiSearchMap(int[,] originalGrayMap, bool useEightMillimeterWindow)
    {
        var gaussianMap = ImageProcessingUtils.BuildCeramicGaussianMap(originalGrayMap);
        var sobelMap = ImageProcessingUtils.BuildCeramicSobelMap(gaussianMap);

        if (!useEightMillimeterWindow)
        {
            return sobelMap;
        }

        // Legacy 8mm path repeats Gaussian/Sobel before Binary.roi_8mm.
        var secondGaussianMap = ImageProcessingUtils.BuildCeramicGaussianMap(sobelMap);
        var secondSobelMap = ImageProcessingUtils.BuildCeramicSobelMap(secondGaussianMap);
        return ImageProcessingUtils.BuildCeramicGaussianMap(secondSobelMap);
    }

    private static int[,] BuildGrayMap(Image<Rgba32> image)
    {
        var grayMap = new int[image.Width, image.Height];

        for (var x = 0; x < image.Width; x += 1)
        {
            for (var y = 0; y < image.Height; y += 1)
            {
                var pixel = image[x, y];
                grayMap[x, y] = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
            }
        }

        return grayMap;
    }

    private static void ValidateRoi(RoiBounds roiBounds, int imageHeight)
    {
        if (roiBounds.Start < 0 ||
            roiBounds.Start >= imageHeight ||
            roiBounds.Height <= 0 ||
            roiBounds.Start + roiBounds.Height > imageHeight)
        {
            throw new InvalidOperationException("Ceramic v1 ROI was not detected.");
        }
    }

    private static RoiBounds ResolveInitialRoiBounds(RoiBounds roiBounds, int imageHeight)
    {
        if (roiBounds.Start >= 0 &&
            roiBounds.Start < imageHeight &&
            roiBounds.Height > 0 &&
            roiBounds.Start + roiBounds.Height <= imageHeight)
        {
            return roiBounds;
        }

        var start = imageHeight / 5;
        var end = 4 * (imageHeight / 5);

        return new RoiBounds(start, end, end - start);
    }

    private static byte[] BuildFinalOverlayPng(
        Image<Rgba32> sourceImage,
        RoiBounds roiBounds,
        Rgba32[,] pcmColorMap)
    {
        using var finalImage = new Image<Rgba32>(sourceImage.Width, sourceImage.Height);
        for (var x = 0; x < sourceImage.Width; x += 1)
        {
            for (var y = 0; y < sourceImage.Height; y += 1)
            {
                finalImage[x, y] = sourceImage[x, y];
            }
        }

        var width = pcmColorMap.GetLength(0);
        var height = pcmColorMap.GetLength(1);

        for (var y = 0; y < height; y += 1)
        {
            for (var x = 0; x < width; x += 1)
            {
                var color = pcmColorMap[x, y];
                if (color.R != 0 || color.G != 0 || color.B != 0)
                {
                    finalImage[x, roiBounds.Start + y] = color;
                }
            }
        }

        return EncodePng(finalImage);
    }

    private static byte[] EncodePng(Image<Rgba32> image)
    {
        using var stream = new MemoryStream();
        image.SaveAsPng(stream, new PngEncoder());
        return stream.ToArray();
    }

    private sealed record CeramicV1Profile(
        bool UseEightMillimeterWindow,
        bool UsePeakBand,
        int BrightSidePadding,
        int DarkSidePadding,
        int EraseThreshold);
}
