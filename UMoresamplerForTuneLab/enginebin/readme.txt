Moresampler 0.8.3 - More than a resampler.
Copyright (C) 2015-2017, Kanru Hua (a.k.a. "Sleepwalking").

Moresampler is a synthesis backend for singing voice synthesis program UTAU. Literally
  the name suggests that Moresampler is not only a UTAU resampler. In fact, it is
  a resampler, a wavtool and an automatic voicebank configurator combined in one executable.

Website: http://web.engr.illinois.edu/~khua5/index.php/moresampler/
The complete Moresampler tutorial:
  https://web.engr.illinois.edu/~khua5/index.php/2016/04/07/the-complete-moresampler-tutorial/

Contact the author
===

Email: k.hua.kanru [at] ieee.org

Update log
===

0.8.3 (Jun. 10, 2017)
* Bug fix: occasional crashes when analyzing samples with large chunk of silence.
* Bug fix: occasional crashes when analyzing short samples or samples that do
  not contain any vowel.

0.8.2 (May. 15, 2017)
* Bug fix: the oto generator fails to recognize arpabet phoneme "zh".
* Bug fix: display error message (instead of crashing) when launched outside of
  host software.
* Improved feature: improve pitch accuracy under modulation effect.
* New feature: new flag 'Mp' added for adding a random perturbation to pitch curve.
  The parameter, ranged from 0 to 100, controls the degree of such perturbation and
  is set to 0 by default.

0.8.1 (Mar. 29, 2017)
* Bug fix: a severe bug in the recently upgraded pitch estimator

0.8.0 (Feb. 24, 2017)
* Bug fix: crashes on u-flag-enabled notes.
* Bug fix: occasional crashes during pitch shifting.
* Bug fix: format identification error on float point .wav files.
* Bug fix: short consonants are missing after concatenation.
* Bug fix: "ja", "jo", "ju" are mis-aliased as "じぁ", "じぉ", "じぅ" during romaji to
  hiragana conversion.
* Improved feature: the pitch estimator has been extensively improved. It now makes
  fewer voicng detection errors & fine error.
* New feature: the oto generator now supports Arpasing.

0.7.2 (Oct. 26, 2016)
* This version has undergone some code refactoring on oto-generation. Part of
  the code was rewritten in Lua. Arpasing support is not yet available (work
  in progress).
* Bug fix: note intensity parameter is ignored when utau-style-normalization is off.
* Bug fix: a feature introduced in March 2016 could potentially reduce time-
  resolution of the analysis algorithm; the feature is now disabled by default unless
  analysis-anti-distortion is on.
* Improved feature: oto generation is improved on small voicebanks.
* Improved feature: oto generator can create hiragana entries from voicebanks whose
  filenames are in romaji.
* New feature: oto generator now first loads "index.csv" if available, then loads
  the .wav files in the list.
* New feature: oto generator can update the entries in an existing oto.ini file
  without creating new entries.
* New feature: oto generator can create CVVC/VCV-style entries.

0.7.1 (Aug. 22, 2016)
* This version has undergone some code refactoring on math-related functions.
* Bug fix: occasional glitches at concatenation boundaries.
* Bug fix: pops caused by setting Mr flag to a negative value.
* Bug fix: wrong offset of notes with 'u' flag enabled.
* Improved feature: retrained the text-speech alignment model with better
  configuration. Oto generation accuracy has been improved.
* Improved feature: avoid denormal floating point numbers which may slow down
  synthesis by 1000% when 'u' flag is on.
* New feature: oto generation mode now supports romaji voicebanks.

0.7.0 (Jun. 7, 2016)
* Improved feature: the effect of 'Mt' flag is improved on sharp/intense voices.
* Improved feature: prevent glitches at note boundaries introduced in wavtool mode.
* New feature: drag a voicebank folder onto moresampler.exe to automatically analyze
  the audio and generate oto.ini. This is an experimental feature which still has
  lots of things to improve. Currently only continuous-speech Japanese voicebanks
  with filenames written in Hiragana and/or Katakana are supported. The folder should
  contain .wav files and those in the subfolders won't be loaded. The output is
  VCV-style oto.ini. Please backup the oto.ini file before using this feature.
