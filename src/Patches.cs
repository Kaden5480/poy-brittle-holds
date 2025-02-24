using HarmonyLib;
using UnityEngine;

namespace BrittleHolds.Patches {
    [HarmonyPatch(typeof(Climbing), "Update")]
    static class PatchClimbing {
        private static BrittleHold grabbedL = null;
        private static BrittleHold grabbedR = null;

        static bool grabbingL = false;
        static bool grabbingR = false;

        private static void Release(bool left) {
            if (left == true) {
                grabbedL = null;
                grabbingL = false;
            }
            else {
                grabbedR = null;
                grabbingR = false;
            }
        }

        private static void Grab(Climbing climbing, GameObject obj, bool left) {
            BrittleHold hold = obj.GetComponent<BrittleHold>();

            // If this is a brittle hold, update hp
            if (hold != null) {
                hold.Grab();
            }

            if (left == true) {
                grabbedL = hold;
                grabbingL = true;
            }
            else {
                grabbedR = hold;
                grabbingR = true;
            }

            // Check for destruction
            if (grabbedL != null && grabbedL.destroyed == true) {
                climbing.ReleaseLHand(true);
            }

            if (grabbedR != null && grabbedR.destroyed == true) {
                climbing.ReleaseRHand(true);
            }
        }

        static void Postfix(Climbing __instance) {
            Transform objL = __instance.grabbedObjectL;
            Transform objR = __instance.grabbedObjectR;

            // Cache left hand
            if (grabbingL == false && objL != null) {
                Grab(__instance, objL.gameObject, true);
            }
            else if (grabbingL == true && objL == null) {
                Release(true);
            }

            // Grab right hand
            if (grabbingR == false && objR != null) {
                Grab(__instance, objR.gameObject, false);
            }
            else if (grabbingR == true && objR == null) {
                Release(false);
            }
        }
    }
}
