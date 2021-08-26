﻿using Proline.Freemode;
using Proline.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;

namespace Proline.Freemode.Components.CDebugInterface
{
    public class DebugInterfaceHandler : ComponentHandler
    {
        private int[] _handles;

        public override void OnUpdate()
        {
            API.SetTextFont(0);
            API.SetTextProportional(true);
            API.SetTextScale(0.0f, 0.3f);
            API.SetTextColour(255, 255, 255, 255);
            // API.SetTextDropshadow(0, 0, 0, 0, 255);
            // API.SetTextEdge(1, 0, 0, 0, 255);
            API.SetTextDropShadow();
            API.SetTextOutline();
            API.SetTextEntry("STRING");
            API.AddTextComponentString(Game.PlayerPed.Position.ToString() + "H:" + Game.PlayerPed.Heading + "\n"
               + Game.PlayerPed.Model.Hash + "\n"
               + Game.PlayerPed.Health + "\n"
               + Game.PlayerPed.Handle + "\n");
            API.DrawText(0.005f, 0.05f);


            foreach (var handle in _handles)
            {
                var entity = Entity.FromHandle(handle);
                if (entity == null) continue;
                if (!API.IsEntityVisible(entity.Handle) || World.GetDistance(entity.Position, Game.PlayerPed.Position) > 10f) continue;
                var pos = entity.Model.GetDimensions();
                var d = entity.Position + new Vector3(0, 0, (entity.Model.GetDimensions().Z * 0.8f));
                var x = $"{entity.Handle}\n" +
                    $"{entity.Model.Hash}\n" +
                    $"{Math.Round(entity.Heading, 2)}\n" +
                    $"{entity.Health}\n";// +
                                         //$"{ExampleAPI.IsEntityScripted(entity.Handle)}";
                                         //ExampleAPI.DrawEntityBoundingBox(entity.Handle, 125, 125, 125, 100);
                                         //ExampleAPI.DrawDebugText3D(x, d, 3f, 0);
            }
        }
        public override void OnStart()
        {
            _handles = new int[0];
        }

        //public override void OnEngineEvent(string eventName, params object[] args)
        //{
        //    if (eventName.Equals("entitiesInWorld"))
        //    {
        //        _handles = (int[])args[0];
        //    }
        //}
    }
}
