namespace CommonResources
{
    public static class ResourcesVendingMachine
    {
        public static string GetResourceString(string name)
        {
            string retVal = string.Empty;

            retVal = Properties.Resources_IO.ResourceManager.GetString(name);

            return retVal;
        }
    }
}
