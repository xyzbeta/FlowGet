namespace FlowGet.RestServer.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal abstract class BaseAttribute : Attribute
    {
        public string ExceptionMsg { get; set; } = default!;
        public abstract bool Validate(object obj, object value);
    }
}
