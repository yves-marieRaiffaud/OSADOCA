using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ObjHand = CommonMethods.ObjectsHandling;
using Planets = CelestialBodiesConstants;
using System.Linq;
using System;

namespace MissionAnalysis
{
    public static class GroundTracks
    {
        // Ground Track Type
        public enum GroundTrackType { bodyFixed, bodyRotating, both };
        public static Dictionary<GroundTrackType, string> gtType_Names = new Dictionary<GroundTrackType, string>(){
            { GroundTrackType.bodyFixed   , "Body-Fixed GT" },
            { GroundTrackType.bodyRotating, "Body-Rotating GT" },
            { GroundTrackType.both        , "Both" },
        };

        // Ground Track Points of Interest
        [Flags]
        public enum GroundTrack_POI { None=0, apocentre=1, pericentre=2, ascendingNode=4, descendingNode=8, Everything=16 };
        public static Dictionary<GroundTrack_POI, string> gtPOI_Names = new Dictionary<GroundTrack_POI, string>() {
            { GroundTrack_POI.None          , "None" },
            { GroundTrack_POI.apocentre     , "Apocentre" },
            { GroundTrack_POI.pericentre    , "Pericenter" },
            { GroundTrack_POI.ascendingNode , "Ascending Node" },
            { GroundTrack_POI.descendingNode, "Descending Node" },
            { GroundTrack_POI.Everything    , "Everything" }
        };
    }
}