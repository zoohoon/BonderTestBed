using System.Collections.Generic;

namespace ProberInterfaces.Wizard
{
    using ProberErrorCode;
    using System.Collections.ObjectModel;

    public interface IWizardManager : IFactoryModule , IHasDevParameterizable
    {
        string GetWizardRecipeBasePath();
        List<string> GetWizardRecipesName();
        EventCodeEnum CreateNewRecipe(string prerecipepath, string recipename);
        EventCodeEnum SetSeletedIndexWizardRecipe(int index);
        ObservableCollection<IWizardCategoryForm> GetIndexRecipe(int index);

        IWizardCategoryForm GetSeletedIndexRecipeCategory(int index);
        EventCodeEnum CheckCategories();
        ObservableCollection<IWizardCategoryForm> Recipe { get; }
        void GetWizardStepsFormModule();
    }


}
