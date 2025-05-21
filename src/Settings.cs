using ModSettings;
using System.IO;
using System;
using System.Text;

namespace HouseLights
{
    internal class HouseLightsSettings : JsonModSettings
    {
        [Section("Visuals")]

        [Name("Intensity Value")]
        [Description("Set the intensity for the lights.")]
        [Slider(0f, 3f, 1)]
        public float intensityValue = 2f;

        [Name("Range Multiplier")]
        [Description("Values above 1 make the lights cast light further. 2 will make them reach double the distance than default, 0 turns the lights off.")]
        [Slider(0f, 5f, 1)]
        public float rangeMultiplier = 1.4f;

        [Name("Colorless lights")]
        [Description("If set to yes, lights will cast a more white light. If set to no, they will cast light with the default color.")]
        public bool whiteLights = false;

        [Name("Turn off aurora light flicker")]
        [Description("If set to yes, aurora powered lights won't flicker and will stay on.")]
        public bool disableAuroraFlicker = true;

        [Section("Performance")]

        [Name("Enable Outside")]
        [Description("Toggle to enable or disable the mod while outdoors. Can impact performance, but will make it available in places without a loading screen. WARNING: Highly experimental, it will turn on lights such as street lamps.")]
        public bool enableOutside = false;

        [Name("Distance Culling")]
        [Description("Reduce this value if you are having performance issues. It will make lights further from this distance be always turned off.")]
        [Slider(10, 50, 1)]
        public int cullDistance = 50;

        [Name("Cast Shadows")]
        [Description("If set to yes, lights will cast shadows (can show artifacts and might reduce performance)")]
        public bool castShadows = false;

        [Section("Misc")]

        [Name("Enable Light Audio")]
        [Description("If enabled, lamps will emit a buzzing sound when turned on, even without an aurora active.")]
        public bool LightAudio = false;

        [Name("Interaction Distance")]
        [Description("Controls how far away from the switch you can interact with it, in meters.")]
        [Slider(1, 3, 1)]
        public int InteractDistance = 1;

        [Section("Debug")]

        [Name("Enable debug logging")]
        [Description("Enables debug information in the melon log. Only enable for troubleshooting reasons, or if you know what you're doing.")]
        public bool Debug = false;
        [Name("Enable Placer.")]
        [Description("Enables debug features for placement of new switches in scenes, this shouldn't be enabled by users, placed switches will not be saved.")]
        public bool Placer = false;

    }

    internal static class Settings
    {
        public static HouseLightsSettings options;

        public static void OnLoad()
        {
            options = new HouseLightsSettings();
            options.AddToModSettings("House Lights Settings");
        }
    }
}
