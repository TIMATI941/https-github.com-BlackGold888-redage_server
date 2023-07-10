using System;
using System.Collections.Generic;
using System.Text;
using Redage.SDK;
using NeptuneEvo.Handles;
using GTANetworkAPI;

namespace NeptuneEvo.World.Drugs.Models
{
    public class Field
    {
        private static readonly nLog Log = new nLog("field");

        public int ID;
        public Vector3 Position;
        public List<FieldPlant> Plants = new List<FieldPlant>();
        public float Range;

        private ColShape ColShape;
        public void GTAElements()
        {
            try
            {
                ColShape = NAPI.ColShape.CreateSphereColShape(Position, Range, 0);
                ColShape.OnEntityEnterColShape += (s, e) =>
                {
                    e.SetData("drug.field", this);
                };
                ColShape.OnEntityExitColShape += (s, e) =>
                {
                    e.ResetData("drug.field");
                };
            }
            catch(Exception ex) { Log.Write("GTAElements: " + ex.ToString()); }
        }

        public void Reload()
        {
            try
            {
                foreach (var plant in Plants)
                {
                    plant.IsAlive = true;
                    plant.GTAElements();
                    plant.RespawnWeed();
                }
            }
            catch(Exception ex) { Log.Write("Reload: " + ex.ToString()); }
        }
    }
}
