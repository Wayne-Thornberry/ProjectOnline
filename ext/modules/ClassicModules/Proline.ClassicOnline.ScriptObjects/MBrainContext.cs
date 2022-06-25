﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Proline.Modularization.Core;
using Proline.Resource.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proline.ClassicOnline.MBrain.Data;
using Proline.ClassicOnline.MData;
using Proline.ClassicOnline.MBrain.Entity;
#if CLIENT
using Proline.CFXExtended.Core;
#endif
using Proline.Resource;
using Proline.ClassicOnline.MDebug;
using System.Reflection;
using Proline.ClassicOnline.MScripting;
using Proline.ClassicOnline.MGame.Data;

namespace Proline.ClassicOnline.MBrain
{
    public class MBrainContext : ModuleScript
    {
        private static Log _log = new Log();

        public MBrainContext(Assembly source) : base(source)
        {
            _trackedHandles = new HashSet<int>();
            _ht = HandleTracker.GetInstance();
            _sm = ScriptObjectManager.GetInstance();
            _instance = ScriptPositionManager.GetInstance();
        }


        private ScriptObjectManager _sm;
        private HashSet<int> _trackedHandles;
        private HandleTracker _ht;
        private ScriptPositions _scriptPosition;
        private ScriptObjectData[] _objs;
        private ScriptPositionManager _instance;

        public override async Task OnStart()
        {
            var data = MDataAPI.LoadResourceFile("data/scriptpositions.json");
            MDebugAPI.LogDebug(data);
            _scriptPosition = JsonConvert.DeserializeObject<ScriptPositions>(data);
            _instance.AddScriptPositionPairs(_scriptPosition.scriptPositionPairs);
            PosBlacklist.Create();

            var data2 = MDataAPI.LoadResourceFile("data/scriptobjects.json");
            MDebugAPI.LogDebug(data2);
            _objs = JsonConvert.DeserializeObject<ScriptObjectData[]>(data2);
            var sm = ScriptObjectManager.GetInstance();
        }

        public override async Task OnUpdate()
        {

            foreach (var item in _objs)
            {
                var hash = string.IsNullOrEmpty(item.ModelHash) ? item.ModelName : API.GetHashKey(item.ModelHash);
                if (!_sm.ContainsKey(hash))
                    _sm.Add(hash, new List<ScriptObjectData>());
                _sm.Get(hash).Add(item);
            }

            var entityHandles = new Queue<int>(HandleManager.EntityHandles);
            var addedHandles = new List<object>();
            var removedHanldes = new List<object>();

            while (entityHandles.Count > 0)
            {
                var handle = entityHandles.Dequeue();
                if (_trackedHandles.Contains(handle))
                    continue;
                _trackedHandles.Add(handle);
                addedHandles.Add(handle);
                _ht.Add(handle);

                if (!API.DoesEntityExist(handle)) continue;
                var modelHash = API.GetEntityModel(handle);
                if (!_sm.ContainsSO(handle) && _sm.ContainsKey(modelHash))
                {
                    _log.Debug(handle + " Oh boy, we found a matching script object with that model hash from that handle, time to track it");
                    _sm.AddSO(handle, new ScriptObject()
                    {
                        Data = _sm.Get(modelHash),
                        Handle = handle,
                        State = 0,
                    });
                }
            }

            var combinedHandles = new Queue<int>(_trackedHandles);
            while (combinedHandles.Count > 0)
            {
                var handle = combinedHandles.Dequeue();
                if (API.DoesEntityExist(handle))
                    continue;
                _trackedHandles.Remove(handle);
                removedHanldes.Add(handle);
                _ht.Remove(handle);

                if (API.DoesEntityExist(handle)) continue;
                var modelHash = API.GetEntityModel(handle);
                if (!_sm.ContainsKey(modelHash)) continue;
                if (_sm.ContainsKey(handle))
                    _sm.Remove(handle);
            }

            ProcessScriptObjects();
            //return; 

            if (_instance.HasScriptPositionPairs())
            {
                foreach (var positionsPair in _instance.GetScriptPositionsPairs())
                {
                    var vector = new Vector3(positionsPair.X, positionsPair.Y, positionsPair.Z);
                    if (World.GetDistance(vector, Game.PlayerPed.Position) < 10f && !PosBlacklist.Contains(positionsPair))
                    {
                        Resource.Console.WriteLine(_log.Debug("In range"));
                        MScriptingAPI.StartNewScript(positionsPair.ScriptName, vector);
                        PosBlacklist.Add(positionsPair);
                    }
                    else if (World.GetDistance(vector, Game.PlayerPed.Position) > 10f && PosBlacklist.Contains(positionsPair))
                    {
                        PosBlacklist.Remove(positionsPair);
                    };
                }
            }
            await Delay(1000);
        }