* New feature: added 'Mr' flag which creates a "singer's formant" around 3kHz.
  Range: [-100, 100], real number; default: 0.
* New feature: Moresampler now supports .aiff/.aif files as input.
* New configuration & feature:
  meta-flag-1/2/3/4/...  This feature allows customization of flags. Multiple flags
    can be combined into one meta flag which saves effort when typing flag sequences
    in UTAU's note settings panel. Meta flags are activatived in the format
    M+number (e.g. M1, M2, M3). By putting dot and a number after a meta flag,
    the effectiveness can be scaled by the number (as long as the result of scaling
    is still within the allowed range of each flag). For example, the following
    configuration
      meta-flag-1 MG50MD30MC20Mb30Mt50
    and flag sequence 'eMo20M1.50' expands to 'eMo20MG25MD15MC10Mb15Mt25'.
  analysis-suppress-subharmonics on/off When set to "on", automatically remove the
    subharmonics (if there are any) from input speech during analysis. This might
    be helpful for screamy voices but slightly degrades the quality of breathy voices.

0.6.4 (May. 5, 2016)
* Bug fix: occasional crashes with "load-frq on/strict" option.
* Bug fix: pitch estimation errors caused by an improper library call.
* Bug fix: "$direct=true" and 'u' flag give results inconsistent with the default wavtool.

0.6.3 (Apr. 18, 2016)
* Bug fix: crashes when 'MC' flag is applied on a note whose pitch is shifted down.
* Bug fix: crashes when "overlap" note property is negative.
* Bug fix: not responding to frqeditor's "initialize frequency map" call.

0.6.2 (Apr. 16, 2016)
* Improved feature: pitch estimation error rate has been reduced by ten folds.
* New feature: support UTAU's "$direct=true" note setting, which let the sample
  bypass modifications, going directly to output. Useful for sound effects such
  as breathing. Notice: Moresampler won't render if the last note in the selected
  region has "$direct=true"; unfortunately this problem is unsolvable due to the
  design of UTAU <-> resampler & wavtool interface.
* New feature: added 'u' flag, which has the same effect as "$direct=true" but
  provides a workaround to the above problem. 'u' means 'unmodified/untouched'.
* New feature: 3 Moresampler Extension Flags for growling-like effects were added.
    MC: coarseness, range: [0, 100], real number
    MG: growl, range: [0, 100], real number
    MD: distortion, range: [0, 100], real number

0.6.1 (Apr. 7, 2016)
* Bug fix: ending of notes before 'R' are sometimes truncated (wavtool mode).
* Bug fix: project flags can't be overwritten by flags on notes.
* Improved feature: updated mrq file format.
* New configuration & feature:
   auto-update-llsm-mrq on/off  Check the last modified time of .wav file and
     corresponding .llsm file and mrq data entry. If the .wav file is newer than
     the .llsm file, then reanalyze. If the .wav file is also newer than the mrq
     data entry, then re-estimate pitch before reanalyzing .llsm.

0.6.0 (Mar. 30, 2016)
* Bug fix: UAC warning window keep showing up for 64bit version.
* Bug fix: timing error for the first note in wavtool mode.
* Bug fix: 'b' flag sometimes can't suppress the consonant.
* Bug fix: potential (not very likely) crashes in resampler mode.
* Improved feature: prevent Moresampler from re-synthesizing when no note nor the
  project has been modified.
* Improved feature: more robust pitch estimation.
* Improved feature: better aperiodic component extraction for inharmonic input.
* Improved feature: better stability on breathy voices.
* New feature: 8 Moresampler Extension Flags (Abbr: MEF) were added.
    Mt: tenseness, range: [-100, 100], real number
    Mb: breathiness, range: [-100, 100], real number
    Mo: openness, range: [-100, 100], real number
    Md: dryness, range: [-100, 100], real number
    ME: formant emphasis, range: [-100, 100], real number
    Mm: interpolating between the classical speech model used before version 0.3.0
      and the novel model since 0.3.0, range: [0, 100], real number, default: 100
    Ms: stabilization, range: [0, 10], integer
    Me: the force looping flag ':e' introduced in 0.5.0 has been renamed to 'Me'
  For more information on MEFs, please visit tutorial page.
