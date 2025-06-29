using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Rebecca.Services
{
    // --- Enumerations ---

    /// <summary>
    /// Defines constants that indicate the data-flow direction for an audio endpoint device.
    /// </summary>
    /// <remarks>
    /// Defined in mmdeviceapi.h
    /// </remarks>
    public enum EDataFlow
    {
        eRender,
        eCapture,
        eAll,
        EDataFlow_enum_count
    }

    /// <summary>
    /// Defines constants that indicate the role that the user has assigned to an audio endpoint device.
    /// </summary>
    /// <remarks>
    /// Defined in mmdeviceapi.h
    /// </remarks>
    public enum ERole
    {
        eConsole,
        eMultimedia,
        eCommunications,
        ERole_enum_count
    }

    /// <summary>
    /// Defines constants that indicate the current state of an audio endpoint device.
    /// </summary>
    /// <remarks>
    /// Defined in mmdeviceapi.h
    /// </remarks>
    [Flags]
    public enum DEVICE_STATE : uint
    {
        /// <summary>
        /// The audio endpoint device is active.
        /// </summary>
        ACTIVE = 0x00000001,
        /// <summary>
        /// The audio endpoint device is disabled.
        /// </summary>
        DISABLED = 0x00000002,
        /// <summary>
        /// The audio endpoint device is not present because it has been unplugged or is no longer accessible.
        /// </summary>
        NOTPRESENT = 0x00000004,
        /// <summary>
        /// The audio endpoint device is unplugged.
        /// </summary>
        UNPLUGGED = 0x00000008,
        /// <summary>
        /// All audio endpoint devices.
        /// </summary>
        ALL = ACTIVE | DISABLED | NOTPRESENT | UNPLUGGED
    }

    // --- COM Interface Definitions ---

    /// <summary>
    /// The IMMDevice interface represents an audio endpoint device.
    /// </summary>
    /// <remarks>
    /// Defined in mmdeviceapi.h
    /// IID: D666063F-1587-4E43-81F1-B948E807363F
    /// </remarks>
    [ComImport]
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDevice
    {
        [PreserveSig]
        int Activate(
            [In] ref Guid iid,
            [In] int dwClsCtx,
            [In] IntPtr pActivationParams,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);

        [PreserveSig]
        int OpenPropertyStore(
            [In] int stgmAccess, // STGM_READ
            out IPropertyStore ppProperties);

        [PreserveSig]
        int GetId(
            [MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);

        [PreserveSig]
        int GetState(
            out DEVICE_STATE pdwState);
    }

    /// <summary>
    /// The IMMDeviceCollection interface manages a collection of multimedia device resources.
    /// </summary>
    /// <remarks>
    /// Defined in mmdeviceapi.h
    /// IID: 0BD7A1ED-73B5-4CE5-8CD2-EF6FC1BEE57B
    /// </remarks>
    [ComImport]
    [Guid("0BD7A1ED-73B5-4CE5-8CD2-EF6FC1BEE57B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceCollection
    {
        [PreserveSig]
        int GetCount(
            out uint pcDevices);

        [PreserveSig]
        int Item(
            [In] uint nDevice,
            out IMMDevice ppDevice);
    }

    /// <summary>
    /// The IMMDeviceEnumerator interface provides methods for enumerating multimedia device resources.
    /// </summary>
    /// <remarks>
    /// Defined in mmdeviceapi.h
    /// IID: A95664D2-9614-4F35-A746-DE8DB63617E6
    /// </remarks>
    [ComImport]
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceEnumerator
    {
        [PreserveSig]
        int EnumAudioEndpoints(
            [In] EDataFlow dataFlow,
            [In] DEVICE_STATE dwStateMask,
            out IMMDeviceCollection ppDevices);

        [PreserveSig]
        int GetDefaultAudioEndpoint(
            [In] EDataFlow dataFlow,
            [In] ERole role,
            out IMMDevice ppEndpoint);

        [PreserveSig]
        int GetDevice(
            [MarshalAs(UnmanagedType.LPWStr)] [In] string pwstrId,
            out IMMDevice ppDevice);

        [PreserveSig]
        int RegisterEndpointNotificationCallback(
            [In] IMMNotificationClient pClient);

        [PreserveSig]
        int UnregisterEndpointNotificationCallback(
            [In] IMMNotificationClient pClient);
    }

    /// <summary>
    /// The IMMNotificationClient interface provides callbacks for notification of changes in the status of an audio endpoint device.
    /// </summary>
    /// <remarks>
    /// Defined in mmdeviceapi.h
    /// IID: 7991EECB-7F12-4E52-A042-98CEECFD653E
    /// </remarks>
    [ComImport]
    [Guid("7991EECB-7F12-4E52-A042-98CEECFD653E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMNotificationClient
    {
        [PreserveSig]
        int OnDeviceStateChanged(
            [MarshalAs(UnmanagedType.LPWStr)] [In] string pwstrDeviceId,
            [In] DEVICE_STATE dwNewState);

        [PreserveSig]
        int OnDeviceAdded(
            [MarshalAs(UnmanagedType.LPWStr)] [In] string pwstrDeviceId);

        [PreserveSig]
        int OnDeviceRemoved(
            [MarshalAs(UnmanagedType.LPWStr)] [In] string pwstrDeviceId);

        [PreserveSig]
        int OnDefaultDeviceChanged(
            [In] EDataFlow flow,
            [In] ERole role,
            [MarshalAs(UnmanagedType.LPWStr)] [In] string pwstrDefaultDeviceId);

        [PreserveSig]
        int OnPropertyValueChanged(
            [MarshalAs(UnmanagedType.LPWStr)] [In] string pwstrDeviceId,
            [In] PROPERTYKEY key);
    }

    /// <summary>
    /// The IAudioEndpointVolume interface represents the volume controls on the audio stream to or from an audio endpoint device.
    /// </summary>
    /// <remarks>
    /// Defined in Endpointvolume.h
    /// IID: 5CDF2C82-841E-4546-9722-0CF74078229A
    /// </remarks>
    [ComImport]
    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioEndpointVolume
    {
        [PreserveSig]
        int RegisterControlChangeNotify(
            [In] IAudioEndpointVolumeCallback pNotify);

        [PreserveSig]
        int UnregisterControlChangeNotify(
            [In] IAudioEndpointVolumeCallback pNotify);

        [PreserveSig]
        int GetChannelCount(
            out int pnChannelCount);

        [PreserveSig]
        int SetMasterVolumeLevel(
            [In] float fLevelDB,
            [In] ref Guid pguidEventContext);

        [PreserveSig]
        int SetMasterVolumeLevelScalar(
            [In] float fLevel,
            [In] ref Guid pguidEventContext);

        [PreserveSig]
        int GetMasterVolumeLevel(
            out float pfLevelDB);

        [PreserveSig]
        int GetMasterVolumeLevelScalar(
            out float pfLevel);

        [PreserveSig]
        int SetChannelVolumeLevel(
            [In] uint nChannel,
            [In] float fLevelDB,
            [In] ref Guid pguidEventContext);

        [PreserveSig]
        int SetChannelVolumeLevelScalar(
            [In] uint nChannel,
            [In] float fLevel,
            [In] ref Guid pguidEventContext);

        [PreserveSig]
        int GetChannelVolumeLevel(
            [In] uint nChannel,
            out float pfLevelDB);

        [PreserveSig]
        int GetChannelVolumeLevelScalar(
            [In] uint nChannel,
            out float pfLevel);

        [PreserveSig]
        int SetMute(
            [MarshalAs(UnmanagedType.Bool)] [In] bool bMute,
            [In] ref Guid pguidEventContext);

        [PreserveSig]
        int GetMute(
            [MarshalAs(UnmanagedType.Bool)] out bool pbMute);

        [PreserveSig]
        int GetVolumeStepInfo(
            out uint pnStep,
            out uint pnStepCount);

        [PreserveSig]
        int VolumeStepUp(
            [In] ref Guid pguidEventContext);

        [PreserveSig]
        int VolumeStepDown(
            [In] ref Guid pguidEventContext);

        [PreserveSig]
        int QueryHardwareSupport(
            out uint pdwHardwareSupportMask);

        [PreserveSig]
        int GetVolumeRange(
            out float pflVolumeMinDB,
            out float pflVolumeMaxDB,
            out float pflVolumeIncrementDB);
    }

    /// <summary>
    /// The IAudioEndpointVolumeCallback interface provides callbacks for notification of a change in the volume level or muting state of an audio endpoint device.
    /// </summary>
    /// <remarks>
    /// Defined in Endpointvolume.h
    /// IID: 657804FA-D6AD-4496-8A60-352752C48322
    /// </remarks>
    [ComImport]
    [Guid("657804FA-D6AD-4496-8A60-352752C48322")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioEndpointVolumeCallback
    {
        [PreserveSig]
        int OnNotify(
            [In] IntPtr pNotify); // Pointer to AUDIO_VOLUME_NOTIFICATION_DATA
    }

    // --- Supporting Structures and Classes ---

    /// <summary>
    /// Represents a property key.
    /// </summary>
    /// <remarks>
    /// Defined in wtypes.h
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
    }

    /// <summary>
    /// Represents a PROPVARIANT structure.
    /// </summary>
    /// <remarks>
    /// Defined in propvarutil.h
    /// This is a simplified version for common use cases.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public struct PROPVARIANT
    {
        [FieldOffset(0)] public ushort vt;
        [FieldOffset(2)] public ushort wReserved1;
        [FieldOffset(4)] public ushort wReserved2;
        [FieldOffset(6)] public ushort wReserved3;
        [FieldOffset(8)] public sbyte cVal;
        [FieldOffset(8)] public byte bVal;
        [FieldOffset(8)] public short iVal;
        [FieldOffset(8)] public ushort uiVal;
        [FieldOffset(8)] public int lVal;
        [FieldOffset(8)] public uint ulVal;
        [FieldOffset(8)] public long hVal;
        [FieldOffset(8)] public ulong uhVal;
        [FieldOffset(8)] public float fltVal;
        [FieldOffset(8)] public double dblVal;
        [FieldOffset(8)] public short boolVal;
        [FieldOffset(8)] public int scode;
        [FieldOffset(8)] public FILETIME filetime;
        [FieldOffset(8)] public IntPtr pclsid;
        [FieldOffset(8)] public IntPtr pstr;
        [FieldOffset(8)] public IntPtr pguid;
        [FieldOffset(8)] public uint ulint;
        [FieldOffset(8)] public int intVal;
        [FieldOffset(8)] public uint uintVal;
        [FieldOffset(8)] public IntPtr caub;
        [FieldOffset(8)] public IntPtr cal;
        [FieldOffset(8)] public IntPtr caflt;
        [FieldOffset(8)] public IntPtr cadbl;
        [FieldOffset(8)] public IntPtr cabool;
        [FieldOffset(8)] public IntPtr cascode;
        [FieldOffset(8)] public IntPtr capropvar;
        [FieldOffset(8)] public IntPtr pcVal;
        [FieldOffset(8)] public IntPtr pbVal;
        [FieldOffset(8)] public IntPtr piVal;
        [FieldOffset(8)] public IntPtr puiVal;
        [FieldOffset(8)] public IntPtr plVal;
        [FieldOffset(8)] public IntPtr pulVal;
        [FieldOffset(8)] public IntPtr phVal;
        [FieldOffset(8)] public IntPtr puhVal;
        [FieldOffset(8)] public IntPtr pfltVal;
        [FieldOffset(8)] public IntPtr pdblVal;
        [FieldOffset(8)] public IntPtr pboolVal;
        [FieldOffset(8)] public IntPtr pscode;
        [FieldOffset(8)] public IntPtr pfiletime;
        [FieldOffset(8)] public IntPtr pclsidArray;
        [FieldOffset(8)] public IntPtr pstrArray;
        [FieldOffset(8)] public IntPtr pguidArray;
        [FieldOffset(8)] public IntPtr pArray;
        [FieldOffset(8)] public IntPtr pVector;
        [FieldOffset(8)] public IntPtr pUnknown;
        [FieldOffset(8)] public IntPtr pdispVal;
        [FieldOffset(8)] public IntPtr pStream;
        [FieldOffset(8)] public IntPtr pStorage;
        [FieldOffset(8)] public IntPtr pStreamInit;
        [FieldOffset(8)] public IntPtr pStorageInit;
        [FieldOffset(8)] public IntPtr pbstrVal;
        [FieldOffset(8)] public IntPtr pvarVal;
        [FieldOffset(8)] public IntPtr byref;
        [FieldOffset(8)] public byte bstrblobVal;
        [FieldOffset(8)] public byte blobVal;
        [FieldOffset(8)] public byte clipboardData;
        [FieldOffset(8)] public byte decVal;
        [FieldOffset(8)] public byte date;
        [FieldOffset(8)] public byte cyVal;
        [FieldOffset(8)] public byte bstr;
        [FieldOffset(8)] public byte pRecord;
        [FieldOffset(8)] public byte pvoid;
    }

    /// <summary>
    /// The IPropertyStore interface enumerates and retrieves properties from a property store.
    /// </summary>
    /// <remarks>
    /// Defined in propsys.h
    /// IID: 886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99
    /// </remarks>
    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyStore
    {
        [PreserveSig]
        int GetCount(out uint cProps);

        [PreserveSig]
        int GetAt([In] uint iProp, out PROPERTYKEY pkey);

        [PreserveSig]
        int GetValue([In] ref PROPERTYKEY key, out PROPVARIANT pv);

        [PreserveSig]
        int SetValue([In] ref PROPERTYKEY key, [In] ref PROPVARIANT pv);

        [PreserveSig]
        int Commit();
    }

    /// <summary>
    /// Class ID for the MMDeviceEnumerator COM object.
    /// </summary>
    /// <remarks>
    /// CLSID: BCDE0395-E52F-467C-8E3D-C4579291692E
    /// </remarks>
    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    public class MMDeviceEnumeratorComObject
    {
    }

    /// <summary>
    /// Context for COM class instantiation.
    /// </summary>
    /// <remarks>
    /// Defined in wtypesbase.h
    /// </remarks>
    public enum CLSCTX
    {
        CLSCTX_INPROC_SERVER = 0x1,
        CLSCTX_INPROC_HANDLER = 0x2,
        CLSCTX_LOCAL_SERVER = 0x4,
        CLSCTX_INPROC_SERVER16 = 0x8,
        CLSCTX_REMOTE_SERVER = 0x10,
        CLSCTX_INPROC_HANDLER16 = 0x20,
        CLSCTX_RESERVED1 = 0x40,
        CLSCTX_RESERVED2 = 0x80,
        CLSCTX_RESERVED3 = 0x100,
        CLSCTX_RESERVED4 = 0x200,
        CLSCTX_NO_CODE_DOWNLOAD = 0x400,
        CLSCTX_NO_CUSTOM_MARSHAL = 0x800,
        CLSCTX_ENABLE_CODE_DOWNLOAD = 0x1000,
        CLSCTX_NO_FAILURE_LOG = 0x2000,
        CLSCTX_DISABLE_AAA = 0x4000,
        CLSCTX_ENABLE_AAA = 0x8000,
        CLSCTX_FROM_DEFAULT_ACTCTX = 0x10000,
        CLSCTX_ACTIVATE_32_BIT_SERVER = 0x20000,
        CLSCTX_ACTIVATE_64_BIT_SERVER = 0x40000,
        CLSCTX_ENABLE_CLOAKING = 0x100000,
        CLSCTX_APPCONTAINER = 0x400000,
        CLSCTX_ACTIVATE_UNC_SERVER = 0x800000,
        CLSCTX_ALL = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER
    }
}
