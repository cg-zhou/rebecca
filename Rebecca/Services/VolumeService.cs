using System.Runtime.InteropServices;
using Rebecca.Services;

namespace Rebecca.Services
{
    public class VolumeService
    {
        private IAudioEndpointVolume _audioEndpointVolume;
        private Guid _eventContext = Guid.Empty; // Use an empty GUID for the event context

        public VolumeService()
        {
            _audioEndpointVolume = GetAudioEndpointVolume();
        }

        private IAudioEndpointVolume GetAudioEndpointVolume()
        {
            IMMDeviceEnumerator? deviceEnumerator = null;
            IMMDevice? audioDevice = null;
            IAudioEndpointVolume? audioEndpointVolume = null;

            try
            {
                // Create the device enumerator
                deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();

                // Get the default audio endpoint for rendering
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole, out audioDevice);

                // Activate the IAudioEndpointVolume interface
                Guid iid = typeof(IAudioEndpointVolume).GUID;
                audioDevice.Activate(ref iid, (int)CLSCTX.CLSCTX_ALL, IntPtr.Zero, out object o);
                audioEndpointVolume = (IAudioEndpointVolume)o;
            }
            finally
            {
                // Release COM objects
                if (audioDevice != null)
                {
                    Marshal.ReleaseComObject(audioDevice);
                }
                if (deviceEnumerator != null)
                {
                    Marshal.ReleaseComObject(deviceEnumerator);
                }
            }

            return audioEndpointVolume;
        }

        public void VolumeUp()
        {
            if (_audioEndpointVolume != null)
            {
                float currentVolume;
                _audioEndpointVolume.GetMasterVolumeLevelScalar(out currentVolume);
                float newVolume = Math.Min(1.0f, currentVolume + 0.05f); // Increase by 5%, max 100%
                _audioEndpointVolume.SetMasterVolumeLevelScalar(newVolume, ref _eventContext);
            }
        }

        public void VolumeDown()
        {
            if (_audioEndpointVolume != null)
            {
                float currentVolume;
                _audioEndpointVolume.GetMasterVolumeLevelScalar(out currentVolume);
                float newVolume = Math.Max(0.0f, currentVolume - 0.05f); // Decrease by 5%, min 0%
                _audioEndpointVolume.SetMasterVolumeLevelScalar(newVolume, ref _eventContext);
            }
        }

        public void Mute()
        {
            if (_audioEndpointVolume != null)
            {
                bool currentMute;
                _audioEndpointVolume.GetMute(out currentMute);
                _audioEndpointVolume.SetMute(!currentMute, ref _eventContext);
            }
        }
    }
}
