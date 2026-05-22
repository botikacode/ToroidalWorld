//Code for Controls/ButtonIconCustom (Container)
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using System.Linq;
using ToroidalWorld.Components.Elements;
namespace ToroidalWorld.Components.Controls;
partial class ButtonIconCustom : global::Gum.Forms.Controls.Button
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Controls/ButtonIconCustom");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named Controls/ButtonIconCustom - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new ButtonIconCustom(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(ButtonIconCustom)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Controls/ButtonIconCustom", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public enum ButtonCategory
    {
        Enabled,
        Disabled,
        Highlighted,
        Pushed,
        HighlightedFocused,
        Focused,
        DisabledFocused,
    }

    ButtonCategory? _buttonCategoryState;
    public ButtonCategory? ButtonCategoryState
    {
        get => _buttonCategoryState;
        set
        {
            _buttonCategoryState = value;
            if(value != null)
            {
                if(Visual.Categories.ContainsKey("ButtonCategory"))
                {
                    var category = Visual.Categories["ButtonCategory"];
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
                else
                {
                    var category = ((global::Gum.DataTypes.ElementSave)this.Visual.Tag).Categories.FirstOrDefault(item => item.Name == "ButtonCategory");
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
            }
        }
    }
    public Icon Icon { get; protected set; }
    public NineSliceRuntime FocusedIndicator { get; protected set; }

    public Icon.IconCategory? IconIconCategoryState
    {
        get => Icon.IconCategoryState;
        set => Icon.IconCategoryState = value;
    }

    public float IconRotation
    {
        get => Icon.Visual.Rotation;
        set => Icon.Visual.Rotation = value;
    }

    public ButtonIconCustom(InteractiveGue visual) : base(visual)
    {
    }
    public ButtonIconCustom()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Icon = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<Icon>(this.Visual,"Icon");
        FocusedIndicator = this.Visual?.GetGraphicalUiElementByName("FocusedIndicator") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
