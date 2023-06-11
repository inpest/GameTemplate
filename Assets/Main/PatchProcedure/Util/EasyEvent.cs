using System;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{

    public static class EasyEvent
    {
        public static Dictionary<string, Action<object[]>> events = new Dictionary<string, Action<object[]>>();

        public static void Register(string str, Action<object[]> action)
        {
            events[str] = action;
        }

        public static void Fire(string str, params object[] pars)
        {
            if (!events.ContainsKey(str))
            {
                Debug.LogError("事件不存在");
                return;
            }
            events[str]?.Invoke(pars);
        }

        public static void UnRegister(string str)
        {
            events[str] = null;
            events.Remove(str);
        }
    }
}
