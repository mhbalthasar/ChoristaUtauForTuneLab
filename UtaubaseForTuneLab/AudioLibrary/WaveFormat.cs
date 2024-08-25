using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AudioLibrary.NAudio.Wave
{
    public enum WaveFormatEncoding : ushort
    {
        /// <summary>WAVE_FORMAT_UNKNOWN,	Microsoft Corporation</summary>
        Unknown = 0x0000,

        /// <summary>WAVE_FORMAT_PCM		Microsoft Corporation</summary>
        Pcm = 0x0001,

        /// <summary>WAVE_FORMAT_ADPCM		Microsoft Corporation</summary>
        Adpcm = 0x0002,

        /// <summary>WAVE_FORMAT_IEEE_FLOAT Microsoft Corporation</summary>
        IeeeFloat = 0x0003,

        /// <summary>WAVE_FORMAT_VSELP		Compaq Computer Corp.</summary>
        Vselp = 0x0004,

        /// <summary>WAVE_FORMAT_IBM_CVSD	IBM Corporation</summary>
        IbmCvsd = 0x0005,

        /// <summary>WAVE_FORMAT_ALAW		Microsoft Corporation</summary>
        ALaw = 0x0006,

        /// <summary>WAVE_FORMAT_MULAW		Microsoft Corporation</summary>
        MuLaw = 0x0007,

        /// <summary>WAVE_FORMAT_DTS		Microsoft Corporation</summary>
        Dts = 0x0008,

        /// <summary>WAVE_FORMAT_DRM		Microsoft Corporation</summary>
        Drm = 0x0009,

        /// <summary>WAVE_FORMAT_WMAVOICE9 </summary>
        WmaVoice9 = 0x000A,

        /// <summary>WAVE_FORMAT_OKI_ADPCM	OKI</summary>
        OkiAdpcm = 0x0010,

        /// <summary>WAVE_FORMAT_DVI_ADPCM	Intel Corporation</summary>
        DviAdpcm = 0x0011,

        /// <summary>WAVE_FORMAT_IMA_ADPCM  Intel Corporation</summary>
        ImaAdpcm = DviAdpcm,

        /// <summary>WAVE_FORMAT_MEDIASPACE_ADPCM Videologic</summary>
        MediaspaceAdpcm = 0x0012,

        /// <summary>WAVE_FORMAT_SIERRA_ADPCM Sierra Semiconductor Corp </summary>
        SierraAdpcm = 0x0013,

        /// <summary>WAVE_FORMAT_G723_ADPCM Antex Electronics Corporation </summary>
        G723Adpcm = 0x0014,

        /// <summary>WAVE_FORMAT_DIGISTD DSP Solutions, Inc.</summary>
        DigiStd = 0x0015,

        /// <summary>WAVE_FORMAT_DIGIFIX DSP Solutions, Inc.</summary>
        DigiFix = 0x0016,

        /// <summary>WAVE_FORMAT_DIALOGIC_OKI_ADPCM Dialogic Corporation</summary>
        DialogicOkiAdpcm = 0x0017,

        /// <summary>WAVE_FORMAT_MEDIAVISION_ADPCM Media Vision, Inc.</summary>
        MediaVisionAdpcm = 0x0018,

        /// <summary>WAVE_FORMAT_CU_CODEC Hewlett-Packard Company </summary>
        CUCodec = 0x0019,

        /// <summary>WAVE_FORMAT_YAMAHA_ADPCM Yamaha Corporation of America</summary>
        YamahaAdpcm = 0x0020,

        /// <summary>WAVE_FORMAT_SONARC Speech Compression</summary>
        SonarC = 0x0021,

        /// <summary>WAVE_FORMAT_DSPGROUP_TRUESPEECH DSP Group, Inc </summary>
        DspGroupTrueSpeech = 0x0022,

        /// <summary>WAVE_FORMAT_ECHOSC1 Echo Speech Corporation</summary>
        EchoSpeechCorporation1 = 0x0023,

        /// <summary>WAVE_FORMAT_AUDIOFILE_AF36, Virtual Music, Inc.</summary>
        AudioFileAf36 = 0x0024,

        /// <summary>WAVE_FORMAT_APTX Audio Processing Technology</summary>
        Aptx = 0x0025,

        /// <summary>WAVE_FORMAT_AUDIOFILE_AF10, Virtual Music, Inc.</summary>
        AudioFileAf10 = 0x0026,

        /// <summary>WAVE_FORMAT_PROSODY_1612, Aculab plc</summary>
        Prosody1612 = 0x0027,

        /// <summary>WAVE_FORMAT_LRC, Merging Technologies S.A. </summary>
        Lrc = 0x0028,

        /// <summary>WAVE_FORMAT_DOLBY_AC2, Dolby Laboratories</summary>
        DolbyAc2 = 0x0030,

        /// <summary>WAVE_FORMAT_GSM610, Microsoft Corporation</summary>
        Gsm610 = 0x0031,

        /// <summary>WAVE_FORMAT_MSNAUDIO, Microsoft Corporation</summary>
        MsnAudio = 0x0032,

        /// <summary>WAVE_FORMAT_ANTEX_ADPCME, Antex Electronics Corporation</summary>
        AntexAdpcme = 0x0033,

        /// <summary>WAVE_FORMAT_CONTROL_RES_VQLPC, Control Resources Limited </summary>
        ControlResVqlpc = 0x0034,

        /// <summary>WAVE_FORMAT_DIGIREAL, DSP Solutions, Inc. </summary>
        DigiReal = 0x0035,

        /// <summary>WAVE_FORMAT_DIGIADPCM, DSP Solutions, Inc.</summary>
        DigiAdpcm = 0x0036,

        /// <summary>WAVE_FORMAT_CONTROL_RES_CR10, Control Resources Limited</summary>
        ControlResCr10 = 0x0037,

        /// <summary></summary>
        WAVE_FORMAT_NMS_VBXADPCM = 0x0038, // Natural MicroSystems 
        /// <summary></summary>
        WAVE_FORMAT_CS_IMAADPCM = 0x0039, // Crystal Semiconductor IMA ADPCM 
        /// <summary></summary>
        WAVE_FORMAT_ECHOSC3 = 0x003A, // Echo Speech Corporation 
        /// <summary></summary>
        WAVE_FORMAT_ROCKWELL_ADPCM = 0x003B, // Rockwell International 
        /// <summary></summary>
        WAVE_FORMAT_ROCKWELL_DIGITALK = 0x003C, // Rockwell International 
        /// <summary></summary>
        WAVE_FORMAT_XEBEC = 0x003D, // Xebec Multimedia Solutions Limited 
        /// <summary></summary>
        WAVE_FORMAT_G721_ADPCM = 0x0040, // Antex Electronics Corporation 
        /// <summary></summary>
        WAVE_FORMAT_G728_CELP = 0x0041, // Antex Electronics Corporation 
        /// <summary></summary>
        WAVE_FORMAT_MSG723 = 0x0042, // Microsoft Corporation 
        /// <summary>WAVE_FORMAT_MPEG, Microsoft Corporation </summary>
        Mpeg = 0x0050,

        /// <summary></summary>
        WAVE_FORMAT_RT24 = 0x0052, // InSoft, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_PAC = 0x0053, // InSoft, Inc. 
        /// <summary>WAVE_FORMAT_MPEGLAYER3, ISO/MPEG Layer3 Format Tag </summary>
        MpegLayer3 = 0x0055,

        /// <summary></summary>
        WAVE_FORMAT_LUCENT_G723 = 0x0059, // Lucent Technologies 
        /// <summary></summary>
        WAVE_FORMAT_CIRRUS = 0x0060, // Cirrus Logic 
        /// <summary></summary>
        WAVE_FORMAT_ESPCM = 0x0061, // ESS Technology 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE = 0x0062, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_CANOPUS_ATRAC = 0x0063, // Canopus, co., Ltd. 
        /// <summary></summary>
        WAVE_FORMAT_G726_ADPCM = 0x0064, // APICOM 
        /// <summary></summary>
        WAVE_FORMAT_G722_ADPCM = 0x0065, // APICOM 
        /// <summary></summary>
        WAVE_FORMAT_DSAT_DISPLAY = 0x0067, // Microsoft Corporation 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_BYTE_ALIGNED = 0x0069, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_AC8 = 0x0070, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_AC10 = 0x0071, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_AC16 = 0x0072, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_AC20 = 0x0073, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_RT24 = 0x0074, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_RT29 = 0x0075, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_RT29HW = 0x0076, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_VR12 = 0x0077, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_VR18 = 0x0078, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_TQ40 = 0x0079, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_SOFTSOUND = 0x0080, // Softsound, Ltd. 
        /// <summary></summary>
        WAVE_FORMAT_VOXWARE_TQ60 = 0x0081, // Voxware Inc 
        /// <summary></summary>
        WAVE_FORMAT_MSRT24 = 0x0082, // Microsoft Corporation 
        /// <summary></summary>
        WAVE_FORMAT_G729A = 0x0083, // AT&T Labs, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_MVI_MVI2 = 0x0084, // Motion Pixels 
        /// <summary></summary>
        WAVE_FORMAT_DF_G726 = 0x0085, // DataFusion Systems (Pty) (Ltd) 
        /// <summary></summary>
        WAVE_FORMAT_DF_GSM610 = 0x0086, // DataFusion Systems (Pty) (Ltd) 
        /// <summary></summary>
        WAVE_FORMAT_ISIAUDIO = 0x0088, // Iterated Systems, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_ONLIVE = 0x0089, // OnLive! Technologies, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_SBC24 = 0x0091, // Siemens Business Communications Sys 
        /// <summary></summary>
        WAVE_FORMAT_DOLBY_AC3_SPDIF = 0x0092, // Sonic Foundry 
        /// <summary></summary>
        WAVE_FORMAT_MEDIASONIC_G723 = 0x0093, // MediaSonic 
        /// <summary></summary>
        WAVE_FORMAT_PROSODY_8KBPS = 0x0094, // Aculab plc 
        /// <summary></summary>
        WAVE_FORMAT_ZYXEL_ADPCM = 0x0097, // ZyXEL Communications, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_PHILIPS_LPCBB = 0x0098, // Philips Speech Processing 
        /// <summary></summary>
        WAVE_FORMAT_PACKED = 0x0099, // Studer Professional Audio AG 
        /// <summary></summary>
        WAVE_FORMAT_MALDEN_PHONYTALK = 0x00A0, // Malden Electronics Ltd. 
        /// <summary>WAVE_FORMAT_GSM</summary>
        Gsm = 0x00A1,

        /// <summary>WAVE_FORMAT_G729</summary>
        G729 = 0x00A2,

        /// <summary>WAVE_FORMAT_G723</summary>
        G723 = 0x00A3,

        /// <summary>WAVE_FORMAT_ACELP</summary>
        Acelp = 0x00A4,

        /// <summary>
        /// WAVE_FORMAT_RAW_AAC1
        /// </summary>
        RawAac = 0x00FF,
        /// <summary></summary>
        WAVE_FORMAT_RHETOREX_ADPCM = 0x0100, // Rhetorex Inc. 
        /// <summary></summary>
        WAVE_FORMAT_IRAT = 0x0101, // BeCubed Software Inc. 
        /// <summary></summary>
        WAVE_FORMAT_VIVO_G723 = 0x0111, // Vivo Software 
        /// <summary></summary>
        WAVE_FORMAT_VIVO_SIREN = 0x0112, // Vivo Software 
        /// <summary></summary>
        WAVE_FORMAT_DIGITAL_G723 = 0x0123, // Digital Equipment Corporation 
        /// <summary></summary>
        WAVE_FORMAT_SANYO_LD_ADPCM = 0x0125, // Sanyo Electric Co., Ltd. 
        /// <summary></summary>
        WAVE_FORMAT_SIPROLAB_ACEPLNET = 0x0130, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        WAVE_FORMAT_SIPROLAB_ACELP4800 = 0x0131, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        WAVE_FORMAT_SIPROLAB_ACELP8V3 = 0x0132, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        WAVE_FORMAT_SIPROLAB_G729 = 0x0133, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        WAVE_FORMAT_SIPROLAB_G729A = 0x0134, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        WAVE_FORMAT_SIPROLAB_KELVIN = 0x0135, // Sipro Lab Telecom Inc. 
        /// <summary></summary>
        WAVE_FORMAT_G726ADPCM = 0x0140, // Dictaphone Corporation 
        /// <summary></summary>
        WAVE_FORMAT_QUALCOMM_PUREVOICE = 0x0150, // Qualcomm, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_QUALCOMM_HALFRATE = 0x0151, // Qualcomm, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_TUBGSM = 0x0155, // Ring Zero Systems, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_MSAUDIO1 = 0x0160, // Microsoft Corporation
        /// <summary>
        /// Windows Media Audio, WAVE_FORMAT_WMAUDIO2, Microsoft Corporation
        /// </summary>
        WindowsMediaAudio = 0x0161,

        /// <summary>
        /// Windows Media Audio Professional WAVE_FORMAT_WMAUDIO3, Microsoft Corporation
        /// </summary>
        WindowsMediaAudioProfessional = 0x0162,

        /// <summary>
        /// Windows Media Audio Lossless, WAVE_FORMAT_WMAUDIO_LOSSLESS
        /// </summary>
        WindowsMediaAudioLosseless = 0x0163,

        /// <summary>
        /// Windows Media Audio Professional over SPDIF WAVE_FORMAT_WMASPDIF (0x0164)
        /// </summary>
        WindowsMediaAudioSpdif = 0x0164,

        /// <summary></summary>
        WAVE_FORMAT_UNISYS_NAP_ADPCM = 0x0170, // Unisys Corp. 
        /// <summary></summary>
        WAVE_FORMAT_UNISYS_NAP_ULAW = 0x0171, // Unisys Corp. 
        /// <summary></summary>
        WAVE_FORMAT_UNISYS_NAP_ALAW = 0x0172, // Unisys Corp. 
        /// <summary></summary>
        WAVE_FORMAT_UNISYS_NAP_16K = 0x0173, // Unisys Corp. 
        /// <summary></summary>
        WAVE_FORMAT_CREATIVE_ADPCM = 0x0200, // Creative Labs, Inc 
        /// <summary></summary>
        WAVE_FORMAT_CREATIVE_FASTSPEECH8 = 0x0202, // Creative Labs, Inc 
        /// <summary></summary>
        WAVE_FORMAT_CREATIVE_FASTSPEECH10 = 0x0203, // Creative Labs, Inc 
        /// <summary></summary>
        WAVE_FORMAT_UHER_ADPCM = 0x0210, // UHER informatic GmbH 
        /// <summary></summary>
        WAVE_FORMAT_QUARTERDECK = 0x0220, // Quarterdeck Corporation 
        /// <summary></summary>
        WAVE_FORMAT_ILINK_VC = 0x0230, // I-link Worldwide 
        /// <summary></summary>
        WAVE_FORMAT_RAW_SPORT = 0x0240, // Aureal Semiconductor 
        /// <summary></summary>
        WAVE_FORMAT_ESST_AC3 = 0x0241, // ESS Technology, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_IPI_HSX = 0x0250, // Interactive Products, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_IPI_RPELP = 0x0251, // Interactive Products, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_CS2 = 0x0260, // Consistent Software 
        /// <summary></summary>
        WAVE_FORMAT_SONY_SCX = 0x0270, // Sony Corp. 
        /// <summary></summary>
        WAVE_FORMAT_FM_TOWNS_SND = 0x0300, // Fujitsu Corp. 
        /// <summary></summary>
        WAVE_FORMAT_BTV_DIGITAL = 0x0400, // Brooktree Corporation 
        /// <summary></summary>
        WAVE_FORMAT_QDESIGN_MUSIC = 0x0450, // QDesign Corporation 
        /// <summary></summary>
        WAVE_FORMAT_VME_VMPCM = 0x0680, // AT&T Labs, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_TPC = 0x0681, // AT&T Labs, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_OLIGSM = 0x1000, // Ing C. Olivetti & C., S.p.A. 
        /// <summary></summary>
        WAVE_FORMAT_OLIADPCM = 0x1001, // Ing C. Olivetti & C., S.p.A. 
        /// <summary></summary>
        WAVE_FORMAT_OLICELP = 0x1002, // Ing C. Olivetti & C., S.p.A. 
        /// <summary></summary>
        WAVE_FORMAT_OLISBC = 0x1003, // Ing C. Olivetti & C., S.p.A. 
        /// <summary></summary>
        WAVE_FORMAT_OLIOPR = 0x1004, // Ing C. Olivetti & C., S.p.A. 
        /// <summary></summary>
        WAVE_FORMAT_LH_CODEC = 0x1100, // Lernout & Hauspie 
        /// <summary></summary>
        WAVE_FORMAT_NORRIS = 0x1400, // Norris Communications, Inc. 
        /// <summary></summary>
        WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS = 0x1500, // AT&T Labs, Inc. 

        /// <summary>
        /// Advanced Audio Coding (AAC) audio in Audio Data Transport Stream (ADTS) format.
        /// The format block is a WAVEFORMATEX structure with wFormatTag equal to WAVE_FORMAT_MPEG_ADTS_AAC.
        /// </summary>
        /// <remarks>
        /// The WAVEFORMATEX structure specifies the core AAC-LC sample rate and number of channels, 
        /// prior to applying spectral band replication (SBR) or parametric stereo (PS) tools, if present.
        /// No additional data is required after the WAVEFORMATEX structure.
        /// </remarks>
        /// <see>http://msdn.microsoft.com/en-us/library/dd317599%28VS.85%29.aspx</see>
        MPEG_ADTS_AAC = 0x1600,

        /// <summary></summary>
        /// <remarks>Source wmCodec.h</remarks>
        MPEG_RAW_AAC = 0x1601,

        /// <summary>
        /// MPEG-4 audio transport stream with a synchronization layer (LOAS) and a multiplex layer (LATM).
        /// The format block is a WAVEFORMATEX structure with wFormatTag equal to WAVE_FORMAT_MPEG_LOAS.
        /// </summary>
        /// <remarks>
        /// The WAVEFORMATEX structure specifies the core AAC-LC sample rate and number of channels, 
        /// prior to applying spectral SBR or PS tools, if present.
        /// No additional data is required after the WAVEFORMATEX structure.
        /// </remarks>
        /// <see>http://msdn.microsoft.com/en-us/library/dd317599%28VS.85%29.aspx</see>
        MPEG_LOAS = 0x1602,

        /// <summary>NOKIA_MPEG_ADTS_AAC</summary>
        /// <remarks>Source wmCodec.h</remarks>
        NOKIA_MPEG_ADTS_AAC = 0x1608,

        /// <summary>NOKIA_MPEG_RAW_AAC</summary>
        /// <remarks>Source wmCodec.h</remarks>
        NOKIA_MPEG_RAW_AAC = 0x1609,

        /// <summary>VODAFONE_MPEG_ADTS_AAC</summary>
        /// <remarks>Source wmCodec.h</remarks>
        VODAFONE_MPEG_ADTS_AAC = 0x160A,

        /// <summary>VODAFONE_MPEG_RAW_AAC</summary>
        /// <remarks>Source wmCodec.h</remarks>
        VODAFONE_MPEG_RAW_AAC = 0x160B,

        /// <summary>
        /// High-Efficiency Advanced Audio Coding (HE-AAC) stream.
        /// The format block is an HEAACWAVEFORMAT structure.
        /// </summary>
        /// <see>http://msdn.microsoft.com/en-us/library/dd317599%28VS.85%29.aspx</see>
        MPEG_HEAAC = 0x1610,

        /// <summary>WAVE_FORMAT_DVM</summary>
        WAVE_FORMAT_DVM = 0x2000, // FAST Multimedia AG 

        // others - not from MS headers
        /// <summary>WAVE_FORMAT_VORBIS1 "Og" Original stream compatible</summary>
        Vorbis1 = 0x674f,

        /// <summary>WAVE_FORMAT_VORBIS2 "Pg" Have independent header</summary>
        Vorbis2 = 0x6750,

        /// <summary>WAVE_FORMAT_VORBIS3 "Qg" Have no codebook header</summary>
        Vorbis3 = 0x6751,

        /// <summary>WAVE_FORMAT_VORBIS1P "og" Original stream compatible</summary>
        Vorbis1P = 0x676f,

        /// <summary>WAVE_FORMAT_VORBIS2P "pg" Have independent headere</summary>
        Vorbis2P = 0x6770,

        /// <summary>WAVE_FORMAT_VORBIS3P "qg" Have no codebook header</summary>
        Vorbis3P = 0x6771,

        /// <summary>WAVE_FORMAT_EXTENSIBLE</summary>
        Extensible = 0xFFFE, // Microsoft 
        /// <summary></summary>
        WAVE_FORMAT_DEVELOPMENT = 0xFFFF,
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public class WaveFormat
    {
        /// <summary>format type</summary>
        protected WaveFormatEncoding waveFormatTag;
        /// <summary>number of channels</summary>
        protected short channels;
        /// <summary>sample rate</summary>
        protected int sampleRate;
        /// <summary>for buffer estimation</summary>
        protected int averageBytesPerSecond;
        /// <summary>block size of data</summary>
        protected short blockAlign;
        /// <summary>number of bits per sample of mono data</summary>
        protected short bitsPerSample;
        /// <summary>number of following bytes</summary>
        protected short extraSize;

        /// <summary>
        /// Creates a new PCM 44.1Khz stereo 16 bit format
        /// </summary>
        public WaveFormat() : this(44100, 16, 2)
        {

        }

        /// <summary>
        /// Creates a new 16 bit wave format with the specified sample
        /// rate and channel count
        /// </summary>
        /// <param name="sampleRate">Sample Rate</param>
        /// <param name="channels">Number of channels</param>
        public WaveFormat(int sampleRate, int channels)
            : this(sampleRate, 16, channels)
        {
        }

        /// <summary>
        /// Gets the size of a wave buffer equivalent to the latency in milliseconds.
        /// </summary>
        /// <param name="milliseconds">The milliseconds.</param>
        /// <returns></returns>
        public int ConvertLatencyToByteSize(int milliseconds)
        {
            int bytes = (int)((AverageBytesPerSecond / 1000.0) * milliseconds);
            if ((bytes % BlockAlign) != 0)
            {
                // Return the upper BlockAligned
                bytes = bytes + BlockAlign - (bytes % BlockAlign);
            }
            return bytes;
        }

        /// <summary>
        /// Creates a WaveFormat with custom members
        /// </summary>
        /// <param name="tag">The encoding</param>
        /// <param name="sampleRate">Sample Rate</param>
        /// <param name="channels">Number of channels</param>
        /// <param name="averageBytesPerSecond">Average Bytes Per Second</param>
        /// <param name="blockAlign">Block Align</param>
        /// <param name="bitsPerSample">Bits Per Sample</param>
        /// <returns></returns>
        public static WaveFormat CreateCustomFormat(WaveFormatEncoding tag, int sampleRate, int channels, int averageBytesPerSecond, int blockAlign, int bitsPerSample)
        {
            WaveFormat waveFormat = new WaveFormat();
            waveFormat.waveFormatTag = tag;
            waveFormat.channels = (short)channels;
            waveFormat.sampleRate = sampleRate;
            waveFormat.averageBytesPerSecond = averageBytesPerSecond;
            waveFormat.blockAlign = (short)blockAlign;
            waveFormat.bitsPerSample = (short)bitsPerSample;
            waveFormat.extraSize = 0;
            return waveFormat;
        }

        /// <summary>
        /// Creates an A-law wave format
        /// </summary>
        /// <param name="sampleRate">Sample Rate</param>
        /// <param name="channels">Number of Channels</param>
        /// <returns>Wave Format</returns>
        public static WaveFormat CreateALawFormat(int sampleRate, int channels)
        {
            return CreateCustomFormat(WaveFormatEncoding.ALaw, sampleRate, channels, sampleRate * channels, channels, 8);
        }

        /// <summary>
        /// Creates a Mu-law wave format
        /// </summary>
        /// <param name="sampleRate">Sample Rate</param>
        /// <param name="channels">Number of Channels</param>
        /// <returns>Wave Format</returns>
        public static WaveFormat CreateMuLawFormat(int sampleRate, int channels)
        {
            return CreateCustomFormat(WaveFormatEncoding.MuLaw, sampleRate, channels, sampleRate * channels, channels, 8);
        }

        /// <summary>
        /// Creates a new PCM format with the specified sample rate, bit depth and channels
        /// </summary>
        public WaveFormat(int rate, int bits, int channels)
        {
            if (channels < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(channels), "Channels must be 1 or greater");
            }
            // minimum 16 bytes, sometimes 18 for PCM
            waveFormatTag = WaveFormatEncoding.Pcm;
            this.channels = (short)channels;
            sampleRate = rate;
            bitsPerSample = (short)bits;
            extraSize = 0;

            blockAlign = (short)(channels * (bits / 8));
            averageBytesPerSecond = this.sampleRate * this.blockAlign;
        }

        /// <summary>
        /// Creates a new 32 bit IEEE floating point wave format
        /// </summary>
        /// <param name="sampleRate">sample rate</param>
        /// <param name="channels">number of channels</param>
        public static WaveFormat CreateIeeeFloatWaveFormat(int sampleRate, int channels)
        {
            var wf = new WaveFormat();
            wf.waveFormatTag = WaveFormatEncoding.IeeeFloat;
            wf.channels = (short)channels;
            wf.bitsPerSample = 32;
            wf.sampleRate = sampleRate;
            wf.blockAlign = (short)(4 * channels);
            wf.averageBytesPerSecond = sampleRate * wf.blockAlign;
            wf.extraSize = 0;
            return wf;
        }

        /// <summary>
        /// Helper function to retrieve a WaveFormat structure from a pointer
        /// </summary>
        /// <param name="pointer">WaveFormat structure</param>
        /// <returns></returns>
        public static WaveFormat MarshalFromPtr(IntPtr pointer)
        {
            var waveFormat = Marshal.PtrToStructure<WaveFormat>(pointer);
            switch (waveFormat.Encoding)
            {
                case WaveFormatEncoding.Pcm:
                    // can't rely on extra size even being there for PCM so blank it to avoid reading
                    // corrupt data
                    waveFormat.extraSize = 0;
                    break;
                default:
                    if (waveFormat.ExtraSize > 0)
                    {
                        waveFormat = Marshal.PtrToStructure<WaveFormatExtraData>(pointer);
                    }
                    break;
            }
            return waveFormat;
        }

        /// <summary>
        /// Helper function to marshal WaveFormat to an IntPtr
        /// </summary>
        /// <param name="format">WaveFormat</param>
        /// <returns>IntPtr to WaveFormat structure (needs to be freed by callee)</returns>
        public static IntPtr MarshalToPtr(WaveFormat format)
        {
            int formatSize = Marshal.SizeOf(format);
            IntPtr formatPointer = Marshal.AllocHGlobal(formatSize);
            Marshal.StructureToPtr(format, formatPointer, false);
            return formatPointer;
        }

        /// <summary>
        /// Reads in a WaveFormat (with extra data) from a fmt chunk (chunk identifier and
        /// length should already have been read)
        /// </summary>
        /// <param name="br">Binary reader</param>
        /// <param name="formatChunkLength">Format chunk length</param>
        /// <returns>A WaveFormatExtraData</returns>
        public static WaveFormat FromFormatChunk(BinaryReader br, int formatChunkLength)
        {
            var waveFormat = new WaveFormatExtraData();
            waveFormat.ReadWaveFormat(br, formatChunkLength);
            waveFormat.ReadExtraData(br);
            return waveFormat;
        }

        private void ReadWaveFormat(BinaryReader br, int formatChunkLength)
        {
            if (formatChunkLength < 16)
                throw new InvalidDataException("Invalid WaveFormat Structure");
            waveFormatTag = (WaveFormatEncoding)br.ReadUInt16();
            channels = br.ReadInt16();
            sampleRate = br.ReadInt32();
            averageBytesPerSecond = br.ReadInt32();
            blockAlign = br.ReadInt16();
            bitsPerSample = br.ReadInt16();
            if (formatChunkLength > 16)
            {
                extraSize = br.ReadInt16();
                if (extraSize != formatChunkLength - 18)
                {
                    Debug.WriteLine("Format chunk mismatch");
                    extraSize = (short)(formatChunkLength - 18);
                }
            }
        }

        /// <summary>
        /// Reads a new WaveFormat object from a stream
        /// </summary>
        /// <param name="br">A binary reader that wraps the stream</param>
        public WaveFormat(BinaryReader br)
        {
            int formatChunkLength = br.ReadInt32();
            ReadWaveFormat(br, formatChunkLength);
        }

        /// <summary>
        /// Reports this WaveFormat as a string
        /// </summary>
        /// <returns>String describing the wave format</returns>
        public override string ToString()
        {
            switch (waveFormatTag)
            {
                case WaveFormatEncoding.Pcm:
                case WaveFormatEncoding.Extensible:
                    // extensible just has some extra bits after the PCM header
                    return $"{bitsPerSample} bit PCM: {sampleRate}Hz {channels} channels";
                case WaveFormatEncoding.IeeeFloat:
                    return $"{bitsPerSample} bit IEEFloat: {sampleRate}Hz {channels} channels";
                default:
                    return waveFormatTag.ToString();
            }
        }

        /// <summary>
        /// Compares with another WaveFormat object
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if the objects are the same</returns>
        public override bool Equals(object obj)
        {
            var other = obj as WaveFormat;
            if (other != null)
            {
                return waveFormatTag == other.waveFormatTag &&
                    channels == other.channels &&
                    sampleRate == other.sampleRate &&
                    averageBytesPerSecond == other.averageBytesPerSecond &&
                    blockAlign == other.blockAlign &&
                    bitsPerSample == other.bitsPerSample;
            }
            return false;
        }

        /// <summary>
        /// Provides a Hashcode for this WaveFormat
        /// </summary>
        /// <returns>A hashcode</returns>
        public override int GetHashCode()
        {
            return (int)waveFormatTag ^
                (int)channels ^
                sampleRate ^
                averageBytesPerSecond ^
                (int)blockAlign ^
                (int)bitsPerSample;
        }

        /// <summary>
        /// Returns the encoding type used
        /// </summary>
        public WaveFormatEncoding Encoding => waveFormatTag;

        /// <summary>
        /// Writes this WaveFormat object to a stream
        /// </summary>
        /// <param name="writer">the output stream</param>
        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write((int)(18 + extraSize)); // wave format length
            writer.Write((short)Encoding);
            writer.Write((short)Channels);
            writer.Write((int)SampleRate);
            writer.Write((int)AverageBytesPerSecond);
            writer.Write((short)BlockAlign);
            writer.Write((short)BitsPerSample);
            writer.Write((short)extraSize);
        }

        /// <summary>
        /// Returns the number of channels (1=mono,2=stereo etc)
        /// </summary>
        public int Channels => channels;

        /// <summary>
        /// Returns the sample rate (samples per second)
        /// </summary>
        public int SampleRate => sampleRate;

        /// <summary>
        /// Returns the average number of bytes used per second
        /// </summary>
        public int AverageBytesPerSecond => averageBytesPerSecond;

        /// <summary>
        /// Returns the block alignment
        /// </summary>
        public virtual int BlockAlign => blockAlign;

        /// <summary>
        /// Returns the number of bits per sample (usually 16 or 32, sometimes 24 or 8)
        /// Can be 0 for some codecs
        /// </summary>
        public int BitsPerSample => bitsPerSample;

        /// <summary>
        /// Returns the number of extra bytes used by this waveformat. Often 0,
        /// except for compressed formats which store extra data after the WAVEFORMATEX header
        /// </summary>
        public int ExtraSize => extraSize;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public class WaveFormatExtraData : WaveFormat
    {
        // try with 100 bytes for now, increase if necessary
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        private byte[] extraData = new byte[100];

        /// <summary>
        /// Allows the extra data to be read
        /// </summary>
        public byte[] ExtraData => extraData;

        /// <summary>
        /// parameterless constructor for marshalling
        /// </summary>
        internal WaveFormatExtraData()
        {
        }

        /// <summary>
        /// Reads this structure from a BinaryReader
        /// </summary>
        public WaveFormatExtraData(BinaryReader reader)
            : base(reader)
        {
            ReadExtraData(reader);
        }

        internal void ReadExtraData(BinaryReader reader)
        {
            if (this.extraSize > 0)
            {
                reader.Read(extraData, 0, extraSize);
            }
        }

        /// <summary>
        /// Writes this structure to a BinaryWriter
        /// </summary>
        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            if (extraSize > 0)
            {
                writer.Write(extraData, 0, extraSize);
            }
        }
    }
}