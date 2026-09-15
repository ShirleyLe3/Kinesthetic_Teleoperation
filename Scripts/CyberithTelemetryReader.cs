// CyberithTelemetryReader.cs
//
// Requires the CybSDK Unity 2022 package from Cyberith.
// Target: Windows Editor / Windows Standalone.
//
// Setup:
// 1. Import CybSDK_Unity_2022.unitypackage.
// 2. Add a CVirtPlayerController prefab, or at least a CVirtDeviceController,
//    to the scene. Set deviceType to NativeVirtualizer while diagnosing a
//    physical connection, or Automatic for normal use.
// 3. Add this component to that same GameObject and assign deviceController
//    in the Inspector (it will also try to find one automatically).
// 4. Subscribe to SampleReceived, or read LatestSample from another script.
//
// CVirtDeviceController establishes, opens, and closes the device itself.
// This reader intentionally never calls Open() or Close().

using System;
using CybSDK;
using UnityEngine;
using UVirtDevice = CybSDK.IVirtDevice;

[DisallowMultipleComponent]
public class CyberithTelemetryReader : MonoBehaviour
{
    [Header("Device")]
    [Tooltip("The CVirtDeviceController that manages the Virtualizer connection.")]
    [SerializeField] private CVirtDeviceController deviceController;

    [Header("Sampling")]
    [Tooltip("How often to publish telemetry samples. Four Hz matches the FlyingBike telemetry collector.")]
    [SerializeField, Min(0.01f)] private float sampleIntervalSeconds = 0.25f;

    [Tooltip("Useful for confirming data in the Unity Console; disable once integrated.")]
    [SerializeField] private bool logSamplesToConsole = true;

    /// <summary>True only while CybSDK has an open Virtualizer device.</summary>
    public bool IsConnected { get; private set; }

    /// <summary>The most recent sample, valid when IsConnected is true.</summary>
    public CyberithTelemetrySample LatestSample { get; private set; }

    /// <summary>Raised at the configured sample interval while connected.</summary>
    public event Action<CyberithTelemetrySample> SampleReceived;

    /// <summary>Raised only when the observed open/closed state changes.</summary>
    public event Action<bool> ConnectionChanged;

    private float nextSampleTime;
    private bool hasReportedConnectionState;
    private bool lastReportedConnectionState;

    private void Awake()
    {
        // Inspector assignment is preferred. This makes the script convenient
        // when it is attached to the CybSDK example's CVirtPlayerController.
        if (deviceController == null)
            deviceController = GetComponent<CVirtDeviceController>();

        if (deviceController == null)
            deviceController = FindObjectOfType<CVirtDeviceController>();

        if (deviceController == null)
            Debug.LogError("CyberithTelemetryReader: no CVirtDeviceController was found.", this);
    }

    private void Update()
    {
        if (Time.unscaledTime < nextSampleTime)
            return;

        nextSampleTime = Time.unscaledTime + sampleIntervalSeconds;

        if (deviceController == null)
        {
            ReportConnectionState(false);
            return;
        }

        // The controller creates the device in Awake(), using Virt.FindDevice()
        // for a NativeVirtualizer, and opens it before this script reads it.
        UVirtDevice device = deviceController.GetDevice();

        if (device == null || !device.IsOpen())
        {
            ReportConnectionState(false);
            return;
        }

        ReportConnectionState(true);

        // Raw values use Cyberith's native SDK units:
        // - speed: metres per second
        // - movement direction: -1..1 == -180..180 degrees, relative to orientation
        // - player orientation: 0..1 == 0..360 degrees, clockwise
        // - height: relative to the calibrated height; 1 == one centimetre
        float speedMetresPerSecond = device.GetMovementSpeed();
        float movementDirectionNormalized = device.GetMovementDirection();
        float playerOrientationNormalized = device.GetPlayerOrientation();
        float heightDeltaCentimetres = device.GetPlayerHeight();

        // Unity-ready equivalents supplied by CybSDK's extension methods.
        Vector3 movementDirectionVector = device.GetMovementDirectionVector();
        Vector3 movementVector = device.GetMovementVector();
        Quaternion playerOrientation = device.GetPlayerOrientationQuaternion();

        LatestSample = new CyberithTelemetrySample
        {
            timestampSeconds = Time.realtimeSinceStartup,
            speedMetresPerSecond = speedMetresPerSecond,
            movementDirectionNormalized = movementDirectionNormalized,
            movementDirectionDegrees = movementDirectionNormalized * 180f,
            movementDirectionVector = movementDirectionVector,
            movementVector = movementVector,
            playerOrientationNormalized = playerOrientationNormalized,
            playerOrientationDegrees = playerOrientationNormalized * 360f,
            playerOrientation = playerOrientation,
            heightDeltaCentimetres = heightDeltaCentimetres
        };

        SampleReceived?.Invoke(LatestSample);

        if (logSamplesToConsole)
        {
            Debug.Log(
                $"Cyberith | speed={LatestSample.speedMetresPerSecond:F2} m/s " +
                $"moveDir={LatestSample.movementDirectionDegrees:F1}° " +
                $"orientation={LatestSample.playerOrientationDegrees:F1}° " +
                $"heightDelta={LatestSample.heightDeltaCentimetres:F1} cm",
                this);
        }
    }

    private void ReportConnectionState(bool isConnected)
    {
        IsConnected = isConnected;

        if (hasReportedConnectionState && lastReportedConnectionState == isConnected)
            return;

        hasReportedConnectionState = true;
        lastReportedConnectionState = isConnected;
        ConnectionChanged?.Invoke(isConnected);

        Debug.Log(
            isConnected
                ? "CyberithTelemetryReader: Virtualizer connected."
                : "CyberithTelemetryReader: Virtualizer not connected.",
            this);
    }
}

[Serializable]
public struct CyberithTelemetrySample
{
    public float timestampSeconds;

    public float speedMetresPerSecond;
    public float movementDirectionNormalized;
    public float movementDirectionDegrees;
    public Vector3 movementDirectionVector;
    public Vector3 movementVector;

    public float playerOrientationNormalized;
    public float playerOrientationDegrees;
    public Quaternion playerOrientation;

    public float heightDeltaCentimetres;
}
