using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck;
using Duck.Content;
using Duck.Graphics;
using Duck.Platform;
using Duck.Ui;
using Duck.Ui.Elements;
using Game.Components;

namespace Game.Systems;

public partial class MainMenuSystem : BaseSystem<World, float>
{
    private readonly IUIModule _uiModule;
    private readonly IContentModule _contentModule;
    private readonly IScene _scene;
    private readonly IRendererModule _rendererModule;
    private readonly IApplication _app;

    private Action _onPlayClicked;
    private Action _onQuitClicked;

    public MainMenuSystem(World world, IScene scene, IUIModule uiModule, IContentModule contentModule, IRendererModule rendererModule, IApplication app)
        : base(world)
    {
        _uiModule = uiModule;
        _contentModule = contentModule;
        _rendererModule = rendererModule;
        _app = app;
        _scene = scene;

        _onPlayClicked = OnPlayClicked;
        _onQuitClicked = OnQuitClicked;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in MainMenuComponent mainMenu)
    {
        var c = _uiModule.GetContextForScene(_scene);

        c.New(
            RootProps.Default with {
                Font = _contentModule.Database.GetAsset<Font>(new Uri("file:///Builtin/Fonts/Manrope/Medium.arfont")).MakeSharedReference(),
                Box = Box.Default with {
                    Margin = BoxArea.All(2f)
                }
            },
            c.VerticalContainer(
                VerticalContainerProps.Default with {
                    VerticalAlignment = VerticalAlign.Bottom,
                    Box = Box.Default,
                },
                c.Button(
                    ButtonProps.Default with {
                        BackgroundColor = Color.DarkGray,
                        Box = Box.Default with {
                            ContentWidth = 6,
                            ContentHeight = 2,
                            Padding = BoxArea.All(2f),
                        },
                    },
                    c.Label(LabelProps.Default with {
                            Content = "Play",
                        }
                    ),
                    _onPlayClicked
                ),
                c.Button(
                    ButtonProps.Default with {
                        BackgroundColor = Color.DarkGray,
                        Box = Box.Default with {
                            ContentWidth = 6,
                            ContentHeight = 2,
                            Padding = BoxArea.All(2f),
                        },
                    },
                    c.Label(LabelProps.Default with {
                            Content = "Quit",
                        }
                    ),
                    _onQuitClicked
                )
            )
        );
    }

    private void OnPlayClicked()
    {
        _rendererModule.GetOrCreateScene(GameConstants.LevelRound);
    }

    private void OnQuitClicked()
    {
        _app.Shutdown();
    }
}
