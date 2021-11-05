﻿using BepInEx;
using Digitalroot.CustomMonoBehaviours;
using Digitalroot.Valheim.Common;
using Digitalroot.Valheim.Common.Names.Vanilla;
using Digitalroot.Valheim.Dungeons.Common;
using Digitalroot.Valheim.Dungeons.Common.Rooms;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = System.Diagnostics.Debug;

namespace Digitalroot.Valheim.Dungeons.OdinsHollow
{
  [BepInPlugin(Guid, Name, Version)]
  public class Main : BaseUnityPlugin, ITraceableLogging
  {
    public const string Version = "1.0.0";
    public const string Name = "Odin's Hollow Dungeon by the Odin Plus Team";

    // ReSharper disable once MemberCanBePrivate.Global
    public const string Guid = "digitalroot.mods.dungeons.odinshollow";
    public const string Namespace = "Digitalroot.Valheim.Dungeons." + nameof(OdinsHollow);
    private Harmony _harmony;
    private AssetBundle _assetBundle;

    // ReSharper disable once MemberCanBePrivate.Global
    public static Main Instance;

    // ReSharper disable once IdentifierTypo
    private const string OdinsHollow = nameof(OdinsHollow);
    private Dungeon _dungeon;

    public Main()
    {
      Instance = this;
#if DEBUG
      EnableTrace = true;
      Log.RegisterSource(Instance);
#else
      EnableTrace = false;
#endif
      Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name}");
    }

