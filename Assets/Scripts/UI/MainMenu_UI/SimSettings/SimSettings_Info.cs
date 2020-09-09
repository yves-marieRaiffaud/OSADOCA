using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimSettings_Info
{
    public string title;
    // bool in the dictionary indicates if the string is a formula
    public Dictionary<string, bool> text_strings;

    public SimSettings_Info(string _title, Dictionary<string, bool> _text_strings)
    {
        title=_title;
        text_strings=_text_strings;
    }
}

public static class SimSettings_InfoList
{
    // hg stands for highlightning color
    public const string hgColor = "#ffd53d";

    public static SimSettings_Info simulateGravity = new SimSettings_Info("Simulate Gravity", new Dictionary<string, bool>() {
        { "Should Stars and Planets have gravitational pull on other bodies, Stars, Planets and Spaceships ?\n\nIf enabled, one object, such as a planet or a spacecraft, will have the following acceleration:", false},
        { "\\opens{ \\color["+hgColor+"]{ {a}^^{\\rightarrow} = \\frac{\\mu}{r^3}.{r}^^{\\rightarrow} } }", true },
        { "\\opens[i] {where \\color["+hgColor+"]{{a}^^{\\rightarrow}} is the object's acceleration, \\color["+hgColor+"]{\\mu} the standard gravitational parameter of the orbited body, \\color["+hgColor+"]{{r}^^{\\rightarrow}} the radial vector from the object to the orbited body's center, and \\color["+hgColor+"]{r} its magnitude}", true }
    });

    public static SimSettings_Info enableNBodySim = new SimSettings_Info("Enable N-Body Simulation", new Dictionary<string, bool>() {
        { "Should one object; planet or spacecraft; be affected by the gravitational pull of every other Stars and Planets ?\n\nIf disabled, the object will be only affected by the body it is orbiting.\nIf enabled, the N-Body simulator will have those steps:", false },
        { "\t- Compute and sort in ascending order the gravitational pull on the object from every celestial body in the simulator", false },
        { "\t- Sum over the N most important accelerations and apply them to the object", false }
    });

    public static SimSettings_Info Nb_NBodySim = new SimSettings_Info("N-Body Value", new Dictionary<string, bool>() {
        { "If 'Enable N-Body Simulation' is enabled, this value defines the number of celestial bodies that will have a gravitational pull on the object, a planet or a spaceship.\n\nThe object will thus have the following acceleration:", false },
        { "\\opens{ \\color["+hgColor+"]{ {a}^^{\\rightarrow} = \\sum__{i=1}^^{N_{bodies}}  ({ \\frac{\\mu_i} {r_i^3}.{r}^^{\\rightarrow}_i }) } }", true },
        { "\\opens[i] {where \\color["+hgColor+"]{N_{bodies}} is the selected N-Body value }", true }
    });

    public static SimSettings_Info enableFPSTargetting = new SimSettings_Info("Enable FPS targetting", new Dictionary<string, bool>() {
        { "Should the rendering engine targets a specific value of frame per seconds (FPS) ?\n\nIf disabled, the engine will render as many images as possible per second.\nIf enabled, the parameter 'Target FPS value' will be used as target", false }
    });

    public static SimSettings_Info fpsTargetValue = new SimSettings_Info("FPS Target Value", new Dictionary<string, bool>() {
        { "Number of frames per second that the rendering engine should target", false }
    });

    public static SimSettings_Info physicsUpdateRate = new SimSettings_Info("Physics Update Rate", new Dictionary<string, bool>() {
        { "Defines the frequency at which the physics engine works.\n\n If set to 50 Hz, the physics engine will be called every 0.02 second", false }
    });

    public static SimSettings_Info timeScale = new SimSettings_Info("Time Scale", new Dictionary<string, bool>() {
        { "Defines the time scale of the physics engine.\n\n If set to 10, 1 second in the real-world is equivalent to 10 seconds in the physics engine.\n\n However and as the 'Physics Update Rate' parameter is not modified by the time scale, a signficant drop of performance can be expected:\n With a time scale of 10, the physics engine will be called 10 times more within the user time frame", false }
    });
}