        [Command("SaveCurrentVehicle")]
        public void SaveCurrentVehicle()
        {
            if (Game.PlayerPed.IsInVehicle())
            {
                var pv = new PersonalVehicle();
                var cv = Game.PlayerPed.CurrentVehicle;
                pv.ModelHash = cv.Model.Hash;
                pv.LastPosition = cv.Position;
                var json = JsonConvert.SerializeObject(pv);
                MDataAPI.AddFileValue("PersonalVehicle", json);
            }
        }

        [Command("BuyRandomWeapon")]
        public void BuyRandomWeapon()
        {
            var stat = MPStat.GetStat<long>("MP0_WALLET_BALANCE");
            var stat2 = MPStat.GetStat<long>("BANK_BALANCE");

            if (stat.GetValue() > 250)
            {
                Array values = Enum.GetValues(typeof(WeaponHash));
                Random random = new Random();
                WeaponHash weaponHash = (WeaponHash)values.GetValue(random.Next(values.Length));
                var pw = new PersonalWeapon();
                pw.AmmoCount = random.Next(1, 100);
                pw.Hash = (uint)weaponHash;
                Game.PlayerPed.Weapons.Give(weaponHash, pw.AmmoCount, true, true);
                var list = new List<PersonalWeapon>();
                if (MDataAPI.DoesValueExist("PersonalWeapons"))
                {
                    list = MData.MDataAPI.GetFileValue<List<PersonalWeapon>>("PersonalWeapons");
                }
                list.Add(pw);
                MDataAPI.AddFileValue("PersonalWeapons", JsonConvert.SerializeObject(list));
                stat.SetValue(stat.GetValue() - 250);

            }
        }


        private void ProcessScriptObjects()
        {
            var values = _sm.GetValues();
            if (values == null)
                return;
            var quew = new Queue<ScriptObject>(values);
            while (quew.Count > 0)
            {
                var so = quew.Dequeue();
                ProcessScriptObject(so);
            }
        }

        private void ProcessScriptObject(ScriptObject so)
        {
            if (!API.DoesEntityExist(so.Handle))
            {
                _sm.Remove(so.Handle);
                return;
            }
            var entity = CitizenFX.Core.Entity.FromHandle(so.Handle);
            foreach (var item in so.Data)
            {
                if (IsEntityWithinActivationRange(entity, Game.PlayerPed, item.ActivationRange) && so.State == 0)
                {
                    _log.Debug(so.Handle + " Player is within range here, we should start the script and no longer track this for processing");
                    MScriptingAPI.StartNewScript(item.ScriptName, so.Handle);
                    so.State = 1;
                    _sm.Remove(so.Handle);
                    return;
                }
            }
        }


        private bool IsEntityWithinActivationRange(CitizenFX.Core.Entity entity, CitizenFX.Core.Entity playerPed, float activationRange)
        {
            var pos = Game.PlayerPed.Position;
            var pos2 = entity.Position;
            return API.Vdist2(pos.X, pos.Y, pos.Z, pos2.X, pos2.Y, pos2.Z) <= activationRange;
        }



    }
}