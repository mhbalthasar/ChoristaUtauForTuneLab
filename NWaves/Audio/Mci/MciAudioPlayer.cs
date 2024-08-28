﻿using System;
using System.Text;
using System.Threading.Tasks;
using NWaves.Audio.Interfaces;
using NWaves.Signals;

namespace NWaves.Audio.Mci
{
    /// <summary>
    /// <para>Audio player based on MCI.</para>
    /// <para>
    /// MciAudioPlayer works only on Windows, since it uses winmm.dll and MCI commands.
    /// </para>
    /// <para>
    /// MciAudioPlayer lets MCI do all the heavy-lifting with sound playback. 
    /// It launches MCI command and just awaits for amount of time 
    /// corresponding to the duration of a given segment.
    /// </para>
    /// </summary>
    public class MciAudioPlayer : IAudioPlayer
    {
        /// <summary>
        /// Hidden alias for an MCI waveaudio device.
        /// </summary>
        private string _alias;

        /// <summary>
        /// Duration of pause in milliseconds.
        /// </summary>
        private int _pauseDuration;

        /// <summary>
        /// The exact time when playback was paused.
        /// </summary>
        private DateTime _pauseTime;

        /// <summary>
        /// The flag indicating whether audio playback is currently paused.
        /// </summary>
        private bool _isPaused;

        /// <summary>
        /// Gets or sets audio volume (measured in percents from the range [0..1]).
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// Plays audio contained in WAV file asynchronously.
        /// </summary>
        /// <param name="source">Path to WAV file to play</param>
        /// <param name="startPos">Index of the first sample to play</param>
        /// <param name="endPos">Index of the last sample to play</param>
        public async Task PlayAsync(string source, int startPos = 0, int endPos = -1)
        {
            if (_isPaused)
            {
                Resume();
                return;
            }

            Stop();

            _alias = Guid.NewGuid().ToString();

            var mciCommand = string.Format("open \"{0}\" type waveaudio alias {1}", source, _alias);
            Mci.SendString(mciCommand, null, 0, 0);

            mciCommand = string.Format("set {0} time format samples", _alias);
            Mci.SendString(mciCommand, null, 0, 0);

            var durationBuffer = new StringBuilder(255);
            mciCommand = string.Format("status {0} length", _alias);
            Mci.SendString(mciCommand, durationBuffer, 255, 0);
            var duration = int.Parse(durationBuffer.ToString());

            var samplingRateBuffer = new StringBuilder(255);
            mciCommand = string.Format("status {0} samplespersec", _alias);
            Mci.SendString(mciCommand, samplingRateBuffer, 255, 0);
            var samplingRate = int.Parse(samplingRateBuffer.ToString());

            mciCommand = string.Format("play {2} from {0} to {1} notify", startPos, endPos, _alias);
            mciCommand = mciCommand.Replace(" to -1", "");
            Mci.SendString(mciCommand, null, 0, 0);

            // ======= here's how we do asynchrony with old technology from 90's )) ========

            var currentAlias = _alias;

            await Task.Delay((int)(duration * 1000.0 / samplingRate));

            // During this time someone could Pause player.
            // In this case we add pause duration to awaiting time.

            while (_isPaused || _pauseDuration > 0)
            {
                // first, we check if the pause is right now
                if (_isPaused)
                { 
                    // then just await one second more
                    // (the stupidest wait spin I wrote in years ))))
                    await Task.Delay(1000);
                }

                if (_pauseDuration > 0)
                {
                    await Task.Delay(_pauseDuration);

                    _pauseDuration = 0;
                }
            }

            // During this time someone could stop and run player again, so _alias is already different.
            // In this case we don't stop player here because it was stopped some time before.

            if (currentAlias == _alias)
            {
                Stop();
            }
            // =============================================================================
        }

        /// <summary>
        /// <para><see cref="MciAudioPlayer"/> does not implement this method.</para>
        /// <para>
        /// Unfortunately, MCI does not provide means for playing audio from buffers in memory. 
        /// Moreover, since NWaves library is portable, there's even no easy way to write the buffer 
        /// into temporary file and play it here (it could be a workaround for the problem).
        /// </para>
        /// </summary>
        /// <param name="signal">Signal to play</param>
        /// <param name="startPos">Index of the first sample to play</param>
        /// <param name="endPos">Index of the last sample to play</param>
        /// <param name="bitDepth">Number of bits per one sample</param>
        public Task PlayAsync(DiscreteSignal signal, int startPos = 0, int endPos = -1, short bitDepth = 16)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Pauses playing audio.
        /// </summary>
        public void Pause()
        {
            if (_alias is null)
            {
                return;
            }

            var mciCommand = string.Format("pause {0}", _alias);
            Mci.SendString(mciCommand, null, 0, 0);

            // If the playback was paused, the player memorizes how many milliseconds 
            // it was "idle" and then adds this time to the total awaiting time.

            _pauseTime = DateTime.Now;
            _isPaused = true;
        }

        /// <summary>
        /// Resumes playing audio.
        /// </summary>
        public void Resume()
        {
            if (_alias is null || !_isPaused)
            {
                return;
            }

            var mciCommand = string.Format("resume {0}", _alias);
            Mci.SendString(mciCommand, null, 0, 0);

            var pause = DateTime.Now - _pauseTime;

            _pauseDuration += pause.Duration().Seconds * 1000 + pause.Duration().Milliseconds;
            _isPaused = false;
        }

        /// <summary>
        /// Stops playing audio and close MCI device.
        /// </summary>
        public void Stop()
        {
            if (_alias is null)
            {
                return;
            }

            if (_isPaused)
            {
                Resume();
            }

            var mciCommand = string.Format("stop {0}", _alias);
            Mci.SendString(mciCommand, null, 0, 0);

            mciCommand = string.Format("close {0}", _alias);
            Mci.SendString(mciCommand, null, 0, 0);

            _alias = null;
        }
    }
}
