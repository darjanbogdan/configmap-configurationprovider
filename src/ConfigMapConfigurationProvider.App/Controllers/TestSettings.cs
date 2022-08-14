namespace ConfigMapConfigurationProvider.App.Controllers
{
    public record TestSettings(bool? Prop, string Str)
    {
        public TestSettings() : this(Prop: default, Str: default)
        {

        }
    }
}