//Code for Elements/PercentBarCustom (Container)
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
namespace ToroidalWorld.Components.Elements;
partial class PercentBarCustom : global::Gum.Forms.Controls.FrameworkElement
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void RegisterRuntimeType()
    {
        var template = new global::Gum.Forms.VisualTemplate((vm, createForms) =>
        {
            var visual = new global::MonoGameGum.GueDeriving.ContainerRuntime();
            var element = ObjectFinder.Self.GetElementSave("Elements/PercentBarCustom");
#if DEBUG
if(element == null) throw new System.InvalidOperationException("Could not find an element named Elements/PercentBarCustom - did you forget to load a Gum project?");
#endif
            element.SetGraphicalUiElement(visual, RenderingLibrary.SystemManagers.Default);
            if(createForms) visual.FormsControlAsObject = new PercentBarCustom(visual);
            return visual;
        });
        global::Gum.Forms.Controls.FrameworkElement.DefaultFormsTemplates[typeof(PercentBarCustom)] = template;
        ElementSaveExtensions.RegisterGueInstantiation("Elements/PercentBarCustom", () => 
        {
            var gue = template.CreateContent(null, true) as InteractiveGue;
            return gue;
        });
    }
    public enum BarDecorCategory
    {
        None,
        CautionLines,
        VerticalLines,
    }

    BarDecorCategory? _barDecorCategoryState;
    public BarDecorCategory? BarDecorCategoryState
    {
        get => _barDecorCategoryState;
        set
        {
            _barDecorCategoryState = value;
            if(value != null)
            {
                if(Visual.Categories.ContainsKey("BarDecorCategory"))
                {
                    var category = Visual.Categories["BarDecorCategory"];
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
                else
                {
                    var category = ((global::Gum.DataTypes.ElementSave)this.Visual.Tag).Categories.FirstOrDefault(item => item.Name == "BarDecorCategory");
                    var state = category.States.Find(item => item.Name == value.ToString());
                    this.Visual.ApplyState(state);
                }
            }
        }
    }
    public TextRuntime Normal { get; protected set; }
    public ContainerRuntime ContainerInstance { get; protected set; }
    public NineSliceRuntime Background { get; protected set; }
    public StackPanel StackPanelInstance { get; protected set; }
    public NineSliceRuntime BarContainer { get; protected set; }
    public NineSliceRuntime Bar { get; protected set; }
    public CautionLines CautionLinesInstance { get; protected set; }
    public VerticalLines VerticalLinesInstance { get; protected set; }


    public float BarPercent
    {
        get => Bar.Width;
        set => Bar.Width = value;
    }

    public string Value
    {
        get => Normal.Text;
        set => Normal.Text = value;
    }

    public PercentBarCustom(InteractiveGue visual) : base(visual)
    {
    }
    public PercentBarCustom()
    {



    }
    protected override void ReactToVisualChanged()
    {
        base.ReactToVisualChanged();
        Normal = this.Visual?.GetGraphicalUiElementByName("Normal") as global::MonoGameGum.GueDeriving.TextRuntime;
        ContainerInstance = this.Visual?.GetGraphicalUiElementByName("ContainerInstance") as global::MonoGameGum.GueDeriving.ContainerRuntime;
        Background = this.Visual?.GetGraphicalUiElementByName("Background") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        StackPanelInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<StackPanel>(this.Visual,"StackPanelInstance");
        BarContainer = this.Visual?.GetGraphicalUiElementByName("BarContainer") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        Bar = this.Visual?.GetGraphicalUiElementByName("Bar") as global::MonoGameGum.GueDeriving.NineSliceRuntime;
        CautionLinesInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<CautionLines>(this.Visual,"CautionLinesInstance");
        VerticalLinesInstance = global::Gum.Forms.GraphicalUiElementFormsExtensions.TryGetFrameworkElementByName<VerticalLines>(this.Visual,"VerticalLinesInstance");
        CustomInitialize();
    }
    //Not assigning variables because Object Instantiation Type is set to By Name rather than Fully In Code
    partial void CustomInitialize();
}
