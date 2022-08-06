﻿using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using Newtonsoft.Json;
using Proline.ClassicOnline.MWorld.Internal;
using Proline.ClassicOnline.MWord;
using Proline.ClassicOnline.MWorld.Client.Data;

using Proline.Resource.IO;

namespace Proline.ClassicOnline.MWorld.Client.Scripts
{
    public class InitCore 
    {


        public async Task Execute()
        {
            var twocarjson = ResourceFile.Load("data/world/garages/2cargarage.json");
            var sixcarjson = ResourceFile.Load("data/world/garages/6cargarage.json");
            var tencarjson = ResourceFile.Load("data/world/garages/10cargarage.json");
            var apts = ResourceFile.Load("data/world/apartments/apt_dpheights.json");

            var instance = InteriorManager.GetInstance();
            var twoCarInterior = JsonConvert.DeserializeObject<GarageInterior>(twocarjson.Load());
            var sixCarInterior = JsonConvert.DeserializeObject<GarageInterior>(sixcarjson.Load());
            var tenCarInterior = JsonConvert.DeserializeObject<GarageInterior>(tencarjson.Load());
            var aptInterior = JsonConvert.DeserializeObject<List<ApartmentInterior>>(apts.Load());

            instance.Register(twoCarInterior.Id, twoCarInterior);
            instance.Register(sixCarInterior.Id, sixCarInterior);
            instance.Register(tenCarInterior.Id, tenCarInterior);
            foreach (var item in aptInterior)
            { 
                instance.Register(item.Id, item);
            }
        }


    }
}
