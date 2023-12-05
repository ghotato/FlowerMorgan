using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using FlowerMorgan.Patches;
using UnityEngine;
using UnityEngine.Networking;

namespace FlowerMorgan
{
  [BepInPlugin(modGUID, modName, modVersion)]
  public class FlowerMorganModBase : BaseUnityPlugin
  {
    private const string modGUID = "Q.FlowerMorgan";
    private const string modName = "Flower Morgan";
    private const string modVersion = "1.0.0";

    private readonly Harmony harmony = new Harmony(modGUID);
    private static FlowerMorganModBase Instance;
    public static ManualLogSource mls;

    public static AudioClip[] audioClips = new AudioClip[2];

    void Awake()
    {
      if (Instance == null)
      {
        Instance = this;
      }

      mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
      mls.LogInfo("Starting " + modName);
      mls.LogWarning("Read sounds start!");
      string[] paths = new string[]
      {
        "file://" + Paths.PluginPath + $@"\{modGUID}\rehehe.mp3",
        "file://" + Paths.PluginPath + $@"\{modGUID}\smell.mp3"
      };

      for (int i = 0; i < paths.Length; i++)
      {
        mls.LogWarning("Sound: " + paths[i] + " Loadded!");
        UnityWebRequest audioClip = UnityWebRequestMultimedia.GetAudioClip(paths[i], AudioType.MPEG);
        audioClip.SendWebRequest();

        while (!audioClip.isDone)
        {
        }
        audioClips[i] = DownloadHandlerAudioClip.GetContent(audioClip);
      }
      mls.LogWarning("Read sounds end!");

      harmony.PatchAll(typeof(FlowerMorganModBase));
      harmony.PatchAll(typeof(FlowerMorganPatch));
    }
  }
}
