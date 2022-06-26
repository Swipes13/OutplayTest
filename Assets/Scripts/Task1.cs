using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public interface Racer {
    public bool IsAlive();
    public bool IsCollidable();
    public bool CollidesWith(Racer other);
    public void Destroy();
    public void Update(float dt);

    // !B! public setter    
    public bool IsAlive_B { get; set; }

    // This is for test
    public int RandomValueForCollide { get; set; }
}

public class RacerImpl : Racer {
    public int RandomValueForCollide { get; set; }
    
    private bool _isAlive = true;    
    public bool IsAlive_B { get => _isAlive; set => _isAlive = value; }
    public bool IsAlive() => _isAlive;
    
    private bool _isCollidable = true;
    public bool IsCollidable() => _isCollidable;
    
    public bool CollidesWith(Racer other) => other.RandomValueForCollide == RandomValueForCollide;

    public RacerImpl(bool isAlive, bool isCollidable, int randomValueForCollide) {
        this._isAlive = isAlive;
        this._isCollidable = isCollidable;
        this.RandomValueForCollide = randomValueForCollide;
    }
    public void Update(float dt) { }
    public void Destroy() { }
}

[RequireComponent(typeof(Task1Properties))]
public class Task1 : MonoBehaviour {
    [InspectorButton("TestClick", 120)]
    public bool test;
    private void TestClick() => Test();

    private void Start() => Test();

    public void Test() {
        if (!Application.isPlaying) {
            Debug.Log("Only in Play mode!");
            return;
        }
        var props = this.GetComponent<Task1Properties>();
        var collidePercent = 1f - props.CollisionPercent;
        var randCountCollideCount = Mathf.FloorToInt(collidePercent * props.CarCount);

        var racers1 = new List<Racer>(props.CarCount);
        var racers2 = new List<Racer>(props.CarCount);
        var racers3 = new List<Racer>(props.CarCount);
        for (var i = 0; i < props.CarCount; i++) {
            var isAlive = Random.value <= props.StartAlivePercent;
            var isCollidable = Random.value <= props.StartCollidablePercent;
            var carCollideFx = Random.Range(0, randCountCollideCount - 1);
            racers1.Add(new RacerImpl(isAlive, isCollidable, carCollideFx));
            racers2.Add(new RacerImpl(isAlive, isCollidable, carCollideFx));
            racers3.Add(new RacerImpl(isAlive, isCollidable, carCollideFx));
        }

        var dtTime = 1.0f;

        var startData = System.DateTime.Now;
        UpdateRacers(dtTime, racers1);
        Debug.Log($"[TEST1] {racers1.Count} -> {(System.DateTime.Now - startData).TotalMilliseconds}");
        
        startData = System.DateTime.Now;
        UpdateRacersA(dtTime, racers2);
        Debug.Log($"[TEST2] {racers2.Count} -> {(System.DateTime.Now - startData).TotalMilliseconds}");

        startData = System.DateTime.Now;
        UpdateRacersB(dtTime, ref racers3);
        Debug.Log($"[TEST3] {racers3.Count} -> {(System.DateTime.Now - startData).TotalMilliseconds}");
    }

    private void OnRacerExplodes(Racer racer) { }
    
    // !B! explode function with Destroy. If have explosion -> 100% need Destroy after. so
    private void OnRacerExplodesWithDestroy(Racer racer) { 
        OnRacerExplodes(racer);
        racer.Destroy(); 
    }

