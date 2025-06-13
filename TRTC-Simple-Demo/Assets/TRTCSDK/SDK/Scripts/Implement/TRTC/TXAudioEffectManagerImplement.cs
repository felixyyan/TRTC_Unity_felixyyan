// Copyright (c) 2023 Tencent. All rights reserved.
// Author: felixyyan

using System;
using System.Collections.Generic;
using AOT;

namespace trtc {
  public class TXAudioEffectManagerImplement : ITXAudioEffectManager {
#region callbaks

    private static readonly Dictionary<int, ITXMusicPlayObserver> PlayObserver =
        new Dictionary<int, ITXMusicPlayObserver>();

    [MonoPInvokeCallback(typeof(TXAudioEffectManagerNative.TRTCUnityAEMMusicPlayOnStart))]
    public static void TRTCUnityAEMMusicPlayOnStart(IntPtr instance, int musicId, int errCode) {
      ITXMusicPlayObserver observer;
      if (PlayObserver.TryGetValue(musicId, out observer) && null != observer) {
        observer.onStart(musicId, errCode);
      }
    }

    [MonoPInvokeCallback(typeof(TXAudioEffectManagerNative.TRTCUnityAEMMusicPlayOnPlayProgress))]
    public static void TRTCUnityAEMMusicPlayOnPlayProgress(IntPtr instance,
                                                           int musicId,
                                                           long curPtsMS,
                                                           long durationMS) {
      ITXMusicPlayObserver observer;
      if (PlayObserver.TryGetValue(musicId, out observer) && null != observer) {
        observer.onPlayProgress(musicId, Convert.ToInt64(curPtsMS), Convert.ToInt64(durationMS));
      }
    }

    [MonoPInvokeCallback(typeof(TXAudioEffectManagerNative.TRTCUnityAEMMusicPlayOnComplete))]
    public static void TRTCUnityAEMMusicPlayOnComplete(IntPtr instance, int musicId, int errCode) {
      ITXMusicPlayObserver observer;
      if (PlayObserver.TryGetValue(musicId, out observer) && null != observer) {
        observer.onComplete(musicId, errCode);
      }
    }

    private static ITXMusicPreloadObserver _preloadObserver;

    [MonoPInvokeCallback(typeof(TXAudioEffectManagerNative.TRTCUnityAEMMusicPreloadOnLoadProgress))]
    public static void TRTCUnityAEMMusicPreloadOnLoadProgress(IntPtr instance,
                                                              int musicId,
                                                              int progress) {
      if (null != _preloadObserver) {
        _preloadObserver.onLoadProgress(musicId, progress);
      }
    }

    [MonoPInvokeCallback(typeof(TXAudioEffectManagerNative.TRTCUnityAEMMusicPreloadOnLoadError))]
    public static void TRTCUnityAEMMusicPreloadOnLoadError(IntPtr instance,
                                                           int musicId,
                                                           int errCode) {
      if (null != _preloadObserver) {
        _preloadObserver.onLoadError(musicId, errCode);
      }
    }

#endregion

    private IntPtr _nativeObj;
    private IntPtr _globalMusicObserver;
    private IntPtr _globalPreloadObserver;

    public TXAudioEffectManagerImplement(IntPtr nativeObj) {
      _nativeObj = nativeObj;
      _globalMusicObserver =
          TXAudioEffectManagerNative.tx_audio_effect_manager_create_music_play_observer(
              nativeObj, TRTCUnityAEMMusicPlayOnStart, TRTCUnityAEMMusicPlayOnPlayProgress,
              TRTCUnityAEMMusicPlayOnComplete);

      _globalPreloadObserver =
          TXAudioEffectManagerNative.tx_audio_effect_manager_create_music_preload_observer(
              nativeObj, TRTCUnityAEMMusicPreloadOnLoadProgress,
              TRTCUnityAEMMusicPreloadOnLoadError);
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_preload_observer(
          nativeObj, _globalPreloadObserver);
    }

    ~TXAudioEffectManagerImplement() {
      TXAudioEffectManagerNative.tx_audio_effect_manager_destroy_music_play_observer(
          _globalMusicObserver);
      _globalMusicObserver = IntPtr.Zero;

      PlayObserver.Clear();
      TXAudioEffectManagerNative.tx_audio_effect_manager_destroy_music_preload_observer(
          _globalPreloadObserver);
      _globalPreloadObserver = IntPtr.Zero;

      _nativeObj = IntPtr.Zero;
    }

    public void DestroyNativeObj() {
      _nativeObj = IntPtr.Zero;
    }

