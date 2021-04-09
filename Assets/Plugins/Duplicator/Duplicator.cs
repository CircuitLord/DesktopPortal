using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;

namespace Duplicator
{
    public class Duplicator : INotifyPropertyChanged, IDisposable
    {
        // use that guid in TraceSpy's ETW Trace Provider (https://github.com/smourier/TraceSpy) 
        // or use is with MFTrace https://msdn.microsoft.com/en-us/library/windows/desktop/ff685116 as MFTrace can also display our custom traces
        // you can use trace.bat and config.xml in the project. Make sure you use mftrace X64 if this is ran as X4 also.
        //private static EventProvider _provider = new EventProvider(new Guid("964D4572-ADB9-4F3A-8170-FCBECEC27465"));

        // common
        private int _resized;
        private System.Threading.Timer _frameRateTimer;
        private Lazy<TextFormat> _diagsTextFormat;
        private Lazy<Brush> _diagsBrush;
        private Size2 _size;
#if DEBUG
        private ConcurrentQueue<TraceEvent> _events = new ConcurrentQueue<TraceEvent>();
        private Lazy<DeviceDebug> _deviceDebug;
#endif

        // recording common
        private Thread _recordingThread;
        private DuplicatorState _recordingState;
        private Lazy<SinkWriter> _sinkWriter;
        private long _startTime;
        private object _lock = new object();

        // video recording
        private AutoResetEvent _queuedVideoFrameEvent = new AutoResetEvent(false);
        private ConcurrentQueue<VideoFrame> _videoFramesQueue = new ConcurrentQueue<VideoFrame>();
        private Lazy<DXGIDeviceManager> _devManager;
        private int _videoOutputIndex;
        private long _videoElapsedNs;

        // sound recording
        private int _soundOutputIndex;
        private int _microphoneOutputIndex;
        private long _soundElapsedNs;
        private long _microphoneElapsedNs;
        private AudioCapture _soundCapture;
        private AudioCapture _microphoneCapture;
        private int _soundSamplesCount;
        private int _soundGapsCount;
        private int _microphoneSamplesCount;
        private int _microphoneGapsCount;

        // duplicating
        private Lazy<SharpDX.Direct2D1.Device> _2D1Device;
        private Lazy<SharpDX.Direct2D1.DeviceContext> _backBufferDc;
        private Lazy<SharpDX.Direct3D11.Device> _device;
        private Lazy<OutputDuplication> _outputDuplication;
        private Lazy<Output1> _output;
        private Lazy<Size2> _screenSize;
        private Lazy<SwapChain1> _swapChain;
        private DuplicatorState _duplicatingState;
        private Bitmap _pointerBitmap;
        private Thread _duplicationThread;
        private int _currentAccumulatedFramesCount;
        private int _videoSamplesCount;
        private int _duplicationFrameNumber;
        private int _duplicationFrameRate;
        private RawPoint _pointerPosition;
        private RawPoint _pointerHotspot;
        private bool _pointerVisible;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DuplicatorInformationEventArgs> InformationAvailable;

        public Duplicator(DuplicatorOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            Options = options;
            Options.PropertyChanged += OnOptionsChanged;

            _devManager = new Lazy<DXGIDeviceManager>(CreateDeviceManager, true);
            _sinkWriter = new Lazy<SinkWriter>(CreateSinkWriter, true);
            _device = new Lazy<SharpDX.Direct3D11.Device>(CreateDevice, true);
#if DEBUG
            _deviceDebug = new Lazy<DeviceDebug>(CreateDeviceDebug, true);
#endif
            _screenSize = new Lazy<Size2>(CreateScreenSize, true);
            _output = new Lazy<Output1>(CreateOutput, true);
            _outputDuplication = new Lazy<OutputDuplication>(CreateOutputDuplication, true);
            _diagsTextFormat = new Lazy<TextFormat>(CreateDiagsTextFormat, true);
            _diagsBrush = new Lazy<Brush>(CreateDiagsBrush, true);

            // note: We simply compute duplication FPS every second, it's not a super smart moving average thing ...
            _frameRateTimer = new System.Threading.Timer((state) =>
            {
                _duplicationFrameRate = _duplicationFrameNumber;
                _duplicationFrameNumber = 0;
            }, null, 0, 1000);

            _soundCapture = new AudioCapture();
            _soundCapture.RaiseNativeDataEvents = true;
            _soundCapture.RaiseDataEvents = false;
            _soundCapture.NativeDataReady += SoundCaptureDataReady;

            _microphoneCapture = new AudioCapture();
            _microphoneCapture.RaiseNativeDataEvents = true;
            _microphoneCapture.RaiseDataEvents = false;
            _microphoneCapture.NativeDataReady += MicrophoneCaptureDataReady;

            _swapChain = new Lazy<SwapChain1>(CreateSwapChain, true);
            _2D1Device = new Lazy<SharpDX.Direct2D1.Device>(Create2D1Device, true);
            _backBufferDc = new Lazy<SharpDX.Direct2D1.DeviceContext>(CreateBackBufferDc, true);
        }

        public DuplicatorOptions Options { get; }
        public Size2 ScreenSize => _screenSize.Value;
        public Size2 RenderSize { get; private set; }
        public bool IsUsingDirect3D11AwareEncoder { get; private set; }
        public bool IsUsingHardwareBasedEncoder { get; private set; }
        public bool IsUsingBuiltinEncoder { get; private set; }
        public string EncoderFriendlyName { get; private set; }
        public int SoundSamplesPerSecond { get; private set; }
        public int MicrophoneSamplesPerSecond { get; private set; }
        public IntPtr Hwnd { get; set; }
        public string RecordFilePath { get; set; }

        public Size2 Size
        {
            get => _size;
            set
            {
                if (_size == value)
                    return;

                _size = value;
                Interlocked.Exchange(ref _resized, 1);
                if (Options.PreserveRatio && value.Height > 0 && value.Width > 0 && ScreenSize.Width > 0 && ScreenSize.Height > 0)
                {
                    if (value.Width * ScreenSize.Height > value.Height * ScreenSize.Width)
                    {
                        RenderSize = new Size2((value.Height * ScreenSize.Width) / ScreenSize.Height, value.Height);
                    }
                    else
                    {
                        RenderSize = new Size2(value.Width, (value.Width * ScreenSize.Height) / ScreenSize.Width);
                    }
                }
                else
                {
                    RenderSize = value;
                }

                OnPropertyChanged(nameof(Size), _size.ToString());
                OnPropertyChanged(nameof(RenderSize), RenderSize.ToString());
            }
        }

        public DuplicatorState RecordingState
        {
            get => _recordingState;
            private set
            {
                if (_recordingState == value)
                    return;

                _recordingState = value;
                OnPropertyChanged(nameof(RecordingState), _recordingState.ToString());
            }
        }

        public DuplicatorState DuplicatingState
        {
            get => _duplicatingState;
            private set
            {
                if (_duplicatingState == value)
                    return;

                _duplicatingState = value;
                OnPropertyChanged(nameof(DuplicatingState), _duplicatingState.ToString());
            }
        }