* New feature: support 'P' flag, which adjusts the degree of amplitude
  normalization. This flag has range [0, 100] and is only effective when
  synthesis-utau-style-normalization is 'full' or 'voiced'. Default value is 86,
  being compatible with resampler.exe.
* New configuration & feature:
    analysis-f0-range-from-path on/off  automatically infer pitch range of the
      sample from the name of its parent directory. For example, a .wav file under
      'C_D4' has pitch range around 294Hz.

0.5.0 (Mar. 1, 2016)
* Lots of optimization without compromising on quality.
  Moresampler 0.5.0 runs 500% faster than 0.3.1; analysis also gets 200% faster.
  Size of .llsm files is reduced to about the same as (or smaller than) the
  corresponding .wav file.
  Memory consumption is also reduced (in wavtool mode).
* From now on, both 32-bit and 64-bit versions are released.
* Bug fix: potential spectral distortion when input pitch is lower than 100Hz.
* Bug fix: invalid memory access in wavtool mode.
* New feature: support 'e' and ':e' flags, which are "force stretching" and
  "force looping" respectively. These flags have no parameter associated.
* New feature: support 'b' flag, which amplifies or attenuates unvoiced consonants
  (e.g. /t/ /k/ /s/) by a factor of 0.05 times the number after b. Example: b20
  amplifies the unvoiced consonant by 200%; b-20 completely removes the unvoiced
  consonant. Range: [-20, 100], real number
* New feature: support 'A' flag, which modulates the amplitude of voiced parts with
  regard to pitch change. The number after 'A' specifies the degree to which amplitude
  change correlates with pitch fluctuation. Can be used to enrich vibrato effects.
  Range: [-100, 100], real number
* New configuration & feature:
    multithread-synthesis on/full/off   when turned on, the final synthesis stage
      in wavtool mode will run in multiple threads (which means faster). When set
      to "full", resampler mode will also become multithreaded. This feature is
      inherently different from "multiprocess" which works by launching multiple
      instances of Moresampler at a time.

0.3.1 (Feb. 7, 2016)
* Bug fix: loss of temporal details of aperiodic component in wavtool mode.
* New feature: frqeditor support.
* New feature: multi-process analysis and synthesis support.
* New configuration & feature:
    analysis-anti-distortion on/off     when turned on, Moresampler will automatically
      fix analysis inaccuracy caused by noise distortion or low volume (quantization
      error) which may result in "sharp", "gross" voice after pitch shifting.
      However turning on this feature may (in theory) slightly blur the speech.

0.3.0 (Jan. 29, 2016)
* A 50% rewrite of the whole project, including a major upgrade to the algorithm
  and a major upgrade to the internal data structure.
* New feature: before creating .llsm files, Moresampler now first scans for desc.mrq
  whose function is similar to .frq file (i.e. it is a pitch table which can be
  manually edited by some tool). When desc.mrq is not detected or corresponding
  data entry is not found in an existing desc.mrq file, Moresampler will generate
  what is missing. The graphical editor for .mrq files is still under development.
* Improved feature: more formatted log file for diagnostics & debugging.

0.2.3 (Jan. 4, 2016)
* Bug fix: crashes caused by memory overflow when frequency upperbound is greater than
  sampling rate.
* Bug fix: incorrect interpolation at the boundary between voiced and unvoiced frames.

0.2.2 (Jan. 1, 2016)
* Bug fix: clips produced under resampler mode when output sampling rate is greater than
  analysis sampling rate.

0.2.1 (Dec. 15, 2015)
* Bug fix: synthesis mode (under wavtool mode) not triggered for usts with lots
  of trailing R notes.
* Bug fix: disproportional gain after pitch shifting.
* Bug fix: a minor bug in wav output.
* Bug fix: maximum noise frequency may be different among llsm files.
* Improved feature: higher time resolution for sinusoidal analysis.

