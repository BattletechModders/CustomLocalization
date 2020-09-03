using BattleTech;
using Harmony;
using System.Collections.Generic;
using System.Reflection;

namespace CustomTranslation {
  /*[HarmonyPatch(typeof(BaseDescriptionDef))]
  [HarmonyPatch("Name")]
  [HarmonyPatch(MethodType.Getter)]
  public static class BaseDescriptionDef_NameGet {
    public static bool Prefix(BaseDescriptionDef __instance, MethodBase __originalMethod, ref string __result) {
      string val = (string)__originalMethod.Invoke(__instance, null);
      string unlock = val;
      Text_Append.Localize(ref val);
      __result = val;
      Log.LogWrite("BaseDescriptionDef.Name "+__instance.Id+" "+unlock+"->"+__result+"\n");
      return false;
    }
  }
  [HarmonyPatch(typeof(BaseDescriptionDef))]
  [HarmonyPatch("Details")]
  [HarmonyPatch(MethodType.Getter)]
  public static class BaseDescriptionDef_DetailsGet {
    public static bool Prefix(BaseDescriptionDef __instance, MethodBase __originalMethod, ref string __result) {
      string val = (string)__originalMethod.Invoke(__instance, null);
      string unlock = val;
      Text_Append.Localize(ref val);
      __result = val;
      Log.LogWrite("BaseDescriptionDef.Details " + __instance.Id + " " + unlock + "->" + __result + "\n");
      return false;
    }
  }*/
  [HarmonyPatch(typeof(DescriptionDef))]
  [HarmonyPatch("UIName")]
  [HarmonyPatch(MethodType.Getter)]
  public static class BaseDescriptionDef_UINameGet {
    public static void Postfix(DescriptionDef __instance, ref string __result) {
      if (string.IsNullOrEmpty(__result)) { return; }
      string unlock = __result;
      Text_Append.Localize(ref __result);
      Log.LogWrite("DescriptionDef.UIName " + __instance.Id + " " + unlock + "->" + __result + "\n");
    }
  }
  [HarmonyPatch(typeof(BaseDescriptionDef))]
  [HarmonyPatch("Name")]
  [HarmonyPatch(MethodType.Getter)]
  public static class BaseDescriptionDef_NameGet {
    public static void Postfix(DescriptionDef __instance, ref string __result) {
      if (string.IsNullOrEmpty(__result)) { return; }
      string unlock = __result;
      Text_Append.Localize(ref __result);
      Log.LogWrite("BaseDescriptionDef.Name " + __instance.Id + " " + unlock + "->" + __result + "\n");
    }
  }
  [HarmonyPatch(typeof(BaseDescriptionDef))]
  [HarmonyPatch("Details")]
  [HarmonyPatch(MethodType.Getter)]
  public static class BaseDescriptionDef_DetailsGet {
    public static void Postfix(DescriptionDef __instance, ref string __result) {
      if (string.IsNullOrEmpty(__result)) { return; }
      string unlock = __result;
      Text_Append.Localize(ref __result);
      Log.LogWrite("DescriptionDef.Details " + __instance.Id + " " + unlock + "->" + __result + "\n");
    }
  }
}