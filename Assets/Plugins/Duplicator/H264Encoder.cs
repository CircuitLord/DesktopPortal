using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;

namespace Duplicator
{
    public class H264Encoder : IDisposable
    {
        private H264Encoder(Activate activate)
        {
            Activate = activate;
            FriendlyName = activate.Get(TransformAttributeKeys.MftFriendlyNameAttribute);
            Clsid = activate.Get(TransformAttributeKeys.MftTransformClsidAttribute);
            Flags = (TransformEnumFlag)activate.Get(TransformAttributeKeys.TransformFlagsAttribute);
            var list = new List<string>();
            var inputTypes = activate.Get(TransformAttributeKeys.MftInputTypesAttributes);
            for (int j = 0; j < inputTypes.Length; j += 32) // two guids
            {
                var majorType = new Guid(Enumerable.Range(0, 16).Select(index => Marshal.ReadByte(inputTypes, j + index)).ToArray()); // Should be video in this context
                var subType = new Guid(Enumerable.Range(0, 16).Select(index => Marshal.ReadByte(inputTypes, j + 16 + index)).ToArray());
                list.Add(GetFourCC(subType));
            }

            list.Sort();
            InputTypes = list;
            try
            {
                using (var tf = activate.ActivateObject<Transform>())
                {
                    IsBuiltin = IsBuiltinEncoder(tf);
                    IsDirect3D11Aware = IsDirect3D11AwareEncoder(tf);
                    IsHardwareBased = IsHardwareBasedEncoder(tf);
                }
            }
            catch
            {
                // do nothing
            }

            using (var key = Registry.ClassesRoot.OpenSubKey(Path.Combine("CLSID", Clsid.ToString("B"), "InprocServer32")))
            {
                if (key != null)
                {
                    DllPath = key.GetValue(null) as string;
                }
            }
        }

        public Activate Activate { get; }
        public string FriendlyName { get; }
        public Guid Clsid { get; }
        public IEnumerable<string> InputTypes { get; }
        public TransformEnumFlag Flags { get; }
        public bool IsBuiltin { get; }
        public bool IsDirect3D11Aware { get; }
        public bool IsHardwareBased { get; }
        public string DllPath { get; }
        public override string ToString() => FriendlyName;

        public Transform GetTransform() => Activate.ActivateObject<Transform>();

        public void Dispose() => Activate.Dispose();

        private static IntPtr GetTransformPtr(SinkWriter writer, int streamIndex)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            var tf = IntPtr.Zero;
            try
            {
                writer.GetServiceForStream(streamIndex, Guid.Empty, typeof(Transform).GUID, out tf);
            }
            catch
            {
                // do nothing
            }
            return tf;
        }

        public static Transform GetTransform(SinkWriter writer, int streamIndex)
        {
            var ptr = GetTransformPtr(writer, streamIndex);
            return ptr != IntPtr.Zero ? new Transform(ptr) : null;
        }

        public static bool IsBuiltinEncoder(SinkWriter writer, int streamIndex)
        {
            var ptr = GetTransformPtr(writer, streamIndex);
            if (ptr == IntPtr.Zero)
                return false;

            return Marshal.GetObjectForIUnknown(ptr) as IMFObjectInformation != null;
        }

        public static TOutputStreamInformation GetOutputStreamInfo(SinkWriter writer, int streamIndex)
        {
            using (var transform = GetTransform(writer, streamIndex))
            {
                if (transform == null)
                    return new TOutputStreamInformation();

                transform.GetOutputStreamInfo(streamIndex, out TOutputStreamInformation info);
                return info;
            }
        }

        public static bool IsDirect3D11AwareEncoder(SinkWriter writer, int streamIndex)
        {
            using (var transform = GetTransform(writer, streamIndex))
            {
                if (transform == null)
                    return false;

                return IsDirect3D11AwareEncoder(transform);
            }
        }

        public static string GetEncoderFriendlyName(SinkWriter writer, int streamIndex)
        {
            try
            {
                using (var transform = GetTransform(writer, streamIndex))
                {
                    if (transform != null)
                    {
                        if (IsBuiltinEncoder(transform))
                            return Enumerate().First(e => e.IsBuiltin).FriendlyName;

                        var clsid = transform.Attributes.Get(TransformAttributeKeys.MftTransformClsidAttribute);
                        return Enumerate().First(e => e.Clsid == clsid).FriendlyName;
                    }
                }
            }
            catch
            {
                // continue
            }
            return "Unknown";
        }

        public static bool IsHardwareBasedEncoder(SinkWriter writer, int streamIndex)
        {
            using (var transform = GetTransform(writer, streamIndex))
            {
                if (transform == null)
                    return false;

                return IsHardwareBasedEncoder(transform);
            }
        }

        public static bool IsHardwareBasedEncoder(Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return EnumerateAttributes(transform.Attributes).Any(a => a.Key == TransformAttributeKeys.MftEnumHardwareUrlAttribute.Guid);
        }

        public static bool IsDirect3D11AwareEncoder(Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return EnumerateAttributes(transform.Attributes).Any(a => a.Key == TransformAttributeKeys.D3D11Aware.Guid && a.Value.Equals(1));
        }

        public static IReadOnlyDictionary<Guid, object> GetAttributes(MediaAttributes atts)
        {
            var dic = new Dictionary<Guid, object>();
            if (atts != null)
            {
                for (int i = 0; i < atts.Count; i++)
                {
                    object value = atts.GetByIndex(i, out Guid guid);
                    dic[guid] = value;
                }
            }
            return dic;
        }

        internal static IEnumerable<KeyValuePair<Guid, object>> EnumerateAttributes(MediaAttributes atts)
        {
            for (int i = 0; i < atts.Count; i++)
            {
                object value = atts.GetByIndex(i, out Guid guid);
                yield return new KeyValuePair<Guid, object>(guid, value);
            }
        }

        public static bool IsBuiltinEncoder(Transform transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return Marshal.GetObjectForIUnknown(transform.NativePointer) as IMFObjectInformation != null;
        }

        public static IEnumerable<H264Encoder> Enumerate() => Enumerate(TransformEnumFlag.All);
        public static IEnumerable<H264Encoder> Enumerate(TransformEnumFlag flags)
        {
            var output = new TRegisterTypeInformation();
            output.GuidMajorType = MediaTypeGuids.Video;
            output.GuidSubtype = VideoFormatGuids.FromFourCC(new FourCC("H264"));
            foreach (var activate in MediaFactory.FindTransform(TransformCategoryGuids.VideoEncoder, flags, null, output))
            {
                yield return new H264Encoder(activate);
            }
        }

        private static string GetFourCC(Guid guid)
        {
            var s = guid.ToString();
            if (s.EndsWith("0000-0010-8000-00aa00389b71"))
            {
                var bytes = guid.ToByteArray();
                if (bytes.Take(4).Any(b => b < 32 || b > 127))
                    return s;

                return new string(bytes.Take(4).Select(b => (char)b).ToArray());
            }

            return s;
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CE6BE8E7-D757-435F-9DE9-BE3EF330B805")]
        private interface IMFObjectInformation { }
    }
}
