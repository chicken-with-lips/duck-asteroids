using System;
using Duck;
using Duck.Ecs;
using Duck.Scene;
using Duck.Ui;
using Duck.Ui.RmlUi;
using Duck.Ui.Scripting;
using Game.Components;
using Game.Components.Tags;

namespace Game.UI;

public class Hud : IUserInterfaceLoaded, IUserInterfaceTick
{
    private readonly IScene _scene;
    private readonly IFilter<EnemyTag> _enemyFilter;
    private RmlUserInterface? _ui;

    public Hud(IScene scene)
    {
        _scene = scene;
        _enemyFilter = new FilterBuilder<EnemyTag>(scene.World).Build();
    }

    ~Hud()
    {
        // FIXME: unbind events
        Console.WriteLine("FIXME: unbind events");
    }

    public void OnLoaded(RmlUserInterface ui)
    {
        _ui = ui;
    }

    public void OnTick()
    {
        _ui?.Document.GetElementById("enemy-count")?.SetInnerRml(_enemyFilter.EntityList.Length.ToString());


        var planets = _scene.World.GetEntitiesByComponent<PlanetTag>();
        var health = 0;

        if (planets.Length > 0) {
            health = _scene.World.GetComponent<HealthComponent>(planets[0].Id).Value;
        }

        _ui?.Document.GetElementById("planet-health")?.SetInnerRml((health / 50).ToString());
    }
}
