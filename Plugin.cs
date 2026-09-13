using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Bassment
{
    [BepInPlugin("xyz.eggstudios.bassment", "Bassment", "0.3.2")]
    public class BassmentPlugin : BaseUnityPlugin
    {
        private const string OldMotdHost = "http://www.againstgrav.com";
        private const string OldApiHost = "http://recroomwebapplication2.azurewebsites.net";

        internal static ConfigEntry<string> RevivalBaseUrl;
        internal static ConfigEntry<bool> LogRedirects;
        internal static ConfigEntry<string> PhotonHost;
        internal static ConfigEntry<int> PhotonPort;
        internal static ConfigEntry<string> PhotonApp;

        internal static Harmony HarmonyInstance;
        internal static bool PhotonPatchApplied;

        private void Awake()
        {
            RevivalBaseUrl = Config.Bind("General", "RevivalBaseUrl", "http://X.X.X.X", "");
            LogRedirects = Config.Bind("General", "LogRedirects", true, "");
            PhotonHost = Config.Bind("Photon", "PhotonHost", "X.X.X.X", "");
            PhotonPort = Config.Bind("Photon", "PhotonPort", 5055, "");
            PhotonApp = Config.Bind("Photon", "PhotonApp", "Master", "");

            Logger.LogInfo("[Bassment] redirecting 2016 API to " + RevivalBaseUrl.Value.TrimEnd('/'));

            Harmony harmony = new Harmony("xyz.eggstudios.bassment");
            HarmonyInstance = harmony;
            harmony.PatchAll(typeof(WWWPatches));
            harmony.PatchAll(typeof(WebRequestStringPatch));
            harmony.PatchAll(typeof(WebRequestUriPatch));
            harmony.PatchAll(typeof(BootActivityPatch));
            harmony.PatchAll(typeof(PhotonEnsurePatch));
            Logger.LogInfo("[Bassment] patches applied.");
        }

        internal static string RewriteUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            string target = RevivalBaseUrl.Value.TrimEnd('/');
            string original = url;

            if (url.StartsWith(OldMotdHost, StringComparison.OrdinalIgnoreCase))
            {
                url = target + url.Substring(OldMotdHost.Length);
            }
            else if (url.StartsWith(OldApiHost, StringComparison.OrdinalIgnoreCase))
            {
                url = target + url.Substring(OldApiHost.Length);
            }
            else 
            {
                return url;
            }

            if (LogRedirects.Value)
            {
                Debug.Log("[Bassment] " + original + " -> " + url);
            }

            return url;
        }
    }

    [HarmonyPatch]
    internal static class WWWPatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (ConstructorInfo ctor in typeof(WWW).GetConstructors())
            {
                ParameterInfo[] ps = ctor.GetParameters();
                if (ps.Length > 0 && ps[0].ParameterType == typeof(string))
                {
                    yield return ctor;
                }
            }
        }

        private static void Prefix(ref string url)
        {
            url = BassmentPlugin.RewriteUrl(url);
        }
    }

    [HarmonyPatch(typeof(System.Net.WebRequest), "Create", new Type[] { typeof(string) })]
    internal static class WebRequestStringPatch
    {
        private static void Prefix(ref string requestUriString)
        {
            requestUriString = BassmentPlugin.RewriteUrl(requestUriString);
        }
    }

    [HarmonyPatch(typeof(System.Net.WebRequest), "Create", new Type[] { typeof(Uri) })]
    internal static class WebRequestUriPatch
    {
        private static void Prefix(ref Uri requestUri)
        {
            string rewritten = BassmentPlugin.RewriteUrl(requestUri.ToString());
            if (rewritten != requestUri.ToString()) 
            {
                requestUri = new Uri(rewritten);
            }
        }
    }

    [HarmonyPatch(typeof(BootSequence), "LoadInitialScene")]
    internal static class BootActivityPatch
    {
        private static bool Prefix()
        {
            if (UnityEngine.VR.VRDevice.isPresent)
            {
                PUNNetworkManager.Instance.SwitchActivity("Intro", null, false, 0, true);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PhotonNetwork), "ConnectUsingSettings", new Type[] { typeof(string) })]
    internal static class PhotonConnectPatch
    {
        private static void Prefix()
        {
            ServerSettings settings = PhotonNetwork.PhotonServerSettings;
            if (settings == null)
            {
                return;
            }
            settings.UseMyServer(BassmentPlugin.PhotonHost.Value, BassmentPlugin.PhotonPort.Value, BassmentPlugin.PhotonApp.Value);
            Debug.Log("[Bassment] photon self-hosted: " + BassmentPlugin.PhotonHost.Value + ":" + BassmentPlugin.PhotonPort.Value);
        }
    }

    [HarmonyPatch(typeof(PUNNetworkManager), "InitializeNewConnection")]
    internal static class PhotonEnsurePatch
    {
        private static void Prefix()
        {
            if (BassmentPlugin.PhotonPatchApplied)
            {
                return;
            }
            BassmentPlugin.PhotonPatchApplied = true;
            try
            {
                BassmentPlugin.HarmonyInstance.PatchAll(typeof(PhotonConnectPatch));
                Debug.Log("[Bassment] photon patch applied.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Bassment] photon patch failed: " + ex);
            }
        }
    }
}
