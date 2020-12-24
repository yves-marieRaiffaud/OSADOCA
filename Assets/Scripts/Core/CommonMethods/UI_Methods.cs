using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using Mathd_Lib;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Universe;

namespace CommonMethods
{
    public static class UI_Methods
    {
        public static void SetUpDropdown(TMPro.TMP_Dropdown dropdown, List<string> dataList)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(dataList);
        }
    }
}