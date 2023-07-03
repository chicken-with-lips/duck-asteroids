using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck;
using Duck.Content;
using Duck.Renderer;
using Duck.Ui;
using Duck.Ui.Elements;
using Game.Components;

namespace Game.Systems;

public partial class MainMenuSystem : BaseSystem<World, float>
{
    private readonly IUiModule _uiModule;
    private readonly IContentModule _contentModule;
    private readonly IScene _scene;
    private readonly RendererModule _rendererModule;
    private readonly IApplication _app;

    public MainMenuSystem(World world, IUiModule uiModule, IContentModule contentModule)
        : base(world)
    {
        _uiModule = uiModule;
        _contentModule = contentModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in MainMenuComponent mainMenu)
    {
        var c = _uiModule.Context;

        c.New(
            RootProps.Default with {
                Font = _contentModule.Database.GetAsset<Font>(new Uri("file:///Builtin/Fonts/Manrope/Medium.arfont")).MakeSharedReference(),
                Box = Box.Default with {
                    Padding = BoxArea.All(2)
                }
            },
            c.VerticalContainer(
                VerticalContainerProps.Default with {
                    VerticalAlignment = VerticalAlign.Bottom,
                    Box = Box.Default with {
                        Padding = BoxArea.Default with {
                            Bottom = 5,
                        }
                    },
                },
                c.Panel(
                    PanelProps.Default with {
                        BackgroundColor = Color.DarkGray,
                        Box = Box.Default with {
                            ContentWidth = 12,
                            ContentHeight = 6,
                            Padding = BoxArea.All(2f) with {
                                Left = 5f
                            },
                        },
                    },
                    c.Label(LabelProps.Default with {
                            Content = "Play",
                        }
                    )
                ),
                c.Panel(
                    PanelProps.Default with {
                        BackgroundColor = Color.DarkGray,
                        Box = Box.Default with {
                            ContentWidth = 12,
                            ContentHeight = 6,
                            Padding = BoxArea.All(2) with {
                                Left = 5f
                            },
                        },
                    },
                    c.Label(LabelProps.Default with {
                            Content = "Quit",
                        }
                    )
                )
            )
        );

        /*var play = ui.Document.GetElementById("btn-play");

        if (play != null) {
            ui.AddEventListener(play, "click", @event => {
                _scene.IsActive = false;
                _rendererModule.GetOrCreateScene(GameConstants.LevelRound);
            });
        }

        var quit = ui.Document.GetElementById("btn-quit");

        if (quit != null) {
            ui.AddEventListener(quit, "click", @event => _app.Shutdown());
        }*/
    }
}
