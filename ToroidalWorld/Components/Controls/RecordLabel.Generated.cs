//Code for Controls/RecordLabel (Container)
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
partial class RecordLabel : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Controls/RecordLabel");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named Controls/RecordLabel - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new RecordLabel(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(RecordLabel)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Controls/RecordLabel", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public Label NumAttemptLabel { get; protected set; }
    public Label DateLabel { get; protected set; }
    public Label DateValue { get; protected set; }
    public Label TimeLabel { get; protected set; }
    public Label TimeValue { get; protected set; }
    public Label KillsLabel { get; protected set; }
    public Label KillsValue { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }

    public string AtributeNameText
    {
        get => TimeLabel.Text;
        set => TimeLabel.Text = value;
    }

    public string AtributeValueText
    {
        get => TimeValue.Text;
        set => TimeValue.Text = value;
    }

    public RecordLabel(InteractiveGue visual) : base(visual)
    {
    }
    public RecordLabel()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        NumAttemptLabel = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"NumAttemptLabel");
        DateLabel = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"DateLabel");
        DateValue = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"DateValue");
        TimeLabel = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"TimeLabel");
        TimeValue = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"TimeValue");
        KillsLabel = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"KillsLabel");
        KillsValue = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"KillsValue");
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
