//Code for TitleScreenView
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
using ToroidalWorld.Components.Controls;
namespace ToroidalWorld.Screens;
partial class TitleScreenView : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("TitleScreenView");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named TitleScreenView - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new TitleScreenView(visual);
            visual.Width = 0;
            visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            visual.Height = 0;
            visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(TitleScreenView)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("TitleScreenView", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public ButtonStandardCustom StartButton { get; protected set; }
    public ButtonStandardCustom LeaderboardButton { get; protected set; }
    public ButtonStandardCustom OptionsButton { get; protected set; }
    public ButtonStandardCustom ExitButton { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }
    public ContainerRuntime ContainerInstance1 { get; protected set; }
    public Label LabelInstance { get; protected set; }
    public ContainerRuntime ContainerInstance { get; protected set; }

    public TitleScreenView(InteractiveGue visual) : base(visual)
    {
    }
    public TitleScreenView()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        StartButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonStandardCustom>(this.Visual,"StartButton");
        LeaderboardButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonStandardCustom>(this.Visual,"LeaderboardButton");
        OptionsButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonStandardCustom>(this.Visual,"OptionsButton");
        ExitButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonStandardCustom>(this.Visual,"ExitButton");
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        ContainerInstance1 = this.Visual?.GetGraphicalUiElementByName("ContainerInstance1") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        LabelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"LabelInstance");
        ContainerInstance = this.Visual?.GetGraphicalUiElementByName("ContainerInstance") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
