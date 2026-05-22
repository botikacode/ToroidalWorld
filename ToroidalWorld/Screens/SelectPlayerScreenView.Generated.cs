//Code for SelectPlayerScreenView
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
partial class SelectPlayerScreenView : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("SelectPlayerScreenView");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named SelectPlayerScreenView - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new SelectPlayerScreenView(visual);
            visual.Width = 0;
            visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            visual.Height = 0;
            visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(SelectPlayerScreenView)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("SelectPlayerScreenView", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public SpriteRuntime SpriteInstance { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }
    public Label RemaingPointsLabel { get; protected set; }
    public ButtonIconCustom LeftButton { get; protected set; }
    public ContainerRuntime ContainerInstance { get; protected set; }
    public ButtonIconCustom RightButton { get; protected set; }
    public Label LabelInstance { get; protected set; }
    public StackPanel StackPanelInstance1 { get; protected set; }
    public ButtonStandardCustom SelectButton { get; protected set; }
    public Window WindowInstance { get; protected set; }

    public SelectPlayerScreenView(InteractiveGue visual) : base(visual)
    {
    }
    public SelectPlayerScreenView()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        SpriteInstance = this.Visual?.GetGraphicalUiElementByName("SpriteInstance") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        RemaingPointsLabel = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"RemaingPointsLabel");
        LeftButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonIconCustom>(this.Visual,"LeftButton");
        ContainerInstance = this.Visual?.GetGraphicalUiElementByName("ContainerInstance") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        RightButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonIconCustom>(this.Visual,"RightButton");
        LabelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"LabelInstance");
        StackPanelInstance1 = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance1");
        SelectButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonStandardCustom>(this.Visual,"SelectButton");
        WindowInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Window>(this.Visual,"WindowInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
