﻿using Digitalroot.Valheim.Common;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Reflection;
using UnityEngine;

namespace Digitalroot.Valheim.Dungeons.OdinsHollow
{
  [UsedImplicitly]
  public class Patch
  {
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    public static class PatchZNetSceneAwake
    {
      [UsedImplicitly]
      [HarmonyPostfix]
      [HarmonyPriority(Priority.Normal)]
      // ReSharper disable once InconsistentNaming
      public static void Postfix([NotNull] ref ZNetScene __instance)
      {
        try
        {
          Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name}");
          if (!Valheim.Common.Utils.IsZNetSceneReady())
          {
            Log.Debug(Main.Instance, $"[{MethodBase.GetCurrentMethod().DeclaringType?.Name}] ZNetScene not ready - skipping");
            return;
          }

          Main.Instance.OnZNetSceneAwake(ref __instance);
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
    public static class PatchZNetAwake
    {
      [UsedImplicitly]
      [HarmonyPostfix]
      [HarmonyPriority(Priority.Normal)]
      // ReSharper disable once InconsistentNaming
      public static void Postfix([NotNull] ref ZNet __instance)
      {
        try
        {
          Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name}");
          if (!Valheim.Common.Utils.IsZNetReady())
          {
            Log.Debug(Main.Instance, $"[{MethodBase.GetCurrentMethod().DeclaringType?.Name}] ZNet not ready - skipping");
            return;
          }

          Main.Instance.OnZNetAwake(ref __instance);
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))]
    public static class PatchGame
    {
      [HarmonyPostfix]
      [HarmonyPriority(Priority.Normal)]
      [UsedImplicitly]
      // ReSharper disable once InconsistentNaming
      public static void PostfixLoad([NotNull] ref Game __instance, Vector3 spawnPoint)
      {
        try
        {
          Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name}({spawnPoint})");

          if (!Valheim.Common.Utils.IsPlayerReady())
          {
            Log.Debug(Main.Instance, $"[{MethodBase.GetCurrentMethod().DeclaringType?.Name}] Player not ready - skipping");
            return;
          }

          Main.Instance.OnSpawnedPlayer(ref __instance, spawnPoint);
        }
        catch (Exception e)
        {
          Log.Error(Main.Instance, e);
        }
      }
    }
  }
}