0.2.0 (Nov. 8, 2015)
* Bug fix: crashes when loading stereo wav file.
* Bug fix: fails to load a particular type of wav file.
* Bug fix: crashes when encountering pitchbends in (presumably) non-standard format.
* Bug fix: non-ascii characters disappear in log file.
* Bug fix: incorrect gain for noise component when synthesis-utau-style-normalization
    is enabled.
* Bug fix: inaccurate total time calculation which occasionally cause crashes in wavtool mode.
* Bug fix: occasional wrong time calculation for the first few segments in wavtool mode.
* Bug fix: cannot output 8/24/32-bit wav file in wavtool mode.
* Bug fix: F0 is not interpolated in wavtool mode.
* Bug fix: discontinuities and wrong fading of noise component introduced in wavtool mode.
* Bug fix: re-estimated F0 is not utilized.
* Improved feature: more robust pitch estimation.
* Improved feature: better phase alignment.
* Improved feature: better noise energy handling.
* New feature: support UTAU's "modulation" parameter.
* New feature: keep track of system time (UTC) when appending to log file.
* New feature: support pitch adjustment flag 't'.
* New feature: support gender flag 'g' (unstable feature; will be upgraded in a future version).
* New feature: check inter-model parameter consistency before concatenation in wavtool mode.
* Improved configuration:
    synthesis-utau-style-normalization full/voiced/off   apply an adaptive gain to
      each segment such that the peak of synthesized waveform goes to half of the
      maximum amplitude when volume is 100%; full: gain both voiced and unvoiced
      parts; voiced: only gain voiced part; off: do not adjust volume.
* New configuration & feature:
    analysis-noise-reduction on/off         automatically reduces noise when analyzing
      LLSM from original wavs; works better with longer recordings.
    analysis-biased-f0-estimation on/off    (over) emphasize voicing probability
      during joint pitch & voicing activity estimation, followed by a pitch & voicing
      correction procedure; tend to reduce false negative(1) but raise false positive rate;
      works well for noisy/coarse speech but may degrade the quality for clean/smooth
      speech. (experimental feature)
    synthesis-duration-extension-method stretch/loop/auto   determines how Moresampler
      extends the duration of each speech segment; auto: automatically stretch/loop
      the segment based on its original and target duration
(1) false negative in this case means "voiced speech being misidentified as unvoiced speech"
    false positive in this case means "unvoiced speech being misidentified as voiced speech"

0.1.7 (Oct. 9, 2015) (internal version)
* Bug fix: crashes when empty note(s) are selected & synthesized.
* Bug fix: occasionally stops rendering in midway.
* Bug fix: pitch bend shouldn't be extrapolated.
* Bug fix: weak COLA factor for noise filtering.
* New feature: high frequency phase reconstruction & alignment (gives more natural-sounding voice).
* New feature: automatically corrects "click"-like artifacts.
* New feature: automatically smooths over drastic phase changes around phoneme transitions.
* New configuration & feature:
    synthesis-utau-style-normalization on/off   apply an adaptive gain to each segment
      such that the peak of synthesized waveform goes to half of the maximum amplitude
      when volume is 100%.
    synthesis-loudness-preservation on/off      retain the perceived loudness after
      modification, based on a psychoacoustic loudness measure.
    dump-log-file <path-of-log-file>/off        output debug information into a
      specified file path (so you can send me the log when Moresampler goes wrong).

0.1.5 (Oct. 3, 2015)
* The first published version;
* Fixed a major bug in wavtool mode which makes Moresampler output nothing on some usts;
* Make more configurations available in moreconfig.txt.

0.1.4 (Oct. 3, 2015) (internal version)
* Fixed two bugs which may lead to crashes;
* Fixed a minor bug in unit concatenation.

0.1.3 (Oct. 3, 2015) (internal version)
* Support arbitrary sampling rate for wav input/output;
* Add UTAU-style amplitude normalization for LLSM parameters;
* Add resampler compatibility mode.

0.1.2 (Oct. 2, 2015) (internal version)
* The first functioning version.

