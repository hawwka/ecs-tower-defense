namespace Game.UI
{
    public static class RadialMenuLayout
    {
        public const float PanelPixels = 400f;
        public const float PanelWorldMeters = 4f;
        public const float PixelsPerMeter = 100f;

        public const float RingSizePixels = 390f;
        public const float RingBorderPixels = 58f;
        public const float OuterRadiusPixels = RingSizePixels * 0.5f;
        public const float InnerRadiusPixels = OuterRadiusPixels - RingBorderPixels;
        public const float ButtonOrbitRadiusPixels = (OuterRadiusPixels + InnerRadiusPixels) * 0.5f;
        public const float ButtonSizePixels = 82f;
        public const float StartAngleDegrees = 0f;
    }
}
