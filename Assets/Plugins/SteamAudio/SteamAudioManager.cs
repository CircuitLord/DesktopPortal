﻿//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SteamAudio
{
    [AddComponentMenu("Steam Audio/Steam Audio Manager")]
    public class SteamAudioManager : MonoBehaviour
    {
        private void Awake()
        {
            if (managerSingleton == null)
                managerSingleton = this;

            Initialize(GameEngineStateInitReason.Playing);
        }

        void OnEnable()
        {
            StartCoroutine(EndOfFrameUpdate());
        }

        private void OnDestroy()
        {
            Destroy();
            ClearSingleton();
        }

        void OnApplicationQuit()
        {
            var instancedScenes = managerData.gameEngineState.instancedScenes;
            if (instancedScenes != null) {
                foreach (var item in instancedScenes) {
                    var instancedScene = item.Value;
                    PhononCore.iplDestroyScene(ref instancedScene);
                }
                instancedScenes.Clear();
            }

            PhononUnityNative.iplUnityResetAudioEngine();
            PhononCore.iplCleanup();
        }

        void Update()
        {
            var scene = managerData.gameEngineState.Scene().GetScene();
            PhononCore.iplCommitScene(scene);
        }

        IEnumerator EndOfFrameUpdate()
        {
            while (true)
            {
                var audioListener = currentAudioListener;
                if (updateComponents)
                    audioListener = managerData.componentCache.AudioListener();

                if (audioListener != null)
                {
                    var listenerPosition = Common.ConvertVector(audioListener.transform.position);
                    var listenerAhead = Common.ConvertVector(audioListener.transform.forward);
                    var listenerUp = Common.ConvertVector(audioListener.transform.up);

                    managerData.audioEngineState.UpdateListener(listenerPosition, listenerAhead, listenerUp);
                }

                managerData.audioEngineState.UpdateSOFAFile(currentSOFAFile);

                yield return waitForEndOfFrame;
            }
        }

        // Initializes Phonon Manager, in particular various Phonon API handles contained within Phonon Manager.
        // Initialize will be performed only once despite repeated calls to Initialize without calls to Destroy.
        public void Initialize(GameEngineStateInitReason reason)
        {
            var sofaFiles = GetSOFAFileNames();

            var sofaPaths = new string[sofaFiles.Length];
            for (var i = 0; i < sofaFiles.Length; ++i) {
                var sofaPath = "";
                if (sofaFiles[i] != null && sofaFiles[i].Length > 0) {
                    var sofaFileName = sofaFiles[i];
                    if (!sofaFileName.EndsWith(".sofa")) {
                        sofaFileName += ".sofa";
                    }

                    sofaPath = Common.GetStreamingAssetsFileName(sofaFileName);
                    if (!File.Exists(sofaPath)) {
                        Debug.LogWarning("Unable to find SOFA file: " + sofaPath + ", reverting to built-in HRTF.");
                        sofaPath = "";
                    }
                }
                sofaPaths[i] = sofaPath;
            }

            managerData.Initialize(reason, audioEngine, simulationValue, sofaPaths);
        }

        // Destroys Phonon Manager.
        public void Destroy()
        {
            managerData.Destroy();
        }

        public static bool IsAudioEngineInitializing()
        {
#if UNITY_EDITOR
            return Application.isEditor && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
            return true;
#endif
        }

        // Exports Unity Scene and saves it in a phononscene file.
        public void ExportScene(bool exportOBJ)
        {
            Initialize(GameEngineStateInitReason.ExportingScene);
            managerData.gameEngineState.ExportScene(materialValue, exportOBJ);
            Destroy();
        }

        public AudioListener AudioListener()
        {
            return managerData.componentCache.AudioListener();
        }

        public SteamAudioListener SteamAudioListener()
        {
            return managerData.componentCache.SteamAudioListener();
        }

        public GameEngineState GameEngineState()
        {
            return managerData.gameEngineState;
        }

        public AudioEngineState AudioEngineState()
        {
            return managerData.audioEngineState;
        }

        public ManagerData ManagerData()
        {
            return managerData;
        }

        public static SteamAudioManager GetSingleton()
        {
            if (managerSingleton == null)
                managerSingleton = FindObjectOfType<SteamAudioManager>();

            return managerSingleton;
        }

        public int NumSOFAFiles() 
        {
            var numSOFAFiles = 0;
            if (sofaFiles != null) {
                foreach (var sofaFile in sofaFiles) {
                    if (sofaFile.Length > 0) {
                        ++numSOFAFiles;
                    }
                }
            }
            return numSOFAFiles;
        }

        public string[] GetSOFAFileNames() 
        {
            var numSOFAFiles = NumSOFAFiles();
            var sofaFileNames = new string[numSOFAFiles + 1];

            sofaFileNames[0] = "";

            var index = 1;
            if (sofaFiles != null) {
                foreach (var sofaFile in sofaFiles) {
                    if (sofaFile.Length > 0) {
                        sofaFileNames[index] = sofaFile;
                        ++index;
                    }
                }
            }

            return sofaFileNames;
        }

        static void ClearSingleton()
        {
            managerSingleton = null;
        }

        public void CreateInstancedMesh(string assetFileName, Transform instanceTransform, ref IntPtr instancedMesh)
        {
            var status = Error.None;

            var serializedData = File.ReadAllBytes(Application.streamingAssetsPath + "/" + assetFileName);
            if (serializedData == null || serializedData.Length == 0) {
                return;
            }

            var context = managerData.gameEngineState.Context();
            var computeDevice = managerData.gameEngineState.ComputeDevice().GetDevice();
            var simulationSettings = managerData.gameEngineState.SimulationSettings();
            var scene = managerData.gameEngineState.Scene().GetScene();

            if (simulationSettings.sceneType != SceneType.Embree) {
                Debug.LogWarning("Dynamic Objects are only supported with Embree.");
                return;
            }

            var instancedScenes = managerData.gameEngineState.instancedScenes;

            var instancedScene = IntPtr.Zero;
            if (instancedScenes != null && instancedScenes.ContainsKey(assetFileName)) {
                instancedScene = instancedScenes[assetFileName];
            } else {
                status = PhononCore.iplLoadScene(context, simulationSettings, serializedData, serializedData.Length,
                    computeDevice, null, ref instancedScene);
                if (status != Error.None) {
                    throw new Exception();
                }
            }

            var transformMatrix = Common.ConvertTransform(instanceTransform);

            status = PhononCore.iplCreateInstancedMesh(scene, instancedScene, transformMatrix, ref instancedMesh);
            if (status != Error.None) {
                throw new Exception();
            }

            if (instancedScenes == null) {
                managerData.gameEngineState.instancedScenes = new Dictionary<string, IntPtr>();
            }
            if (!managerData.gameEngineState.instancedScenes.ContainsKey(assetFileName)) {
                managerData.gameEngineState.instancedScenes.Add(assetFileName, instancedScene);
            }
        }

        public void DestroyInstancedMesh(ref IntPtr instancedMesh)
        {
            if (instancedMesh == IntPtr.Zero) {
                return;
            }

            PhononCore.iplDestroyInstancedMesh(ref instancedMesh);
        }

        public void EnableInstancedMesh(IntPtr instancedMesh)
        {
            if (instancedMesh == IntPtr.Zero) {
                return;
            }

            var scene = managerData.gameEngineState.Scene().GetScene();

            PhononCore.iplAddInstancedMesh(scene, instancedMesh);
        }

        public void DisableInstancedMesh(IntPtr instancedMesh)
        {
            if (instancedMesh == IntPtr.Zero) {
                return;
            }

            var scene = managerData.gameEngineState.Scene().GetScene();

            PhononCore.iplRemoveInstancedMesh(scene, instancedMesh);
        }

        public void UpdateInstancedMeshTransform(IntPtr instancedMesh, Transform instanceTransform)
        {
            if (instancedMesh == IntPtr.Zero) {
                return;
            }

            var transformMatrix = Common.ConvertTransform(instanceTransform);

            PhononCore.iplUpdateInstancedMeshTransform(instancedMesh, transformMatrix);
        }

        static SteamAudioManager managerSingleton = null;

        ManagerData         managerData         = new ManagerData();
        WaitForEndOfFrame   waitForEndOfFrame   = new WaitForEndOfFrame();

        public AudioEngine              audioEngine             = AudioEngine.UnityNative;
        public string[]                 sofaFiles               = null;
        public int                      currentSOFAFile         = 0;
        public MaterialPreset           materialPreset          = MaterialPreset.Generic;
        public MaterialValue            materialValue;
        public SimulationSettingsPreset simulationPreset        = SimulationSettingsPreset.Low;
        public SimulationSettingsValue  simulationValue;
        public bool                     updateComponents        = true;
        public AudioListener            currentAudioListener    = null;
        public bool                     showLoadTimeOptions     = false;
        public bool                     showMassBakingOptions   = false;
        public Baker                    phononBaker             = new Baker();
        public LayerMask                layerMask               = new LayerMask();
    }
}