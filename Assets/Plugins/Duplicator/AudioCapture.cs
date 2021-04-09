using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Duplicator
{
    public sealed class AudioCapture : IDisposable
    {
        private AutoResetEvent _dataEvent = new AutoResetEvent(false);
        private AutoResetEvent _stopEvent;
        private AudioDevice _device;
        private bool _stopping;
        private int _waitTimeout;

        public event EventHandler<AudioCaptureNativeDataEventArgs> NativeDataReady;
        public event EventHandler<AudioCaptureDataEventArgs> DataReady;

        public AudioCapture()
        {
            RaiseDataEvents = true;
            WaitTimeout = 100;
        }

        public bool RaiseNativeDataEvents { get; set; }
        public bool RaiseDataEvents { get; set; }
        public AudioDevice Device => _device;

        public int WaitTimeout
        {
            get => _waitTimeout;
            set => _waitTimeout = Math.Max(1, value);
        }

        public WaveFormat GetFormat()
        {
            using (var device = GetSpeakersDevice())
            {
                return GetFormat(device);
            }
        }

        public WaveFormat GetFormat(AudioDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            var audioClient = device.ActivateClient();
            try
            {
                audioClient.GetMixFormat(out IntPtr format);
                var fex = Marshal.PtrToStructure<CoreAudio.WAVEFORMATEXTENSIBLE>(format);
                Marshal.FreeCoTaskMem(format);
                return new WaveFormat(fex);

            }
            finally
            {
                Marshal.ReleaseComObject(audioClient);
            }
        }

        public void Start() => Start(GetSpeakersDevice());
        public void Start(AudioDevice device) => Start(device, null);
        public void Start(AudioDevice device, string threadTaskName)
        {
            if (_device != null)
                return;

            var thread = new Thread(ThreadLoop);
            thread.Name = nameof(AudioCapture) + DateTime.Now.TimeOfDay;
            thread.IsBackground = true;
            //thread.Priority = ThreadPriority.Lowest;
            var state = new ThreadState();
            state.Device = device;
            state.TaskName = threadTaskName;
            thread.Start(state);
        }

        private class ThreadState
        {
            public string TaskName;
            public AudioDevice Device;
        }

        public void Stop()
        {
            if (_device == null)
                return;

            _stopEvent.Set();
            _stopping = true;
        }

        private void ThreadLoop(object obj)
        {
            var state = (ThreadState)obj;
            _stopEvent?.Dispose();
            _stopEvent = new AutoResetEvent(false);

            if (state.Device == null)
            {
                Loop(_stopEvent, state.TaskName);
            }
            else
            {
                Loop(state.Device, _stopEvent, state.TaskName);
            }
        }

        private static bool IsRenderDevice(AudioDevice device) => GetDevices(DataFlow.Render).Any(d => d.Id == device.Id);

        // loops is public in case someone wants to handle his own threading/task stuff
        public void Loop(WaitHandle stopHandle) => Loop(stopHandle, null);
        public void Loop(WaitHandle stopHandle, string threadTaskName)
        {
            using (var device = GetSpeakersDevice())
            {
                Loop(device, stopHandle, threadTaskName);
            }
        }

        public void Loop(AudioDevice device, WaitHandle stopHandle) => Loop(device, stopHandle, null);
        public void Loop(AudioDevice device, WaitHandle stopHandle, string threadTaskName)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (stopHandle == null)
                throw new ArgumentNullException(nameof(stopHandle));

            if (_device != null)
                throw new InvalidOperationException("Capture loop was already started.");

            _device = device;
            bool renderDevice = IsRenderDevice(_device);

            if (string.IsNullOrEmpty(threadTaskName))
            {
                threadTaskName = "Audio";
            }

            var audioClient = device.ActivateClient();
            try
            {
                audioClient.GetMixFormat(out IntPtr format);
                var fex = Marshal.PtrToStructure<CoreAudio.WAVEFORMATEXTENSIBLE>(format);
                Marshal.FreeCoTaskMem(format);

                // ask MF to do the resampling work to PCM 16 for us
                fex.SubFormat = CoreAudio.KSDATAFORMAT_SUBTYPE_PCM;
                fex.wValidBitsPerSample = 16;
                fex.Format.wBitsPerSample = 16;
                fex.Format.nBlockAlign = (short)(fex.Format.nChannels * fex.Format.wBitsPerSample / 8);
                fex.Format.nAvgBytesPerSec = fex.Format.nBlockAlign * fex.Format.nSamplesPerSec;

                format = Marshal.AllocCoTaskMem(Marshal.SizeOf<CoreAudio.WAVEFORMATEXTENSIBLE>());
                Marshal.StructureToPtr(fex, format, false);

                var initFlags = CoreAudio.AUDCLNT_FLAGS.AUDCLNT_STREAMFLAGS_EVENTCALLBACK;
                if (renderDevice)
                {
                    initFlags |= CoreAudio.AUDCLNT_FLAGS.AUDCLNT_STREAMFLAGS_LOOPBACK;
                }

                try
                {
                    audioClient.Initialize(CoreAudio.AUDCLNT_SHAREMODE.AUDCLNT_SHAREMODE_SHARED, initFlags, 0, 0, format, Guid.Empty);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(format);
                }

                int blockAlign = fex.Format.nBlockAlign;

                audioClient.SetEventHandle(_dataEvent.SafeWaitHandle);
                audioClient.GetService(typeof(CoreAudio.IAudioCaptureClient).GUID, out object acc);
                var captureClient = (CoreAudio.IAudioCaptureClient)acc;
                try
                {
                    // profiles names are stored as sub keys of HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks
                    var task = CoreAudio.AvSetMmThreadCharacteristics(threadTaskName, out int taskIndex);
                    if (task == IntPtr.Zero)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // reuse the same buffer
                    byte[] data = null;
                    try
                    {
                        audioClient.Start();
                        RaiseEvents(IntPtr.Zero, 0, ref data);
                        do
                        {
                            if (_stopping || _dataEvent == null) // we've been disposed
                                break;

                            do
                            {
                                int size = captureClient.GetNextPacketSize();
                                if (size == 0)
                                    break;

                                captureClient.GetBuffer(out IntPtr dataPtr, out int frames, out CoreAudio.AUDCLNT_BUFFERFLAGS flags, out long devPosition, out long qpcPosition);
                                //Duplicator.Trace("frames:" + frames + " flags: " + flags);
                                int bytesCount;
                                if (flags.HasFlag(CoreAudio.AUDCLNT_BUFFERFLAGS.AUDCLNT_BUFFERFLAGS_SILENT))
                                {
                                    bytesCount = 0;
                                }
                                else
                                {
                                    bytesCount = frames * blockAlign;
                                }
                                RaiseEvents(dataPtr, bytesCount, ref data);
                                captureClient.ReleaseBuffer(frames);
                            }
                            while (true);

                            int index;
                            try
                            {
                                index = WaitHandle.WaitAny(new[] { stopHandle, _dataEvent }, WaitTimeout);
                            }
                            catch (ObjectDisposedException)
                            {
                                index = 0;
                            }

                            if (index == WaitHandle.WaitTimeout)
                                continue;

                            if (index == 0) // stop
                                break;

                        }
                        while (true);
                        audioClient.Stop();
                    }
                    finally
                    {
                        CoreAudio.AvRevertMmThreadCharacteristics(task);
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(captureClient);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(audioClient);
            }

            _device?.Dispose();
            _device = null;
            _stopping = false;
        }

        private void RaiseEvents(IntPtr dataPtr, int bytesCount, ref byte[] data)
        {
            long ticks = Stopwatch.GetTimestamp();
            bool handled = false;
            if (RaiseNativeDataEvents)
            {
                var ne = new AudioCaptureNativeDataEventArgs(dataPtr, bytesCount, ticks);
                NativeDataReady?.Invoke(this, ne);
                handled = ne.Handled;
            }

            if (!handled && RaiseDataEvents)
            {
                if (bytesCount > 0 && (data == null || data.Length < bytesCount))
                {
                    data = new byte[bytesCount];
                }

                if (bytesCount > 0)
                {
                    Marshal.Copy(dataPtr, data, 0, bytesCount);
                }

                var e = new AudioCaptureDataEventArgs(data, bytesCount, ticks);
                DataReady?.Invoke(this, e);
            }
        }

        public void Dispose()
        {
            // we want to handling looping in another thread
            var dataEvent = Interlocked.Exchange(ref _dataEvent, null);
            if (dataEvent != null)
            {
                _stopping = true;
                dataEvent.Set();
                dataEvent.Dispose();
                while (_device != null && _stopping)
                {
                    Thread.Sleep(10);
                }
            }
        }

        public enum AudioDeviceState
        {
            Active = 0x00000001,
            Disabled = 0x00000002,
            NotPresent = 0x00000004,
            Unplugged = 0x00000008,
        }

        public static AudioDevice GetSpeakersDevice() => CreateDevice(GetSpeakers());
        public static AudioDevice GetMicrophoneDevice() => CreateDevice(GetMicrophone());

        public static IReadOnlyList<AudioDevice> GetDevices(DataFlow flow)
        {
            var list = new List<AudioDevice>();
            CoreAudio.IMMDeviceEnumerator deviceEnumerator = null;
            try
            {
                deviceEnumerator = (CoreAudio.IMMDeviceEnumerator)(new CoreAudio.MMDeviceEnumerator());
            }
            catch
            {
            }

            if (deviceEnumerator != null)
            {
                const int DEVICE_STATEMASK_ALL = 0x0000000f;
                deviceEnumerator.EnumAudioEndpoints(flow, (AudioDeviceState)DEVICE_STATEMASK_ALL, out CoreAudio.IMMDeviceCollection collection);
                if (collection != null)
                {
                    int count = collection.GetCount();
                    for (int i = 0; i < count; i++)
                    {
                        var adev = CreateDevice(collection.Item(i));
                        if (adev != null)
                        {
                            list.Add(adev);
                        }
                    }
                }
            }
            return list;
        }

        private static AudioDevice CreateDevice(CoreAudio.IMMDevice dev)
        {
            if (dev == null)
                return null;

            dev.GetId(out string id);
            var state = dev.GetState();
            var store = dev.OpenPropertyStore(CoreAudio.STGM.STGM_READ);
            string friendlyName = GetValue(store, new CoreAudio.PROPERTYKEY { fmtid = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), pid = 14 });
            string description = GetValue(store, new CoreAudio.PROPERTYKEY { fmtid = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), pid = 2 });
            return new AudioDevice(dev, id, state, friendlyName, description);
        }

        private static string GetValue(CoreAudio.IPropertyStore ps, CoreAudio.PROPERTYKEY pk)
        {
            if (ps == null)
                return null;

            var pv = Marshal.AllocCoTaskMem(IntPtr.Size == 8 ? 24 : 16);
            if (ps.GetValue(ref pk, pv) != 0)
                return null;

            try
            {
                CoreAudio.PropVariantToStringAlloc(pv, out IntPtr ptr);
                if (ptr == IntPtr.Zero)
                    return null;

                var str = Marshal.PtrToStringUni(ptr);
                Marshal.FreeCoTaskMem(ptr);
                return str;
            }
            finally
            {
                Marshal.FreeCoTaskMem(pv);
            }
        }

        private static CoreAudio.IMMDevice GetSpeakers()
        {
            // get the speakers (1st render + multimedia) device
            try
            {
                var deviceEnumerator = (CoreAudio.IMMDeviceEnumerator)(new CoreAudio.MMDeviceEnumerator());
                deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, CoreAudio.ERole.eMultimedia, out CoreAudio.IMMDevice speakers);
                return speakers;
            }
            catch
            {
                // huh? not on vista?
                return null;
            }
        }

        private static CoreAudio.IMMDevice GetMicrophone()
        {
            try
            {
                var deviceEnumerator = (CoreAudio.IMMDeviceEnumerator)(new CoreAudio.MMDeviceEnumerator());
                deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, CoreAudio.ERole.eMultimedia, out CoreAudio.IMMDevice mic);
                return mic;
            }
            catch
            {
                // huh? not on vista?
                return null;
            }
        }

        public enum DataFlow
        {
            Render,
            Capture,
            All,
        }

        // this is public so the client can choose the device
        public sealed class AudioDevice : IDisposable
        {
            private CoreAudio.IMMDevice _device;

            internal AudioDevice(CoreAudio.IMMDevice device, string id, AudioDeviceState state, string friendlyName, string description)
            {
                _device = device;
                Id = id;
                State = state;
                FriendlyName = friendlyName;
                Description = description;
            }

            public string Id { get; }
            public AudioDeviceState State { get; }
            public string FriendlyName { get; }
            public string Description { get; }

            internal CoreAudio.IAudioClient ActivateClient()
            {
                var o = _device.Activate(typeof(CoreAudio.IAudioClient).GUID, CoreAudio.CLSCTX.CLSCTX_ALL, IntPtr.Zero);
                return (CoreAudio.IAudioClient)o;
            }

            public override string ToString() => FriendlyName;

            public void Dispose() => Marshal.ReleaseComObject(_device);
        }

        internal static class CoreAudio
        {
            public static readonly Guid KSDATAFORMAT_SUBTYPE_PCM = new Guid("00000001-0000-0010-8000-00aa00389b71");

            [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
            public class MMDeviceEnumerator { }

            public enum AUDCLNT_SHAREMODE
            {
                AUDCLNT_SHAREMODE_SHARED,
                AUDCLNT_SHAREMODE_EXCLUSIVE
            }

            [Flags]
            public enum AUDCLNT_BUFFERFLAGS
            {
                AUDCLNT_BUFFERFLAGS_DATA_DISCONTINUITY = 0x1,
                AUDCLNT_BUFFERFLAGS_SILENT = 0x2,
                AUDCLNT_BUFFERFLAGS_TIMESTAMP_ERROR = 0x4
            }

            [Flags]
            public enum AUDCLNT_FLAGS
            {
                AUDCLNT_STREAMFLAGS_CROSSPROCESS = 0x00010000,
                AUDCLNT_STREAMFLAGS_LOOPBACK = 0x00020000,
                AUDCLNT_STREAMFLAGS_EVENTCALLBACK = 0x00040000,
                AUDCLNT_STREAMFLAGS_NOPERSIST = 0x00080000,
                AUDCLNT_STREAMFLAGS_RATEADJUST = 0x00100000,
                AUDCLNT_STREAMFLAGS_SRC_DEFAULT_QUALITY = 0x08000000,
                AUDCLNT_STREAMFLAGS_AUTOCONVERTPCM = unchecked((int)0x80000000),
                AUDCLNT_SESSIONFLAGS_EXPIREWHENUNOWNED = 0x10000000,
                AUDCLNT_SESSIONFLAGS_DISPLAY_HIDE = 0x20000000,
                AUDCLNT_SESSIONFLAGS_DISPLAY_HIDEWHENEXPIRED = 0x40000000,
            }

            [StructLayout(LayoutKind.Sequential, Pack = 2)]
            public struct WAVEFORMATEX
            {
                public ushort wFormatTag;
                public short nChannels;
                public int nSamplesPerSec;
                public int nAvgBytesPerSec;
                public short nBlockAlign;
                public ushort wBitsPerSample;
                public ushort cbSize;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 2)]
            public struct WAVEFORMATEXTENSIBLE
            {
                public WAVEFORMATEX Format;
                public ushort wValidBitsPerSample;
                public uint dwChannelMask;
                public Guid SubFormat;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct PROPERTYKEY
            {
                public Guid fmtid;
                public int pid;
                public override string ToString() => fmtid.ToString("B") + " " + pid;
            }

            public enum STGM
            {
                STGM_READ = 0x00000000,
            }

            [Flags]
            public enum CLSCTX
            {
                CLSCTX_INPROC_SERVER = 0x1,
                CLSCTX_INPROC_HANDLER = 0x2,
                CLSCTX_LOCAL_SERVER = 0x4,
                CLSCTX_REMOTE_SERVER = 0x10,
                CLSCTX_ALL = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER
            }

            public enum ERole
            {
                eConsole,
                eMultimedia,
                eCommunications,
            }

            [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IMMDeviceEnumerator
            {
                [PreserveSig]
                int EnumAudioEndpoints(DataFlow dataFlow, AudioDeviceState dwStateMask, out IMMDeviceCollection ppDevices);

                [PreserveSig]
                int GetDefaultAudioEndpoint(DataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);

                [PreserveSig]
                int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);
            }

            [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IMMDeviceCollection
            {
                int GetCount();
                IMMDevice Item(int nDevice);
            }

            [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IMMDevice
            {
                [return: MarshalAs(UnmanagedType.IUnknown)]
                object Activate([MarshalAs(UnmanagedType.LPStruct)] Guid riid, CLSCTX dwClsCtx, IntPtr pActivationParams);

                IPropertyStore OpenPropertyStore(STGM stgmAccess);

                [PreserveSig]
                int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);

                AudioDeviceState GetState();
            }

            [Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IAudioClient
            {
                void Initialize(AUDCLNT_SHAREMODE ShareMode, AUDCLNT_FLAGS StreamFlags, long hnsBufferDuration, long hnsPeriodicity, /*ref WAVEFORMATEX*/ IntPtr pFormat, [MarshalAs(UnmanagedType.LPStruct)] Guid AudioSessionGuid);
                int GetBufferSize();
                long GetStreamLatency();
                int GetCurrentPadding();

                [PreserveSig]
                int IsFormatSupported(AUDCLNT_SHAREMODE ShareMode, ref WAVEFORMATEX pFormat, out WAVEFORMATEX ppClosestMatch);

                void GetMixFormat(out IntPtr ppDeviceFormat);

                void GetDevicePeriod(out long phnsDefaultDevicePeriod, out long phnsMinimumDevicePeriod);

                void Start();
                void Stop();
                void Reset();
                void SetEventHandle(SafeWaitHandle eventHandle);
                void GetService([MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
            }

            [Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IAudioCaptureClient
            {
                void GetBuffer(out IntPtr ppData, out int NumFramesToRead, out AUDCLNT_BUFFERFLAGS pdwFlags, out long pu64DevicePosition, out long pu64QPCPosition);
                void ReleaseBuffer(int NumFramesRead);
                int GetNextPacketSize();
            }

            [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IPropertyStore
            {
                int GetCount();

                [PreserveSig]
                int GetAt(int iProp, out PROPERTYKEY pkey);

                [PreserveSig]
                int GetValue(ref PROPERTYKEY key, IntPtr pv);
            }

            [DllImport("propsys")]
            public static extern int PropVariantToStringAlloc(IntPtr propvar, out IntPtr ppszOut);

            [DllImport("avrt", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr AvSetMmThreadCharacteristics([MarshalAs(UnmanagedType.LPWStr)] string TaskName, out int TaskIndex);

            [DllImport("avrt", SetLastError = true)]
            public static extern bool AvRevertMmThreadCharacteristics(IntPtr AvrtHandle);
        }
    }

    public class WaveFormat
    {
        internal WaveFormat(AudioCapture.CoreAudio.WAVEFORMATEXTENSIBLE fex)
        {
            ChannelsCount = fex.Format.nChannels;
            SamplesPerSecond = fex.Format.nSamplesPerSec;
            AverageBytesPerSecond = fex.Format.nAvgBytesPerSec;
            BitsPerSample = fex.Format.wBitsPerSample;
            ChannelMask = (int)fex.dwChannelMask;
        }

        public int ChannelsCount { get; }
        public int SamplesPerSecond { get; }
        public int AverageBytesPerSecond { get; }
        public int BitsPerSample { get; }
        public int ChannelMask { get; }
        public Guid Format { get; }
    }

    public class AudioCaptureNativeDataEventArgs : HandledEventArgs
    {
        internal AudioCaptureNativeDataEventArgs(IntPtr data, int size, long time)
        {
            Data = data;
            Size = size;
            Time = time;
        }

        public IntPtr Data { get; }
        public int Size { get; }
        public long Time { get; }
    }

    public class AudioCaptureDataEventArgs : EventArgs
    {
        internal AudioCaptureDataEventArgs(byte[] data, int size, long time)
        {
            Data = data;
            Size = size;
            Time = time;
        }

        public byte[] Data { get; }
        public int Size { get; }
        public long Time { get; }
    }
}