    // 1.1
    public override void enableVoiceEarMonitor(bool enable) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_enable_voice_ear_monitor(_nativeObj,
                                                                                  enable);
    }

    // 1.2
    public override void setVoiceEarMonitorVolume(int volume) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_voice_ear_monitor_volume(_nativeObj,
                                                                                      volume);
    }

    // 1.3
    public override void setVoiceReverbType(TXVoiceReverbType reverbType) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_voice_reverb_type(_nativeObj,
                                                                               reverbType);
    }

    // 1.4
    public override void setVoiceChangerType(TXVoiceChangeType changerType) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_voice_changer_type(_nativeObj,
                                                                                changerType);
    }

    // 1.5
    public override void setVoiceCaptureVolume(int volume) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_voice_capture_volume(_nativeObj,
                                                                                  volume);
    }

    // 1.6
    public override void setVoicePitch(double pitch) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_voice_pitch(_nativeObj, pitch);
    }

    // 2.0
    public override void setMusicObserver(int musicId, ITXMusicPlayObserver observer) {
      if (observer == null) {
        PlayObserver.Remove(musicId);
        TXAudioEffectManagerNative.tx_audio_effect_manager_set_music_observer(_nativeObj, musicId,
                                                                              IntPtr.Zero);
      } else {
        PlayObserver[musicId] = observer;
        TXAudioEffectManagerNative.tx_audio_effect_manager_set_music_observer(_nativeObj, musicId,
                                                                              _globalMusicObserver);
      }
    }

    // 2.1
    public override void startPlayMusic(AudioMusicParam musicParam) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_start_play_music(_nativeObj, musicParam);
    }

    // 2.2
    public override void stopPlayMusic(int musicId) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_stop_play_music(_nativeObj, musicId);
    }

    // 2.3
    public override void pausePlayMusic(int musicId) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_pause_play_music(_nativeObj, musicId);
    }

    // 2.4
    public override void resumePlayMusic(int musicId) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_resume_play_music(_nativeObj, musicId);
    }

    // 2.5
    public override void setAllMusicVolume(int volume) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_all_music_volume(_nativeObj, volume);
    }

    // 2.6
    public override void setMusicPublishVolume(int musicId, int volume) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_music_publish_volume(_nativeObj,
                                                                                  musicId, volume);
    }

    // 2.7
    public override void setMusicPlayoutVolume(int musicId, int volume) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_music_playout_volume(_nativeObj,
                                                                                  musicId, volume);
    }

    // 2.8
    public override void setMusicPitch(int musicId, double pitch) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_music_pitch(_nativeObj, musicId,
                                                                         (float)pitch);
    }

    // 2.9
    public override void setMusicSpeedRate(int musicId, double speedRate) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_music_speed_rate(_nativeObj, musicId,
                                                                              (float)speedRate);
    }

    // 2.10
    public override int getMusicCurrentPosInMS(int musicId) {
      return (int)TXAudioEffectManagerNative.tx_audio_effect_manager_get_current_pos_in_ms(
          _nativeObj, musicId);
    }

    // 2.11
    public override int getMusicDurationInMS(string path) {
      return (int)TXAudioEffectManagerNative.tx_audio_effect_manager_get_music_duration_in_ms(
          _nativeObj, path);
    }

    // 2.12
    public override void seekMusicToPosInTime(int musicId, int pts) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_seek_music_to_pos_in_time(_nativeObj,
                                                                                   musicId, pts);
    }

    // 2.13
    public override void setMusicScratchSpeedRate(int musicId, float scratchSpeedRate) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_music_scratch_speed_rate(
          _nativeObj, musicId, scratchSpeedRate);
    }

    // 2.14
    public override void setPreloadObserver(ITXMusicPreloadObserver observer) {
      _preloadObserver = observer;
    }

    // 2.15
    public override void preloadMusic(AudioMusicParam preloadParam) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_preload_music(_nativeObj, preloadParam);
    }

    // 2.16
    public override int getMusicTrackCount(int musicId) {
      return (int)TXAudioEffectManagerNative.tx_audio_effect_manager_get_music_track_count(
          _nativeObj, musicId);
    }

    // 2.17
    public override void setMusicTrack(int musicId, int trackIndex) {
      TXAudioEffectManagerNative.tx_audio_effect_manager_set_music_track(_nativeObj, musicId,
                                                                         trackIndex);
    }

    [Obsolete("use seekMusicToPosInTime replace")]
    public override void seekMusicToPosInMS(int musicId, int pts) {
      seekMusicToPosInTime(musicId, pts);
    }
  }
}