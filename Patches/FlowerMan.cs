using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowerMorgan.Patches
{
  internal class FlowerMorganPatch
  {
    public class FlowermanAudioInfo
    {
      public bool IsPlaying { get; set; }
      public AudioSource AudioSource { get; set; }
    }

    public static Dictionary<FlowermanAI, FlowermanAudioInfo> flowermanAudioInfoMap = new Dictionary<FlowermanAI, FlowermanAudioInfo>();

    [HarmonyPatch(typeof(FlowermanAI), "Start")]
    [HarmonyPostfix]
    public static void FlowerMorganAudioPatch(FlowermanAI __instance)
    {
      if (!flowermanAudioInfoMap.TryGetValue(__instance, out var flowermanAudioInfo))
        flowermanAudioInfo = new FlowermanAudioInfo
        {
          IsPlaying = false,
          AudioSource = __instance.gameObject.AddComponent<AudioSource>()
        };
      flowermanAudioInfo.AudioSource.spatialBlend = 1f;
      flowermanAudioInfo.AudioSource.dopplerLevel = 1f;
      flowermanAudioInfo.AudioSource.rolloffMode = AudioRolloffMode.Linear;
      flowermanAudioInfo.AudioSource.minDistance = 5f;
      flowermanAudioInfo.AudioSource.maxDistance = 100f;
      flowermanAudioInfo.AudioSource.volume = 1.0f;
      flowermanAudioInfoMap[__instance] = flowermanAudioInfo;

      __instance.crackNeckSFX = FlowerMorganModBase.audioClips[0];
    }

    [HarmonyPatch(typeof(FlowermanAI), nameof(FlowermanAI.Update))]
    [HarmonyPrefix]
    static void UpdatePatch(FlowermanAI __instance, ref bool ___wasInEvadeMode)
    {
      if (flowermanAudioInfoMap.TryGetValue(__instance, out var flowermanAudioInfo))
      {
        if (!___wasInEvadeMode && !flowermanAudioInfo.IsPlaying && Vector3.Distance(__instance.transform.position, __instance.GetClosestPlayer().serverPlayerPosition) <= 50f)
        {
          flowermanAudioInfo.IsPlaying = true;
          FlowerMorganModBase.mls.LogWarning(__instance.GetInstanceID() + " Playing audio...");
          flowermanAudioInfo.AudioSource.PlayOneShot(FlowerMorganModBase.audioClips[1]);
          __instance.StartCoroutine(StopAudioAfterDelay(flowermanAudioInfo));
        }
      }
    }
    static IEnumerator StopAudioAfterDelay(FlowermanAudioInfo flowermanAudioInfo)
    {
      double randomDelay = new System.Random().NextDouble() * 5f + 30f;
      yield return new WaitForSeconds(FlowerMorganModBase.audioClips[1].length + (float)randomDelay);
      flowermanAudioInfo.AudioSource.Stop();
      flowermanAudioInfo.IsPlaying = false;
    }

    [HarmonyPatch(typeof(FlowermanAI), nameof(FlowermanAI.HitEnemy))]
    [HarmonyPrefix]
    public static void FlowerMorganCleanPatch(FlowermanAI __instance)
    {
      if (__instance.enemyHP <= 0)
      {
        FlowerMorganModBase.mls.LogWarning(__instance.GetInstanceID() + " Killed!");
        flowermanAudioInfoMap.Remove(__instance);
      }
    }

    [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.EndGame))]
    [HarmonyPostfix]
    public static void StartMatchLeverCleanUp()
    {
      FlowerMorganModBase.mls.LogWarning("Clearing `flowermanAudioInfoMap`...");
      flowermanAudioInfoMap.Clear();
    }

    [HarmonyPatch(typeof(StartOfRound), "gameOverAnimation")]
    [HarmonyPostfix]
    public static void StartOfRoundCleanUp()
    {
      FlowerMorganModBase.mls.LogWarning("Clearing `flowermanAudioInfoMap`...");
      flowermanAudioInfoMap.Clear();
    }
  }
}
