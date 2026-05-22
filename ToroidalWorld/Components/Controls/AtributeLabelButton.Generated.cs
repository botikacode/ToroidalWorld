//Code for Controls/AtributeLabelButton (Container)
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
partial class AtributeLabelButton : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Controls/AtributeLabelButton");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named Controls/AtributeLabelButton - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new AtributeLabelButton(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(AtributeLabelButton)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Controls/AtributeLabelButton", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public Label AtributeName { get; protected set; }
    public Label Value { get; protected set; }
    public ButtonStandardCustom ButtonStandardCustomInstance { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }

    public string AtributeNameText
    {
        get => AtributeName.Text;
        set => AtributeName.Text = value;
    }

    public string AtributeValueText
    {
        get => Value.Text;
        set => Value.Text = value;
    }

    public AtributeLabelButton(InteractiveGue visual) : base(visual)
    {
    }
    public AtributeLabelButton()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        AtributeName = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"AtributeName");
        Value = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Label>(this.Visual,"Value");
        ButtonStandardCustomInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<ButtonStandardCustom>(this.Visual,"ButtonStandardCustomInstance");
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
