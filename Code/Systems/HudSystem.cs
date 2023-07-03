using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck.Content;
using Duck.Ui;
using Duck.Ui.Elements;
using Game.Components;

namespace Game.Systems;

public partial class HudSystem : BaseSystem<World, float>
{
    private readonly IUiModule _uiModule;
    private readonly IContentModule _contentModule;
    private readonly QueryDescription _planetQueryDescription;
    private readonly QueryDescription _enemyQueryDescription;

    private readonly List<Entity> _planets = new(100);

    public HudSystem(World world, IUiModule uiModule, IContentModule contentModule)
        : base(world)
    {
        _uiModule = uiModule;
        _contentModule = contentModule;

        _planetQueryDescription = new QueryDescription();
        _planetQueryDescription.WithAll<PlanetTag>();

        _enemyQueryDescription = new QueryDescription();
        _enemyQueryDescription.WithAll<EnemyTag>();
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in HudComponent hud)
    {
        var c = _uiModule.Context;

        _planets.Clear();
        World.GetEntities(_planetQueryDescription, _planets);

        var health = 0;

        if (_planets.Count > 0) {
            health = _planets[0].Get<HealthComponent>().Value;
        }

        c.New(
            RootProps.Default with {
                Font = _contentModule.Database.GetAsset<Font>(new Uri("file:///Builtin/Fonts/Manrope/Medium.arfont")).MakeSharedReference(),
                Box = Box.Default with {
                    Padding = BoxArea.All(2)
                }
            },
            c.HorizontalContainer(
                HorizontalContainerProps.Default with {
                    HorizontalAlignment = HorizontalAlign.Left, 
                },
                c.Panel(
                    PanelProps.Default with {
                        BackgroundColor = Color.DarkGray,
                        Box = Box.Default with {
                            ContentWidth = 12,
                            ContentHeight = 6,
                            Padding = BoxArea.All(2),
                        },
                    },
                    c.Label(LabelProps.Default with {
                            Content = "Asteroids: " + World.CountEntities(_enemyQueryDescription),
                        }
                    )
                ),
                c.Panel(
                    PanelProps.Default with {
                        BackgroundColor = Color.DarkGray,
                        Box = Box.Default with {
                            ContentWidth = 12,
                            ContentHeight = 6,
                            Padding = BoxArea.All(2),
                        },
                    },
                    c.Label(LabelProps.Default with {
                            Content = "Lives: " + (health / 50),
                        }
                    )
                )
            )
        );
    }
}
