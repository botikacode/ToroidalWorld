//Code for Controls/NameValueLabel (Container)
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
namespace ToroidalWorld.Components.Controls;
partial class NameValueLabel : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Controls/NameValueLabel");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named Controls/NameValueLabel - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new NameValueLabel(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(NameValueLabel)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Controls/NameValueLabel", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public Label LabelInstance { get; protected set; }
    public Label LabelInstance1 { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }

    public string NameText
    {
        get => LabelInstance.Text;
        set => LabelInstance.Text = value;
    }

    public string ValueText
    {
        get => LabelInstance1.Text;
        set => LabelInstance1.Text = value;
    }

    public NameValueLabel(InteractiveGue visual) : base(visual)
    {
    }
    public NameValueLabel()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        LabelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"LabelInstance");
        LabelInstance1 = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"LabelInstance1");
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