        public void StartDuplicating()
        {
            if (DuplicatingState == DuplicatorState.Starting || DuplicatingState == DuplicatorState.Started)
                return;

            DuplicatingState = DuplicatorState.Starting;
            _duplicationThread = new Thread(DuplicationThreadFunc);
            _duplicationThread.Name = "Duplication" + DateTime.Now.TimeOfDay;
            _duplicationThread.IsBackground = true;
            _duplicationThread.Priority = ThreadPriority.Highest;
            _duplicationThread.Start();
        }

        public void StopDuplicating()
        {
            if (DuplicatingState == DuplicatorState.Stopping || DuplicatingState == DuplicatorState.Stopped)
                return;

            DuplicatingState = DuplicatorState.Stopping;
            StopRecording();

            // wait for thread a bit longer than the main acquisition timeout
            var t = _duplicationThread;
            _duplicationThread = null;
            if (t != null)
            {
                var result = t.Join((int)Math.Min(Options.FrameAcquisitionTimeout * 2L, int.MaxValue));
                if (!result)
                {
                    Trace("Duplication thread timed out");
                }
            }
        }

        public void StartRecording()
        {
            if (RecordingState == DuplicatorState.Starting || RecordingState == DuplicatorState.Started)
                return;

            StartDuplicating();
            RecordingState = DuplicatorState.Starting;
            _recordingThread = new Thread(RecordingThreadFunc);
            _recordingThread.Name = "Recording" + DateTime.Now.TimeOfDay;
            _recordingThread.IsBackground = true;
            _recordingThread.Priority = ThreadPriority.Lowest;
            _recordingThread.Start();
        }

        public void StopRecording()
        {
            if (RecordingState == DuplicatorState.Stopping || RecordingState == DuplicatorState.Stopped)
                return;

            RecordingState = DuplicatorState.Stopping;
            var t = _recordingThread;
            _recordingThread = null;
            if (t != null)
            {
                var result = t.Join((int)Math.Min(Options.AudioAcquisitionTimeout * 2L, int.MaxValue));
                if (!result)
                {
                    Trace("Recording thread timed out");
                }
            }
        }

        private class VideoFrame
        {
            public Texture2D Texture;
            public long Time;
            public long Duration;
        }

        private void RecordingThreadFunc()
        {
            do
            {
                switch (RecordingState)
                {
                    case DuplicatorState.Stopping:
                        DrainRecordingVideoQueue();
                        _microphoneCapture.Stop();
                        _soundCapture.Stop();

                        if (_videoSamplesCount > 0)
                        {
                            Trace("SinkWriter Finalize video samples:" + _videoSamplesCount + " sound samples:" + _soundSamplesCount + " gaps:" + _soundGapsCount + " mic samples:" + _microphoneSamplesCount + " gaps:" + _microphoneGapsCount);
                            _sinkWriter.Value.Finalize();
                            _videoSamplesCount = 0;
                        }

                        _soundSamplesCount = 0;
                        _microphoneSamplesCount = 0;
                        _sinkWriter = Reset(_sinkWriter, CreateSinkWriter);
                        _devManager = Reset(_devManager, CreateDeviceManager);
                        _startTime = 0;
                        _videoElapsedNs = 0;
                        _soundElapsedNs = 0;
                        _microphoneElapsedNs = 0;
                        _videoOutputIndex = 0;
                        _soundOutputIndex = 0;
                        _microphoneOutputIndex = 0;
#if DEBUG
                        DumpTraceEvents();
#endif
                        RecordFilePath = null;
                        RecordingState = DuplicatorState.Stopped;
                        return;

                    case DuplicatorState.Starting:
                        RecordingState = DuplicatorState.Started;
                        break;
                }

                bool result = _queuedVideoFrameEvent.WaitOne(Options.AudioAcquisitionTimeout);
                Trace("result:" + result + " queued:" + _videoFramesQueue.Count);
                if (!result && _videoFramesQueue.Count == 0)
                    continue;

                DrainRecordingVideoQueue();
            }
            while (true);
        }

        private void DrainRecordingVideoQueue()
        {
            Trace("Draining video frames:" + _videoFramesQueue.Count);
            while (_videoFramesQueue.TryDequeue(out VideoFrame frame))
            {
                WriteVideoSample(frame);
            }
        }

        private void WriteVideoSample(VideoFrame frame)
        {
            using (frame.Texture)
            {
                using (var sample = MediaFactory.CreateSample())
                {
                    MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID, frame.Texture, 0, new RawBool(true), out MediaBuffer buffer);
                    using (buffer)
                    using (var buffer2 = buffer.QueryInterface<Buffer2D>())
                    {
                        sample.SampleTime = frame.Time;
                        sample.SampleDuration = frame.Duration;

                        buffer.CurrentLength = buffer2.ContiguousLength;
                        sample.AddBuffer(buffer);
                        Trace("[" + _videoSamplesCount + "] queued:" + _videoFramesQueue.Count + " time(ms):" + sample.SampleTime / 10000 + " duration(ms):" + sample.SampleDuration / 10000 + " fps:" + _duplicationFrameRate);
#if DEBUG
                        AddEvent(TraceEventType.WriteVideoFrame, sample.SampleTime, sample.SampleDuration);
#endif
                        _sinkWriter.Value.WriteSample(_videoOutputIndex, sample);
                        _videoSamplesCount++;
                    }
                }
            }
        }

        private void MicrophoneCaptureDataReady(object sender, AudioCaptureNativeDataEventArgs e)
        {
            WriteAudioSample(e.Data, e.Size, _microphoneOutputIndex, ref _microphoneGapsCount, ref _microphoneSamplesCount, ref _microphoneElapsedNs);
            e.Handled = true;
        }

        private void SoundCaptureDataReady(object sender, AudioCaptureNativeDataEventArgs e)
        {
            WriteAudioSample(e.Data, e.Size, _soundOutputIndex, ref _soundGapsCount, ref _soundSamplesCount, ref _soundElapsedNs);
            e.Handled = true;
        }

        private void WriteAudioSample(IntPtr data, int size, int streamIndex, ref int gaps, ref int samplesCount, ref long elapsed)
        {
            if (size == 0)
            {
                var ticks = Stopwatch.GetTimestamp() - _startTime;
                var elapsedNs = (10000000 * ticks) / Stopwatch.Frequency;
                //Trace("SendStreamTick :" + elapsedNs);
#if DEBUG
                AddEvent(TraceEventType.WriteAudioTick, elapsedNs, 0);
#endif

                if (Debugger.IsAttached)
                {
                    _sinkWriter.Value.SendStreamTick(streamIndex, elapsedNs);
                }
                else
                {
                    try
                    {
                        _sinkWriter.Value.SendStreamTick(streamIndex, elapsedNs);
                    }
                    catch (Exception e)
                    {
                        // we may be closing down
                        ReleaseTrace("SendStreamTick[" + streamIndex + "] failed:" + e.Message);
#if DEBUG
                        AddEvent(TraceEventType.Error, elapsedNs, 0, e.Message);
#endif
                    }
                }

                gaps++;
                return;
            }

            using (var buffer = MediaFactory.CreateMemoryBuffer(size))
            {
                var ptr = buffer.Lock(out int max, out int current);
                CopyMemory(ptr, data, size);
                buffer.CurrentLength = size;
                buffer.Unlock();

                using (var sample = MediaFactory.CreateSample())
                {
                    var ticks = Stopwatch.GetTimestamp() - _startTime;
                    var elapsedNs = (10000000 * ticks) / Stopwatch.Frequency;
                    sample.SampleTime = elapsed;
                    sample.SampleDuration = elapsedNs - elapsed;
                    elapsed = elapsedNs;
                    sample.AddBuffer(buffer);
                    //Trace("sample time(ms)[" + index + "]:" + (sample.SampleTime / 10000) + " duration(ms):" + (sample.SampleDuration / 10000) + " bytes:" + size);
#if DEBUG
                    AddEvent(TraceEventType.WriteAudioFrame, sample.SampleTime, sample.SampleDuration);
#endif
                    _sinkWriter.Value.WriteSample(streamIndex, sample);
                    samplesCount++;
                }
            }
        }

