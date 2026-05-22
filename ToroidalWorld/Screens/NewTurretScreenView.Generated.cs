//Code for NewTurretScreenView
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
partial class NewTurretScreenView : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("NewTurretScreenView");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named NewTurretScreenView - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new NewTurretScreenView(visual);
            visual.Width = 0;
            visual.WidthUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            visual.Height = 0;
            visual.HeightUnits = global::Gum.DataTypes.DimensionUnitType.RelativeToParent;
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(NewTurretScreenView)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("NewTurretScreenView", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public Label LabelInstance { get; protected set; }
    public ButtonStandardIconCustom NewTurretButton { get; protected set; }
    public ButtonDenyCustom ButtonDeny { get; protected set; }
    public Window WindowInstance { get; protected set; }

    public NewTurretScreenView(InteractiveGue visual) : base(visual)
    {
    }
    public NewTurretScreenView()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        LabelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"LabelInstance");
        NewTurretButton = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonStandardIconCustom>(this.Visual,"NewTurretButton");
        ButtonDeny = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonDenyCustom>(this.Visual,"ButtonDeny");
        WindowInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Window>(this.Visual,"WindowInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
