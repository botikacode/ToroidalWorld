//Code for Controls/StatsLabel1 (Container)
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
using ToroidalWorld.Components.Elements;
namespace ToroidalWorld.Components.Controls;
partial class StatsLabel1 : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Controls/StatsLabel1");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named Controls/StatsLabel1 - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new StatsLabel1(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(StatsLabel1)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Controls/StatsLabel1", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public Icon Icon { get; protected set; }
    public Label StatValue { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }

    public string AtributeValueText
    {
        get => StatValue.Text;
        set => StatValue.Text = value;
    }

    public StatsLabel1(InteractiveGue visual) : base(visual)
    {
    }
    public StatsLabel1()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Icon = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Icon>(this.Visual,"Icon");
        StatValue = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"StatValue");
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