        private void SafeAcquireFrame()
        {
            if (Debugger.IsAttached)
            {
                AcquireFrame();
                return;
            }

            try
            {
                AcquireFrame();
            }
            catch (SharpDXException ex)
            {
                ReleaseTrace("TryGetFrame failed: " + ex);
                // DXGI_ERROR_ACCESS_LOST
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.AccessLost.Code)
                {
                    _outputDuplication = new Lazy<OutputDuplication>(CreateOutputDuplication, true);
                    return;
                }
                throw;
            }
        }

        private void AcquireFrame()
        {
            var od = _outputDuplication.Value; // can be null if adapter is not connected
            if (od == null)
                return;

            SharpDX.DXGI.Resource frame;
            OutputDuplicateFrameInformation frameInfo;

            long ts = Stopwatch.GetTimestamp();
            try
            {
                od.AcquireNextFrame(Options.FrameAcquisitionTimeout, out frameInfo, out frame);
            }
            catch (SharpDXException ex)
            {
                long mse = ((Stopwatch.GetTimestamp() - ts) * 1000) / Stopwatch.Frequency;
                Trace("DXGI_ERROR_WAIT_TIMEOUT ms:" + mse);
                // DXGI_ERROR_WAIT_TIMEOUT
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                    return;

                throw;
            }

            long ms = ((Stopwatch.GetTimestamp() - ts) * 1000) / Stopwatch.Frequency;

            using (frame)
            {
                _duplicationFrameNumber++;
                _currentAccumulatedFramesCount = frameInfo.AccumulatedFrames;
                Trace("lpt:" + frameInfo.LastPresentTime + " ac:" + _currentAccumulatedFramesCount + " fps:" + _duplicationFrameRate + " wait:" + ms + "ms");

                if (frameInfo.LastMouseUpdateTime != 0)
                {
                    _pointerVisible = frameInfo.PointerPosition.Visible;
                    _pointerPosition = frameInfo.PointerPosition.Position;
                    if (frameInfo.PointerShapeBufferSize != 0)
                    {
                        ComputePointerBitmap(ref frameInfo);
                    }
                }

                if (frameInfo.LastPresentTime != 0)
                {
                    RenderDuplicatedFrame(frame);
                }

                if (RecordingState == DuplicatorState.Started &&
                    (frameInfo.LastPresentTime != 0 | (frameInfo.LastPresentTime != 0 && _pointerBitmap != null)))
                {
                    if (_startTime == 0)
                    {
                        _startTime = frameInfo.LastPresentTime;
                        _videoElapsedNs = 0;
                        _soundElapsedNs = 0;
                        _microphoneElapsedNs = 0;
                    }

                    var vf = new VideoFrame();
                    var ticks = frameInfo.LastPresentTime - _startTime;
                    var elapsedNs = (10000000 * ticks) / Stopwatch.Frequency;
                    vf.Time = _videoElapsedNs;
                    vf.Duration = elapsedNs - _videoElapsedNs;
                    _videoElapsedNs = elapsedNs;
                    vf.Texture = CreateTexture2D();
                    using (var res = frame.QueryInterface<SharpDX.Direct3D11.Resource>())
                    {
                        _device.Value.ImmediateContext.CopyResource(res, vf.Texture);
                    }

                    if (_pointerBitmap != null)
                    {
                        using (var dc = new SharpDX.Direct2D1.DeviceContext(_2D1Device.Value, DeviceContextOptions.EnableMultithreadedOptimizations))
                        {
                            using (var surface = vf.Texture.QueryInterface<Surface>())
                            {
                                using (var bmp = new Bitmap1(dc, surface))
                                {
                                    dc.Target = bmp;
                                }
                            }

                            dc.BeginDraw();
                            DrawPointerBitmap(dc, false, 0, 0, ScreenSize);
                            dc.EndDraw();
                        }
                    }

                    Trace("enqueue count:" + _videoFramesQueue.Count + " time(ms):" + vf.Time / 10000 + " elapsed(ms):" + elapsedNs / 10000);
                    if (Options.UseRecordingQueue)
                    {
                        _videoFramesQueue.Enqueue(vf);
                        _queuedVideoFrameEvent.Set();
                    }
                    else
                    {
                        WriteVideoSample(vf);
                    }
                }
                od.ReleaseFrame();
            }
        }

        private void DuplicationThreadFunc()
        {
            do
            {
                switch (DuplicatingState)
                {
                    case DuplicatorState.Stopping:
                        ClearDuplicatedFrame();
                        _duplicationFrameRate = 0;
                        _duplicationFrameNumber = 0;
                        _screenSize = new Lazy<Size2>(CreateScreenSize, true);
                        _pointerBitmap = Dispose(_pointerBitmap);
                        _diagsTextFormat = Reset(_diagsTextFormat, CreateDiagsTextFormat);
                        _diagsBrush = Reset(_diagsBrush, CreateDiagsBrush);
                        _output = Reset(_output, CreateOutput);
                        _outputDuplication = Reset(_outputDuplication, CreateOutputDuplication);
#if DEBUG
                        _deviceDebug = Reset(_deviceDebug, CreateDeviceDebug);
#endif

                        _2D1Device = Reset(_2D1Device, Create2D1Device);
                        _backBufferDc = Reset(_backBufferDc, CreateBackBufferDc);
                        _swapChain = Reset(_swapChain, CreateSwapChain);

                        _device = Reset(_device, CreateDevice);
                        DuplicatingState = DuplicatorState.Stopped;
                        return;

                    case DuplicatorState.Starting:
                        DuplicatingState = DuplicatorState.Started;
                        break;
                }

                // handle host window resize
                if (Interlocked.CompareExchange(ref _resized, 0, 1) == 1)
                {
                    Trace("Resized new size:" + Size);
                    if (_swapChain.IsValueCreated)
                    {
                        if (_backBufferDc.IsValueCreated)
                        {
                            // release outstanding references to swapchain's back buffers
                            _backBufferDc = Reset(_backBufferDc, CreateBackBufferDc);
                        }

                        //Trace("resize:" + RenderSize.Width + "x" + RenderSize.Height);
                        _swapChain.Value.ResizeBuffers(
                            _swapChain.Value.Description1.BufferCount,
                            Size.Width,
                            Size.Height,
                            _swapChain.Value.Description1.Format,
                            _swapChain.Value.Description1.Flags);
                    }
                }

                SafeAcquireFrame();
            }
            while (true);
        }

        // How to render by using a Direct2D device context
        // https://msdn.microsoft.com/en-us/library/windows/desktop/hh780339.aspx
        private void RenderDuplicatedFrame(SharpDX.DXGI.Resource frame)
        {
            //Trace("SW:" + _swapChain.Value.Description1.Width + "x" + _swapChain.Value.Description1.Height + " rs:" + RenderSize.Width + "x" + RenderSize.Height);
            _backBufferDc.Value.BeginDraw();
            _backBufferDc.Value.Clear(new RawColor4(0, 0, 0, 1));
            using (var frameSurface = frame.QueryInterface<Surface>())
            {
                // build a DC
                using (var frameDc = new SharpDX.Direct2D1.DeviceContext(_2D1Device.Value, DeviceContextOptions.EnableMultithreadedOptimizations))
                {
                    // bind a bitmap corresponding to the frame to this DC
                    using (var frameBitmap = new Bitmap1(frameDc, frameSurface))
                    {
                        var renderX = (Size.Width - RenderSize.Width) / 2;
                        var renderY = (Size.Height - RenderSize.Height) / 2;

                        // draw this bitmap to the swap chain's backbuffer-bound DC
                        _backBufferDc.Value.DrawBitmap(frameBitmap, new RawRectangleF(renderX, renderY, renderX + RenderSize.Width, renderY + RenderSize.Height), 1, BitmapInterpolationMode.Linear);

                        // add some useful info if required
                        var diags = new List<string>();
                        if (Options.ShowInputFps)
                        {
                            diags.Add(_duplicationFrameRate + " fps");
                        }

                        if (Options.ShowAccumulatedFrames)
                        {
                            diags.Add(_currentAccumulatedFramesCount + " af");
                        }

                        // draw the pointer if visible and if we have it
                        if (Options.ShowCursor && _pointerVisible)
                        {
                            DrawPointerBitmap(_backBufferDc.Value, Options.IsCursorProportional, renderX, renderY, RenderSize);
                        }

                        // add diags, if any
                        if (diags.Count > 0)
                        {
                            _backBufferDc.Value.DrawText(string.Join(Environment.NewLine, diags), _diagsTextFormat.Value, new RawRectangleF(0, 0, Size.Width, Size.Height), _diagsBrush.Value);
                        }
                    }
                }
            }

            _backBufferDc.Value.EndDraw();

            // let's flip it
            _swapChain.Value.Present(1, 0);
        }

        private void ClearDuplicatedFrame()
        {
            if (!_backBufferDc.IsValueCreated)
                return;

            _backBufferDc.Value.BeginDraw();
            _backBufferDc.Value.Clear(new RawColor4(0, 0, 0, 1));
            _backBufferDc.Value.EndDraw();
            _swapChain.Value.Present(1, 0);
        }

        private void DrawPointerBitmap(SharpDX.Direct2D1.DeviceContext dc, bool proportional, int renderX, int renderY, Size2 renderSize)
        {
            if (dc == null)
                return;

            var pb = _pointerBitmap;
            if (pb == null)
                return;

            RawRectangleF rect;

            // note: the doc says not to use the hotspot, but it seems we still need to need it...
            if (proportional)
            {
                int captureX = ((_pointerPosition.X - _pointerHotspot.X) * renderSize.Width) / ScreenSize.Width + renderX;
                int captureY = ((_pointerPosition.Y - _pointerHotspot.Y) * renderSize.Height) / ScreenSize.Height + renderY;
                rect = new RawRectangleF(
                    captureX,
                    captureY,
                    captureX + (pb.Size.Width * renderSize.Width) / ScreenSize.Width,
                    captureY + (pb.Size.Height * renderSize.Height) / ScreenSize.Height);
            }
            else
            {
                int captureX = (_pointerPosition.X * renderSize.Width) / ScreenSize.Width - _pointerHotspot.X + renderX;
                int captureY = (_pointerPosition.Y * renderSize.Height) / ScreenSize.Height - _pointerHotspot.Y + renderY;
                rect = new RawRectangleF(
                    captureX,
                    captureY,
                    captureX + pb.Size.Width,
                    captureY + pb.Size.Height);
            }

            dc.DrawBitmap(pb, rect, 1, BitmapInterpolationMode.NearestNeighbor);
        }

        public void Dispose()
        {
            _frameRateTimer = Dispose(_frameRateTimer);
            StopDuplicating(); // stop recording implicit

#if DEBUG
            DXGIReportLiveObjects();
#endif
        }

        private void OnInformationAvailable(string text)
        {
            if (text == null)
                return;

            InformationAvailable?.Invoke(this, new DuplicatorInformationEventArgs(text));
        }

        private void OnPropertyChanged(string name, string traceValue)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            Trace("Name " + name + " value:" + traceValue);
        }

        // we need to handle some important properties change
        private void OnOptionsChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DuplicatorOptions.Adapter):
                case nameof(DuplicatorOptions.Output):
                    var restart = DuplicatingState == DuplicatorState.Starting || DuplicatingState == DuplicatorState.Started;
                    StopDuplicating();
                    _output = new Lazy<Output1>(CreateOutput, true);
                    _screenSize = new Lazy<Size2>(CreateScreenSize, true);
                    if (restart)
                    {
                        StartDuplicating();
                    }
                    break;

                case nameof(DuplicatorOptions.PreserveRatio):
                    Size = Size;
                    break;
            }
        }

