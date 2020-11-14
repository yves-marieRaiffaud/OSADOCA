using UnityEngine;
using System.Collections;
using ChartAndGraph;

public class GraphDataHandler : MonoBehaviour
{
    public GraphChart Graph;

    [SerializeField] UniverseRunner universeRunner;
    Spaceship ship;

    StopWatch missionTimer;
    public float updatePeriod = 1f;

    void Start()
    {
        if (Graph == null || universeRunner == null) // the ChartGraph info is obtained via the inspector
            return;

        missionTimer = universeRunner.simEnv.missionTimer;
        ship = universeRunner.activeSpaceship;

        Graph.DataSource.StartBatch(); // calling StartBatch allows changing the graph data without redrawing the graph for every change
        Graph.DataSource.ClearCategory("Velocity");
        Graph.DataSource.EndBatch(); // finally we call EndBatch , this will cause the GraphChart to redraw itself

        StartCoroutine(UpdateGraph());
    }

    IEnumerator UpdateGraph()
    {
        while(true)
        {
            yield return new WaitForSeconds(updatePeriod);
            Graph.DataSource.AddPointToCategoryRealtime("Velocity", System.DateTime.Now, ship.orbitedBodyRelativeAcc.magnitude, 1f);
        }
    }
}
