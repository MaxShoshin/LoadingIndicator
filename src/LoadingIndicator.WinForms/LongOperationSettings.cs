using System;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace LoadingIndicator.WinForms
{
    public sealed class LongOperationSettings
    {
        private const float DefaultCircleSize = 1f;
        private const int DefaultNumberOfCircles = 8;

        public static readonly LongOperationSettings Default = new LongOperationSettings();

        private static readonly Color DefaultCircleColor = Color.FromArgb(178, Color.Orange);
        private static readonly TimeSpan DefaultAnimationInterval = TimeSpan.FromMilliseconds(150);

        public LongOperationSettings()
            :this(
                TimeSpan.FromMilliseconds(700),
                TimeSpan.FromMilliseconds(400),
                CreateProgressIndicator,
                ChangeImage,
                false)
        {
        }

        private LongOperationSettings(
            TimeSpan beforeShowIndicatorDelay,
            TimeSpan minIndicatorShowTime,
            [NotNull] Func<Control> indicatorFactory,
            [NotNull] Func<Image, Image> processImage,
            bool allowStopBeforeStart)
        {
            if (indicatorFactory == null) throw new ArgumentNullException(nameof(indicatorFactory));
            if (processImage == null) throw new ArgumentNullException(nameof(processImage));

            BeforeShowIndicatorDelay = beforeShowIndicatorDelay;
            MinIndicatorShowTime = minIndicatorShowTime;
            IndicatorFactory = indicatorFactory;
            ProcessImage = processImage;
            AllowStopBeforeStart = allowStopBeforeStart;
        }

        public TimeSpan BeforeShowIndicatorDelay { get; }

        public TimeSpan MinIndicatorShowTime { get; }

        [NotNull]
        public Func<Control> IndicatorFactory { get; }

        [NotNull]
        public Func<Image, Image> ProcessImage { get; }

        public bool AllowStopBeforeStart { get; }

        [NotNull]
        public LongOperationSettings ShowIndicatorAfter(TimeSpan indicatorShowTime)
        {
            return new LongOperationSettings(
                indicatorShowTime,
                MinIndicatorShowTime,
                IndicatorFactory,
                ProcessImage,
                AllowStopBeforeStart);
        }

        [NotNull]
        public LongOperationSettings HideIndicatorOnCompleteAfter(TimeSpan minIndicatorShownTime)
        {
            return new LongOperationSettings(
                BeforeShowIndicatorDelay,
                minIndicatorShownTime,
                IndicatorFactory,
                ProcessImage,
                AllowStopBeforeStart);
        }

        [NotNull]
        public LongOperationSettings HideIndicatorImmediatleyOnComplete()
        {
            return HideIndicatorOnCompleteAfter(TimeSpan.Zero);
        }

        [NotNull]
        public LongOperationSettings WithGrayscaleAndBlurBackground()
        {
            return new LongOperationSettings(
                BeforeShowIndicatorDelay,
                MinIndicatorShowTime,
                IndicatorFactory,
                image => image.MakeGrayscale().ImageBlurFilter(),
                AllowStopBeforeStart);
        }

        [NotNull]
        public LongOperationSettings WithSepiaAndBlurBackground()
        {
            return new LongOperationSettings(
                BeforeShowIndicatorDelay,
                MinIndicatorShowTime,
                IndicatorFactory,
                image => image.MakeSepia().ImageBlurFilter(),
                AllowStopBeforeStart);
        }

        [NotNull]
        public LongOperationSettings WithCirclesIndicator(
            TimeSpan animationInterval,
            float circleSize = DefaultCircleSize,
            int numberOfCircles = DefaultNumberOfCircles)
        {
            return WithCirclesIndicator(DefaultCircleColor, animationInterval, circleSize, numberOfCircles);
        }

        [NotNull]
        public LongOperationSettings WithCirclesIndicator(
            Color circleColor,
            float circleSize = DefaultCircleSize,
            int numberOfCircles = DefaultNumberOfCircles)
        {
            return WithCirclesIndicator(circleColor, DefaultAnimationInterval, circleSize, numberOfCircles);
        }

        [NotNull]
        public LongOperationSettings WithCirclesIndicator(
            float circleSize = DefaultCircleSize,
            int numberOfCircles = DefaultNumberOfCircles)
        {
            return WithCirclesIndicator(DefaultCircleColor, DefaultAnimationInterval, circleSize, numberOfCircles);
        }

        [NotNull]
        public LongOperationSettings WithCustomIndicator([NotNull] Func<Control> indicatorFactory)
        {
            if (indicatorFactory == null) throw new ArgumentNullException(nameof(indicatorFactory));

            return new LongOperationSettings(
                BeforeShowIndicatorDelay,
                MinIndicatorShowTime,
                indicatorFactory,
                ProcessImage,
                AllowStopBeforeStart);
        }

        [NotNull]
        public LongOperationSettings WithCirclesIndicator(
            Color circleColor,
            TimeSpan animationInterval,
            float circleSize = DefaultCircleSize,
            int numberOfCircles = DefaultNumberOfCircles)
        {
            return new LongOperationSettings(
                BeforeShowIndicatorDelay,
                MinIndicatorShowTime,
                () =>
                {
                    var indicator = new LoadingIndicatorControl();
                    indicator.CircleColor = circleColor;
                    indicator.CircleSize = circleSize;
                    indicator.NumberOfCircles = numberOfCircles;
                    indicator.AnimationInterval = (int)animationInterval.TotalMilliseconds;
                    return indicator;
                },
                ProcessImage,
                AllowStopBeforeStart);
        }

        [NotNull]
        public LongOperationSettings AllowStopBeforeStartMethods()
        {
            return new LongOperationSettings(
                BeforeShowIndicatorDelay,
                MinIndicatorShowTime,
                IndicatorFactory,
                ProcessImage,
                true);
        }

        [NotNull]
        private static Image ChangeImage([NotNull] Image sourceImage)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));

            return sourceImage.MakeGrayscale().ImageBlurFilter();
        }

        [NotNull]
        private static Control CreateProgressIndicator()
        {
            return new LoadingIndicatorControl();
        }
    }
}