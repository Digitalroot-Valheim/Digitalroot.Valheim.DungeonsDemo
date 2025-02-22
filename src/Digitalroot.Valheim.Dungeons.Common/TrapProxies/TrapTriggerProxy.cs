﻿using Digitalroot.Valheim.Common;
using Digitalroot.Valheim.TrapSpawners;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Digitalroot.Valheim.Dungeons.Common.TrapProxies
{
  public class TrapTriggerProxy : AbstractProxy<TrapTrigger>
  {
    public bool IsTriggered => RealObject.IsTriggered;
    public bool TriggerOnStart => RealObject.m_triggerOnStart;
    private const string RoomTriggerName = "SpawnTrigger";
    public IEnumerable<TrapSpawnerProxy> Spawners => RealObject.TrapSpawners.Select(trapSpawner => TrapSpawnerProxy.CreateInstance(trapSpawner.GetComponent<TrapSpawner>(), Logger));

    private TrapTriggerProxy([NotNull] TrapTrigger realObject, [NotNull] ITraceableLogging logger)
      : base(realObject, logger)
    {
      Log.Trace(Logger, $"[{MethodBase.GetCurrentMethod().DeclaringType?.Name}] Creating Trigger ({realObject.name})");
      // realObject.LogEvent += HandleLogEvent;
    }

    private TrapTriggerProxy([NotNull] GameObject dungeon, [NotNull] string roomName, [NotNull] ITraceableLogging logger, [NotNull] string roomTriggerName = RoomTriggerName)
      : this(dungeon.transform.Find(GetPath(roomName, roomTriggerName))
                    ?.gameObject?.GetComponent<TrapTrigger>()
             ?? throw new NullReferenceException($"{nameof(TrapTriggerProxy)} '{GetPath(roomName, roomTriggerName)}' not found.")
             , logger) { }

    public static TrapTriggerProxy CreateInstance([NotNull] TrapTrigger realObject, [NotNull] ITraceableLogging logger)
    {
      return new TrapTriggerProxy(realObject, logger);
    }

    public static TrapTriggerProxy CreateInstance([NotNull] GameObject dungeon, [NotNull] string roomName, [NotNull] ITraceableLogging logger, [NotNull] string roomTriggerName = RoomTriggerName)
    {
      return dungeon.transform.Find(roomName) != null ? new TrapTriggerProxy(dungeon, roomName, logger, roomTriggerName) : null;
    }

    private static string GetPath(string roomName, string roomTriggerName) => $"Interior/Dungeon/Rooms/{roomName}/Spawners/{roomTriggerName}";

    public void SetIsTriggered(bool value) => RealObject.IsTriggered = value;

    [UsedImplicitly]
    private bool ShouldTrigger(Collider other)
    {
      var character = other.gameObject.GetComponent<Character>();
      Log.Trace(Logger, $"{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name} character == null : {character == null}");
      Log.Trace(Logger, $"{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name} character?.IsPlayer() : {character?.IsPlayer()}");
      return character != null && character.IsPlayer();
    }
  }
}