    [UsedImplicitly]
    private void Awake()
    {
      try
      {
        Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name}");

        RepositoryLoader.LoadAssembly("Digitalroot.Valheim.TrapSpawners.dll");

        _assetBundle = AssetUtils.LoadAssetBundleFromResources("op_dungeons", typeof(Main).Assembly);

#if DEBUG
        foreach (var scene in _assetBundle.GetAllScenePaths())
        {
          Log.Trace(Main.Instance, scene);
          SceneManager.LoadSceneAsync(System.IO.Path.GetFileNameWithoutExtension(scene), LoadSceneMode.Additive);
        }

        foreach (var assetName in _assetBundle.GetAllAssetNames())
        {
          Log.Trace(Main.Instance, assetName);
        }
#endif
        PrefabManager.Instance.AddPrefab(new CustomPrefab(_assetBundle.LoadAsset<GameObject>(OdinsHollow), true));
        PrefabManager.OnVanillaPrefabsAvailable += OnVanillaPrefabsAvailable;

        _harmony = Harmony.CreateAndPatchAll(typeof(Main).Assembly, Guid);
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    private void OnVanillaPrefabsAvailable()
    {
      try
      {
        Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name}");
        var dungeonPrefab = PrefabManager.Instance.GetPrefab(OdinsHollow);
        Log.Trace(Main.Instance, $"[{MethodBase.GetCurrentMethod().DeclaringType?.Name}] dungeonPrefab == null : {dungeonPrefab == null}");
        Debug.Assert(dungeonPrefab != null, nameof(dungeonPrefab) + " != null");

        // Configure
        _dungeon = new Dungeon(OdinsHollow, dungeonPrefab);
        _dungeon.SetEnableTrace(EnableTrace);
        _dungeon.AddDungeonBossRoom(DungeonsRoomNames.BlueRoom);

        // Seed
        SeedGlobalSpawnPoolIfNecessary();
        foreach (var dungeonDungeonBossRoom in _dungeon.DungeonBossRooms)
        {
          SeedSpawnPoolsFor(dungeonDungeonBossRoom);
        }
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
      finally
      {
        PrefabManager.OnVanillaPrefabsAvailable -= OnVanillaPrefabsAvailable;
      }
    }

    [UsedImplicitly]
    private void OnDestroy()
    {
      try
      {
        Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name}");
        _harmony?.UnpatchSelf();
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    #region Spawn Pool Seeding

    private void SeedGlobalSpawnPoolIfNecessary()
    {
      Log.Trace(Main.Instance, $"[{MethodBase.GetCurrentMethod().DeclaringType?.Name}] Seeding Global Spawn Pool");
      Log.Trace(Main.Instance, $"[{MethodBase.GetCurrentMethod().DeclaringType?.Name}] _dungeon.GlobalSpawnPool == null : {_dungeon.GlobalSpawnPool == null}");

      if (_dungeon.GlobalSpawnPool == null)
      {
        Log.Trace(Main.Instance, $"[{MethodBase.GetCurrentMethod().DeclaringType?.Name}] Skipping Seeding of Global Spawn Pool");
        return;
      }

      _dungeon.GlobalSpawnPool?.Clear(); // Remove anything already in the GSP.
      _dungeon.GlobalSpawnPool?.AddEnemy(EnemyNames.SkeletonPoison);
      _dungeon.GlobalSpawnPool?.AddEnemy(EnemyNames.Blob);
      _dungeon.GlobalSpawnPool?.AddEnemy(EnemyNames.BlobElite);
      _dungeon.GlobalSpawnPool?.AddEnemy(EnemyNames.Draugr);
      _dungeon.GlobalSpawnPool?.AddEnemy(EnemyNames.DraugrElite);
      _dungeon.GlobalSpawnPool?.AddEnemy(EnemyNames.DraugrRanged);
      _dungeon.GlobalSpawnPool?.AddEnemy(PrefabNames.SkeletonNoArcher);
      _dungeon.GlobalSpawnPool?.AddEnemy(PrefabNames.SkeletonNoArcher);
      _dungeon.GlobalSpawnPool?.AddEnemy(PrefabNames.SkeletonNoArcher);
      _dungeon.GlobalSpawnPool?.AddEnemy(PrefabNames.SkeletonNoArcher);
      _dungeon.GlobalSpawnPool?.AddPrefab(PrefabNames.BonePileSpawner);
      _dungeon.GlobalSpawnPool?.AddPrefab(PrefabNames.SpawnerDraugrPile);
    }

    private void SeedSpawnPoolsFor(DungeonBossRoom room)
    {
      Log.Trace(Main.Instance, $"Seeding bosses for {room.Name}");
      Log.Trace(Main.Instance, $"Room Health Check [{room.Name}]");
      Log.Trace(Main.Instance, $"room.RoomBossSpawnPoints == null [{room.RoomBossSpawnPoints == null}]");
      Log.Trace(Main.Instance, $"room.RoomBossSpawnPoints?.Count [{room.RoomBossSpawnPoints?.Count}]");
      Log.Trace(Main.Instance, $"room.RoomBossSpawnPool == null [{room.RoomBossSpawnPool == null}]");
      Log.Trace(Main.Instance, $"room.RoomBossTrigger == null [{room.RoomBossTrigger == null}]");
      // ReSharper disable once IdentifierTypo
      // var miniBossSpawner = odinsHollow?.transform.Find(room.Name)?.Find(room.MiniBossSpawnPointName).gameObject?.GetComponent<TrapSpawner>();
      // Log.Trace(Main.Instance, $"miniBossSpawner == null : {miniBossSpawner == null}");
      if (room.RoomBossSpawnPoints?.FirstOrDefault() != null)
      {
        room.FirstBossSpawnPoint.AddBoss(EnemyNames.DraugrElite);
        room.FirstBossSpawnPoint.SetIgnoreSpawnPoolOverrides(true); // true | false
      }

      SeedSpawnPoolsFor(room as DungeonRoom);
    }

    private void SeedSpawnPoolsFor(DungeonRoom room)
    {
      Log.Trace(Main.Instance, $"Seeding trash for {room.Name}");
      Log.Trace(Main.Instance, $"Room Health Check [{room.Name}]");
      Log.Trace(Main.Instance, $"room.RoomSpawnPool == null : {room.RoomSpawnPool == null}");
      Log.Trace(Main.Instance, $"room.RoomSpawnPool?.SpawnPoolCount() == null : {room.RoomSpawnPool?.SpawnPoolCount()}");
      Log.Trace(Main.Instance, $"room.RoomTrigger == null : {room.RoomTrigger == null}");
      Log.Trace(Main.Instance, $"room.RoomSpawnPoints == null : {room.RoomSpawnPoints == null}");
      Log.Trace(Main.Instance, $"room.RoomSpawnPoints?.Count : {room.RoomSpawnPoints?.Count}");

      if (room.RoomSpawnPool == null) return;

      room.RoomSpawnPool?.AddEnemy(EnemyNames.Draugr);
      room.RoomSpawnPool?.AddEnemy(EnemyNames.DraugrRanged);
    }

    #endregion

    #region Implementation of ITraceableLogging

    /// <inheritdoc />
    public string Source => Namespace;

    /// <inheritdoc />
    public bool EnableTrace { get; }

    #endregion
  }
}
