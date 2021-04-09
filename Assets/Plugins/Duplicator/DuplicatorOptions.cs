using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace Duplicator
{
    public class DuplicatorOptions : DictionaryObject
    {
        public const string DisplayCategory = "Duplicated Display";
        public const string RecordingCategory = "Recording";
        public const string SoundRecordingCategory = "Sound Recording";
        public const string InputCategory = "Input";
        public const string DiagnosticsCategory = "Diagnostics";
        private const string DefaultFileFormat = "Capture_{0:yyyy_MM_dd_HH_mm_ss}";

        public DuplicatorOptions()
        {
            Adapter1 adapter;
            using (var fac = new Factory1())
            {
                adapter = fac.Adapters1.FirstOrDefault(a => !a.Description1.Flags.HasFlag(AdapterFlags.Software));
                if (adapter == null)
                {
                    adapter = fac.Adapters1.First();
                }
            }

            Adapter = adapter.Description.Description;
            Output = adapter.Outputs.First().Description.DeviceName;
            FrameAcquisitionTimeout = 500;
            AudioAcquisitionTimeout = 500;
            ShowCursor = true;
            PreserveRatio = true;
            RecordingFrameRate = 0;
            OutputFileFormat = DefaultFileFormat;
            OutputDirectoryPath = GetDefaultOutputDirectoryPath();
            EnableHardwareTransforms = true;
            CaptureSound = true;
            CaptureMicrophone = false;
            SoundDevice = AudioCapture.GetSpeakersDevice()?.FriendlyName;
            MicrophoneDevice = AudioCapture.GetMicrophoneDevice()?.FriendlyName;
            UseRecordingQueue = false;
        }

        [DisplayName("File Format")]
        [Category(RecordingCategory)]
        [DefaultValue(DefaultFileFormat)]
        public virtual string OutputFileFormat { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Directory Path")]
        [Category(RecordingCategory)]
        public virtual string OutputDirectoryPath { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [Browsable(false)] // doesn't work
        [DisplayName("Use Intermediate Queue")]
        [Category(RecordingCategory)]
        [DefaultValue(false)]
        public virtual bool UseRecordingQueue { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        //[Browsable(false)] // not used
        [DisplayName("Frame Rate")]
        [Category(RecordingCategory)]
        [DefaultValue(0f)]
        public virtual float RecordingFrameRate { get => DictionaryObjectGetPropertyValue<float>(); set => DictionaryObjectSetPropertyValue(Math.Max(0f, value)); }

        [DisplayName("Enable Hardware Encoding")]
        [Category(RecordingCategory)]
        [DefaultValue(true)]
        public virtual bool EnableHardwareTransforms { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Capture Sound")]
        [Category(SoundRecordingCategory)]
        [DefaultValue(true)]
        public virtual bool CaptureSound { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Capture Microphone")]
        [Category(SoundRecordingCategory)]
        [DefaultValue(false)]
        public virtual bool CaptureMicrophone { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Sound Device")]
        [Category(SoundRecordingCategory)]
        public virtual string SoundDevice { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Microphone Device")]
        [Category(SoundRecordingCategory)]
        public virtual string MicrophoneDevice { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Disable Throttling")]
        [Category(RecordingCategory)]
        [DefaultValue(false)]
        public virtual bool DisableThrottling { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Low Latency")]
        [Category(RecordingCategory)]
        [DefaultValue(false)]
        public virtual bool LowLatency { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Video Adapter")]
        [Category(InputCategory)]
        public virtual string Adapter { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Video Monitor")]
        [Category(InputCategory)]
        [TypeConverter(typeof(DisplayDeviceTypeConverter))]
        public virtual string Output { get => DictionaryObjectGetPropertyValue<string>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Show Cursor")]
        [Category(DisplayCategory)]
        [DefaultValue(true)]
        public virtual bool ShowCursor { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Proportional Cursor")]
        [Category(DisplayCategory)]
        [DefaultValue(false)]
        public virtual bool IsCursorProportional { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Show Acquisition Rate")]
        [Category(DiagnosticsCategory)]
        [DefaultValue(false)]
        public virtual bool ShowInputFps { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Preserve Input Ratio")]
        [Category(DisplayCategory)]
        [DefaultValue(true)]
        public virtual bool PreserveRatio { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Show Accumulated Frames")]
        [Category(DiagnosticsCategory)]
        [DefaultValue(false)]
        public virtual bool ShowAccumulatedFrames { get => DictionaryObjectGetPropertyValue<bool>(); set => DictionaryObjectSetPropertyValue(value); }

        [DisplayName("Frame Acquisition Timeout")]
        [Category(InputCategory)]
        [DefaultValue(500)]
        public virtual int FrameAcquisitionTimeout
        {
            get => DictionaryObjectGetPropertyValue<int>();
            set
            {
                // we don't want infinite
                DictionaryObjectSetPropertyValue(Math.Max(0, value));
            }
        }

        [DisplayName("Audio Acquisition Timeout")]
        [Category(InputCategory)]
        [DefaultValue(500)]
        public virtual int AudioAcquisitionTimeout
        {
            get => DictionaryObjectGetPropertyValue<int>();
            set
            {
                // we don't want infinite
                DictionaryObjectSetPropertyValue(Math.Max(0, value));
            }
        }

        public AudioCapture.AudioDevice GetSoundDevice() => AudioCapture.GetDevices(AudioCapture.DataFlow.All).FirstOrDefault(d => d.FriendlyName == SoundDevice);
        public AudioCapture.AudioDevice GetMicrophoneDevice() => AudioCapture.GetDevices(AudioCapture.DataFlow.All).FirstOrDefault(d => d.FriendlyName == MicrophoneDevice);

        public Adapter1 GetAdapter()
        {
            using (var fac = new Factory1())
            {
                return fac.Adapters1.FirstOrDefault(a => a.Description.Description == Adapter);
            }
        }

        public Output1 GetOutput()
        {
            using (var adapter = GetAdapter())
            {
                var output = adapter.Outputs.FirstOrDefault(o => o.Description.DeviceName == Output);
                if (output == null)
                    return null; // this can happen if the adapter is not connected to a display

                return output.QueryInterface<Output1>();
            }
        }

        public static string GetDefaultOutputDirectoryPath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Duplicator");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        public string GetNewFilePath()
        {
            var dir = OutputDirectoryPath;
            if (string.IsNullOrWhiteSpace(dir))
            {
                dir = GetDefaultOutputDirectoryPath();
            }

            string format = OutputFileFormat;
            if (string.IsNullOrEmpty(OutputFileFormat))
            {
                format = DefaultFileFormat;
            }

            string fileName = string.Format(format, DateTime.Now);
            return Path.Combine(dir, fileName);
        }

        public static string GetDisplayDeviceName(string deviceName)
        {
            if (deviceName == null)
                throw new ArgumentNullException(nameof(deviceName));

            var dd = new DISPLAY_DEVICE();
            dd.cb = Marshal.SizeOf<DISPLAY_DEVICE>();
            if (!EnumDisplayDevices(deviceName, 0, ref dd, 0))
                return deviceName;

            return dd.DeviceString;
        }

        private class DisplayDeviceTypeConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                var name = value as string;
                if (name != null)
                    return GetDisplayDeviceName(name);

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        [Flags]
        private enum DISPLAY_DEVICE_FLAGS
        {
            DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x00000001,
            DISPLAY_DEVICE_MULTI_DRIVER = 0x00000002,
            DISPLAY_DEVICE_PRIMARY_DEVICE = 0x00000004,
            DISPLAY_DEVICE_MIRRORING_DRIVER = 0x00000008,
            DISPLAY_DEVICE_VGA_COMPATIBLE = 0x00000010,
            DISPLAY_DEVICE_REMOVABLE = 0x00000020,
            DISPLAY_DEVICE_ACC_DRIVER = 0x00000040,
            DISPLAY_DEVICE_MODESPRUNED = 0x08000000,
            DISPLAY_DEVICE_RDPUDD = 0x01000000,
            DISPLAY_DEVICE_REMOTE = 0x04000000,
            DISPLAY_DEVICE_DISCONNECT = 0x02000000,
            DISPLAY_DEVICE_TS_COMPATIBLE = 0x00200000,
            DISPLAY_DEVICE_UNSAFE_MODES_ON = 0x00080000,
            DISPLAY_DEVICE_ACTIVE = 0x00000001,
            DISPLAY_DEVICE_ATTACHED = 0x00000002,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public DISPLAY_DEVICE_FLAGS StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);
    }
}
