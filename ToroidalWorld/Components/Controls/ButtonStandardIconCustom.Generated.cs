//Code for Controls/ButtonStandardIconCustom (Container)
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
partial class ButtonStandardIconCustom : global::Gum.Forms.Controls.Button
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Controls/ButtonStandardIconCustom");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named Controls/ButtonStandardIconCustom - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new ButtonStandardIconCustom(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(ButtonStandardIconCustom)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Controls/ButtonStandardIconCustom", () => 
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
    public SpriteRuntime SpriteInstance { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }
    public TextRuntime TurretNameLabel { get; protected set; }
    public TextRuntime TurretDescriptionLabel { get; protected set; }
    public NineSliceRuntime Background { get; protected set; }
    public StackPanel StackPanelInstance1 { get; protected set; }
    public NineSliceRuntime FocusedIndicator { get; protected set; }

    public string SpriteInstanceSourceFile
    {
        set => SpriteInstance.SourceFileName = value;
    }

    public override string Text
    {
        get => TurretNameLabel.Text;
        set => TurretNameLabel.Text = value;
    }

    public ButtonStandardIconCustom(InteractiveGue visual) : base(visual)
    {
    }
    public ButtonStandardIconCustom()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        SpriteInstance = this.Visual?.GetGraphicalUiElementByName("SpriteInstance") as global::MonoGameGum.GueDeriving.SpriteRuntime;
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        TurretNameLabel = this.Visual?.GetGraphicalUiElementByName("TurretNameLabel") as global::MonoGameGum.GueDeriving.TextRuntime;
        TurretDescriptionLabel = this.Visual?.GetGraphicalUiElementByName("TurretDescriptionLabel") as global::MonoGameGum.GueDeriving.TextRuntime;
        Background = this.Visual?.GetGraphicalUiElementByName("Background") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        StackPanelInstance1 = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance1");
        FocusedIndicator = this.Visual?.GetGraphicalUiElementByName("FocusedIndicator") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
