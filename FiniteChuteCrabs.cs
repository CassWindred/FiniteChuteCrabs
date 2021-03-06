using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XRL;
using XRL.Core;
using XRL.Messages;
using XRL.World;
using XRL.World.Parts;
using Random = System.Random;

namespace FiniteChuteCrabs.HarmonyPatches
{
    [HasCallAfterGameLoadedAttribute]
    [Serializable]
    class FiniteCrabsPatch : IPart
    {
        static public Dictionary<String, List<int>> CrabTraps = new Dictionary<String, List<int>>();
        [HarmonyPatch(typeof(WalltrapCrabs), "SpawnCrab")]
        class FiniteCrabsFireEventPatch
        {
            static int MaximumCrabs = 40; //The Maximum Number of Crabs that can be spawned by any trap
                                          //A dictionary mapping crab traps ids to an array that represents crabs 
                                          // spawned/maximum crabs for that spawner

            static Random rand = new Random();
            static bool Prefix(WalltrapCrabs __instance)
            {
                String trapid = __instance.ParentObject.id;
                MessageQueue.AddPlayerMessage($"Prefixing crabtrap {__instance.ParentObject.id}");
                MessageQueue.AddPlayerMessage($"Dictionary Contains: {CrabTraps.Count} crabtraps");
                if (CrabTraps.ContainsKey(trapid))
                {
                    //MessageQueue.AddPlayerMessage($"Running Prefix on a crab trap with {CrabTraps[__instance][0]} crabs spawned out of {CrabTraps[__instance][1]}");

                    if (CrabTraps[trapid][0] >= CrabTraps[trapid][1])
                    {
                        //If the CrabTrap has already spawned its maximum number of crabs then skip SpawnCrab()
                        //MessageQueue.AddPlayerMessage("No more crabs for you sir!!");
                        return false;
                    }
                    else
                    {
                        //If the CrabTrap has not already spawned its maximum number of crabs then add 1 and continue
                        CrabTraps[trapid][0]++;
                        return true;
                    }
                }
                else
                {
                    //MessageQueue.AddPlayerMessage("ADDING A NEW CRAB TRAP BAYBEE");
                    //Add the crab trap to the dictionary if its not already there and continue
                    CrabTraps.Add(
                        trapid,
                        new List<int> { 0, rand.Next(5, MaximumCrabs + 1) }
                        );
                    return true;
                }
            }
        }

        public override void SaveData(SerializationWriter Writer)
        {
            List<string> keys = CrabTraps.Keys.ToList<string>();
            UnityEngine.Debug.Log($"Saving Keys:\n{keys}");

            List<List<int>> values = CrabTraps.Values.ToList<List<int>>();
            UnityEngine.Debug.Log($"Saving Values:\n{values}");

            Writer.Write(keys);
            Writer.Write(values);

            base.SaveData(Writer);
        }

        public override void LoadData(SerializationReader Reader)
        {



            List<string> keys = Reader.ReadList<string>();
            List<List<int>> values = Reader.ReadList<List<int>>();
            UnityEngine.Debug.Log($"Loading Keys:\n{keys}");

            UnityEngine.Debug.Log($"Loading Values:\n{values}");
            CrabTraps = keys.Zip(values, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
            UnityEngine.Debug.Log($"Loaded Dictionary:\n{CrabTraps}");

            base.LoadData(Reader);
        }

        [CallAfterGameLoadedAttribute]
        public static void AddFiniteCrabTrackerToCharacter()
        {
            // Called whenever loading a save game
            XRL.World.GameObject player = XRLCore.Core?.Game?.Player?.Body;
            if (player != null)
            {
                player.RequirePart<FiniteCrabsPatch>(); //RequirePart will add the part only if the player doesn't already have it. This ensures your part only gets added once, even after multiple save loads.
            }
        }


    }
}