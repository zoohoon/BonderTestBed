using System.Windows.Input;

namespace ProberInterfaces
{
    public interface ISoakingRecipeSettingVM
    {
        SoakingRecipeDataDescription GetSoakingRecipeDataDescription();

        //ObservableCollection<string> EventSoakingTypelist { get; set; } // SoakingModule을 통해서 얻어올 수 있음

        string SelectedItem { get; set; }

        ICommand DropDownClosedCommand { get; }

        int TabControlSelectedIndex { get; set; }

        //RecipeEditorParamEditViewModel RecipeEditorParamEdit
    }
}