#if DEBUG
        private DeviceDebug CreateDeviceDebug() => _device.Value.QueryInterface<DeviceDebug>();
#endif

        private SharpDX.Direct2D1.Device Create2D1Device()
        {
            using (var fac = new SharpDX.Direct2D1.Factory1())
            {
                using (var dxDev = _device.Value.QueryInterface<SharpDX.DXGI.Device>())
                {
                    return new SharpDX.Direct2D1.Device(fac, dxDev);
                }
            }
        }

        private SharpDX.Direct2D1.DeviceContext CreateBackBufferDc()
        {
            var dc = new SharpDX.Direct2D1.DeviceContext(_2D1Device.Value, DeviceContextOptions.EnableMultithreadedOptimizations);
            using (var backBufferSurface = _swapChain.Value.GetBackBuffer<Surface>(0))
            {
                using (var bmp = new Bitmap1(dc, backBufferSurface))
                {
                    dc.Target = bmp;
                    return dc;
                }
            }
        }

        private SharpDX.Direct3D11.Device CreateDevice()
        {
            using (var fac = new SharpDX.DXGI.Factory1())
            {
                using (var adapter = Options.GetAdapter())
                {
                    if (adapter == null)
                        return null;

                    var flags = DeviceCreationFlags.BgraSupport; // for D2D cooperation
                    flags |= DeviceCreationFlags.VideoSupport;
#if DEBUG
                    flags |= DeviceCreationFlags.Debug;
#endif
                    var device = new SharpDX.Direct3D11.Device(adapter, flags);
                    using (var mt = device.QueryInterface<DeviceMultithread>())
                    {
                        mt.SetMultithreadProtected(new RawBool(true));
                    }
                    return device;
                }
            }
        }

        private SwapChain1 CreateSwapChain()
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/hh780339.aspx
            using (var fac = new SharpDX.Direct2D1.Factory1())
            {
                using (var dxFac = new SharpDX.DXGI.Factory2(
#if DEBUG
                    true
#else
                    false
#endif
                    ))
                {
                    using (var dxDev = _device.Value.QueryInterface<SharpDX.DXGI.Device1>())
                    {
                        var desc = new SwapChainDescription1();
                        desc.SampleDescription = new SampleDescription(1, 0);
                        desc.SwapEffect = SwapEffect.FlipSequential;
                        desc.Scaling = Scaling.None;
                        desc.Usage = Usage.RenderTargetOutput;
                        desc.Format = Format.B8G8R8A8_UNorm;
                        desc.BufferCount = 2;
                        var sw = new SwapChain1(dxFac, dxDev, Hwnd, ref desc);
                        dxDev.MaximumFrameLatency = 1;
                        return sw;
                    }
                }
            }
        }

        private Size2 CreateScreenSize() => _output.Value != null ? new Size2(
                _output.Value.Description.DesktopBounds.Right - _output.Value.Description.DesktopBounds.Left,
                _output.Value.Description.DesktopBounds.Bottom - _output.Value.Description.DesktopBounds.Top) : new Size2();

        private Output1 CreateOutput() => Options.GetOutput();

        private Texture2D CreateTexture2D()
        {
            // this is meant for GPU to GPU operation for maximum performance, so CPU has no access to this
            var desc = new Texture2DDescription()
            {
                CpuAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.RenderTarget,
                Format = Format.B8G8R8A8_UNorm,
                Width = ScreenSize.Width,
                Height = ScreenSize.Height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
            };

            return new Texture2D(_device.Value, desc);
        }

        private Brush CreateDiagsBrush() => new SolidColorBrush(_backBufferDc.Value, new RawColor4(1, 0, 0, 1));
        private TextFormat CreateDiagsTextFormat()
        {
            using (var fac = new SharpDX.DirectWrite.Factory1())
            {
                var diagsTextFormat = new TextFormat(fac, "Lucida Console", 16);
                diagsTextFormat.WordWrapping = WordWrapping.Character;
                return diagsTextFormat;
            }
        }

        private DeviceContextRenderTarget CreateRenderTarget()
        {
            using (var fac = new SharpDX.Direct2D1.Factory1())
            {
                var props = new RenderTargetProperties();
                props.PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore);
                return new DeviceContextRenderTarget(fac, props);
            }
        }

        private OutputDuplication CreateOutputDuplication()
        {
            if (Debugger.IsAttached)
                return _output.Value?.DuplicateOutput(_device.Value);

            try
            {
                return _output.Value?.DuplicateOutput(_device.Value);
            }
            catch (Exception e)
            {
                ReleaseTrace("CreateOutputDuplication failed:" + e.Message);
            }
            return null;
        }

        private DXGIDeviceManager CreateDeviceManager()
        {
            var devManager = new DXGIDeviceManager();
            MediaFactory.CreateDXGIDeviceManager(out int token, devManager);
            devManager.ResetDevice(_device.Value);
            return devManager;
        }

        private SinkWriter CreateSinkWriter()
        {
            MediaManager.Startup(); // this is ok to be called more than once

            if (string.IsNullOrEmpty(RecordFilePath))
            {
                RecordFilePath = Options.GetNewFilePath();
            }

            RecordFilePath += ".mp4"; // we only support MP4...

            string dir = Path.GetDirectoryName(RecordFilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            int width = ScreenSize.Width;
            int height = ScreenSize.Height;

            SinkWriter writer;
            using (var ma = new MediaAttributes())
            {
                // note this doesn't mean you *will* have a hardware transform. Intel Media SDK sometimes is not happy with configuration for HDCP issues.
                ma.Set(SinkWriterAttributeKeys.ReadwriteEnableHardwareTransforms, Options.EnableHardwareTransforms ? 1 : 0);
                ma.Set(SinkWriterAttributeKeys.D3DManager, _devManager.Value);

                // by default, the sink writer's WriteSample method limits the data rate by blocking the calling thread.
                // this prevents the application from delivering samples too quickly.
                // to disable this behavior, set the attribute to 1
                ma.Set(SinkWriterAttributeKeys.DisableThrottling, Options.DisableThrottling ? 1 : 0);
                ma.Set(SinkWriterAttributeKeys.LowLatency, Options.LowLatency);

                Trace("CreateSinkWriterFromURL path:" + RecordFilePath);
                writer = MediaFactory.CreateSinkWriterFromURL(RecordFilePath, IntPtr.Zero, ma);
            }

            int frameRateNumerator = 0;
            int frameRateDenominator = 0;
            if (Options.RecordingFrameRate != 0)
            {
                MediaFactory.AverageTimePerFrameToFrameRate((long)(10000000 / Options.RecordingFrameRate), out frameRateNumerator, out frameRateDenominator);
            }

            using (var videoStream = new MediaType())
            {
                // avg bitrate is mandatory for builtin encoder, not for some others like Intel Media SDK
                // in fact, what will that be used for? anyway, here is a standard formula from here
                // https://stackoverflow.com/questions/5024114/suggested-compression-ratio-with-h-264
                //
                // [image width] x [image height] x [framerate] x [motion rank] x 0.07 = [desired bitrate]
                //

                var rate = Options.RecordingFrameRate != 0 ? Options.RecordingFrameRate : 25f;
                int motionRank = 1;
                int bitrate = (int)(width * height * rate * motionRank * 0.07f);
                if (bitrate <= 0)
                    throw new InvalidOperationException();

                videoStream.Set(MediaTypeAttributeKeys.AvgBitrate, bitrate);
                videoStream.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                videoStream.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.FromFourCC(new FourCC("H264")));
                videoStream.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                //videoStream.Set(MediaTypeAttributeKeys.Mpeg2Profile, (int)eAVEncH264VProfile.eAVEncH264VProfile_444);

                if (Options.RecordingFrameRate != 0)
                {
                    videoStream.Set(MediaTypeAttributeKeys.FrameRate, ((long)frameRateNumerator << 32) | (uint)frameRateDenominator);
                }
                videoStream.Set(MediaTypeAttributeKeys.FrameSize, ((long)width << 32) | (uint)height);
                writer.AddStream(videoStream, out _videoOutputIndex);
                Trace("Added Video Stream index:" + _videoOutputIndex);
            }

            using (var video = new MediaType())
            {
                video.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                video.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Rgb32);
                video.Set(MediaTypeAttributeKeys.FrameSize, ((long)width << 32) | (uint)height);
                if (Options.RecordingFrameRate != 0)
                {
                    video.Set(MediaTypeAttributeKeys.FrameRate, ((long)frameRateNumerator << 32) | (uint)frameRateDenominator);
                }

                Trace("Add Video Input Media Type");
                writer.SetInputMediaType(_videoOutputIndex, video, null);
            }

            //// https://msdn.microsoft.com/en-us/library/windows/desktop/dd797816.aspx
            //writer.GetServiceForStream(_videoOutputIndex, Guid.Empty, typeof(ICodecAPI).GUID, out IntPtr capiPtr);
            //using (var capi = new CodecApi(capiPtr))
            //{
            //    var br = (uint)capi.GetValue(CODECAPI_AVEncCommonMeanBitRate);
            //    var mode = (eAVEncCommonRateControlMode)(uint)capi.GetValue(CODECAPI_AVEncCommonRateControlMode);
            //    capi.SetValue(CODECAPI_AVEncCommonRateControlMode, (uint)(int)eAVEncCommonRateControlMode.eAVEncCommonRateControlMode_Quality);
            //    mode = (eAVEncCommonRateControlMode)(uint)capi.GetValue(CODECAPI_AVEncCommonRateControlMode);

            //    //var quality = (uint)capi.GetValue(CODECAPI_AVEncCommonQuality);
            //    //capi.SetValue(CODECAPI_AVEncCommonQuality, (uint)10);
            //    //capi.SetValue(CODECAPI_AVEncAdaptiveMode, (uint)eAVEncAdaptiveMode.eAVEncAdaptiveMode_FrameRate);
            //}

            if (Options.CaptureSound)
            {
                var format = _soundCapture.GetFormat();
                SoundSamplesPerSecond = format.SamplesPerSecond;
                OnInformationAvailable("Sound Samples Per Second : " + SoundSamplesPerSecond);
                OnPropertyChanged(nameof(SoundSamplesPerSecond), SoundSamplesPerSecond.ToString());

                using (var audioStream = new MediaType())
                {
                    audioStream.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Audio);
                    audioStream.Set(MediaTypeAttributeKeys.Subtype, AudioFormatGuids.Aac);
                    audioStream.Set(MediaTypeAttributeKeys.AudioNumChannels, format.ChannelsCount);
                    audioStream.Set(MediaTypeAttributeKeys.AudioSamplesPerSecond, format.SamplesPerSecond);
                    audioStream.Set(MediaTypeAttributeKeys.AudioBitsPerSample, 16); // loopback forces 16
                    writer.AddStream(audioStream, out _soundOutputIndex);
                    Trace("Added Sound Stream index:" + _soundOutputIndex);
                }

                using (var audio = new MediaType())
                {
                    audio.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Audio);
                    audio.Set(MediaTypeAttributeKeys.Subtype, AudioFormatGuids.Pcm);
                    audio.Set(MediaTypeAttributeKeys.AudioNumChannels, format.ChannelsCount);
                    audio.Set(MediaTypeAttributeKeys.AudioSamplesPerSecond, format.SamplesPerSecond);
                    audio.Set(MediaTypeAttributeKeys.AudioBitsPerSample, 16); // loopback forces 16
                    Trace("Add Sound Input Media Type");
                    writer.SetInputMediaType(_soundOutputIndex, audio, null);
                }
            }

            if (Options.CaptureMicrophone)
            {
                var format = _microphoneCapture.GetFormat();
                MicrophoneSamplesPerSecond = format.SamplesPerSecond;
                OnInformationAvailable("Microphone Samples Per Second : " + MicrophoneSamplesPerSecond);
                OnPropertyChanged(nameof(MicrophoneSamplesPerSecond), MicrophoneSamplesPerSecond.ToString());

                using (var audioStream = new MediaType())
                {
                    audioStream.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Audio);
                    audioStream.Set(MediaTypeAttributeKeys.Subtype, AudioFormatGuids.Aac);
                    audioStream.Set(MediaTypeAttributeKeys.AudioNumChannels, format.ChannelsCount);
                    audioStream.Set(MediaTypeAttributeKeys.AudioSamplesPerSecond, format.SamplesPerSecond);
                    audioStream.Set(MediaTypeAttributeKeys.AudioBitsPerSample, 16); // loopback forces 16
                    writer.AddStream(audioStream, out _microphoneOutputIndex);
                    Trace("Added Microphone Stream index:" + _microphoneOutputIndex);
                }

                using (var audio = new MediaType())
                {
                    audio.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Audio);
                    audio.Set(MediaTypeAttributeKeys.Subtype, AudioFormatGuids.Pcm);
                    audio.Set(MediaTypeAttributeKeys.AudioNumChannels, format.ChannelsCount);
                    audio.Set(MediaTypeAttributeKeys.AudioSamplesPerSecond, format.SamplesPerSecond);
                    audio.Set(MediaTypeAttributeKeys.AudioBitsPerSample, 16); // loopback forces 16
                    Trace("Add Microphone Input Media Type");
                    writer.SetInputMediaType(_microphoneOutputIndex, audio, null);
                }
            }

            // gather some information
            IsUsingBuiltinEncoder = H264Encoder.IsBuiltinEncoder(writer, _videoOutputIndex);
            IsUsingDirect3D11AwareEncoder = H264Encoder.IsDirect3D11AwareEncoder(writer, _videoOutputIndex);
            IsUsingHardwareBasedEncoder = H264Encoder.IsHardwareBasedEncoder(writer, _videoOutputIndex);
            EncoderFriendlyName = H264Encoder.GetEncoderFriendlyName(writer, _videoOutputIndex);
            OnInformationAvailable("Encoder Type : " + (IsUsingHardwareBasedEncoder ? "Hardware" : "Software"));
            OnInformationAvailable("Direct3D 11 Aware Encoder Type : " + IsUsingDirect3D11AwareEncoder);
            OnInformationAvailable("Encoder Name : " + EncoderFriendlyName);
            Trace("IsBuiltinEncoder:" + IsUsingBuiltinEncoder + " IsUsingDirect3D11AwareEncoder:" + IsUsingDirect3D11AwareEncoder + " IsUsingHardwareBasedEncoder:" + IsUsingHardwareBasedEncoder);
            OnPropertyChanged(nameof(IsUsingBuiltinEncoder), IsUsingBuiltinEncoder.ToString());
            OnPropertyChanged(nameof(IsUsingDirect3D11AwareEncoder), IsUsingDirect3D11AwareEncoder.ToString());
            OnPropertyChanged(nameof(IsUsingHardwareBasedEncoder), IsUsingHardwareBasedEncoder.ToString());
            Trace("Begin Writing");

            writer.BeginWriting();

            if (Options.CaptureMicrophone)
            {
                var ad = Options.GetMicrophoneDevice() ?? AudioCapture.GetMicrophoneDevice();
                if (ad != null)
                {
                    _microphoneCapture.Start(ad);
                }
            }

            if (Options.CaptureSound)
            {
                var ad = Options.GetSoundDevice() ?? AudioCapture.GetSpeakersDevice();
                if (ad != null)
                {
                    _soundCapture.Start(ad);
                }
            }
            OnPropertyChanged(nameof(RecordFilePath), RecordFilePath);
            return writer;
        }

        // compute the shape of the pointer bitmap
        private void ComputePointerBitmap(ref OutputDuplicateFrameInformation frameInfo)
        {
            var bmp = _pointerBitmap;
            _pointerBitmap = null;
            if (bmp != null)
            {
                bmp.Dispose();
            }

            var pointerShapeBuffer = Marshal.AllocHGlobal(frameInfo.PointerShapeBufferSize);
            OutputDuplicatePointerShapeInformation shapeInfo;
            try
            {
                _outputDuplication.Value.GetFramePointerShape(frameInfo.PointerShapeBufferSize, pointerShapeBuffer, out int shapeInfoSize, out shapeInfo);
                //Trace("new pointer alloc size:" + frameInfo.PointerShapeBufferSize + " size:" + shapeInfo.Width + "x" + shapeInfo.Height +
                //    " hs:" + shapeInfo.HotSpot.X + "x" + shapeInfo.HotSpot.Y +
                //    " pitch:" + shapeInfo.Pitch + " type:" + shapeInfo.Type +
                //    " pos:" + _pointerPosition.X + "x" + _pointerPosition.Y);
            }
            catch
            {
                Marshal.FreeHGlobal(pointerShapeBuffer);
                throw;
            }

            try
            {
                _pointerHotspot = shapeInfo.HotSpot;
                int bufferSize;
                int pitch;
                Size2 size;
                const int DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MONOCHROME = 1;
                if (shapeInfo.Type == DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MONOCHROME)
                {
                    var ptr = ComputeMonochromePointerShape(shapeInfo, pointerShapeBuffer, out bufferSize);
                    Marshal.FreeHGlobal(pointerShapeBuffer);
                    pointerShapeBuffer = ptr;
                    size = new Size2(shapeInfo.Width, shapeInfo.Height / 2);
                    pitch = shapeInfo.Width * 4;
                }
                else // note: we do not handle DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MASKED_COLOR...
                {
                    bufferSize = frameInfo.PointerShapeBufferSize;
                    size = new Size2(shapeInfo.Width, shapeInfo.Height);
                    pitch = shapeInfo.Pitch;
                }

                // we need to handle alpha channel for the pointer
                var bprops = new BitmapProperties();
                bprops.PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied);
                _pointerBitmap = new Bitmap(_backBufferDc.Value, size, new DataPointer(pointerShapeBuffer, bufferSize), pitch, bprops);
            }
            finally
            {
                Marshal.FreeHGlobal(pointerShapeBuffer);
            }
        }

        // handling DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MONOCHROME
        // https://msdn.microsoft.com/en-us/library/windows/desktop/hh404520.aspx
        // The pointer type is a monochrome mouse pointer, which is a monochrome bitmap.
        // The bitmap's size is specified by width and height in a 1 bits per pixel (bpp) device independent bitmap (DIB) format AND mask
        //  that is followed by another 1 bpp DIB format XOR mask of the same size.
        private static IntPtr ComputeMonochromePointerShape(OutputDuplicatePointerShapeInformation shapeInfo, IntPtr inBuffer, out int size)
        {
            const int bpp = 4;
            size = shapeInfo.Width * (shapeInfo.Height / 2) * bpp;
            var ptr = Marshal.AllocHGlobal(size);
            for (int row = 0; row < shapeInfo.Height / 2; row++)
            {
                int mask = 0x80;
                for (int col = 0; col < shapeInfo.Width; col++)
                {
                    var and = (Marshal.ReadByte(inBuffer, col / 8 + row * shapeInfo.Pitch) & mask) != 0;
                    var xor = (Marshal.ReadByte(inBuffer, col / 8 + (row + (shapeInfo.Height / 2)) * shapeInfo.Pitch) & mask) != 0;

                    uint value;
                    if (and)
                    {
                        if (xor)
                        {
                            value = 0xFF000000;
                        }
                        else
                        {
                            value = 0x00000000;
                        }
                    }
                    else
                    {
                        if (xor)
                        {
                            value = 0xFFFFFFFF;
                        }
                        else
                        {
                            value = 0xFF000000;
                        }
                    }
                    Marshal.WriteInt32(ptr, row * shapeInfo.Width * bpp + col * bpp, (int)value);

                    if (mask == 0x01)
                    {
                        mask = 0x80;
                    }
                    else
                    {
                        mask = mask >> 1;
                    }
                }
            }
            return ptr;
        }

        internal static void ReleaseTrace(object value, [CallerMemberName] string methodName = null) => Console.WriteLine("yay");//_provider.WriteMessageEvent("#Duplicator(" + Thread.CurrentThread.ManagedThreadId + ")::" + methodName + " " + string.Format("{0}", value), 0, 0);

        [Conditional("DEBUG")]
        internal static void Trace(object value, [CallerMemberName] string methodName = null) => Console.WriteLine("yay");//_provider.WriteMessageEvent("#Duplicator(" + Thread.CurrentThread.ManagedThreadId + ")::" + methodName + " " + string.Format("{0}", value), 0, 0);

        // a multi thread version is:
        // disposable = Interlocked.Exchange(ref disposable, null)?.Dispose();
        private static T Dispose<T>(T disposable) where T : IDisposable
        {
            disposable?.Dispose();
            return default(T);
        }

        private static Lazy<T> Reset<T>(Lazy<T> disposable, Func<T> valueFactory) where T : IDisposable
        {
            if (disposable != null && disposable.IsValueCreated)
            {
                disposable.Value?.Dispose();
            }
            return new Lazy<T>(valueFactory, true);
        }

        [DllImport("kernel32")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

#if DEBUG
        private void AddEvent(TraceEventType type, long time, long duration) => AddEvent(type, time, duration, null);
        private void AddEvent(TraceEventType type, long time, long duration, string text)
        {
            var evt = new TraceEvent();
            evt.Type = type;
            evt.TimeMs = time / 10000;
            evt.DurationMs = duration / 10000;
            evt.Text = text;
            evt.DupFps = _duplicationFrameRate;
            evt.Queued = _videoFramesQueue.Count;
            evt.AccumulatedFrames = _currentAccumulatedFramesCount;
            _events.Enqueue(evt);
        }

        private void DumpTraceEvents()
        {
            if (RecordFilePath == null)
            {
                _events = new ConcurrentQueue<TraceEvent>();
                return;
            }

            var events = _events.ToList();
            _events = new ConcurrentQueue<TraceEvent>();
            events.Sort();

            string path = Path.ChangeExtension(RecordFilePath, ".csv");

            // note: unicode encoding + tabs as separator ensure Excel opens this with columns already set
            using (var writer = new StreamWriter(path, false, Encoding.Unicode))
            {
                writer.WriteLine("Type\tTimeMs\tDurationMs\tDupFps\tQueued\tAccFrames\tText");
                foreach (var evt in events)
                {
                    writer.Write(evt.Type.ToString());
                    writer.Write('\t');

                    writer.Write(evt.TimeMs.ToString());
                    writer.Write('\t');

                    writer.Write(evt.DurationMs.ToString());
                    writer.Write('\t');

                    writer.Write(evt.DupFps.ToString());
                    writer.Write('\t');

                    writer.Write(evt.Queued.ToString());
                    writer.Write('\t');

                    writer.Write(evt.AccumulatedFrames.ToString());
                    writer.Write('\t');

                    if (evt.Text != null)
                    {
                        writer.Write(evt.Text);
                    }
                    writer.WriteLine();
                }
            }
        }

        private enum TraceEventType
        {
            GotFrame,
            WriteAudioFrame,
            WriteVideoFrame,
            WriteAudioTick,
            Error,
        }

        private class TraceEvent : IComparable<TraceEvent>
        {
            public TraceEventType Type;
            public long TimeMs;
            public long DurationMs;
            public int DupFps;
            public int Queued;
            public int AccumulatedFrames;
            public string Text;

            public int CompareTo(TraceEvent other) => TimeMs.CompareTo(other.TimeMs);
        }
#endif

    private enum eAVEncH264VProfile
        {
            eAVEncH264VProfile_unknown = 0,
            eAVEncH264VProfile_Simple = 66,
            eAVEncH264VProfile_Base = 66,
            eAVEncH264VProfile_Main = 77,
            eAVEncH264VProfile_High = 100,
            eAVEncH264VProfile_422 = 122,
            eAVEncH264VProfile_High10 = 110,
            eAVEncH264VProfile_444 = 144,
            eAVEncH264VProfile_Extended = 88,
            eAVEncH264VProfile_ScalableBase = 83,
            eAVEncH264VProfile_ScalableHigh = 86,
            eAVEncH264VProfile_MultiviewHigh = 118,
            eAVEncH264VProfile_StereoHigh = 128,
            eAVEncH264VProfile_ConstrainedBase = 256,
            eAVEncH264VProfile_UCConstrainedHigh = 257,
            eAVEncH264VProfile_UCScalableConstrainedBase = 258,
            eAVEncH264VProfile_UCScalableConstrainedHigh = 259
        }

        private enum eAVEncAdaptiveMode
        {
            eAVEncAdaptiveMode_None = 0,
            eAVEncAdaptiveMode_Resolution = 1,
            eAVEncAdaptiveMode_FrameRate = 2
        };

        private enum eAVEncCommonRateControlMode
        {
            eAVEncCommonRateControlMode_CBR = 0,
            eAVEncCommonRateControlMode_PeakConstrainedVBR = 1,
            eAVEncCommonRateControlMode_UnconstrainedVBR = 2,
            eAVEncCommonRateControlMode_Quality = 3,
            eAVEncCommonRateControlMode_LowDelayVBR = 4,
            eAVEncCommonRateControlMode_GlobalVBR = 5,
            eAVEncCommonRateControlMode_GlobalLowDelayVBR = 6
        };

        private class CodecApi : ComObject
        {
            private ICodecAPI _api;

            public CodecApi(IntPtr ptr)
                : base(ptr)
            {
                _api = (ICodecAPI)Marshal.GetObjectForIUnknown(NativePointer);
            }

            public void SetValue(Guid api, object value) => _api.SetValue(api, ref value);

            public object GetValue(Guid api)
            {
                int hr = _api.GetValue(api, out object value);
                if (hr != 0)
                    throw new Win32Exception(hr);

                return value;
            }

            public bool TryGetValue(Guid api, out object value) => _api.GetValue(api, out value) == 0;
        }

        //private static readonly Guid CLSID_EnhancedVideoRenderer = new Guid("fa10746c-9b63-4b6c-bc49-fc300ea5f256");
        private static readonly Guid CODECAPI_AVEncCommonRateControlMode = new Guid("1c0608e9-370c-4710-8a58-cb6181c42423");
        private static readonly Guid CODECAPI_AVEncAdaptiveMode = new Guid("4419b185-da1f-4f53-bc76-097d0c1efb1e");
        private static readonly Guid CODECAPI_AVEncCommonQuality = new Guid("fcbf57a3-7ea5-4b0c-9644-69b40c39c391");
        private static readonly Guid CODECAPI_AVEncCommonMeanBitRate = new Guid("f7222374-2144-4815-b550-a37f8e12ee52");

        [Guid("901db4c7-31ce-41a2-85dc-8fa0bf41b8da"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ICodecAPI
        {
            [PreserveSig]
            int IsSupported([MarshalAs(UnmanagedType.LPStruct)] Guid Api);

            [PreserveSig]
            int IsModifiable([MarshalAs(UnmanagedType.LPStruct)] Guid Api);

            [PreserveSig]
            int GetParameterRange([MarshalAs(UnmanagedType.LPStruct)] Guid Api, out object ValueMin, out object ValueMax, out object SteppingDelta);

            [PreserveSig]
            int GetParameterValues([MarshalAs(UnmanagedType.LPStruct)] Guid Api, out IntPtr Values, out int ValuesCount);

            object GetDefaultValue([MarshalAs(UnmanagedType.LPStruct)] Guid Api);

            [PreserveSig]
            int GetValue([MarshalAs(UnmanagedType.LPStruct)] Guid Api, out object Value);

            void SetValue([MarshalAs(UnmanagedType.LPStruct)] Guid Api, [In] ref object Value);
            // other undefined
        }

#if DEBUG
        private static void DXGIReportLiveObjects() => DXGIReportLiveObjects(DXGI_DEBUG_ALL, DXGI_DEBUG_RLO_FLAGS.DXGI_DEBUG_RLO_ALL);
        private static void DXGIReportLiveObjects(Guid apiid) => DXGIReportLiveObjects(apiid, DXGI_DEBUG_RLO_FLAGS.DXGI_DEBUG_RLO_ALL);
        private static void DXGIReportLiveObjects(Guid apiid, DXGI_DEBUG_RLO_FLAGS flags)
        {
            DXGIGetDebugInterface(typeof(IDXGIDebug).GUID, out IDXGIDebug debug);
            if (debug == null)
                return;

            debug.ReportLiveObjects(apiid, flags);
            Marshal.ReleaseComObject(debug);
        }

        [DllImport("Dxgidebug")]
        private static extern int DXGIGetDebugInterface([MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IDXGIDebug debug);

        private enum DXGI_DEBUG_RLO_FLAGS
        {
            DXGI_DEBUG_RLO_SUMMARY = 0x1,
            DXGI_DEBUG_RLO_DETAIL = 0x2,
            DXGI_DEBUG_RLO_IGNORE_INTERNAL = 0x4,
            DXGI_DEBUG_RLO_ALL = 0x7
        }

        private static Guid DXGI_DEBUG_ALL = new Guid("e48ae283-da80-490b-87e6-43e9a9cfda08");
        private static Guid DXGI_DEBUG_DX = new Guid("35cdd7fc-13b2-421d-a5d7-7e4451287d64");
        private static Guid DXGI_DEBUG_DXGI = new Guid("25cddaa4-b1c6-47e1-ac3e-98875b5a2e2a");
        private static Guid DXGI_DEBUG_APP = new Guid("06cd6e01-4219-4ebd-8709-27ed23360c62");
        private static Guid DXGI_DEBUG_D3D10 = new Guid("243b4c52-3606-4d3a-99d7-a7e7b33ed706");
        private static Guid DXGI_DEBUG_D3D11 = new Guid("4b99317b-ac39-4aa6-bb0b-baa04784798f");
        private static Guid DXGI_DEBUG_D3D12 = new Guid("cf59a98c-a950-4326-91ef-9bbaa17bfd95");

        [Guid("119e7452-de9e-40fe-8806-88f90c12b441"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDXGIDebug
        {
            void ReportLiveObjects(Guid apiid, DXGI_DEBUG_RLO_FLAGS flags);
        }
#endif
    }
}