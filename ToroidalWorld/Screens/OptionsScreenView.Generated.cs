//Code for OptionsScreenView
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
partial class OptionsScreenView : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("OptionsScreenView");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named OptionsScreenView - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new OptionsScreenView(visual);
            visual.Width = 0;
            visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            visual.Height = 0;
            visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(OptionsScreenView)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("OptionsScreenView", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public Window WindowInstance { get; protected set; }
    public TextRuntime H3 { get; protected set; }
    public TextRuntime H2 { get; protected set; }
    public ButtonStandardCustom ToggleFullscreenButton { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }
    public StackPanel StackPanelInstance2 { get; protected set; }
    public ButtonStandardCustom DeleteProgressButton { get; protected set; }
    public StackPanel StackPanelInstance1 { get; protected set; }
    public TextRuntime Title { get; protected set; }
    public ButtonStandardCustom ExitOptionsButton { get; protected set; }
    public ContainerRuntime ContainerInstance1 { get; protected set; }
    public ContainerRuntime ContainerInstance { get; protected set; }

    public OptionsScreenView(InteractiveGue visual) : base(visual)
    {
    }
    public OptionsScreenView()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        WindowInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Window>(this.Visual,"WindowInstance");
        H3 = this.Visual?.GetGraphicalUiElementByName("H3") as global::MonoGameGum.GueDeriving.TextRuntime;
        H2 = this.Visual?.GetGraphicalUiElementByName("H2") as global::MonoGameGum.GueDeriving.TextRuntime;
        ToggleFullscreenButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonStandardCustom>(this.Visual,"ToggleFullscreenButton");
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        StackPanelInstance2 = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance2");
        DeleteProgressButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonStandardCustom>(this.Visual,"DeleteProgressButton");
        StackPanelInstance1 = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance1");
        Title = this.Visual?.GetGraphicalUiElementByName("Title") as global::MonoGameGum.GueDeriving.TextRuntime;
        ExitOptionsButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonStandardCustom>(this.Visual,"ExitOptionsButton");
        ContainerInstance1 = this.Visual?.GetGraphicalUiElementByName("ContainerInstance1") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        ContainerInstance = this.Visual?.GetGraphicalUiElementByName("ContainerInstance") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