    private void UpdateRacers(float deltaTimeS, List<Racer> racers) {
        List<Racer> racersNeedingRemoved = new List<Racer>();
        racersNeedingRemoved.Clear();
        
        // Updates the racers that are alive
        int racerIndex = 0;
        for (racerIndex = 1; racerIndex <= 1000; racerIndex++)
        {
            if (racerIndex <= racers.Count)
            {
                if (racers[racerIndex - 1].IsAlive())
                {
                    //Racer update takes milliseconds
                    racers[racerIndex - 1].Update(deltaTimeS * 1000.0f);
                } 
            }
        }
        // Collides
        for (int racerIndex1 = 0; racerIndex1 < racers.Count; racerIndex1++)
        {
            for (int racerIndex2 = 0; racerIndex2 < racers.Count; racerIndex2++)
            {
                Racer racer1 = racers[racerIndex1];
                Racer racer2 = racers[racerIndex2];
                if (racerIndex1 != racerIndex2)
                {
                    if (racer1.IsCollidable() && racer2.IsCollidable() && racer1.CollidesWith(racer2))
                    {   // !POSSIBLE BUG! racer1 can be exploded many times if racer1 have more than 1 collision.
                        // !POSSIBLE BUG! racer1 or racer2 could be IsAlive() == false.
                        OnRacerExplodes(racer1);
                        racersNeedingRemoved.Add(racer1);
                        racersNeedingRemoved.Add(racer2);
                    }
                }
            }
        }

        // Gets the racers that are still alive
        List<Racer> newRacerList = new List<Racer>();
        for (racerIndex = 0; racerIndex != racers.Count; racerIndex++)
        {
            // check if this racer must be removed
            if (racersNeedingRemoved.IndexOf(racers[racerIndex]) < 0)
            {
                newRacerList.Add(racers[racerIndex]);
            }
        }
        // Get rid of all the exploded racers
        for (racerIndex = 0; racerIndex != racersNeedingRemoved.Count; racerIndex++)
        {
            int foundRacerIndex = racers.IndexOf(racersNeedingRemoved[racerIndex]);
            if (foundRacerIndex >= 0) // Check we&#39;ve not removed this already!
            {
                racersNeedingRemoved[racerIndex].Destroy();
                racers.Remove(racersNeedingRemoved[racerIndex]);
            }
        }
        // Builds the list of remaining racers
        racers.Clear();
        for (racerIndex = 0; racerIndex < newRacerList.Count; racerIndex++)
        {
            racers.Add(newRacerList[racerIndex]);
        }
        for (racerIndex = 0; racerIndex < newRacerList.Count; racerIndex++)
        {
            newRacerList.RemoveAt(0);
        }
    }

    private void UpdateRacersA(float deltaTimeS, List<Racer> racers) {
        // Updates the racers that are alive
        racers.ForEach(r => { 
            if (r.IsAlive()) r.Update(deltaTimeS * 1000.0f);
        });
        
        // Collides
        // ! First take only collidable -> performance impr
        // ! We need to implement IEquatable interface, but I assume that racers List filled with uniq instances
        var toDestroyHS = new HashSet<Racer>(); 
        var collidableRacers = racers.Where(r => r.IsCollidable()).ToList();
        for (var i = 0; i < collidableRacers.Count - 1; i++) {
            var r1 = collidableRacers[i];
            for (var j = i + 1; j < collidableRacers.Count; j++) {
                var r2 = collidableRacers[j];
                if (r1.CollidesWith(r2)) {
                    toDestroyHS.Add(r1);
                    toDestroyHS.Add(r2);
                }
            }
        }
        toDestroyHS.ToList().ForEach(r => {
            OnRacerExplodes(r);
            r.Destroy();
            racers.Remove(r);
        });
    }
    private void UpdateRacersB(float deltaTimeS, ref List<Racer> racers) {
        // Updates the racers that are alive
        var alive = racers.Where(r => r.IsAlive());
        alive.ToList().ForEach(r => r.Update(deltaTimeS * 1000.0f));
        
        // Collides
        // !B! check only alive ones collide. I think there was a possible bug
        var collidableAlive = alive.Where(r => r.IsCollidable()).ToList();
        for (var i = 0; i < collidableAlive.Count - 1; i++) {
            var r1 = collidableAlive[i];
            for (var j = i + 1; j < collidableAlive.Count; j++) {
                var r2 = collidableAlive[j];
                if (r1.CollidesWith(r2)) {
                    // !B! public setter
                    r1.IsAlive_B = false; 
                    r2.IsAlive_B = false; 
                }
            }
        }

        // !B! lookup without HashSet by IsAlive_B
        var lookupItems = racers.ToLookup(r => r.IsAlive_B);
        // !B! racers modified correctly
        racers = lookupItems[true].ToList();
        // !B! every racer that should be exploded and destroyed -> bye bye 
        lookupItems[false].ToList().ForEach(r => OnRacerExplodesWithDestroy(r));
    }

}
